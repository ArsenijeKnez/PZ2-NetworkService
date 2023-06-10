using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using System.Xml;
using NetworkService.AdditionalElements;
using NetworkService.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Reactive.Linq;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : BindableBase
    {

        public MyICommand<string> NavCommand { get; private set; }
        public MyICommand Help { get; set; }
        private NetworkEntitiesViewModel networkEntitiesViewModel = new NetworkEntitiesViewModel();
        private NetworkDisplayViewModel networkDisplayViewModel = new NetworkDisplayViewModel();
        private MeasurmentGraphViewModel measurmentGraphViewModel = new MeasurmentGraphViewModel();

        private BindableBase currentViewModel;
   
  
        private static Dictionary<int,int> keys = new Dictionary<int,int>();


        public MainWindowViewModel()
        {
            NavCommand = new MyICommand<string>(OnNav);
            CurrentViewModel = networkEntitiesViewModel;
            Help = new MyICommand(HelpMe);
            MessageReceiver();
        }

        public BindableBase CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                SetProperty(ref currentViewModel, value);
            }
        }

        private void OnNav(string destination)
        {
            switch (destination)
            {
                case "entity":
                    CurrentViewModel = networkEntitiesViewModel;
                    break;
                case "display":
                    CurrentViewModel = networkDisplayViewModel;
                    break;
                case "graph":
                    CurrentViewModel = measurmentGraphViewModel;
                    break;

            }
        }
        private void HelpMe()
        {
            if (currentViewModel == networkEntitiesViewModel)
                System.Windows.MessageBox.Show("Pretraga: Oznaci koju vrstu pretrage koristis i unesi Naziv/Tip \nAdd: Unesi jedinstven ID(obavezno), Naziv i Tip i pritisnite Add \nDelete: Oznaci merac u tabeli i pritisni delete dugme", "Help", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (currentViewModel == networkDisplayViewModel)
                System.Windows.MessageBox.Show("Uzmite jedan element iz liste sa leve strane i stvite na jedno od slobodnih polja \nOslobodite pritiskom na dugme \"Oslobodi\"\nSpojite merace pritiskom na dugme \"Spoji\" na dva od zauzetih polja\nPonisti linije pritiskom na dugme \"Ponisti linije\"", "Help", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                System.Windows.MessageBox.Show("Izaberi merac iz como boxa meraca, na grafiku ce biti prikazano poslednjih 4 merenja ili manje u slucaju da jos nisu zabelezena 4\n Dodatni grafikon prikazuje 4 vrednosti zajedno sa njihovom brojnom vrednoscu", "Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void MessageReceiver()
        {
            try
            {
                int port = 25565;

                TcpListener server = new TcpListener(IPAddress.Any, port);
                server.Start();


                Task.Run(() =>
                {

                    while (true)
                    {
                        TcpClient client = server.AcceptTcpClient();

                        Thread thread = new Thread(() => HandleClient(client, keys));
                        thread.Start();
                    }

                });
            }
            catch (Exception e)
            {
                networkEntitiesViewModel.ValidationText = e.Message;
                //ValidationText = "Neuspesna konekcija";
            }
        }
        private static void HandleClient(object clientObj, Dictionary<int,int> keys)
        {
            TcpClient client = (TcpClient)clientObj;
            NetworkStream stream = client.GetStream();

            byte[] data = new byte[1024];
            int bytesRead = stream.Read(data, 0, data.Length);
            string request = Encoding.ASCII.GetString(data, 0, bytesRead);


            if (request == "Need object count")
            {
                int i = 0;
                keys.Clear();
                int numObjects = NetworkEntitiesViewModel.Meters.Count();
                foreach(ElectricityMeter meter in NetworkEntitiesViewModel.Meters)
                {
                    keys[i] = meter.ID;
                    i++;
                }
                byte[] responseData = Encoding.ASCII.GetBytes(numObjects.ToString());
                stream.Write(responseData, 0, responseData.Length);
                StreamWriter wr;
                using (wr = new StreamWriter((new Uri("Log.txt", UriKind.Relative)).ToString()))
                {
                    wr.WriteLine("LOG dat");
                }

            }
            else if (request.StartsWith("Entitet_"))
            {
                string[] parts = request.Split(':');
                int objectNum = int.Parse(parts[0].Split('_')[1]);
                double value = double.Parse(parts[1]);
                
                ElectricityMeter em = NetworkEntitiesViewModel.Meters.FirstOrDefault(meter => meter.ID == (keys[objectNum]));
                if (em != null)
                {
                    em.MesurmentValue = value;
                    if (MeasurmentGraphViewModel.last4.ContainsKey(em.ID))
                    {
                        List<double> values = MeasurmentGraphViewModel.last4[em.ID];
                        values.Add(value);
                        int k = 3;
                        for(int i= values.Count()-1; i >= 4;i--)
                        {
                            if(k >=0)
                            values[k]= values[i];
                            values.RemoveAt(i);
                        }
                        MeasurmentGraphViewModel.last4[em.ID] = values;

                            StreamWriter wr;
                            using (wr = new StreamWriter((new Uri("Log.txt", UriKind.Relative)).ToString(), true))
                            {
                                wr.WriteLine(keys[objectNum] + "_" + value + "_" + DateTime.Now.ToString());
                            }
            
                    }
                    else
                    {
                        MeasurmentGraphViewModel.last4.Add(em.ID, new List<double>() { value });
                    }
                }
            }

            stream.Close();
            client.Close();

        }


    }
}
