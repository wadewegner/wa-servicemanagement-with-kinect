namespace AzureManagementWithKinect
{
    using System.Windows.Controls;
    using Microsoft.Research.Kinect.Nui;
    using KinectNui = Microsoft.Research.Kinect.Nui; //Microsoft.Runtime is conflicting with using Runtime without an expicit namespace. This happens because the namespace starts with "Microsoft."
    
    public partial class KinectColorViewer : UserControl
    {
        private KinectNui.Runtime kinect;
        private InteropBitmapHelper imageHelper;

        public RuntimeOptions RuntimeOptions { get; set; }

        public KinectColorViewer()
        {
            InitializeComponent();
        }

        public KinectNui.Runtime Kinect
        {
            get
            {
                return kinect;
            }

            set
            {
                if (kinect != null)
                {
                    kinect.VideoFrameReady -= ColorImageReady;
                }

                kinect = value;

                if (kinect != null && kinect.Status == KinectStatus.Connected)
                {
                    kinect.VideoStream.Open(ImageStreamType.Video, 1, ImageResolution.Resolution640x480, ImageType.Color);
                    kinect.VideoFrameReady += ColorImageReady;
                }
            }
        }

        void ColorImageReady(object sender, ImageFrameReadyEventArgs e)
        {
            PlanarImage planarImage = e.ImageFrame.Image;

            //An interopBitmap is a WPF construct that enables resetting the Bits of the image.
            //This is more efficient than doing a BitmapSource.Create call every frame.
            if (imageHelper == null)
            {
                imageHelper = new InteropBitmapHelper(planarImage.Width, planarImage.Height, planarImage.Bits);
                kinectColorImage.Source = imageHelper.InteropBitmap;
            }
            else
            {
                imageHelper.UpdateBits(planarImage.Bits);
            }
        }
    }
}
