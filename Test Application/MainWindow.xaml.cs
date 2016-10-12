﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Test_Application
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaUriElement.Stop();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            var result = dlg.ShowDialog();
            if (result == true)
            {
                mediaUriElement.Source = new Uri(dlg.FileName);
                mediaUriElement.Volume = slrVolume.Value;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = ((Label) cmbBoxCorrection.SelectedItem).Content.ToString();
            var corSec  = int.Parse(selectedText);

            mediaUriElement.Start(DateTime.Now.AddSeconds((corSec)));
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaUriElement.Pause();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            mediaUriElement.Play();
        }

        private void slrVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                mediaUriElement.Volume = slrVolume.Value;                
            }
        }
    }
}
