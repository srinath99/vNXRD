using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Input;
using vWXR.KeyboardHelper;
using System.Windows.Interop;
using System.Windows;
using MapControl;
using System.Windows.Threading;

namespace vNXRD
{

    class ViewModel : INotifyPropertyChanged
    {
        KeyboardListener KListener = new KeyboardListener();
        bool ctrl;
        bool shift;
        bool alt;
        private Brush _windowBackground;
        private NexradTileLayer _systemType;
        private double _orientation;
        private double _brite;
        private LocationCollection _boundary;
        private WindowState _state;

        static LocationCollection ZOA = LocationCollection.Parse("41.3333385514631,-122.416680358443 41.3333385006297,-123.000013779294 41.3333384540043,-123.533347191072 40.3875050559668,-123.533347050395 40.216671685957,-123.833347069445 40.2166715604792,-125.333347279425 40.9833381375346,-126.900014266805 40.8333381205303,-127.000014256839 37.5063934779845,-127.000013815062 36.4619489690616,-126.933347024164 35.500004530243,-125.833346785563 36.0000046758254,-124.200013280644 34.5000046092927,-123.250013005483 35.5333381851133,-120.850012739782 35.7500049197541,-120.116679311768 35.6333382804672,-119.500012534256 36.1333383521181,-119.166679195304 36.1333383901059,-118.583345764575 37.2000051648076,-118.583345874896 37.2000052509555,-117.333345661302 37.3666719577541,-117.008345622511 37.4416719609208,-117.075845642514 37.5500053038842,-117.09473454671 37.8833386713375,-117.094734583948 38.0833386794682,-117.26667908191 39.9333388537806,-117.366679328965 41.0000054421724,-119.500013176605 41.0000052940653,-121.25001346044 41.3333385514631,-122.416680358443");
        static LocationCollection NCT = LocationCollection.Parse("39.5,-121.08555583333333 39.28333305555555,-120.95 39.133610833333336,-120.85750027777777 39.04416666666666,-120.80194472222222 38.97972194444445,-120.76166694444444 38.70777777777778,-120.59361138888889 38.55555555555555,-120.5 38.46278305555556,-120.44193416666667 38.04095833333333,-120.17837527777779 37.990833333333335,-120.53000027777777 37.77472222222222,-120.41055555555556 37.48833333333334,-120.27555583333333 37.38,-120.18583361111112 37.17361111111111,-120.01611138888889 37.18055555555555,-120.09166666666667 37.10555527777778,-120.18833361111112 36.89388861111111,-120.28805555555556 36.736111111111114,-120.18916694444445 36.66222194444444,-120.60750027777777 36.653055277777774,-120.65138916666668 36.54999972222222,-120.81611138888888 36.431666388888885,-121.06444472222222 36.408055555555556,-121.11361138888888 36.38916666666667,-121.15333361111112 36.19583305555555,-121.55555555555556 36.15694416666666,-121.825 36.113888611111115,-122.04583333333333 36.18333305555555,-122.39277777777778 36.39222222222222,-122.53194444444445 36.94944444444444,-122.90583361111112 36.983333333333334,-122.92972250000001 37.25555527777778,-122.83749999999999 37.43611083333333,-122.8522225 37.73416666666667,-122.85833333333333 37.865833333333335,-122.6613888888889 37.926944444444445,-122.53777805555555 38.0,-122.41666666666667 38.08277777777778,-122.27916694444444 38.14333305555556,-122.18222250000001 38.17499972222222,-122.13222222222221 38.290277499999995,-121.94638916666668 38.37777777777778,-121.80111111111111 38.5002775,-121.77222222222223 38.588055555555556,-121.7508336111111 38.643055555555556,-121.77583361111111 38.86666666666667,-121.87083333333332 38.983333333333334,-121.92444472222223 39.09111111111111,-121.93305583333334 39.2,-121.93833361111112 39.41361083333333,-121.87972222222221 39.5,-121.61611138888888 39.5,-121.4286113888889 39.5,-121.08555583333333");
        static LocationCollection RNO = LocationCollection.Parse("40.083333333333336,-119.63333333333334 39.78944416666666,-119.27222222222223 39.59722166666667,-119.25 39.329166111111114,-119.20777805555556 39.0,-119.5 38.92499972222222,-119.70694444444445 38.93055555555555,-119.84944499999999 39.108333333333334,-119.88333333333334 39.36666666666667,-120.01611138888889 39.583333333333336,-120.18333333333334 39.78333305555555,-120.18333333333334 39.88472222222222,-120.17361138888889 40.083333333333336,-119.63333333333334");
        static LocationCollection FAT = LocationCollection.Parse("37.18055555555555,-120.09166666666667 37.17361111111111,-120.01611138888889 37.12777777777778,-119.50833333333334 37.080555555555556,-119.26611138888889 36.71388833333334,-119.25 36.54999972222222,-119.23333333333333 36.2277775,-119.08277777777778 36.13333277777778,-119.03888888888889 36.13333277777778,-119.16666666666667 36.075,-119.20777805555556 35.8277775,-119.37166694444444 36.00166611111111,-119.4622225 36.021943888888885,-119.47277805555555 36.066944444444445,-119.49638944444445 36.16694444444444,-119.54944444444445 36.2475,-119.59166666666667 36.50833277777778,-119.72916666666667 36.574444444444445,-119.76416666666667 36.664722222222224,-120.00111111111111 36.68277777777777,-120.04777805555555 36.736111111111114,-120.18916722222222 36.89388833333333,-120.28805555555556 37.105555,-120.18833388888889 37.18055555555555,-120.09166666666667");


        public ViewModel()
        {
            var timer = new DispatcherTimer();

            _boundary = null;
            _windowBackground = new SolidColorBrush(Color.FromArgb(0x01, 0, 0, 0));
            ctrl = false;
            shift = false;
            alt = false;
            SetMode = true;

            KListener.KeyDown += new RawKeyEventHandler(KListener_KeyDown);
            KListener.KeyUp += new RawKeyEventHandler(KListener_KeyUp);
            Brite = 0.5;

            SystemType = new NexradTileLayer
            {
                SourceName = "ERAM",
                Description = "© [Srinath Nandakumar & Iowa State University](http://mesonet.agron.iastate.edu/)",
                TileSource = new TileSource { UriFormat = "https://web.ics.purdue.edu/~snandaku/atc/processor.php?x={x}&y={y}&z={z}" },
                UpdateWhileViewportChanging = true,
                MinZoomLevel = 7,
                MaxZoomLevel = 11
            };
            //https://wms.chartbundle.com/tms/1.0.0/sec/{z}/{x}/{y}.png?origin=nw
            timer.Interval = TimeSpan.FromSeconds(300);
            timer.Tick += timer_Tick;
            timer.Start();

            Orientation = 0;

            State = WindowState.Maximized;

        }

        void KListener_KeyDown(object sender, RawKeyEventArgs args)
        {
            if (alt && args.Key == Key.S && !ctrl && !shift)
            {
                WindowBackground.Dispatcher.Invoke(() =>
                {
                    var hwnd = new WindowInteropHelper(Application.Current.MainWindow).Handle;

                    if (WindowBackground.ToString() == "#00FFFFFF")
                    {
                        WindowsServices.SetWindowExLayered(hwnd);

                        WindowBackground = new SolidColorBrush(Color.FromArgb(0x01, 0, 0, 0));
                    }
                    else if (WindowBackground.ToString() == "#01000000")
                    {
                        WindowsServices.SetWindowExTransparent(hwnd);

                        WindowBackground = new SolidColorBrush(Colors.Transparent);
                    }
                });

                SetMode = !SetMode;
            }
            else if (SetMode && args.Key == Key.E && !ctrl && !alt && !shift)
            {
                string time = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString();
                WindowBackground.Dispatcher.Invoke(() =>
                {
                    if (!SystemType.SourceName.Contains("ERAM"))
                    {
                        SystemType = new NexradTileLayer
                        {
                            SourceName = "ERAM" + time,
                            Description = "© [Srinath Nandakumar & Iowa State University](http://mesonet.agron.iastate.edu/)",
                            TileSource = new TileSource { UriFormat = "https://web.ics.purdue.edu/~snandaku/atc/processor.php?x={x}&y={y}&z={z}" },
                            UpdateWhileViewportChanging = true,
                            MinZoomLevel = 6,
                            MaxZoomLevel = 11
                        };

                        Orientation = 0;
                        Boundary = null;
                    }
                });
            }
            else if (SetMode && args.Key == Key.S && !ctrl && !alt && !shift)
            {
                string time = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString();
                WindowBackground.Dispatcher.Invoke(() =>
                {
                    if (!SystemType.SourceName.Contains("STARS"))
                    {
                        SystemType = new NexradTileLayer
                        {
                            SourceName = "STARS" + time,
                            Description = "© [Srinath Nandakumar & Iowa State University](http://mesonet.agron.iastate.edu/)",
                            TileSource = new TileSource { UriFormat = "https://web.ics.purdue.edu/~snandaku/atc/processorS.php?x={x}&y={y}&z={z}" },
                            UpdateWhileViewportChanging = true,
                            MinZoomLevel = 8,
                            MaxZoomLevel = 13
                        };

                        Orientation = -15;
                        Boundary = null;
                    }
                });
            }
            else if (args.Key == Key.B && !ctrl && alt && !shift)
            {
                WindowBackground.Dispatcher.Invoke(() =>
                {
                    if (SystemType.SourceName.Contains("STARS"))
                    {
                        if (Boundary == null)
                        {
                            Boundary = NCT;
                        }
                        else if (Boundary == NCT)
                        {
                            Boundary = RNO;
                        }
                        else if (Boundary == RNO)
                        {
                            Boundary = FAT;
                        }
                        else if (Boundary == FAT)
                        {
                            Boundary = null;
                        }
                    }
                    if (SystemType.SourceName.Contains("ERAM"))
                    {
                        if (Boundary == null)
                        {
                            Boundary = ZOA;
                        }
                        else if (Boundary == ZOA)
                        {
                            Boundary = null;
                        }
                    }
                });
            }
            else if (alt && args.Key == Key.Up && !ctrl && !shift)
            {
                if (Brite < 1)
                {
                    Brite += 0.05;
                }
            }
            else if (alt && args.Key == Key.Down && !ctrl && !shift)
            {

                if (Brite > 0)
                {
                    Brite -= 0.05;
                }
            }
            else if (!alt && args.Key == Key.M && ctrl && shift)
            {

                if (State == WindowState.Maximized)
                {
                    State = WindowState.Minimized;
                }
                else if (State == WindowState.Minimized)
                {
                    State = WindowState.Maximized;
                }
            }
            else if (!alt && args.Key == Key.X && ctrl && shift)
            {
                WindowBackground.Dispatcher.Invoke(() =>
                { Application.Current.Shutdown(); });
            }
            else if (args.Key == Key.LeftCtrl || args.Key == Key.RightCtrl)
            {
                ctrl = true;
            }
            else if (args.Key == Key.LeftShift || args.Key == Key.RightShift)
            {
                shift = true;
            }
            else if (args.Key == Key.LeftAlt || args.Key == Key.RightAlt)
            {
                alt = true;
            }
        }

        void KListener_KeyUp(object sender, RawKeyEventArgs args)
        {

            if (args.Key == Key.LeftCtrl || args.Key == Key.RightCtrl)
            {
                ctrl = false;
            }
            else if (args.Key == Key.LeftShift || args.Key == Key.RightShift)
            {
                shift = false;
            }
            else if (args.Key == Key.LeftAlt || args.Key == Key.RightAlt)
            {
                alt = false;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            RefreshTiles();
        }

        public void RefreshTiles()
        {
            string time = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString();
            WindowBackground.Dispatcher.Invoke(() =>
            {
                if (SystemType.SourceName.Contains("STARS"))
                {
                    SystemType = new NexradTileLayer
                    {
                        SourceName = "STARS" + time,
                        Description = "© [Srinath Nandakumar & Iowa State University](http://mesonet.agron.iastate.edu/)",
                        TileSource = new TileSource { UriFormat = "https://web.ics.purdue.edu/~snandaku/atc/processorS.php?x={x}&y={y}&z={z}" },
                        UpdateWhileViewportChanging = true
                    };
                }
                else if (SystemType.SourceName.Contains("ERAM"))
                {
                    SystemType = new NexradTileLayer
                    {
                        SourceName = "ERAM" + time,
                        Description = "© [Srinath Nandakumar & Iowa State University](http://mesonet.agron.iastate.edu/)",
                        TileSource = new TileSource { UriFormat = "https://web.ics.purdue.edu/~snandaku/atc/processor.php?x={x}&y={y}&z={z}" },
                        UpdateWhileViewportChanging = true
                    };
                }

            });

            Console.WriteLine(SystemType.SourceName);
        }
        public Brush WindowBackground
        {
            get { return _windowBackground; }
            set
            {
                _windowBackground = value;
                RaisePropertyChanged("WindowBackground");
            }
        }

        public NexradTileLayer SystemType
        {
            get { return _systemType; }
            set
            {
                _systemType = value;
                RaisePropertyChanged("SystemType");
            }
        }

        public double Orientation
        {
            get { return _orientation; }
            set
            {
                _orientation = value;
                RaisePropertyChanged("Orientation");
            }
        }

        public double Brite
        {
            get { return _brite; }
            set
            {
                _brite = Math.Round(value, 2);
                RaisePropertyChanged("Brite");
            }
        }

        public LocationCollection Boundary
        {
            get { return _boundary; }
            set
            {
                _boundary = value;
                RaisePropertyChanged("Boundary");
            }
        }

        public WindowState State
        {
            get { return _state; }
            set
            {
                _state = value;
                RaisePropertyChanged("State");
            }
        }

        public bool SetMode { get; set; }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            KListener.Dispose();
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
