﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace ZXing.Mobile.WindowsPhone
{
    public partial class ZXingScannerControl : UserControl, IDisposable
    {
        public ZXingScannerControl()
        {
            InitializeComponent();

            this.topText.Text = TopText;
            this.bottomText.Text = BottomText;

            if (UseCustomOverlay && CustomOverlay != null)
            {
                gridCustomOverlay.Children.Add(CustomOverlay);

                gridCustomOverlay.Visibility = Visibility.Visible;
                gridDefaultOverlay.Visibility = Visibility.Collapsed;
            }
            else
            {
                gridCustomOverlay.Visibility = Visibility.Collapsed;
                gridDefaultOverlay.Visibility = Visibility.Visible;
            }

            // Initialize a new instance of SimpleCameraReader with Auto-Focus mode on
            _reader = new SimpleCameraReader(Scanner, ScanningOptions);
            _reader.ScanInterval = ScanningOptions.DelayBetweenAnalyzingFrames;
            
            // We need to set the VideoBrush we're going to display the preview feed on
            // IMPORTANT that it gets set before Camera initializes
            _previewVideo.SetSource(_reader.Camera);

            // The reader throws an event when a result is available 
            _reader.DecodingCompleted += (o, r) => DisplayResult(r);

            // The reader throws an event when the camera is initialized and ready to use
            _reader.CameraInitialized += ReaderOnCameraInitialized;
        }

        public MobileBarcodeScanningOptions ScanningOptions { get; set; }
        public MobileBarcodeScannerBase Scanner { get; set; }
        public System.Windows.UIElement CustomOverlay { get; set; }
        public string TopText { get; set; }
        public string BottomText { get; set; }
        public bool UseCustomOverlay { get; set; }

        public Result LastScanResult { get; set; }

        readonly SimpleCameraReader _reader;

        public event Action<ZXing.Result> OnScanResult;
        
        public bool IsTorchOn
        {
            get { return _reader.Camera.FlashMode == FlashMode.On; }
        }

        public void Torch(bool on)
        {
            _reader.Camera.FlashMode = on ? FlashMode.On : FlashMode.Auto;
        }

        public void ToggleTorch()
        {
            _reader.Camera.FlashMode = _reader.Camera.FlashMode == FlashMode.On ? FlashMode.Auto : FlashMode.On;
        }

        public void AutoFocus()
        {
            _reader.Camera.Focus();
        }

        public void Cancel()
        {
            LastScanResult = null;

            _reader.Stop();

            var evt = OnScanResult;
            if (evt != null)
                evt(null);
        }
        
        private void ReaderOnCameraInitialized(object sender, bool initialized)
        {
            // We dispatch (invoke) to avoid access exceptions
            Dispatcher.BeginInvoke(() => _previewTransform.Rotation = _reader.CameraOrientation);

            // We can set if Camera should flash or not when focused
			_reader.FlashMode = Microsoft.Devices.FlashMode.Off;

            // Starts the capturing process
            _reader.Start();
        }
        
        private void DisplayResult(Result result)
        {
			_reader.Stop();

            var evt = OnScanResult;
            if (evt != null)
                evt(result);
        }
        
        public void Dispose()
        {
            this.gridCustomOverlay.Children.Clear();

            _reader.Stop(); 
        }
    }
}
