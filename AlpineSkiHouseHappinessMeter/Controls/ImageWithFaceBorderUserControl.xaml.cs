﻿// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Cognitive Services: http://www.microsoft.com/cognitive
// 
// Microsoft Cognitive Services Github:
// https://github.com/Microsoft/Cognitive
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Collections.Generic;
using System.IO;
using ServiceHelpers;
using Newtonsoft.Json.Linq;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IntelligentKioskSample.Controls
{
    public sealed partial class ImageWithFaceBorderUserControl : UserControl
    {
        private ImageAnalyzer currentImage;

        public event EventHandler EmotionDetecting;
        public event EventHandler EmotionDetected;

        public ImageWithFaceBorderUserControl()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty DetectFacesOnLoadProperty =
            DependencyProperty.Register(
            "DetectFacesOnLoad",
            typeof(bool),
            typeof(ImageWithFaceBorderUserControl),
            new PropertyMetadata(false)
            );

        public static readonly DependencyProperty ShowMultipleFacesProperty =
            DependencyProperty.Register(
            "ShowMultipleFaces",
            typeof(bool),
            typeof(ImageWithFaceBorderUserControl),
            new PropertyMetadata(false)
            );

        public static readonly DependencyProperty PerformRecognitionProperty =
            DependencyProperty.Register(
            "PerformRecognition",
            typeof(bool),
            typeof(ImageWithFaceBorderUserControl),
            new PropertyMetadata(false)
            );
        
        public static readonly DependencyProperty ShowRecognitionResultsProperty =
            DependencyProperty.Register(
            "ShowRecognitionResults",
            typeof(bool),
            typeof(ImageWithFaceBorderUserControl),
            new PropertyMetadata(false)
            );

        public static readonly DependencyProperty ShowDialogOnApiErrorsProperty =
            DependencyProperty.Register(
            "ShowDialogOnApiErrors",
            typeof(bool),
            typeof(ImageWithFaceBorderUserControl),
            new PropertyMetadata(false)
            );

        public static readonly DependencyProperty ShowEmotionRecognitionProperty =
            DependencyProperty.Register(
            "ShowEmotionRecognition",
            typeof(bool),
            typeof(ImageWithFaceBorderUserControl),
            new PropertyMetadata(false)
            );

        public static readonly DependencyProperty BalloonBackgroundProperty =
            DependencyProperty.Register(
            "BalloonBackground",
            typeof(SolidColorBrush),
            typeof(ImageWithFaceBorderUserControl),
            new PropertyMetadata(null)
            );

        public static readonly DependencyProperty BalloonForegroundProperty =
            DependencyProperty.Register(
            "BalloonForeground",
            typeof(SolidColorBrush),
            typeof(ImageWithFaceBorderUserControl),
            new PropertyMetadata(null)
            );

        public static readonly DependencyProperty DetectFaceLandmarksProperty =
            DependencyProperty.Register(
            "DetectFaceLandmarks",
            typeof(bool),
            typeof(ImageWithFaceBorderUserControl),
            new PropertyMetadata(false)
            );

        public SolidColorBrush BalloonBackground
        {
            get { return (SolidColorBrush)GetValue(BalloonBackgroundProperty); }
            set { SetValue(BalloonBackgroundProperty, (SolidColorBrush)value); }
        }

        public SolidColorBrush BalloonForeground
        {
            get { return (SolidColorBrush)GetValue(BalloonForegroundProperty); }
            set { SetValue(BalloonForegroundProperty, (SolidColorBrush)value); }
        }

        public bool ShowEmotionRecognition
        {
            get { return (bool)GetValue(ShowEmotionRecognitionProperty); }
            set { SetValue(ShowEmotionRecognitionProperty, (bool)value); }
        }

        public bool ShowMultipleFaces
        {
            get { return (bool)GetValue(ShowMultipleFacesProperty); }
            set { SetValue(ShowMultipleFacesProperty, (bool)value); }
        }

        public bool DetectFacesOnLoad
        {
            get { return (bool)GetValue(DetectFacesOnLoadProperty); }
            set { SetValue(DetectFacesOnLoadProperty, (bool)value); }
        }

        public bool PerformRecognition
        {
            get { return (bool)GetValue(PerformRecognitionProperty); }
            set { SetValue(PerformRecognitionProperty, (bool)value); }
        }

        public bool ShowRecognitionResults
        {
            get { return (bool)GetValue(ShowRecognitionResultsProperty); }
            set { SetValue(ShowRecognitionResultsProperty, (bool)value); }
        }

        public bool ShowDialogOnApiErrors
        {
            get { return (bool)GetValue(ShowDialogOnApiErrorsProperty); }
            set { SetValue(ShowDialogOnApiErrorsProperty, (bool)value); }
        }

        public bool DetectFaceLandmarks
        {
            get { return (bool)GetValue(DetectFaceLandmarksProperty); }
            set { SetValue(DetectFaceLandmarksProperty, (bool)value); }
        }

        private async void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            ImageAnalyzer dataContext = this.DataContext as ImageAnalyzer;

            if (this.currentImage != dataContext)
            {
                this.currentImage = dataContext;
            }
            else
            {
                // Windows sometimes fires multiple DataContextChanged events. 
                // If we are here that is one of those cases, and since we already set 
                // the data context to this value we can ignore it
                return;
            }

            foreach (var child in this.hostGrid.Children.Where(c => !(c is Image)).ToArray())
            {
                this.hostGrid.Children.Remove(child);
            }

            // remove the current source
            this.bitmapImage.UriSource = null;

            if (dataContext != null)
            {
                try
                {
                    if (dataContext.ImageUrl != null)
                    {
                        this.bitmapImage.UriSource = new Uri(dataContext.ImageUrl);
                    }
                    else if (dataContext.GetImageStreamCallback != null)
                    {
                        await this.bitmapImage.SetSourceAsync((await dataContext.GetImageStreamCallback()).AsRandomAccessStream());
                    }
                }
                catch (Exception ex)
                {
                    // If we fail to load the image we will just not display it
                    this.bitmapImage.UriSource = null;
                    if (this.ShowDialogOnApiErrors)
                    {
                        await Util.GenericApiCallExceptionHandler(ex, "Error loading captured image.");
                    }
                }
            }
        }

        private void HideProgressBar()
        {
            this.progressIndicator.Visibility = Visibility.Collapsed;
            this.progressIndicator.IsIndeterminate = false;
        }

        private void ShowProgressBar()
        {
            this.progressIndicator.Visibility = Visibility.Visible;
            this.progressIndicator.IsIndeterminate = true;
        }

        private async Task DetectAndShowEmotion()
        {
            ShowProgressBar();

            foreach (var child in this.hostGrid.Children.Where(c => !(c is Image)).ToArray())
            {
                this.hostGrid.Children.Remove(child);
            }

            ImageAnalyzer imageWithFace = this.DataContext as ImageAnalyzer;
            if (imageWithFace != null)
            {
                if (imageWithFace.DetectedEmotion == null)
                {
                    EmotionDetecting?.Invoke(this, new EventArgs());

                    await imageWithFace.DetectEmotionAsync();

                    EmotionDetected?.Invoke(this, new EventArgs());
                }

                double renderedImageXTransform = this.imageControl.RenderSize.Width / this.bitmapImage.PixelWidth;
                double renderedImageYTransform = this.imageControl.RenderSize.Height / this.bitmapImage.PixelHeight;

                foreach (Emotion emotion in imageWithFace.DetectedEmotion)
                {
                    FaceIdentificationBorder faceUI = new FaceIdentificationBorder();

                    faceUI.Margin = new Thickness((emotion.FaceRectangle.Left * renderedImageXTransform) + ((this.ActualWidth - this.imageControl.RenderSize.Width) / 2),
                                                    (emotion.FaceRectangle.Top * renderedImageYTransform) + ((this.ActualHeight - this.imageControl.RenderSize.Height) / 2), 0, 0);

                    faceUI.BalloonBackground = this.BalloonBackground;
                    faceUI.BalloonForeground = this.BalloonForeground;

                    faceUI.ShowFaceRectangle(emotion.FaceRectangle.Width * renderedImageXTransform, emotion.FaceRectangle.Height * renderedImageYTransform);

                    faceUI.ShowEmotionData(emotion);
                    this.emotionEmojiControl.UpdateEmotion(emotion.Scores);

                    this.hostGrid.Children.Add(faceUI);

                    if (!this.ShowMultipleFaces)
                    {
                        break;
                    }
                }
            }

            //this.progressIndicator.IsActive = false;
            HideProgressBar();
        }

        private async Task PreviewImageFaces()
        {
            if (!this.DetectFacesOnLoad || this.progressIndicator.Visibility != Visibility.Collapsed)
            {
                return;
            }

            ImageAnalyzer img = this.DataContext as ImageAnalyzer;
            if (img != null)
            {
                img.UpdateDecodedImageSize(this.bitmapImage.PixelHeight, this.bitmapImage.PixelWidth);
            }

            if (this.ShowEmotionRecognition)
            {
                await this.DetectAndShowEmotion();
            }
        }

        private async void OnBitmapImageOpened(object sender, RoutedEventArgs e)
        {
            await this.PreviewImageFaces();
        }

        private async void OnImageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            await this.PreviewImageFaces();
        }
    }
}
