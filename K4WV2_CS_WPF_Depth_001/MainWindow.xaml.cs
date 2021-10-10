using System;
using System.Windows;
using Microsoft.Kinect;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace K4WV2_CS_WPF_Depth_001
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //Kinect SDK
        KinectSensor kinect;

        DepthFrameReader depthFrameReader;
        FrameDescription depthFrameDesc;

        //表示用
        WriteableBitmap depthImage;
        ushort[] depthBuffer;
        byte[] depthBitmapBuffer;
        Int32Rect depthRect;
        int depthStride;

        Point depthPoint;
        const int R = 20;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Kinectを開く
                kinect = KinectSensor.GetDefault();
                kinect.Open();

                //表示のためのデータを作成
                depthFrameDesc = kinect.DepthFrameSource.FrameDescription;

                //Depthリーダーを開く
                depthFrameReader = kinect.DepthFrameSource.OpenReader();
                depthFrameReader.FrameArrived += depthFrameReader_FrameArrived;

                //表示のためのビットマップに必要なものを作成
                depthImage = new WriteableBitmap(depthFrameDesc.Width, depthFrameDesc.Height, 96, 96, PixelFormats.Gray8, null);
                depthBuffer = new ushort[depthFrameDesc.LengthInPixels];
                depthBitmapBuffer = new byte[depthFrameDesc.LengthInPixels];
                depthRect = new Int32Rect(0, 0, depthFrameDesc.Width, depthFrameDesc.Height);
                depthStride = (int)(depthFrameDesc.Width);

                ImageDepth.Source = depthImage;

                //初期の位置表示座標（中心点）
                depthPoint = new Point(depthFrameDesc.Width / 2, depthFrameDesc.Height / 2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        private void depthFrameReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            UpdateDepthFrame(e);
            DrawDepthFrame();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (depthFrameReader!=null)
            {
                depthFrameReader.Dispose();
                depthFrameReader = null;
            }
            if (kinect!=null)
            {
                kinect.Close();
                kinect = null;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            depthPoint = e.GetPosition(this);
        }

        private void UpdateDepthValue()
        {
            CanvasPoint.Children.Clear();

            //クリックしたポイントを表示する
            var ellipse = new Ellipse()
            {
                Width = R,
                Height = R,
                StrokeThickness = R / 4,
                Stroke = Brushes.Red,
            };
            Canvas.SetLeft(ellipse, depthPoint.X - (R / 2));
            Canvas.SetTop(ellipse, depthPoint.Y - (R / 2));
            CanvasPoint.Children.Add(ellipse);

            //クリックしたポイントのインデックスを計算する
            int depthindex = (int)((depthPoint.Y * depthFrameDesc.Width) + depthPoint.X);

            //クリックしたポイントの距離を表示する
            var text = new TextBlock()
            {
                Text = string.Format("{0}mm\n{1},{2}", depthBuffer[depthindex],depthPoint.X,depthPoint.Y),
                FontSize = 20,
                Foreground = Brushes.Green,
            };
            Canvas.SetLeft(text, depthPoint.X+R);
            Canvas.SetTop(text, depthPoint.Y-R);
            CanvasPoint.Children.Add(text);
        }

        private void UpdateDepthFrame(DepthFrameArrivedEventArgs e)
        {
            using (var depthFrame=e.FrameReference.AcquireFrame())
            {
                if (depthFrame==null)
                {
                    return;
                }
                //Depthデータを取得する
                depthFrame.CopyFrameDataToArray(depthBuffer);
            }
        }

        private void DrawDepthFrame()
        {
            //距離情報の表示を更新する
            UpdateDepthValue();

            //0-8000のデータを0-65535のデータに変換する（見やすく）
            for (int i=0;i < depthBuffer.Length;i++)
            {
                depthBitmapBuffer[i] = (byte)(depthBuffer[i] % 255);
            }
            depthImage.WritePixels(depthRect, depthBuffer, depthStride, 0);
        }
    }
}
