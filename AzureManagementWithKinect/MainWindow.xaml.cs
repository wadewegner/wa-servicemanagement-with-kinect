namespace AzureManagementWithKinect
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Threading;
    using System.Xml.Linq;
    using AzureManagementWithKinect.Model;
    using AzureManagementWithKinect.ViewModel;
    using Coding4Fun.Kinect.Wpf;
    using Coding4Fun.Kinect.Wpf.Controls;
    using Kinect.Toolbox;
    using Microsoft.Research.Kinect.Nui;

    public partial class MainWindow : Window
    {
        private readonly SwipeGestureDetector swipeGestureRecognizer = new SwipeGestureDetector();

        private dynamic selectedItem;
        private ListViewItem selectedListItem;
        private Button selectedButton;
        private readonly Storyboard loading;

        private static double _handLeft;
        private static double _handTop;

        private const double ButtonOpacity = 0.55;
        private readonly AzureManagementWrapper service;
        private Deployment selectedDeployment;

        private const string ResumeInstanceText = "Resume Instance";
        private const string SuspendInstanceText = "Suspend Instance";

        private readonly BackgroundWorker backgroundWorker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.kinectButton.Click += new RoutedEventHandler(kinectButton_Click);

            var certificatePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), ConfigurationManager.AppSettings["ManagementCertificateName"]);
            var certificate = new X509Certificate2(certificatePath, ConfigurationManager.AppSettings["ManagementCertificatePassword"]);
            var subscriptionId = ConfigurationManager.AppSettings["SubscriptionsIdentifier"];

            this.service = new AzureManagementWrapper(subscriptionId, certificate);

            this.loading = this.Resources["LoadingStoryBoard"] as Storyboard;
            this.BeginStoryboard(loading);

            backgroundWorker.DoWork += ((sender, args) =>
                                            {
                                                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                                        new Action(delegate()
                                                {
                                                    this.kinectButton.Visibility = Visibility.Hidden;
                                                    this.LoadingImage.Visibility = Visibility.Visible;
                                                }));
                                                ((Action)args.Argument).Invoke();
                                            });

            backgroundWorker.RunWorkerCompleted += ((sender, args) => this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                                        new Action(delegate()
                                                                                                       {
                                                                                                           this.kinectButton.Visibility = Visibility.Visible;
                                                                                                           this.LoadingImage.Visibility = Visibility.Hidden;
                                                                                                       })));

            Action loadHostedServices = () =>
                                            {
                                                var serviceNameFilter = ConfigurationManager.AppSettings["ServiceNameFilter"];
                                                var slotFilter = ConfigurationManager.AppSettings["SlotFilter"].Split(';');
                                                
                                                var services = string.IsNullOrWhiteSpace(serviceNameFilter) ? 
                                                    service.GetHostedServices() :
                                                    service.GetHostedServices().Where(hs => hs.ServiceName.ToLowerInvariant().Contains(serviceNameFilter.ToLowerInvariant()));

                                                services.ToList().ForEach(hs => slotFilter.ToList().ForEach(s => AddHostedServiceRoles(hs, s)));
                                            };

            backgroundWorker.RunWorkerAsync(loadHostedServices);
        }

        private void AddHostedServiceRoles(HostedService hostedService, string environment)
        {
            var deployment = service.GetDeployment(hostedService.ServiceName, environment, true);
            if ((deployment != null) && !string.IsNullOrEmpty(deployment.Name))
            {
                foreach (var role in deployment.RoleList)
                {
                    if (role == null) continue;
                    this.MainView.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                                new Action(
                                                                                    () =>
                                                                                    this.MainView.Items.Add(
                                                                                        new
                                                                                            {
                                                                                                HostedServiceName = hostedService.ServiceName,
                                                                                                Environment = deployment.DeploymentSlot,
                                                                                                RoleName = role.RoleName,
                                                                                                Role = role
                                                                                            })));
                }
            }
        }

        void kinectButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DetailView.Visibility == Visibility.Visible)
            {
                if (this.selectedButton.Equals(this.BackButton))
                {
                    this.MainView.Visibility = Visibility.Visible;
                    this.DetailView.Visibility = Visibility.Collapsed;
                }
                else if (this.selectedButton.Equals(this.RefreshButton))
                {
                    AsyncRefreshInstanceData();
                }
                else if (this.selectedButton.Equals(this.ScaleUpButton))
                {
                    var role = this.selectedDeployment.RoleList.FirstOrDefault();

                    if (role != null)
                    {
                        int count = GetInstanceCountByRole(role.RoleName);
                        this.LoadingText.Text = "Scaling up";
                        UpdateInstaceCountOnRole(role.RoleName, ++count);
                    }
                }
                else if (this.selectedButton.Equals(this.ScaleDownButton))
                {
                    var role = this.selectedDeployment.RoleList.FirstOrDefault();

                    if (role != null)
                    {
                        int count = GetInstanceCountByRole(role.RoleName);
                        if (count > 1)
                        {
                            this.LoadingText.Text = "Scaling down";
                            UpdateInstaceCountOnRole(role.RoleName, --count);
                        }
                    }
                }
                else if (this.selectedButton.Equals(this.SwapButton))
                {
                    var role = this.selectedDeployment.RoleList.FirstOrDefault();

                    if (role != null)
                    {
                        this.LoadingText.Text = "Swaping instances";
                        Action action = () =>
                                            {
                                                service.SwapDeployment(this.selectedItem.HostedServiceName);
                                                this.RefreshInstanceData();
                                            };
                        this.backgroundWorker.RunWorkerAsync(action);
                    }
                }
                else if (this.selectedButton.Equals(this.StopButton))
                {
                    if (this.selectedDeployment.Status.Equals(DeploymentState.Running))
                        service.UpdateDeploymentStatus(this.selectedItem.HostedServiceName, this.selectedDeployment.DeploymentSlot, DeploymentStatus.Suspended);
                    else
                        service.UpdateDeploymentStatus(this.selectedItem.HostedServiceName, this.selectedDeployment.DeploymentSlot, DeploymentStatus.Running);

                    RefreshInstanceData();
                }
            }
            else
            {
                AsyncRefreshInstanceData();
                this.MainView.Visibility = Visibility.Collapsed;
                this.DetailView.Visibility = Visibility.Visible;
            }
        }

        private void UpdateInstaceCountOnRole(string roleName, int count)
        {
            Action action = () =>
                                {
                                    service.UpdateRoleInstances(this.selectedItem.HostedServiceName,
                                                                this.selectedDeployment.DeploymentSlot, roleName, count);
                                    RefreshInstanceData();
                                };
            this.backgroundWorker.RunWorkerAsync(action);
        }

        private int GetInstanceCountByRole(string roleName)
        {
            var configXml = XDocument.Parse(this.selectedDeployment.Configuration);
            var roleConfiguration = configXml.Root.Descendants(Constants.ServiceConfigurationNS + "Role").SingleOrDefault(r => r.Attribute("name").Value == roleName);

            if (roleConfiguration != null)
            {
                return Convert.ToInt32(roleConfiguration.Descendants(Constants.ServiceConfigurationNS + "Instances").First().Attribute("count").Value);
            }

            throw new InvalidOperationException("The role does not exist");
        }

        private void AsyncRefreshInstanceData()
        {
            this.backgroundWorker.RunWorkerAsync(this.RefreshInstanceData());
        }

        private Action RefreshInstanceData()
        {
            Action refreshInstance = () =>
                                         {
                                             this.LoadingText.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                                     new Action(
                                                                                         delegate()
                                                                                         {
                                                                                             this.LoadingText.Text =
                                                                                                 "Refreshing data...";
                                                                                         }));
                                             this.selectedDeployment = service.GetDeployment(this.selectedItem.HostedServiceName, this.selectedItem.Environment, true);
                                             this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                                        new Action(delegate()
                                                                                                       {
                                                                                                           this.DataContext = new RoleViewModel() { Deployment = this.selectedDeployment, Role = selectedItem.Role };
                                                                                                       }));
                                             this.StopButton.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                                        new Action(delegate()
                                                                                        {
                                                                                            this.StopButton.Content = this.selectedDeployment.Status.Equals(DeploymentState.Running) ? SuspendInstanceText : ResumeInstanceText;
                                                                                        }));
                                         };
            refreshInstance.Invoke();
            return refreshInstance;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var kinect = Runtime.Kinects.FirstOrDefault();
            InitializeKinect(kinect);
            Runtime.Kinects.StatusChanged += Kinects_StatusChanged;
        }

        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            if (o is ScrollViewer)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }

                return result;

            }
            return null;
        }

        private void InitializeKinect(Runtime kinect)
        {
            if (kinect != null)
            {
                kinect.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseDepthAndPlayerIndex |
                                  RuntimeOptions.UseSkeletalTracking);

                kinect.SkeletonFrameReady += kinectRuntime_SkeletonFrameReady;

                kinect.SkeletonEngine.TransformSmooth = true;

                var parameters = new TransformSmoothParameters
                {
                    Smoothing = 1.0f,
                    Correction = 0.1f,
                    Prediction = 0.1f,
                    JitterRadius = 0.05f,
                    MaxDeviationRadius = 0.05f
                };

                this.swipeGestureRecognizer.OnGestureDetected += OnGestureDetected;

                kinect.SkeletonEngine.SmoothParameters = parameters;

                var foundViewer = this.kinectColorViewer;
                if (foundViewer != null)
                {
                    foundViewer.Kinect = kinect;
                }
            }
        }

        private void OnGestureDetected(string gesture)
        {
            if (this.DetailView.Visibility == Visibility.Visible) return;

            var scrollViewer = GetScrollViewer(this.MainView) as ScrollViewer;
            if (scrollViewer == null) return;
            if (gesture == "SwipeToRight")
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - 1);

            if (gesture == "SwipeToLeft")
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + 1);

            return;
        }

        bool ProcessFrame(SkeletonFrame frame)
        {
            bool tracking = false;

            foreach (var skeleton in frame.Skeletons)
            {
                if (skeleton == null) continue;

                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                    continue;

                tracking = true;

                JointsCollection joints = skeleton.Joints;

                Joint rightHand = joints[JointID.HandRight];
                Joint leftHand = joints[JointID.HandLeft];

                var joinCursorHand = (rightHand.Position.Y > leftHand.Position.Y) ? rightHand : leftHand;

                float posX = joinCursorHand.ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight).Position.X;
                float posY = joinCursorHand.ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight).Position.Y;

                var kinect = Runtime.Kinects.FirstOrDefault();
                swipeGestureRecognizer.Add(joinCursorHand.Position, kinect.SkeletonEngine);

                var scaledCursorJoint = new Joint
                {
                    TrackingState = JointTrackingState.Tracked,
                    Position = new Microsoft.Research.Kinect.Nui.Vector
                    {
                        X = posX,
                        Y = posY,
                        Z = joinCursorHand.Position.Z
                    }
                };

                OnButtonLocationChanged(kinectButton, (int)scaledCursorJoint.Position.X, (int)scaledCursorJoint.Position.Y);
            }

            return tracking;
        }

        void kinectRuntime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            this.TrackingText.Visibility = Visibility.Hidden;

            if (e.SkeletonFrame.Skeletons.Count() == 0) return;

            if (ProcessFrame(e.SkeletonFrame)) this.TrackingText.Visibility = Visibility.Visible;
        }

        private void OnButtonLocationChanged(HoverButton hand, int X, int Y)
        {
            if (IsButtonOverObject(hand)) hand.Hovering(); else hand.Release();

            Canvas.SetLeft(hand, X - (hand.ActualWidth / 2));
            Canvas.SetTop(hand, Y - (hand.ActualHeight / 2));
        }

        public bool IsButtonOverObject(FrameworkElement hand)
        {
            if (!Window.GetWindow(hand).IsActive || this.kinectButton.Visibility.Equals(Visibility.Hidden)) return false;

            // get the location of the top left of the hand and then use it to find the middle of the hand
            var handTopLeft = new Point(Canvas.GetTop(hand), Canvas.GetLeft(hand));
            _handLeft = handTopLeft.X + (hand.ActualWidth / 2);
            _handTop = handTopLeft.Y + (hand.ActualHeight / 2);

            if (this.DetailView.Visibility == Visibility.Visible)
            {
                var buttons = GetListViewElements2<Button>(this.DetailView);

                buttons.ToList().ForEach(i => i.Background = new SolidColorBrush(Colors.Orange) { Opacity = ButtonOpacity });

                foreach (var target in buttons)
                {
                    Point targetTopLeft = target.PointToScreen(new Point());
                    if (_handTop > targetTopLeft.X
                        && _handTop < targetTopLeft.X + target.ActualWidth
                        && _handLeft > targetTopLeft.Y
                        && _handLeft < targetTopLeft.Y + target.ActualHeight)
                    {
                        target.Background = new SolidColorBrush(Colors.Orange);
                        target.Opacity = 1;
                        this.selectedButton = target;
                        return true;
                    }
                }
            }
            else
            {
                var listviewItems = GetListViewElements2<ListViewItem>(this.MainView);

                foreach (var target in listviewItems)
                {
                    Point targetTopLeft = target.PointToScreen(new Point());
                    if (_handTop > targetTopLeft.X
                        && _handTop < targetTopLeft.X + target.ActualWidth
                        && _handLeft > targetTopLeft.Y
                        && _handLeft < targetTopLeft.Y + target.ActualHeight)
                    {
                        this.selectedListItem = target;
                        this.selectedItem = this.selectedListItem.Content;
                        break;
                    }
                }

                var children = GetListViewElements<StackPanel>(this.MainView);

                children.ToList().ForEach(i => i.Background = new SolidColorBrush(Colors.Orange) { Opacity = ButtonOpacity });

                foreach (var target in children)
                {
                    Point targetTopLeft = target.PointToScreen(new Point());
                    if (_handTop > targetTopLeft.X
                        && _handTop < targetTopLeft.X + target.ActualWidth
                        && _handLeft > targetTopLeft.Y
                        && _handLeft < targetTopLeft.Y + target.ActualHeight)
                    {
                        target.Background = new SolidColorBrush(Colors.Orange);
                        target.Opacity = 1;
                        return true;
                    }
                }
            }

            return false;
        }

        public List<T> GetListViewElements<T>(FrameworkElement parent) where T : FrameworkElement
        {
            List<T> elements = new List<T>();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if ((child is T) && (((double)child.GetValue(ActualWidthProperty)) == 200))
                {
                    elements.Add(child as T);
                    continue;
                }

                if (VisualTreeHelper.GetChildrenCount(child) > 0)
                {
                    var children = GetListViewElements<T>(child as FrameworkElement);
                    elements.AddRange(children);
                }
            }

            return elements;
        }

        public List<T> GetListViewElements2<T>(FrameworkElement parent) where T : FrameworkElement
        {
            List<T> elements = new List<T>();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T)
                {
                    elements.Add(child as T);
                    continue;
                }

                if (VisualTreeHelper.GetChildrenCount(child) > 0)
                {
                    var children = GetListViewElements2<T>(child as FrameworkElement);
                    elements.AddRange(children);
                }
            }

            return elements;
        }

        private void Kinects_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (e.Status == KinectStatus.Connected)
            {
                InitializeKinect(e.KinectRuntime);

                var foundViewer = this.kinectColorViewer;
                if (foundViewer != null)
                {
                    foundViewer.Kinect = e.KinectRuntime;
                }
            }
        }
    }
}
