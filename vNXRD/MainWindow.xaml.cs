using MapControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Windows.Threading;
using System.Windows;

using System.Windows.Interop;
using System.Windows.Media;
using vWXR.KeyboardHelper;

namespace vNXRD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            var vm = new ViewModel();

            this.DataContext = vm;
            Closing += vm.OnWindowClosing;
        }



        private void myMap_Loaded(object sender, RoutedEventArgs e)
        {
            myMap.MapProjection = new WebMercatorProjection();
        }
    }
}