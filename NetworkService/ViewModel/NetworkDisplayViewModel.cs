using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NetworkService.AdditionalElements;
using NetworkService.Model;
using OxyPlot;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;


namespace NetworkService.ViewModel
{
    public class NetworkDisplayViewModel : BindableBase
    {
      
        public static BindingList<ElectricityMeter> Meters { get; set; }
        public List<Canvas> CanvasList { get; set; } = new List<Canvas>();
        public static Dictionary<string, ElectricityMeter> MetersCanvas { get; set; } = new Dictionary<string, ElectricityMeter>();


        private int selectedIndex = 0;
        public static ElectricityMeter draggedItem = null;
        private bool dragging = false;
        private Canvas selectedCanvas = null;

        private static bool postoji = false;
        private ListView listViewItem;

        public MyICommand<ElectricityMeter> SelectionChangedCommand { get; set; }
        public MyICommand<ListView> MouseLeftButtonUpCommand { get; set; }
        public MyICommand<Canvas> DragOverCommand { get; set; }
        public MyICommand<Canvas> DragLeaveCommand { get; set; }
        public MyICommand<Canvas> DropCommand { get; set; }
        public MyICommand<Canvas> FreeCommand { get; set; }
        public MyICommand<Canvas> LoadCommand { get; set; }
        public MyICommand<ListView> LoadListViewCommand { get; set; }
        public MyICommand<Canvas> ConnectCommand { get; set; }
        public MyICommand<Canvas> LineDelCommand { get; set; }
        public MyICommand<Canvas> RemoveAllCommand { get; set; }


        public MyICommand<Canvas> SelectionChangedCanvasCommand { get; set; }

        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                selectedIndex = value;
                OnPropertyChanged("SelectedIndex");

            }
        }
         

        public NetworkDisplayViewModel()
        {
            Meters = new BindingList<ElectricityMeter>();

            SelectionChangedCommand = new MyICommand<ElectricityMeter>(OnSelectionChanged);
            MouseLeftButtonUpCommand = new MyICommand<ListView>(OnMouseLeftButtonUp);
            DragOverCommand = new MyICommand<Canvas>(OnDragOver);
            DragLeaveCommand = new MyICommand<Canvas>(OnDragLeave);
            DropCommand = new MyICommand<Canvas>(OnDrop);
            FreeCommand = new MyICommand<Canvas>(OnFree);
            LoadCommand = new MyICommand<Canvas>(OnLoad);
            LoadListViewCommand = new MyICommand<ListView>(OnLoadListView);
            ConnectCommand = new MyICommand<Canvas>(OnConnect, CanConnect);
            LineDelCommand = new MyICommand<Canvas>(OnLineDel);
            RemoveAllCommand = new MyICommand<Canvas>(OnRemoveAll);

            SelectionChangedCanvasCommand = new MyICommand<Canvas>(OnSelectionCanvasChanged);
 

            Load();
        }

        public void OnConnect(Canvas canvas)
        {
            if(selectedCanvas == null)
            {
                selectedCanvas = canvas;
            }
            else
            {
                Canvas parentCanvas = FindParentCanvas(canvas);
                double x1 = Canvas.GetLeft(canvas) + canvas.ActualWidth / 2;
                double y1 = Canvas.GetTop(canvas) + canvas.ActualHeight / 2;
                double x2 = Canvas.GetLeft(selectedCanvas) + selectedCanvas.ActualWidth / 2;
                double y2 = Canvas.GetTop(selectedCanvas) + selectedCanvas.ActualHeight / 2;

                Line line = new Line();
                line.X1 = x1;  
                line.Y1 = y1;  
                line.X2 = x2; 
                line.Y2 = y2; 
                line.Stroke = Brushes.Black; 

       
                parentCanvas.Children.Add(line);

                selectedCanvas = null;
            }
        }
        public void OnLineDel(Canvas canvas)
        {
            var linesToRemove = canvas.Children.OfType<Line>().ToList();

            foreach (var line in linesToRemove)
            {
                canvas.Children.Remove(line);
            }
        }


        public Canvas FindParentCanvas(UIElement element)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(element);

            while (parent != null && !(parent is Canvas))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as Canvas;
        }
        public bool CanConnect(Canvas canvas)
        {
            return canvas.Resources["taken"] != null;
        }

        public void OnSelectionChanged(ElectricityMeter meter)
        {
            if (!dragging)                                                 
            {
                dragging = true;
                draggedItem = meter;
                DragDrop.DoDragDrop(listViewItem, draggedItem, DragDropEffects.Move);
            }
        }

        public void OnMouseLeftButtonUp(ListView lw)
        {                                                                  
            draggedItem = null;
            lw.SelectedItem = null;
            dragging = false;
        }

        public void OnRemoveAll(Canvas canvas)
        {
            for(int i = 2; i <= 13; i++) {
            DependencyObject child = VisualTreeHelper.GetChild(canvas, i);
                if(child != null && (child is Canvas))
                {
                    try
                    {
                        Canvas ca = child as Canvas;
                        if (ca.Resources["taken"] != null)
                        {
                            ca.Background = Brushes.White;
                            foreach (ElectricityMeter meter in NetworkEntitiesViewModel.Meters)
                            {
                                if (!Meters.Contains(meter) && MetersCanvas[ca.Name].ID == meter.ID)
                                    Meters.Add(meter);
                            }
                            ((Label)(ca).Children[2]).Content = "";
                            ca.Resources.Remove("taken");
                            MetersCanvas.Remove(ca.Name);
                            ConnectCommand.RaiseCanExecuteChanged();
                        }
                    }
                    catch (Exception) { }
                }

            }

        }

        public void OnSelectionCanvasChanged(Canvas canvas)
        {
            if (!dragging)
            {
                if (canvas.Resources["taken"] != null)
                {
                    dragging = true;
                    draggedItem = MetersCanvas[canvas.Name];

                    canvas.Background = Brushes.White;
                    canvas.Resources.Remove("taken");
                    MetersCanvas.Remove(canvas.Name);
                    DragDrop.DoDragDrop(listViewItem, draggedItem, DragDropEffects.Move);
                }
            }
        }


        public void Load()
        {

            foreach (ElectricityMeter meter in NetworkEntitiesViewModel.Meters)
            {
                postoji = false;
                foreach (ElectricityMeter meter2 in MetersCanvas.Values)
                {
                    if (meter.ID == meter2.ID)
                    {
                        postoji = true;
                        break;
                    }

                }
                if (postoji == false)
                    Meters.Add(new ElectricityMeter() { ID = meter.ID, Name = meter.Name, MesurmentValue = meter.MesurmentValue, Img= meter.Img, MeterType = meter.MeterType });
            }

        }

        public void OnDragOver(Canvas canvas)
        {                                                                   
            if (canvas.Resources["taken"] != null)
            {
                ((TextBlock)(canvas).Children[1]).Text = "X";
                ((TextBlock)(canvas).Children[1]).FontSize = 30;
                ((TextBlock)(canvas).Children[1]).FontWeight = System.Windows.FontWeights.ExtraBold;
            }
  
        }

        public void OnDragLeave(Canvas canvas)
        {                                                                   
            if (((TextBlock)(canvas).Children[1]).Text == "X")
            {
                ((TextBlock)(canvas).Children[1]).Text = "";
                ((TextBlock)(canvas).Children[1]).Background = Brushes.Transparent;
            }
        }

        public void OnDrop(Canvas canvas)
        {   

            if (draggedItem != null)                                        
            {
                if (canvas.Resources["taken"] == null)
                {
                    BitmapImage logo = new BitmapImage();

                    logo.BeginInit();
                    logo.UriSource = new Uri("pack://application:,,,/Slike/" + draggedItem.MeterType.ToString() + ".png", UriKind.Absolute);
                    logo.EndInit();

                    canvas.Background = new ImageBrush(logo);
                    MetersCanvas[canvas.Name] = draggedItem;
                    ((Label)(canvas).Children[2]).Content = draggedItem.Name;
                    canvas.Resources.Add("taken", true);
                    Meters.Remove(Meters.Single(x => x.ID == draggedItem.ID));
                    SelectedIndex = 0;
                    CheckValue(canvas);
                    ConnectCommand.RaiseCanExecuteChanged();
                }
                else
                {                                                          
                    ((TextBlock)(canvas).Children[1]).Text = "";
                    ((TextBlock)(canvas).Children[1]).Background = Brushes.Transparent;
                }
                dragging = false;
            }

        }

        public void OnFree(Canvas canvas)
        {
            try
            {
                if (canvas.Resources["taken"] != null)
                {
                                                                           
                    canvas.Background = Brushes.White;
                    foreach (ElectricityMeter meter in NetworkEntitiesViewModel.Meters)
                    {
                        if (!Meters.Contains(meter) && MetersCanvas[canvas.Name].ID == meter.ID)
                            Meters.Add(meter);
                    }
                    ((Label)(canvas).Children[2]).Content = "";
                    canvas.Resources.Remove("taken");
                    MetersCanvas.Remove(canvas.Name);
                    ConnectCommand.RaiseCanExecuteChanged();
                }
            }
            catch (Exception) { }

        }

       
        public void OnLoadListView(ListView listview)
        {
            listViewItem = listview;
        }

        public void OnLoad(Canvas canvas)
        {
            if (MetersCanvas.ContainsKey(canvas.Name))
            {
               
                BitmapImage logo = new BitmapImage();

                logo.BeginInit();
                ElectricityMeter temp = MetersCanvas[canvas.Name];
                logo.UriSource = new Uri("pack://application:,,,/Slike/" + temp.MeterType.ToString() +".png", UriKind.Absolute);
                logo.EndInit();

                ((Label)(canvas).Children[2]).Content = temp.Name;
                canvas.Background = new ImageBrush(logo);
                ((TextBlock)(canvas).Children[1]).Text = "";
                canvas.Resources.Add("taken", true);

                CheckValue(canvas);

            }
            if (!CanvasList.Contains(canvas))
            {
                CanvasList.Add(canvas);
            }
        }

        private void CheckValue(Canvas canvas)
        {
            Dictionary<int, ElectricityMeter> temp = new Dictionary<int, ElectricityMeter>();
            foreach (var meter in NetworkEntitiesViewModel.Meters)
            {
                temp.Add(meter.ID, meter);
            }
            Task.Delay(1000).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ((Border)(canvas).Children[0]).BorderBrush = Brushes.Transparent;

                    if (MetersCanvas.Count != 0)
                    {
                        if (MetersCanvas.ContainsKey(canvas.Name))
                        {
                            if(!MetersCanvas.ContainsKey(canvas.Name) || !temp.ContainsKey(MetersCanvas[canvas.Name].ID))
                            {
                                return;
                            }
                            else if (temp[MetersCanvas[canvas.Name].ID].MesurmentValue > 2.73 || temp[MetersCanvas[canvas.Name].ID].MesurmentValue < 0.34)
                            {
                                ((Border)(canvas).Children[0]).BorderBrush = Brushes.Gold;
                            }
                        }
                    }
                });
                CheckValue(canvas);
            });
        }

    }
}
