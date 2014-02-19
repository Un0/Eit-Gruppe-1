using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        KinectSensor sensor;

        const int SKELETON_COUNT = 6;
        Skeleton[] allSkeletons = new Skeleton[SKELETON_COUNT];


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count > 0)
                sensor = KinectSensor.KinectSensors[0];

            if (sensor.Status == KinectStatus.Connected)
            {
                sensor.ColorStream.Enable();
                sensor.DepthStream.Enable();
                sensor.SkeletonStream.Enable();

                sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
                sensor.Start();
            }
        }

        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null)
                    return;

                byte[] pixels = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(pixels);

                int stride = colorFrame.Width * 4;

                vid.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
            }

            Skeleton first = null;
            getSkeleton(e, ref first);

            if (first == null)
                return;

            getCameraPoint(first, e);

        }

        private void getSkeleton(AllFramesReadyEventArgs e, ref Skeleton first) {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame()) {
                if (skeletonFrameData == null) {
                    return;
                }

                skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                first = (from s in allSkeletons where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
            }
        }

        private void getCameraPoint(Skeleton first, AllFramesReadyEventArgs e) {
            using (DepthImageFrame depthData = e.OpenDepthImageFrame()) {
                if (depthData == null || sensor == null) {
                    return;    
                }

                //SkeletonPoint p = first.Joints[JointType.Head].Position;

                //DepthImagePoint l = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(p, DepthImageFormat.Resolution640x480Fps30);

                DepthImagePoint headDepthPoint = depthData.MapFromSkeletonPoint(first.Joints[JointType.Head].Position);

                ColorImagePoint headColorPoint = depthData.MapToColorImagePoint(headDepthPoint.X, headDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);

                Canvas.SetLeft(face, headColorPoint.X - face.Width / 2);
                Canvas.SetTop(face, headColorPoint.Y - face.Height / 2);

            }
        }



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            sensor.Stop();
        }

    }
}
