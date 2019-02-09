﻿using HearMe.Controllers;
using System;
using System.ComponentModel;
using System.Windows;

namespace HearMe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private PlayerController _controller;

        private string _displayText;
        public string DisplayText
        {
            get { return _displayText; }
            set
            {
                _displayText = value;
                OnPropertyChanged("DisplayText");
            }
        }

        private double _songPosition;
        public double SongPosition
        {
            get { return _songPosition; }
            set
            {
                _songPosition = value;
                OnPropertyChanged("SongPosition");
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            _controller = new PlayerController(this);
            songTime.DataContext = this;
            seekBar.DataContext = this;
        }

        private void UpdateSeekPosition(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                SongPosition = _controller.SongPosition - (_controller.SongFormat.AverageBytesPerSecond * 2.5);
                UpdateSongInformation();
            });
        }

        private void UpdateSongInformation()
        {
            DisplayText = _controller.CurrentTime + " / " + _controller.TotalTime;
        }

        private void SetVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _controller.SetVolume((float)volumeBar.Value / (float)volumeBar.Maximum);
        }

        private void Play(object sender, RoutedEventArgs e)
        {
            _controller.Play();
            seekBar.Maximum = _controller.Length;

            var timer = new System.Timers.Timer();
            timer.Interval = 300;
            timer.Elapsed += UpdateSeekPosition;
            timer.Start();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            _controller.Stop();
        }

        private void GoToPosition(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            long newPos = (long)seekBar.Value + (long)(_controller.SongFormat.AverageBytesPerSecond * 2.5);
            // Force it to align to a block boundary
            if ((newPos % _controller.SongFormat.BlockAlign) != 0)
                newPos -= newPos % _controller.SongFormat.BlockAlign;
            // Force new position into valid range
            newPos = Math.Max(0, Math.Min(_controller.Length, newPos));
            // set position
            _controller.SetPosition(newPos);

            UpdateSongInformation();

        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
        {
            var handler = System.Threading.Interlocked.CompareExchange(ref PropertyChanged, null, null);
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

    }
}
