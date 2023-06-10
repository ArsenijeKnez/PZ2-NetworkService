using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NetworkService.AdditionalElements;
using NetworkService.Model;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NetworkService.ViewModel
{
    public class NetworkEntitiesViewModel : BindableBase
    {
        public ObservableCollection<string> MeterTypes { get; }
        public static List<ElectricityMeter> Meters { get; set; }
        public ObservableCollection<ElectricityMeter> SearcedMeters { get; set; }

        public MyICommand AddCommand { get; set; }
        public MyICommand DeleteCommand { get; set; }

        private ElectricityMeter selectedMeter;
        private string idText;
        private string nameText;
        private TypeEnum.type meterType;
        private string nameSearchText;
        private bool isSearchByName;
        private bool isSearchByType;
        private double measurementValue;
        private string validationText = "";
        private ImageSource imageSource;

        public NetworkEntitiesViewModel()
        {
            LoadMeters();
            AddCommand = new MyICommand(OnAdd);
            DeleteCommand = new MyICommand(OnDelete, CanDelete);
            MeterTypes = new ObservableCollection<string>(Enum.GetNames(typeof(TypeEnum.type)));

        }

        public void LoadMeters()
        {
            ObservableCollection<ElectricityMeter> meters = new ObservableCollection<ElectricityMeter>();
            meters.Add(new ElectricityMeter { ID = 1, Name = "Meter 1", MeterType = TypeEnum.type.IntervalMeter, MesurmentValue = 0.0,Img = AppDomain.CurrentDomain.BaseDirectory + "Slike\\IntervalMeter.png"}); ;
            meters.Add(new ElectricityMeter { ID = 2, Name = "Meter 2", MeterType = TypeEnum.type.SmartMeter, MesurmentValue = 0.0, Img = AppDomain.CurrentDomain.BaseDirectory + "Slike\\SmartMeter.png" });
            meters.Add(new ElectricityMeter { ID = 3, Name = "Meter 3", MeterType = TypeEnum.type.IntervalMeter, MesurmentValue = 0.0, Img = AppDomain.CurrentDomain.BaseDirectory + "Slike\\IntervalMeter.png", });

            Meters = meters.ToList();
            SearcedMeters = meters;
        }
        public string NameSearchText
        {
            get { return nameSearchText; }
            set
            {
                if (nameSearchText != value)
                {
                    nameSearchText = value;
                    OnPropertyChanged("NameSearchText");
                    ApplySearch();
                }
            }
        }


        public bool IsSearchByName
        {
            get { return isSearchByName; }
            set
            {
                if (isSearchByName != value)
                {
                    isSearchByName = value;
                    OnPropertyChanged("IsSearchByName");
                    ApplySearch();
                }
            }
        }


        public bool IsSearchByType
        {
            get { return isSearchByType; }
            set
            {
                if (isSearchByType != value)
                {
                    isSearchByType = value;
                    OnPropertyChanged("IsSearchByType");
                    ApplySearch();
                }
            }
        }

        public void ApplySearch()
        {
            ValidationText = "";
            SearcedMeters.Clear();
                foreach (ElectricityMeter meter in Meters)
            {
                if (IsSearchByName && !string.IsNullOrEmpty(NameSearchText) && !meter.Name.Contains(NameSearchText))
                {
                    SearcedMeters.Remove(meter);
                }
                else if (IsSearchByType && !string.IsNullOrEmpty(NameSearchText) && !meter.MeterType.ToString().Contains(NameSearchText))
                {
                    SearcedMeters.Remove(meter);
                }
                else if (!SearcedMeters.Contains(meter))
                    SearcedMeters.Add(meter);
            }
        }

        public string ValidationText
        {
            get
            {
                return validationText;
            }
            set
            {
                if (validationText != value)
                {
                    validationText = value;
                    OnPropertyChanged("ValidationText");
                }
            }
        }

        public string IDText
        {
            get
            {
                return idText;
            }

            set
            {
                if (idText != value)
                {
                    idText = value;
                    OnPropertyChanged("IDText");
                }
            }
        }

        public double MesurmentValueText
        {
            get
            {
                return measurementValue;
            }

            set
            {
                if (measurementValue != value)
                {
                    measurementValue = value;
                    OnPropertyChanged("MesurmentValueText");
                }
            }
        }

        public string NameText
        {
            get
            {
                return nameText;
            }

            set
            {
                if (nameText != value)
                {
                    nameText = value;
                    OnPropertyChanged("NameText");
                }
            }
        }
        public ImageSource ImageSource
        {
            get
            {
                return imageSource;
            }

            set
            {
                if (imageSource != value)
                {
                    imageSource = value;
                    OnPropertyChanged("ImageSource");
                }
            }
        }

        public TypeEnum.type MeterType
        {
            get
            {
                return meterType;
            }

            set
            {
                if (meterType != value)
                {
                    meterType = value;
                    OnPropertyChanged("MeterType");
                }
                if (value == TypeEnum.type.IntervalMeter)
                {
                    ImageSource = new BitmapImage(new Uri("pack://application:,,,/Slike/IntervalMeter.png", UriKind.Absolute));
                }
                else { ImageSource = new BitmapImage(new Uri("pack://application:,,,/Slike/SmartMeter.png", UriKind.Absolute)); }
            }
        }

       
        public ElectricityMeter SelectedMeter
        {
            get
            {
                return selectedMeter;
            }

            set
            {
                if (selectedMeter != value)
                {
                    selectedMeter = value;
                    DeleteCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private void OnAdd()
        {
            if (int.TryParse(IDText, out int id) && !Meters.Exists(m => m.ID == id))
            {
                ElectricityMeter meter = new ElectricityMeter
                {
                    ID = id,
                    Name = NameText,
                    MeterType = MeterType,
                };
                Meters.Add(meter);
                ApplySearch();
            }
            else
            {
                ValidationText = "Niste uneli validan id";
            }
        }


        private void OnDelete()
        {

            Meters.Remove(SelectedMeter);
            foreach(KeyValuePair<string, ElectricityMeter> k in NetworkDisplayViewModel.MetersCanvas)
            {
                if(k.Value.ID == SelectedMeter.ID) { NetworkDisplayViewModel.MetersCanvas.Remove(k.Key);break; }
            }
            ApplySearch();
        }

        private bool CanDelete()
        {
            return SelectedMeter != null;
        }
     
      
    }
}
