using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class Plot : INotifyPropertyChanged
    {
        private PlotModel plotModel;

        public PlotModel PlotModel
        {
            get => plotModel;
            set
            {
                if (plotModel != value)
                {
                    plotModel = value;
                    OnPropertyChanged(nameof(PlotModel));
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
