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
using System.Collections;
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
            init();
        }

        KinectSensor sensor;

        const int SKELETON_COUNT = 6;
        Skeleton[] allSkeletons = new Skeleton[SKELETON_COUNT];
        List<Box> boxes = new List<Box>();
        Box boxe;

        double startX, startY;

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
            if (sensor == null)
                return;

            SkeletonPoint rightHand = first.Joints[JointType.HandRight].Position;

            DepthImagePoint rightHandDepthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(rightHand, sensor.DepthStream.Format);
            ColorImagePoint rightHandColorPoint = this.sensor.CoordinateMapper.MapDepthPointToColorPoint(sensor.DepthStream.Format, rightHandDepthPoint, sensor.ColorStream.Format);

            SkeletonPoint leftHand = first.Joints[JointType.HandLeft].Position;

            DepthImagePoint leftHandDepthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(leftHand, sensor.DepthStream.Format);
            ColorImagePoint leftHandColorPoint = this.sensor.CoordinateMapper.MapDepthPointToColorPoint(sensor.DepthStream.Format, leftHandDepthPoint, sensor.ColorStream.Format);


            //double x1 = Canvas.GetLeft(face);
            //double y1 = Canvas.GetTop(face);
            //Rect box1 = new Rect(x1, y1, face.ActualWidth, face.ActualHeight);

            //double x2 = Canvas.GetLeft(ball1);
            //double y2 = Canvas.GetTop(ball1);
            //Rect box2 = new Rect(x2, y2, ball1.ActualWidth, ball1.ActualHeight);

            //boxes[0].updateHitBox(ball1); // Only for test

            boxe.updateHitBox(boxe1);

            textbox3.Text = boxe.getHitBox().ToString() + "\n" + rightHandColorPoint.X + "," + rightHandColorPoint.Y;
            //textbox1.Text = "" + boxes[0].getHitBox().IntersectsWith(ball.getHitBox()) + "  " + boxes[1].getHitBox().IntersectsWith(ball.getHitBox());
            //textbox3.Text = textbox3.Text + "\n" + boxes[0].getHitBox() + "\n" + boxes[1].getHitBox(); //ball1

            double drawPointX = startX;
            double drawPointY = startY;

            if (rightHandColorPoint.X <= Canvas.GetLeft(boxe1) + boxe1.Width && rightHandColorPoint.X >= Canvas.GetLeft(boxe1)) { 
                drawPointX = rightHandColorPoint.X - boxe1.Width / 2;
                drawPointY = rightHandColorPoint.Y - boxe1.Height / 2;
                textbox1.Text = "" + drawPointX  + ","+ drawPointY;
            }

            //textbox2.Text = "";
            for (int i = 0; i < boxes.Count; i++)
            {
                Box boxen = boxes[i];
                if (boxen.getHitBox().IntersectsWith(boxe.getHitBox()))
                {
                    textbox2.Text = boxen.getName();
                    drawPointX = startX;
                    drawPointY = startY;
                }

            }

            //Canvas.SetLeft(face, rightHandColorPoint.X - face.Width / 2);
            //Canvas.SetTop(face, rightHandColorPoint.Y - face.Height / 2);
            if (rightHandColorPoint.X <= drawPointX + boxe1.Width / 2 && rightHandColorPoint.X >= drawPointX - boxe1.Width / 2)
            {
                Canvas.SetLeft(boxe1, drawPointX);
                Canvas.SetTop(boxe1, drawPointY);
            }
            else {
                Canvas.SetLeft(boxe1, startX);
                Canvas.SetTop(boxe1, startY);
                
            }
        }

        private void init() {
            textbox2.Text = "";
            textbox3.Text = "";
            foreach (FrameworkElement _e in canvas.Children)
            {
                //textbox3.Text = textbox3.Text + _e.Width.ToString() + " ";
                String name = _e.Name;
                if (name.Length > 4)
                {
                    name = name.Substring(0, 4);
                    if (name.Equals("wall"))
                    {
                        textbox2.Text = textbox2.Text + _e.Name + " ";
                        boxes.Add(new Box(_e));
                        textbox1.Text = "" + boxes.Count;
                        //textbox3.Text = _e.ActualWidth.ToString();
                    }
                    else if (name.Equals("boxe")) {
                        boxe = new Box(_e);
                        startX = Canvas.GetLeft(_e);
                        startY = Canvas.GetTop(_e);
                    }
                }
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(sensor!=null)
                sensor.Stop();
        }

    }
}
