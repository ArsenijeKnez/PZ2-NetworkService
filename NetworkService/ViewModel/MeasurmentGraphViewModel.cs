using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using NetworkService.AdditionalElements;
using NetworkService.Model;
//using System.Windows.Media;
using System.Windows.Forms;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Threading;
using System.IO;
using System.Reactive;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;
using InteractiveDataDisplay.WPF;
using System.Xml.Linq;
using System.Windows.Media.Effects;
using System.Reflection.Emit;

namespace NetworkService.ViewModel
{
    public class MeasurmentGraphViewModel : BindableBase
    {
        private ElectricityMeter selectedMeter;
        public PlotModel PM { get; set; }
        public ObservableCollection<ElectricityMeter> Meters { get; set; }

        public static Dictionary<int, List<double>> last4;
        private string text;
        private Canvas chart;
        public MyICommand<Canvas> ChartCommand { get; set; }
        public MeasurmentGraphViewModel()
        {
            InitializePlotModel();
        }

        private void InitializePlotModel()
        {
            ChartCommand = new MyICommand<Canvas>(DrawChart);
            PM = new PlotModel();
            Meters = new ObservableCollection<ElectricityMeter>();
            last4 = new Dictionary<int, List<double>>();
            Load();
            PM.Axes.Add(new LinearAxis { Position = AxisPosition.Left });
            PM.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });

        }

        public ElectricityMeter SelectedMeter
        {
            get => selectedMeter;
            set
            {
                if (selectedMeter != value)
                {
                    selectedMeter = value;
                    UpdatePlotModel();
                    OnPropertyChanged(nameof(SelectedMeter));
                }
            }
        }
        public string Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }
        public Canvas Chart
        {
            get 
            { 
                return chart;
            }
            set 
            { 
                chart = value;
                OnPropertyChanged("Chart");
            }
        }
        public void DrawChart(Canvas canvas)
        {
            List<UIElement> childrenToRemove = new List<UIElement>();
            foreach (UIElement child in canvas.Children)
            {
                if (!(child is System.Windows.Controls.Button))
                {
                    childrenToRemove.Add(child);
                }
            }
            foreach (UIElement child in childrenToRemove)
            {
                canvas.Children.Remove(child);
            }

            List<double> Values = GetLastFourMeasurementValuesForChart2(SelectedMeter);

            double max = Values.Max();

            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;

            double xSpacing = canvasWidth / (Values.Count - 1);
            double ySpacing = canvasHeight / (Values.Max() - Values.Min());

            double startX = 0;
            double startY = canvasHeight - ((Values[0] - Values.Min()) * ySpacing);
            System.Windows.Media.Brush color = System.Windows.Media.Brushes.LightBlue;
            if (Values[0] == max)
                color = System.Windows.Media.Brushes.Red;
            Ellipse circle1 = new Ellipse
            {
                Width = 17,
                Height = 17,
                Fill = color,
                Stroke = System.Windows.Media.Brushes.Black,
                StrokeThickness = 1,
                ToolTip = Values[0].ToString(),
                Margin = new Thickness(0 - 7, startY - 7, 0, 0)
            };
            canvas.Children.Add(circle1);

            System.Windows.Controls.Label label = new System.Windows.Controls.Label();

            label.Content = Values[0].ToString("0.00");
            label.FontWeight = System.Windows.FontWeights.Bold;
            label.FontSize = 12;
            Canvas.SetLeft(label, 0 - 7);
            Canvas.SetTop(label, startY - 7);


            canvas.Children.Add(label);
            for (int i = 1; i < Values.Count; i++)
            {
                double endX = i * xSpacing;
                double endY = canvasHeight - ((Values[i] - Values.Min()) * ySpacing);
                if (Values[i] == max)
                    color = System.Windows.Media.Brushes.Red;
                else
                    color = System.Windows.Media.Brushes.LightBlue;
                Line line = new Line
                {
                    X1 = startX,
                    Y1 = startY,
                    X2 = endX,
                    Y2 = endY,
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 1
                };

                canvas.Children.Add(line);

                Ellipse circle = new Ellipse
                {
                    Width = 17,
                    Height = 17,
                    Fill = color,
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 1,
                    ToolTip = Values[i].ToString(),
                    Margin = new Thickness(endX - 7, endY - 7, 0, 0)
                };

                canvas.Children.Add(circle);
                System.Windows.Controls.Label labela = new System.Windows.Controls.Label();

                labela.Content = Values[i].ToString("0.00");
                labela.FontWeight = System.Windows.FontWeights.Bold;
                labela.FontSize = 10;

                Canvas.SetLeft(labela, endX - 7); 
                Canvas.SetTop(labela, endY - 7);


                canvas.Children.Add(labela);


                startX = endX;
                startY = endY;
            }
        }
        private void UpdatePlotModel()
        {
            PM.Series.Clear();

            if (SelectedMeter != null)
            {
                var series = new LineSeries
                {
                    Title = SelectedMeter.Name,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 4,
                    MarkerStroke = OxyColors.White
                };

                var values = GetLastFourMeasurementValues(SelectedMeter);
                string time = "Vremenska promena:\n";
                DateTime last = DateTime.Now;
                int span = 0;
                for (int i = 0; i < values.Count; i++)
                {
                    if (i != 0)
                    {
                        TimeSpan duration = values[i].Timestamp - last;
                        int seconds = (int)duration.TotalSeconds;
                        span += seconds;
                        series.Points.Add(new DataPoint(span, values[i].Value));
                    }
                    else {
                    series.Points.Add(new DataPoint(0, values[i].Value));
                    }
                    time += "Tačka"+i + ":" + values[i].Timestamp + "\n";
                    last = values[i].Timestamp;
                }
                Text = time;
                PM.Series.Add(series);
            }

            PM.InvalidatePlot(true);
        }

        private List<double> GetLastFourMeasurementValuesForChart2(ElectricityMeter meter)
        {

 
            string[] lines = File.ReadAllLines("Log.txt");
            Array.Reverse(lines);
            List<double> last4Values = new List<double>();

            var filteredLines = lines.Where(line => line.StartsWith(meter.ID + "_"))
                                     .Select(line => double.Parse(line.Split('_')[1]))
                                     .Take(4);


            last4Values.AddRange(filteredLines);
            last4Values.Reverse();


            return last4Values;
        }

        private List<LogEntry> GetLastFourMeasurementValues(ElectricityMeter meter)
        {
            //if(meter != null)
            //    return last4[meter.ID];
            //else
            //{
            //    last4.Add(meter.ID, new List<double>());
            //    return last4[meter.ID];
            //}
            List<LogEntry> recentValues = new List<LogEntry>();
            string[] lines = File.ReadAllLines("Log.txt");
            Array.Reverse(lines);
            var filteredLines = lines.Where(line => line.StartsWith(meter.ID + "_"))
                                     .Select(line =>
                                     {
                                         string[] parts = line.Split('_');
                                         return new LogEntry
                                         {
                                             Value = double.Parse(parts[1]),
                                             Timestamp = DateTime.Parse(parts[2])
                                         };
                                     })
                                     .Take(4);

            recentValues.AddRange(filteredLines);
            recentValues.Reverse();


            return recentValues;
        }

        public void Load()
        {
            foreach (ElectricityMeter meter in NetworkEntitiesViewModel.Meters)
            {
                Meters.Add(new ElectricityMeter()
                {
                    ID = meter.ID,
                    Name = meter.Name,
                    MesurmentValue = meter.MesurmentValue,
                    Img = meter.Img,
                    MeterType = meter.MeterType
                });
                last4.Add(meter.ID, new List<double> { meter.MesurmentValue });
            }
        }

    }
}