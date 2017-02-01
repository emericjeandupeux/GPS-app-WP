using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Windows.Devices.Enumeration;
using Microsoft.Phone.Controls;
using Windows.Phone.Media.Capture;   // For advanced capture APIs
using Microsoft.Xna.Framework.Media; // For the media library
using System.IO;                     // For the memory stream
using Microsoft.Phone.Net.NetworkInformation;
using System.Device.Location;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Maps.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using Microsoft.Devices;

namespace Pano_de_config
{
    public partial class MainPage : PhoneApplicationPage
    {
        private Geolocator geoLoc;
        // Constructeur
        Ellipse myCircle;
        PhotoCamera cam;
        public MainPage()
        {
            InitializeComponent();
            DeviceNetworkInformation.NetworkAvailabilityChanged += new EventHandler<NetworkNotificationEventArgs>(NetworkChangeDetected);

            // Affecter l'exemple de données au contexte de données du contrôle ListBox
            DataContext = App.ViewModel;
            geoLoc = new Geolocator();
            geoLoc.DesiredAccuracy = PositionAccuracy.High;
            geoLoc.ReportInterval = 30000;
            geoLoc.StatusChanged += geoLoc_StatusChanged;
            geoLoc.PositionChanged += geoLoc_PositionChanged;
            myCircle = new Ellipse();
            myCircle.Fill = new SolidColorBrush(Colors.Red);
            myCircle.Height = 20;
            myCircle.Width = 20;
            myCircle.Opacity = 1;
            myCircle.Visibility = System.Windows.Visibility.Visible;

        }

        //Code for initialization, capture completed, image availability events; also setting the source for the viewfinder.
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            // Check to see if the camera is available on the phone.
            if ((PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true) ||
                 (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing) == true))
            {
                // Initialize the camera, when available.
                if (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing))
                {
                    // Use front-facing camera if available.
                    cam = new Microsoft.Devices.PhotoCamera(CameraType.FrontFacing);
                }
                else
                {
                    // Otherwise, use standard camera on back of phone.
                    cam = new Microsoft.Devices.PhotoCamera(CameraType.Primary);
                }
                viewfinderBrush.RelativeTransform = new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = -90, ScaleY = -1};

                // Event is fired when the PhotoCamera object has been initialized.
                /* cam.Initialized += new EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(cam_Initialized);

                 // Event is fired when the capture sequence is complete.
                 cam.CaptureCompleted += new EventHandler<CameraOperationCompletedEventArgs>(cam_CaptureCompleted);

                 // Event is fired when the capture sequence is complete and an image is available.
                 cam.CaptureImageAvailable += new EventHandler<Microsoft.Devices.ContentReadyEventArgs>(cam_CaptureImageAvailable);

                 // Event is fired when the capture sequence is complete and a thumbnail image is available.
                 cam.CaptureThumbnailAvailable += new EventHandler<ContentReadyEventArgs>(cam_CaptureThumbnailAvailable);
                 */
                //Set the VideoBrush source to the camera.
                viewfinderBrush.SetSource(cam);
            }
            else
            {
                // The camera is not supported on the phone.
                this.Dispatcher.BeginInvoke(delegate ()
                {
                    // Write message.
                    txtDebug.Text = "A Camera is not available on this phone.";
                });

                // Disable UI.
                ShutterButton.IsEnabled = false;
            }
        }
        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (cam != null)
            {
                // Dispose camera to minimize power consumption and to expedite shutdown.
                cam.Dispose();

                // Release memory, ensure garbage collection.
                /*cam.Initialized -= cam_Initialized;
                cam.CaptureCompleted -= cam_CaptureCompleted;
                cam.CaptureImageAvailable -= cam_CaptureImageAvailable;
                cam.CaptureThumbnailAvailable -= cam_CaptureThumbnailAvailable;*/
            }
        }


        void geoLoc_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            /*Dispatcher.BeginInvoke(() =>
            {
                TextBlock4.Text = "Pos: " + args.Position.ToString();
            });*/
        }

        void geoLoc_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            string status = "Localisation ";
            switch (args.Status)
            {
                case PositionStatus.Disabled:
                    // le gps n'est pas activé
                    status += "desactivée";
                    break;
                case PositionStatus.Initializing:
                    // le gps est en cours d'initialisation
                    status += "en cours d'initialisation";
                    break;
                case PositionStatus.NoData:
                    // il n'y a pas d'informations
                    status += "impossible";
                    break;
                case PositionStatus.Ready:

                    status += "activée";
                    // le GPS est activé et disponible
                    break;
                case PositionStatus.NotAvailable:
                    status = "not available";
                    // not used in WindowsPhone, Windows desktop uses this value to signal that there is no hardware capable to acquire location information
                    break;
                case PositionStatus.NotInitialized:
                    // the initial state of the geolocator, once the tracking operation is stopped by the user the geolocator moves back to this state
                    status = "not init";
                    break;
            }

            Dispatcher.BeginInvoke(() =>
            {
                TextBlock1.Text = status;
            });
        }


        public void NetworkChangeDetected(object sender, NetworkNotificationEventArgs e)
        {
            NotifOn();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void NotifOn()
        {
            Microsoft.Phone.Notification.HttpNotificationChannel notif = new Microsoft.Phone.Notification.HttpNotificationChannel("Juandup notif");
            notif.BindToShellToast();// = "Notif Juandup";
            notif.Open();

            notif.UnbindToShellToast();

        }
        Geoposition m_position = null;
        MapLayer myLocationLayer = new MapLayer();
        private async void OneShotLocation_Click(object sender, RoutedEventArgs e)
        {

            /*if (false)//(bool)System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                // The user has opted out of Location.
                return;
            }
            */
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            try
            {
                m_position = await geolocator.GetGeopositionAsync(
                maximumAge: TimeSpan.FromMinutes(5),
                timeout: TimeSpan.FromSeconds(10)
                );

                TextBlock2.Text = m_position.Coordinate.Latitude.ToString("0.0000");
                TextBlock3.Text = m_position.Coordinate.Longitude.ToString("0.0000");
                Geocoordinate geocoor = m_position.Coordinate;
                GeoCoordinate myGeoCoordinate = CoordinateConverter.ConvertGeocoordinate(geocoor);
                MapOverlay myLocationOverlay = new MapOverlay();
                myLocationOverlay.Content = myCircle;
                myLocationOverlay.PositionOrigin = new Point(0.5, 0.5);
                myLocationOverlay.GeoCoordinate = myGeoCoordinate;
                myLocationLayer.Add(myLocationOverlay);
                this.m_map.Center = myGeoCoordinate;
                this.m_map.ZoomLevel = 13;
                m_map.Layers.Add(myLocationLayer);

            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off
                    TextBlock1.Text = "location  is disabled in phone settings.";
                }
                //else
                {
                    // something else happened acquring the location
                }
            }
        }


        private void Send_Click(object sender, RoutedEventArgs e)
        {
            GeoCoordinate myGeoCoordinate = new GeoCoordinate(System.Convert.ToDouble(TextBlock2.Text), System.Convert.ToDouble(TextBlock3.Text));
            MapOverlay myLocationOverlay = new MapOverlay();
            myLocationOverlay.Content = myCircle;
            myLocationOverlay.PositionOrigin = new Point(0.5, 0.5);
            myLocationOverlay.GeoCoordinate = myGeoCoordinate;
            myLocationLayer.Add(myLocationOverlay);
            this.m_map.Center = myGeoCoordinate;
            this.m_map.ZoomLevel = 13;
            m_map.Layers.Add(myLocationLayer);
        }


    }
}