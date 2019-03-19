using HearMe.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using PlaylistsNET.Models;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Interop;
using HearMe.Helpers;

namespace HearMe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PlayerViewModel _viewModel;

        private GlobalKeyboardHook _globalKeyboardHook;
        private GlobalMediaKeyHook _globalMediaKeyboardHook;

        public MainWindow()
        {
            _viewModel = new PlayerViewModel();

            InitializeComponent();

            DataContext = _viewModel;

            SetupKeyboardHooks();

            Closing += OnWindowClosing;
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            _viewModel.UnbindOnStopEvent();
            _viewModel.Stop();
            _viewModel.Dispose();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        public void SetupKeyboardHooks()
        {
            _globalKeyboardHook = new GlobalKeyboardHook(new WindowInteropHelper(this).EnsureHandle());
            _globalKeyboardHook.KeyboardPressed += _viewModel.OnKeyPressed;

            // Listen for media keys
            _globalMediaKeyboardHook = new GlobalMediaKeyHook(new WindowInteropHelper(this).EnsureHandle());
            _globalMediaKeyboardHook.MediaKeyNextPressed += _viewModel.OnMediaKeyNextPressed;
            _globalMediaKeyboardHook.MediaKeyPreviousPressed += _viewModel.OnMediaKeyNextPressed;
            _globalMediaKeyboardHook.MediaKeyPlayPressed += _viewModel.OnMediaKeyPlayPressed;

            _globalMediaKeyboardHook.SetHook();

        }


    }
}
