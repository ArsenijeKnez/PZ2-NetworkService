using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class ElectricityMeter : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private TypeEnum.type _type;
        private string _img;

        private double _measurementValue;


        public int ID
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(ID));
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public string Img
        {
            get => _img;
            set
            {
                if (_img != value)
                {
                    _img = value;
                    OnPropertyChanged(nameof(Img));
                }
            }
        }

        public TypeEnum.type MeterType
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged(nameof(MeterType));
                }
            }
        }

        public double MesurmentValue
        {
            get => _measurementValue;
            set
            {
                if (_measurementValue != value)
                {
                    _measurementValue = value;
                    OnPropertyChanged(nameof(MesurmentValue));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
