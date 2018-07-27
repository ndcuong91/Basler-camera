using System;
using System.Drawing;
using System.Collections.Generic;
using LMS.Recognition.Core;
using LMS.Log;
using PylonC.NET;
using PylonC.NETSupportLibrary;

using LMS.Recognition.Config;

namespace LMS.Recognition.Camera.ViewModel
{
    /// <summary>
    /// BaslerViewModel class for streaming live view
    /// </summary>
    public class BaslerViewModel : IDisposable
    {
        #region Members
        /// <summary>
        /// Margin size
        /// </summary>
        private const int MarginSize = 2;

        /// <summary>
        /// Get list of camera device
        /// </summary>
        private static List<DeviceEnumerator.Device> list = null;

        /// <summary>
        /// Camera ip address
        /// </summary>
        private static string ipAddress = string.Empty;

        /// <summary>
        /// Check if Pylon sdk is initialized
        /// </summary>
        private static bool isInitPylon = false;

        /// <summary>
        /// Check if camera is calibrated
        /// </summary>
        private static bool isCalibrated = false;

        /// <summary>
        /// Width resolution of camera image
        /// </summary>
        public static int cameraWidth = LMConstants.DefaultCameraWidth;

        /// <summary>
        /// Height resolution of camera image
        /// </summary>
        public static int cameraHeight = LMConstants.DefaultCameraHeight;

        /// <summary>
        /// Camera index to connect
        /// </summary>
        private static int cameraIndex = 0;

        /// <summary>
        /// To get frame
        /// </summary>
        private ImageProvider imageProvider;

#if CameraLFR
        /// <summary>
        /// Last frame on live view
        /// </summary>
        private Bitmap lastestBitmap = null;
#endif

        /// <summary>
        /// Check if showing coordinates and ROI
        /// </summary>                      
        private bool canShowAxis = false;

        /// <summary>
        /// Check if showing ruler
        /// </summary>
        private bool canShowRuler = false;

        /// <summary>
        /// Check if showing region of interest
        /// </summary>
        private bool canShowRoi = false;

        /// <summary>
        /// Check if showing search area
        /// </summary>
        private bool canShowSearchArea = false;

        /// <summary>
        /// Check if Pylon sdk is initialized
        /// </summary>
        private bool isInitImageProvider = false;

        /// <summary>
        /// Check if camera is connected
        /// </summary>
        private bool isConnected = false;

        /// <summary>
        /// Check if camera has started
        /// </summary>
        private bool isStarted = false;

        /// <summary>
        /// Step size to resize region of interest
        /// </summary>
        private double stepSize;

        /// <summary>
        /// Circle diameter for teaching measurement
        /// </summary>
        private int circleDiameter = 10;

        /// <summary>
        /// Roi width
        /// </summary>
        private double roiWidth;

        /// <summary>
        /// Roi height
        /// </summary>
        private double roiHeight;

        /// <summary>
        /// Roi top
        /// </summary>
        private double roiTop;

        /// <summary>
        /// Roi left
        /// </summary>
        private double roiLeft;

        /// <summary>
        /// Ruler width
        /// </summary>
        private double rulerWidth;

        /// <summary>
        /// Ruler height
        /// </summary>
        private double rulerHeight;

        /// <summary>
        /// Ruler top
        /// </summary>
        private double rulerTop;

        /// <summary>
        /// Ruler left
        /// </summary>
        private double rulerLeft;

        /// <summary>
        /// Scale of frame inside control
        /// </summary>
        private double scale;

        /// <summary>
        /// X min of frame position
        /// </summary>
        private double roiXMin;

        /// <summary>
        /// Y min of frame position
        /// </summary>
        private double roiYMin;

        /// <summary>
        /// X max of frame position
        /// </summary>
        private double roiXMax;

        /// <summary>
        /// Y max of frame position
        /// </summary>
        private double roiYMax;

        /// <summary>
        /// Search area width
        /// </summary>
        private double searchAreaWidth;

        /// <summary>
        /// Search area height
        /// </summary>
        private double searchAreaHeight;

        /// <summary>
        /// Search area top
        /// </summary>
        private double searchAreaTop;

        /// <summary>
        /// Search area left
        /// </summary>
        private double searchAreaLeft;

        /// <summary>
        /// Live view image source
        /// </summary>
        private byte[] liveViewImage;

        /// <summary>
        /// Check if object is disposed
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Clear live view
        /// </summary>
        private bool showLiveView = false;

        /// <summary>
        /// Frame to display on live view
        /// </summary>
        public System.Windows.Controls.Image Frame { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the BaslerViewModel class.
        /// </summary>
        public BaslerViewModel()
        {
            LogManager.Info("BaslerViewModel.cs: BaslerViewModel(): Begin");
            try
            {
                this.InitializeParams();
            }
            catch (Exception ex)
            {
                LMRException exception = new LMRException((int)ErrorCode.BaslerConstructor, "Error: Intializing a new instance of the BaslerViewModel class", ex);
                LogManager.Error("BaslerViewModel.cs: BaslerViewModel(). " + ex.Message);
                throw exception;
            }

            LogManager.Info("BaslerViewModel.cs: BaslerViewModel(): End");
        }
        #endregion

        #region Properties
        /// <summary>
        /// Step to resize and move ROI
        /// </summary>
        public double StepSize
        {
            get
            {
                return this.stepSize;
            }

            set
            {
                this.stepSize = value;
                LMConfig config = LMConfig.LoadParams();
                MeasurementParams measurementParams = config.MeasurementParams;
                this.stepSize = this.stepSize * measurementParams.Ratio / this.scale;
            }
        }

        /// <summary>
        /// Circle diameter for teaching measurement
        /// </summary>
        public int CircleDiameter
        {
            get
            {
                return this.circleDiameter;
            }

            set
            {
                this.circleDiameter = value;
            }
        }

        /// <summary>
        /// Show axis
        /// </summary>
        public bool CanShowAxis
        {
            get
            {
                return this.canShowAxis;
            }

            set
            {
                this.canShowAxis = value;
                this.RaisePropertyChanged("CanShowAxis");
            }
        }

        /// <summary>
        /// Show roi
        /// </summary>
        public bool CanShowRoi
        {
            get
            {
                return this.canShowRoi;
            }

            set
            {
                this.canShowRoi = value;
                this.RaisePropertyChanged("CanShowRoi");
            }
        }

        /// <summary>
        /// Show search area
        /// </summary>
        public bool CanShowSearchArea
        {
            get
            {
                return this.canShowSearchArea;
            }

            set
            {
                this.canShowSearchArea = value;
                this.RaisePropertyChanged("CanShowSearchArea");
            }
        }

        /// <summary>
        /// Show ruler
        /// </summary>
        public bool CanShowRuler
        {
            get
            {
                return this.canShowRuler;
            }

            set
            {
                this.canShowRuler = value;
                this.RaisePropertyChanged("CanShowRuler");
            }
        }

        /// <summary>
        /// Roi width
        /// </summary>
        public double RoiWidth
        {
            get
            {
                return this.roiWidth;
            }

            set
            {
                this.roiWidth = value;
                this.RaisePropertyChanged("RoiWidth");
                this.RaisePropertyChanged("RoiSizeLabel");
            }
        }

        /// <summary>
        /// Roi height
        /// </summary>
        public double RoiHeight
        {
            get
            {
                return this.roiHeight;
            }

            set
            {
                this.roiHeight = value;
                this.RaisePropertyChanged("RoiHeight");
                this.RaisePropertyChanged("RoiSizeLabel");
            }
        }

        /// <summary>
        /// Roi top
        /// </summary>
        public double RoiTop
        {
            get
            {
                return this.roiTop;
            }

            set
            {
                this.roiTop = value;
                this.RaisePropertyChanged("RoiTop");
                this.RaisePropertyChanged("RoiSizeLabel");
            }
        }

        /// <summary>
        /// Roi left
        /// </summary>
        public double RoiLeft
        {
            get
            {
                return this.roiLeft;
            }

            set
            {
                this.roiLeft = value;
                this.RaisePropertyChanged("RoiLeft");
                this.RaisePropertyChanged("RoiSizeLabel");
            }
        }

        /// <summary>
        /// Roi size label
        /// </summary>
        public string RoiSizeLabel
        {
            get
            {
                string roiSizeLabel = string.Empty;
                double roiXMilimet = this.scale * this.RoiWidth;
                double roiYMilimet = this.scale * this.RoiHeight;
                LMConfig config = LMConfig.LoadParams();
                MeasurementParams measureParams = config.MeasurementParams;
                roiXMilimet /= measureParams.Ratio;
                roiYMilimet /= measureParams.Ratio;
                roiSizeLabel = String.Format("W: {0:0.000} mm \nH: {1:0.000} mm", roiXMilimet, roiYMilimet);
                return roiSizeLabel;
            }
        }

        /// <summary>
        /// Roi Y label
        /// </summary>
        public string RoiYLabel
        {
            get
            {
                string roiYLabel = string.Empty;
                double roiMilimet = this.scale * this.RoiHeight;
                LMConfig config = LMConfig.LoadParams();
                MeasurementParams measureParams = config.MeasurementParams;
                roiMilimet /= measureParams.Ratio;
                roiYLabel = String.Format("{0:0.000}", roiMilimet);
                return roiYLabel;
            }
        }

        /// <summary>
        /// Ruler width
        /// </summary>
        public double RulerWidth
        {
            get
            {
                return this.rulerWidth;
            }

            set
            {
                this.rulerWidth = value;
                this.RaisePropertyChanged("RulerWidth");
            }
        }

        /// <summary>
        /// Ruler width
        /// </summary>
        public double RulerHeight
        {
            get
            {
                return this.rulerHeight;
            }

            set
            {
                this.rulerHeight = value;
                this.RaisePropertyChanged("RulerHeight");
            }
        }

        /// <summary>
        /// Ruler width
        /// </summary>
        public double RulerTop
        {
            get
            {
                return this.rulerTop;
            }

            set
            {
                this.rulerTop = value;
                this.RaisePropertyChanged("RulerTop");
            }
        }

        /// <summary>
        /// Ruler width
        /// </summary>
        public double RulerLeft
        {
            get
            {
                return this.rulerLeft;
            }

            set
            {
                this.rulerLeft = value;
                this.RaisePropertyChanged("RulerLeft");
            }
        }

        /// <summary>
        /// Search area width
        /// </summary>
        public double SearchAreaWidth
        {
            get
            {
                return this.searchAreaWidth;
            }

            set
            {
                this.searchAreaWidth = value;
                this.RaisePropertyChanged("SearchAreaWidth");
            }
        }

        /// <summary>
        /// Search area height
        /// </summary>
        public double SearchAreaHeight
        {
            get
            {
                return this.searchAreaHeight;
            }

            set
            {
                this.searchAreaHeight = value;
                this.RaisePropertyChanged("SearchAreaHeight");
            }
        }

        /// <summary>
        /// Search area top
        /// </summary>
        public double SearchAreaTop
        {
            get
            {
                return this.searchAreaTop;
            }

            set
            {
                this.searchAreaTop = value;
                this.RaisePropertyChanged("SearchAreaTop");
            }
        }

        /// <summary>
        /// Search area left
        /// </summary>
        public double SearchAreaLeft
        {
            get
            {
                return this.searchAreaLeft;
            }

            set
            {
                this.searchAreaLeft = value;
                this.RaisePropertyChanged("SearchAreaLeft");
            }
        }

        /// <summary>
        /// Clear live view
        /// </summary>
        public bool ShowLiveView
        {
            get
            {
                return this.showLiveView;
            }

            set
            {
                this.showLiveView = value;
                this.RaisePropertyChanged("ShowLiveView");
            }
        }

        /// <summary>
        /// Actual width of control
        /// </summary>
        public double ControlWidth { get; set; }

        /// <summary>
        /// Actual height of control
        /// </summary>
        public double ControlHeight { get; set; }

        /// <summary>
        /// Live view zoom ratio
        /// </summary>
        public double ZoomRatio { get; set; } = 1.0;



        /// <summary>
        /// Live view image source
        /// </summary>
        public byte[] LiveViewImage
        {
            get
            {
                return this.liveViewImage;
            }

            set
            {
                if (value != this.liveViewImage)
                {
                    this.liveViewImage = value;
                    this.RaisePropertyChanged("LiveViewImage");
                    //if (Environment.TickCount - LastTimeUpdate > 500)
                    //{
                    //    LastTimeUpdate = Environment.TickCount;
                    //}

                }
            }
        }

        //public int LastTimeUpdate { get; private set; }
        #endregion

        #region Public functions
        /// <summary>
        /// Check if device is available
        /// </summary>
        /// <returns>true if device is available</returns>
        public static bool IsAvailable()
        {
            LogManager.Info("BaslerViewModel.cs: IsAvailable(): Begin");
            bool isAvailable = false;
            try
            {
                if (!isInitPylon)
                {
                    Pylon.Initialize();
                    isInitPylon = true;
                }

                isAvailable = IsReadyDevice(true);
            }
            catch (Exception)
            {
                try
                {
                    Pylon.Terminate();
                }
                catch (Exception)
                {
                    isAvailable = false;
                }

                isAvailable = false;
            }

            LogManager.Info("BaslerViewModel.cs: IsAvailable(): End");
            return isAvailable;
        }

        public double GetScale()
        {
            return this.scale;
        }

        public double ConvertMmtoPixel(double mmValue)
        {
            double pixelValue = 0.0d;
            LMConfig config = LMConfig.LoadParams();
            MeasurementParams measurementParams = config.MeasurementParams;
            mmValue = mmValue * measurementParams.Ratio / this.scale;
            pixelValue = mmValue;
            return pixelValue;
        }

        /// <summary>
        /// Get camera IP
        /// </summary>
        /// <returns>ip address</returns>
        public static string GetCameraIP()
        {
            try
            {
                LogManager.Info("BaslerViewModel.cs: GetCameraIP(): Begin");

                // Check if already get ip address
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    return ipAddress;
                }

                try
                {
                    // Initialize pylon 
                    if (!isInitPylon)
                    {
                        LogManager.Info("BaslerViewModel.cs: GetCameraIP(). Initializing Pylon environment.");
                        Pylon.Initialize();
                        isInitPylon = true;
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error("BaslerViewModel.cs: GetCameraIP(). Error when initializing Pylon environment.");
                    throw ex;
                }

                PYLON_DEVICE_HANDLE device = null;
                try
                {
                    LogManager.Info("BaslerViewModel.cs: GetCameraIP(). Getting property value from camera.");

                    // Must Enumerate all camera devices before creating a device.
                    uint numDevices = Pylon.EnumerateDevices();
                    if (numDevices > 0)
                    {
                        // Get a handle for the first device found. 
                        device = Pylon.CreateDeviceByIndex(0);
                        ipAddress = Pylon.DeviceInfoGetPropertyValueByName(Pylon.DeviceGetDeviceInfoHandle(device), "IpAddress");
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error("BaslerViewModel.cs: GetCameraIP(). Error when getting property value from camera.");
                    throw ex;
                }
                finally
                {
                    // Clean up. Release the pylon device.
                    Pylon.DestroyDevice(device);
                }
            }
            catch (Exception ex)
            {
                LMRException exception = new LMRException((int)ErrorCode.BaslerConstructor, "Error: Get camera IP.", ex);
                LogManager.Error("BaslerViewModel.cs: GetCameraIP(). " + ex.Message);
                throw exception;
            }

            LogManager.Info("BaslerViewModel.cs: GetCameraIP(): End");
            return ipAddress;
        }

        /// <summary>
        /// Start live view camera.
        /// </summary>
        public void Start()
        {
            try
            {
                if (!this.isStarted)
                {
                    LogManager.Info("BaslerViewModel.cs: Start(): Begin");
#if DEBUG
                    // Measure time execution
                    var watch = System.Diagnostics.Stopwatch.StartNew();
#endif

                    this.Connect();
                    this.ContinuousShot();
                    this.isStarted = true;
                    this.ShowLiveView = true;

#if DEBUG
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    LogManager.Info("BaslerViewModel.cs: Start(). Time execution to start camera: " + elapsedMs.ToString() + " ms.");
#endif
                }
            }
            catch (Exception ex)
            {
                LMRException exception = new LMRException((int)ErrorCode.BaslerStart, "Error: Starting live view camera.", ex);
                LogManager.Error("BaslerViewModel.cs: Start(). " + ex.Message);
                throw exception;
            }

            LogManager.Info("BaslerViewModel.cs: Start()");
        }

        /// <summary>
        /// Stops the grabbing of images.
        /// </summary>
        public void Stop()
        {

            try
            {
                if (this.isStarted)
                {
                    LogManager.Info("BaslerViewModel.cs: Stop(): Begin");
#if DEBUG
                    // Measure time execution
                    var watch = System.Diagnostics.Stopwatch.StartNew();
#endif
                    this.StopShot();
                    this.Disconnect();
                    this.isStarted = false;
                    this.ShowLiveView = false;

#if DEBUG
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    LogManager.Info("BaslerViewModel.cs: Stop(). Time execution to stop camera: " + elapsedMs.ToString() + " ms.");
#endif
                }
            }
            catch (Exception ex)
            {
                LMRException exception = new LMRException((int)ErrorCode.BaslerStop, "Error: Stopping live view camera.", ex);
                LogManager.Error("BaslerViewModel.cs: Stop(). " + ex.Message);
                throw exception;
            }

            LogManager.Info("BaslerViewModel.cs: Stop()");
        }

#if CameraLFR
        /// <summary>
        /// Acquire the image from the image provider.
        /// </summary>
        /// <returns>current image</returns>
        public Bitmap GetCurrentFrame()
        {
            LogManager.Info("BaslerViewModel.cs: GetCurrentFrame(): Begin");
            Bitmap currentFrame = null;
            try
            {
                // Run camera without live view
                if (this.Frame == null)
                {
                    LogManager.Info("BaslerViewModel.cs: GetCurrentFrame(). Getting frame without live view.");
                    ImageProvider.Image lastestFrame = this.imageProvider.GetLatestImage();
                    if (lastestFrame != null)
                    {
                        BitmapFactory.CreateBitmap(out currentFrame, lastestFrame.Width, lastestFrame.Height, lastestFrame.Color);
                        BitmapFactory.UpdateBitmap(currentFrame, lastestFrame.Buffer, lastestFrame.Width, lastestFrame.Height, lastestFrame.Color);
                        currentFrame.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    }

                    // The processing of the image is done. Release the image buffer. 
                    this.imageProvider.ReleaseImage();
                }
                else
                {
                    LogManager.Info("BaslerViewModel.cs: GetCurrentFrame(). Getting frame with live view.");

                    // Deep copy frame from live view
                    currentFrame = this.lastestBitmap.DeepClone<Bitmap>();
                }                
            }
            catch (Exception ex)
            {
                LMRException exception = new LMRException((int)ErrorCode.BaslerGetCurrentFrame, "Error: Get current frame.", ex);
                LogManager.Error("BaslerViewModel.cs: GetCurrentFrame(). " + ex.Message);
                throw exception;
            }

            LogManager.Info("BaslerViewModel.cs: GetCurrentFrame(): End");
            return currentFrame;
        }
#else
        /// <summary>
        /// Acquire the image from the image provider.
        /// </summary>
        /// <returns>current image</returns>
        public Bitmap GetCurrentFrame()
        {
            Bitmap currentFrame = null;
            try
            {
                //// Run camera without live view
                //if (this.Frame == null)
                //{
                //    LogManager.Info("BaslerViewModel.cs: GetCurrentFrame(). Getting frame without live view.");
                int timeoutFrame = Environment.TickCount;
                ImageProvider.Image lastestFrame = null;
                while (lastestFrame == null)
                {
                    LogManager.Info("BaslerViewModel.cs: GetCurrentFrame(): Begin");
                    lastestFrame = this.imageProvider.GetLatestImage();
                    if (Environment.TickCount - timeoutFrame > 10000) break;//10s
                }

                Bitmap tmpFrame = null;
                if (lastestFrame != null)
                {
                    BitmapFactory.CreateBitmap(out tmpFrame, lastestFrame.Width, lastestFrame.Height, lastestFrame.Color);
                    BitmapFactory.UpdateBitmap(tmpFrame, lastestFrame.Buffer, lastestFrame.Width, lastestFrame.Height, lastestFrame.Color);
                    tmpFrame.RotateFlip(RotateFlipType.Rotate180FlipNone);

                    // The processing of the image is done. Release the image buffer. 
                    this.imageProvider.ReleaseImage();
                }


                //}
                //else
                //{
                //    LogManager.Info("BaslerViewModel.cs: GetCurrentFrame(). Getting frame with live view.");

                //    // Deep copy frame from live view #HienHD - 010617
                //    currentFrame = this.lastestBitmap.DeepClone<Bitmap>();


                //}

                currentFrame = tmpFrame.DeepClone<Bitmap>();
            }
            catch (Exception ex)
            {
                LMRException exception = new LMRException((int)ErrorCode.BaslerGetCurrentFrame, "Error: Get current frame.", ex);
                LogManager.Error("BaslerViewModel.cs: GetCurrentFrame(). " + ex.Message);
                throw exception;
            }
            LogManager.Info("BaslerViewModel.cs: GetCurrentFrame(): End");
            return currentFrame;
        }
#endif

        //private object lockObject = new Object();
        #endregion

        #region Draw roi functions
        /// <summary>
        /// Set limit of roi
        /// </summary>
        public void SetRoiLimit()
        {
            this.scale = Math.Max(cameraWidth / this.ControlWidth, cameraHeight / this.ControlHeight);
            this.roiXMin = (this.ControlWidth - cameraWidth / this.scale) / 2;
            this.roiYMin = (this.ControlHeight - cameraHeight / this.scale) / 2;
            this.roiXMax = this.ControlWidth - this.roiXMin;
            this.roiYMax = this.ControlHeight - this.roiYMin;
        }

        /// <summary>
        /// Show up coordinates axis.
        /// </summary>
        /// <param name="canShow">true if showing coordinates</param>
        public void ShowCoordinates(bool canShow)
        {
            this.CanShowAxis = canShow;
        }

        /// <summary>
        /// Show up roi bounding box.
        /// </summary>
        /// <param name="canShow">true if showing roi</param>
        public void ShowRoi(bool canShow)
        {
            this.CanShowRoi = canShow;
        }

        /// <summary>
        /// Decrease bounding box width.
        /// </summary>
        public void DecreaseRoiWidth()
        {
            if ((this.RoiWidth - this.stepSize) < 2 * MarginSize)
            {
                this.RoiWidth = 2 * MarginSize;
                this.RoiLeft = (this.ControlWidth - this.RoiWidth) / 2;
            }
            else
            {
                this.RoiWidth -= this.StepSize;
                this.RoiLeft += this.StepSize / 2;
            }
        }

        /// <summary>
        /// Increase bounding box width.
        /// </summary>
        public void IncreaseRoiWidth()
        {
            if ((this.RoiWidth + this.stepSize) > this.roiXMax - this.roiXMin - 2 * MarginSize)
            {
                this.RoiWidth = this.roiXMax - this.roiXMin - 2 * MarginSize;
                this.RoiLeft = (this.ControlWidth - this.RoiWidth) / 2;
            }
            else
            {
                this.RoiWidth += this.StepSize;
                this.RoiLeft -= this.StepSize / 2;
            }
        }

        /// <summary>
        /// Decrease bounding box height.
        /// </summary>
        public void DecreaseRoiHeight()
        {
            if ((this.RoiHeight - this.stepSize) < 2 * MarginSize)
            {
                this.RoiHeight = 2 * MarginSize;
                this.RoiTop = (this.ControlHeight - this.RoiHeight) / 2;
            }
            else
            {
                this.RoiHeight -= this.StepSize;
                this.RoiTop += this.StepSize / 2;
            }
        }

        /// <summary>
        /// Increase bounding box width.
        /// </summary>
        public void IncreaseRoiHeight()
        {
            if ((this.RoiHeight + this.stepSize) > this.roiYMax - this.roiYMin - 2 * MarginSize)
            {
                this.RoiHeight = this.roiYMax - this.roiYMin - 2 * MarginSize;
                this.RoiTop = (this.ControlHeight - this.RoiHeight) / 2;
            }
            else
            {
                this.RoiHeight += this.StepSize;
                this.RoiTop -= this.StepSize / 2;
            }
        }

        #endregion

        #region Draw ruler for the measurement
        /// <summary>
        /// Show up ruler for the measurement.
        /// </summary>
        /// <param name="canShow">true if showing the ruler</param>
        public void ShowRuler(bool canShow)
        {
            this.CanShowRuler = canShow;
        }

        /// <summary>
        /// Increase the radius of the circle
        /// </summary>
        /// <param name="step">step to change diameter (in pixel)</param>
        public void IncreaseCircle(int step)
        {
            if ((this.CircleDiameter + step) > Math.Min(this.roiXMax - this.roiXMin - 2 * MarginSize, this.roiYMax - this.roiYMin - 2 * MarginSize))
            {
                this.CircleDiameter = (int)Math.Min(this.roiXMax - this.roiXMin - 2 * MarginSize, this.roiYMax - this.roiYMin - 2 * MarginSize);
                this.RulerTop = (this.ControlHeight - this.CircleDiameter) / 2;
                this.RulerLeft = (this.ControlWidth - this.CircleDiameter) / 2;
                this.RulerWidth = this.CircleDiameter;
                this.RulerHeight = this.CircleDiameter;
            }
            else
            {
                this.CircleDiameter += step;
                this.RulerTop -= step / 2;
                this.RulerLeft -= step / 2;
                this.RulerWidth += step;
                this.RulerHeight += step;
            }
        }

        /// <summary>
        /// Derease the radius of the circle
        /// </summary>
        /// <param name="step">step to change diameter (in pixel)</param>
        public void DecreaseCircle(int step)
        {
            if ((this.CircleDiameter - step) < 2 * MarginSize)
            {
                this.CircleDiameter = 2 * MarginSize;
                this.RulerTop = (this.ControlHeight - this.CircleDiameter) / 2;
                this.RulerLeft = (this.ControlWidth - this.CircleDiameter) / 2;
                this.RulerWidth = this.CircleDiameter;
                this.RulerHeight = this.CircleDiameter;
            }
            else
            {
                this.CircleDiameter -= step;
                this.RulerTop += step / 2;
                this.RulerLeft += step / 2;
                this.RulerWidth -= step;
                this.RulerHeight -= step;
            }
        }
        #endregion

        #region Teaching functions
        /// <summary>
        /// Return region of interest on the control.
        /// </summary>
        /// <returns>ROI on the control</returns>
        public Bitmap GetRoi()
        {
            LogManager.Info("BaslerViewModel.cs: GetRoi(): Begin");
            Bitmap roiBmp = null;
            try
            {
#if DEBUG
                // Measure time execution
                var watch = System.Diagnostics.Stopwatch.StartNew();
#endif

                // Convert to coordinates of view region
                RectangleF realRoi = new RectangleF((float)(this.RoiLeft - this.roiXMin), (float)(this.RoiTop - this.roiYMin), (float)this.RoiWidth, (float)this.RoiHeight);

                // Convert to coordinate of real image
                realRoi = new RectangleF((float)(this.scale * realRoi.X), (float)(this.scale * realRoi.Y), (float)(this.scale * realRoi.Width), (float)(this.scale * realRoi.Height));

                Bitmap currentFrame = this.GetCurrentFrame();
                if (currentFrame == null)
                {
                    LogManager.Error("BaslerViewModel.cs: GetRoi(). Current frame is null.");
                    throw new Exception();
                }

                roiBmp = LMUtils.CropBitmap(currentFrame, realRoi);

#if DEBUG
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                LogManager.Info("BaslerViewModel.cs: GetRoi(). Time execution to get region of interest: " + elapsedMs.ToString() + " ms.");
#endif
            }
            catch (Exception ex)
            {
                LMRException exception = new LMRException((int)ErrorCode.BaslerGetRoi, "Error: Getting region of interest from camera.", ex);
                LogManager.Error("BaslerViewModel.cs: GetRoi(). " + ex.Message);
                throw exception;
            }

            LogManager.Info("BaslerViewModel.cs: GetRoi(): End");
            return roiBmp;
        }

        /// <summary>
        /// Set search area
        /// </summary>
        public void SetSearchArea(LMRect searchArea)
        {
            LogManager.Info("BaslerViewModel.cs: SetSearchArea(): Begin");
            try
            {
                LogManager.Debug(string.Format("BaslerViewModel.cs: SetSearchArea(). Search area: Top={0}, Left={1}, Width={2}, Height={3}. Scale={4}, RoiXMin={5}, RoiYMin={6}", searchArea.X, searchArea.Y, searchArea.Width, searchArea.Height, this.scale, this.roiXMin, this.roiYMin));

                // Convert to live view coordinates
                RectangleF liveViewSearchArea = new RectangleF((float)(searchArea.X / this.scale + this.roiXMin), (float)(searchArea.Y / this.scale + this.roiYMin), (float)(searchArea.Width / this.scale), (float)(searchArea.Height / this.scale));
                this.SearchAreaLeft = liveViewSearchArea.X;
                this.SearchAreaTop = liveViewSearchArea.Y;
                this.SearchAreaWidth = liveViewSearchArea.Width;
                this.SearchAreaHeight = liveViewSearchArea.Height;
            }
            catch (Exception ex)
            {
                LMRException exception = new LMRException((int)ErrorCode.BaslerSaveRoi, "Error: Setting search area.", ex);
                LogManager.Error("BaslerViewModel.cs: SetSearchArea(). " + ex.Message);
                throw exception;
            }

            LogManager.Info("BaslerViewModel.cs: SetSearchArea(): End");
        }

        /// <summary>
        /// Teach ratio between distance on image (pixels) and in reality (in milimet)
        /// </summary>
        /// <param name="distanceInMilimet">distance in reality (in milimet)</param>
        /// <returns>measurement ratio</returns>
        public double TeachMeasurement(double distanceInMilimet)
        {
            LogManager.Info("BaslerViewModel.cs: TeachMeasurement(): Begin");
            double pixelRatio = 0;
            try
            {
                pixelRatio = this.CircleDiameter * this.scale / distanceInMilimet;
                LMConfig config = LMConfig.LoadParams();
                MeasurementParams measurementParams = config.MeasurementParams;
                measurementParams.Ratio = pixelRatio;
            }
            catch (Exception ex)
            {
                LMRException exception = new LMRException((int)ErrorCode.BaslerTeachMeasurement, "Error: Teach measurement.", ex);
                LogManager.Error("BaslerViewModel.cs: TeachMeasurement(). " + ex.Message);
                throw exception;
            }

            LogManager.Info("BaslerViewModel.cs: TeachMeasurement(): End");
            return pixelRatio;
        }
        #endregion

        #region Dispose pattern
        /// <summary>
        /// Dispose object
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        /// <param name="disposing">true if object is disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
#if CameraLFR
                    if (this.lastestBitmap != null)
                    {
                        this.lastestBitmap.Dispose();
                        this.lastestBitmap = null;
                    }
#endif
                    if (this.imageProvider != null)
                    {
                        this.imageProvider.Stop();
                        this.imageProvider.Close();
                    }

                    if (isInitPylon)
                    {
                        Pylon.Terminate();
                    }
                }

                this.disposed = true;
            }
        }
        #endregion

        #region Private static methods
        /// <summary>
        /// Check if device is ready
        /// </summary>
        /// <returns>true if device is ready</returns>
        /// <param name="isUpdateListDevice">true if update list of devices</param>
        private static bool IsReadyDevice(bool isUpdateListDevice)
        {
            LogManager.Info("BaslerViewModel.cs: IsReadyDevice(): Begin");
            bool isReady = false;
            try
            {
                // Update list of cameras
                if (isUpdateListDevice || list == null)
                {
                    list = DeviceEnumerator.EnumerateDevices();
                }

                if (list.Count == 0)
                {
                    LogManager.Error("BaslerViewModel.cs: IsReadyDevice(): No device.");
                    return false;
                }

                PYLON_DEVICE_HANDLE device = null;
                LMConfig config = LMConfig.LoadParams();
                BaslerParams baslerParams = config.BaslerParams;
                LogManager.Info("BaslerViewModel.cs: IsReadyDevice(): Ip config: " + baslerParams.IpAddress);

                for (int i = 0; i < list.Count; i++)
                {
                    // Check ip address
                    try
                    {
                        device = Pylon.CreateDeviceByIndex((uint)i);
                        string ipAddress = Pylon.DeviceInfoGetPropertyValueByName(Pylon.DeviceGetDeviceInfoHandle(device), Pylon.cPylonDeviceInfoIpAddressKey);
                        LogManager.Info("BaslerViewModel.cs: IsReadyDevice(): Checking device " + i.ToString() + " with ip address: " + ipAddress);
                        if (string.Compare(baslerParams.IpAddress, ipAddress) == 0)
                        {
                            LogManager.Info("BaslerViewModel.cs: IsReadyDevice(): Device found with ip address: " + ipAddress);
                            cameraIndex = i;
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        isReady = false;
                        LogManager.Error("BaslerViewModel.cs: IsReadyDevice(): Cannot find IP address. " + ex.Message);
                    }
                    finally
                    {
                        try
                        {
                            // Clean up. Close and release the pylon device.
                            if (Pylon.DeviceIsOpen(device))
                            {
                                Pylon.DeviceClose(device);
                            }

                            Pylon.DestroyDevice(device);
                        }
                        catch (Exception ex)
                        {
                            isReady = false;
                            LogManager.Error("BaslerViewModel.cs: IsReadyDevice(): Cannot destroy device. " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception)
            {
                isReady = false;
            }

            LogManager.Info("BaslerViewModel.cs: IsReadyDevice(): End");
            return isReady;
        }
        #endregion

        #region Initialize parameters
        /// <summary>
        /// Initialize all parameters for live view control.
        /// </summary>
        private void InitializeParams()
        {
            LogManager.Info("BaslerViewModel.cs: InitializeParams(): Begin");
            try
            {
                this.InitializePylon();
                this.InitializeImageProvider();
                this.InitializeLiveViewControl();
                this.Calibrate();
            }
            catch (Exception ex)
            {
                LogManager.Error("BaslerViewModel.cs: InitializeParams(). " + ex.Message);
                throw;
            }

            LogManager.Info("BaslerViewModel.cs: InitializeParams(): End");
        }

        /// <summary>
        /// Initialize Pylon environment.
        /// </summary>
        private void InitializePylon()
        {
            try
            {
                if (!isInitPylon)
                {
                    LogManager.Info("BaslerViewModel.cs: InitializePylon()");
                    Pylon.Initialize();
                    isInitPylon = true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("BaslerViewModel.cs: InitializePylon(). " + ex.Message);
                try
                {
                    Pylon.Terminate();
                }
                catch (Exception)
                {
                    throw;
                }

                throw ex;
            }
        }

        /// <summary>
        /// Register for the events of the image provider needed for proper operation..
        /// </summary>
        private void InitializeImageProvider()
        {
            LogManager.Info("BaslerViewModel.cs: InitializeImageProvider(): Begin");
            try
            {
                if (!this.isInitImageProvider)
                {
                    this.imageProvider = new ImageProvider();
                    this.imageProvider.GrabErrorEvent += new ImageProvider.GrabErrorEventHandler(this.OnGrabErrorEventCallback);
                    this.imageProvider.DeviceRemovedEvent += new ImageProvider.DeviceRemovedEventHandler(this.OnDeviceRemovedEventCallback);
                    this.imageProvider.DeviceOpenedEvent += new ImageProvider.DeviceOpenedEventHandler(this.OnDeviceOpenedEventCallback);
                    this.imageProvider.DeviceClosedEvent += new ImageProvider.DeviceClosedEventHandler(this.OnDeviceClosedEventCallback);
                    this.imageProvider.GrabbingStartedEvent += new ImageProvider.GrabbingStartedEventHandler(this.OnGrabbingStartedEventCallback);
                    this.imageProvider.ImageReadyEvent += new ImageProvider.ImageReadyEventHandler(this.OnImageReadyEventCallback);
                    this.imageProvider.GrabbingStoppedEvent += new ImageProvider.GrabbingStoppedEventHandler(this.OnGrabbingStoppedEventCallback);
                    this.isInitImageProvider = true;
                }
            }
            catch (Exception ex)
            {
                Pylon.Terminate();
                LogManager.Error("BaslerViewModel.cs: InitializeImageProvider(). " + ex.Message);
                throw ex;
            }

            LogManager.Info("BaslerViewModel.cs: InitializeImageProvider(): End");
        }

        /// <summary>
        /// Initialize live view control.
        /// </summary>
        private void InitializeLiveViewControl()
        {
            LogManager.Info("BaslerViewModel.cs: InitializeLiveViewControl(): Begin");
            try
            {
                this.CanShowAxis = false;
                this.CanShowRoi = false;
                this.CanShowRuler = false;
            }
            catch (Exception ex)
            {
                LogManager.Error("BaslerViewModel.cs: InitializeLiveViewControl(). " + ex.Message);
                throw ex;
            }

            LogManager.Info("BaslerViewModel.cs: InitializeLiveViewControl(): End");
        }
        #endregion

        #region Camera controls
        /// <summary>
        /// Connect to the device.
        /// </summary>
        private void Connect()
        {
            LogManager.Info("BaslerViewModel.cs: Connect()");
            try
            {
                this.Open();
            }
            catch (Exception ex)
            {
                LogManager.Error("BaslerViewModel.cs: Connect(). " + ex.Message);
                throw ex;
            }
            //LogManager.Info("BaslerViewModel.cs: Connect(): End");
        }

        /// <summary>
        /// Disconnect the device.
        /// </summary>
        private void Disconnect()
        {
            LogManager.Info("BaslerViewModel.cs: Disconnect(): Begin");
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                LogManager.Error("BaslerViewModel.cs: Disconnect(). " + ex.Message);
                throw ex;
            }

            LogManager.Info("BaslerViewModel.cs: Disconnect(): End");
        }

        /// <summary>
        /// Calibrate camera.
        /// </summary>
        private void Calibrate()
        {
            if (!isCalibrated)
            {
                LogManager.Info("BaslerViewModel.cs: Calibrate()");
                PYLON_DEVICE_HANDLE device = null;
                try
                {
                    // Must Enumerate all camera devices before creating a device.
                    uint numDevices = Pylon.EnumerateDevices();
                    if (numDevices > 0)
                    {
                        // Get a handle for the first device found. 
                        device = Pylon.CreateDeviceByIndex(0);

                        // Open device to configuring parameters
                        Pylon.DeviceOpen(device, Pylon.cPylonAccessModeControl | Pylon.cPylonAccessModeStream);

                        cameraWidth = int.Parse(this.GetConfig(device, "Width"));
                        cameraHeight = int.Parse(this.GetConfig(device, "Height"));

                        // Check to see if the Mono8 pixel format can be set
                        if (!Pylon.DeviceFeatureIsAvailable(device, "EnumEntry_PixelFormat_Mono8"))
                        {
                            throw new Exception("Invalid camera pixel format.");
                        }

                        // Set pixel format
                        this.SetConfig(device, "PixelFormat", "Mono8");
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error("BaslerViewModel.cs: Calibrate(). Error when opening and get config from camera." + ex.Message);
                    throw ex;
                }
                finally
                {
                    try
                    {
                        // Clean up. Close and release the pylon device.
                        if (Pylon.DeviceIsOpen(device))
                        {
                            Pylon.DeviceClose(device);
                        }

                        Pylon.DestroyDevice(device);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error("BaslerViewModel.cs: Calibrate(). Error when closing and destroying device." + ex.Message);
                        throw ex;
                    }
                }

                isCalibrated = true;
            }
        }

        /// <summary>
        /// Set camera config.
        /// </summary>
        /// <param name="device">camera device</param>
        /// <param name="featureName">name of config</param>
        /// <param name="value">value of config</param>
        private void SetConfig(PYLON_DEVICE_HANDLE device, string featureName, object value)
        {
            try
            {
                LogManager.Info("BaslerViewModel.cs: SetConfig(): Begin");
                if (value == null)
                {
                    return;
                }

                // Check to see if a feature is implemented, writable.
                bool isAvailable;
                bool isWritable;

                // Check feature 
                isAvailable = Pylon.DeviceFeatureIsImplemented(device, featureName);
                isWritable = Pylon.DeviceFeatureIsWritable(device, featureName);

                // Set config feature 
                if (isAvailable && isWritable)
                {
                    if (value.GetType() == typeof(int))
                    {
                        Pylon.DeviceSetIntegerFeature(device, featureName, (int)value);
                    }

                    if (value.GetType() == typeof(float))
                    {
                        Pylon.DeviceSetFloatFeature(device, featureName, (float)value);
                    }

                    if (value.GetType() == typeof(string))
                    {
                        Pylon.DeviceFeatureFromString(device, featureName, (string)value);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("BaslerViewModel.cs: SetConfig()" + ex.Message);
                throw ex;
            }

            LogManager.Info("BaslerViewModel.cs: SetConfig(): End");
        }

        /// <summary>
        /// Get camera config
        /// </summary>
        /// <param name="device">camera device</param>
        /// <param name="featureName">feature name</param>
        /// <returns>feature value</returns>
        private string GetConfig(PYLON_DEVICE_HANDLE device, string featureName)
        {
            string value = string.Empty;
            try
            {
                LogManager.Info("BaslerViewModel.cs: GetConfig(): Begin");
                value = Pylon.DeviceFeatureToString(device, featureName);
            }
            catch (Exception ex)
            {
                LogManager.Error("BaslerViewModel.cs: GetConfig()" + ex.Message);
                throw ex;
            }

            LogManager.Info("BaslerViewModel.cs: GetConfig(): End");
            return value;
        }

        /// <summary>
        /// Start live view stream.
        /// </summary>
        private void ContinuousShot()
        {
            try
            {
                LogManager.Info("BaslerViewModel.cs: ContinuousShot(): Begin");
                this.imageProvider.ContinuousShot();
            }
            catch (Exception ex)
            {
                LogManager.Error("BaslerViewModel.cs: ContinuousShot()" + ex.Message);
                throw ex;
            }

            LogManager.Info("BaslerViewModel.cs: ContinuousShot(): End");
        }

        /// <summary>
        /// Stop live view stream.
        /// </summary>
        private void StopShot()
        {
            try
            {
                LogManager.Info("BaslerViewModel.cs: StopShot(): Begin");
                // Release image buffer in case of without live view
                if (this.Frame == null)
                {
                    this.imageProvider.ReleaseImage();
                }

                this.imageProvider.Stop();
            }
            catch (Exception ex)
            {
                LogManager.Error("BaslerViewModel.cs: StopShot()" + ex.Message);
                throw ex;
            }

            LogManager.Info("BaslerViewModel.cs: StopShot(): End");
        }

        /// <summary>
        /// Open the image provider with device index.
        /// </summary>
        private void Open()
        {
            try
            {
                if (!this.isConnected)
                {
                    LogManager.Info("BaslerViewModel.cs: Open()");
                    if (!IsReadyDevice(false))
                    {
                        LogManager.Error("BaslerViewModel.cs: Open(). Cannot find device.");
                        throw new Exception("Cannot find device.");
                    }

                    if (list.Count > 0)
                    {
                        this.imageProvider.Open(list[cameraIndex].Index);
                    }
                    else
                    {
                        throw new Exception("Cannot open device");
                    }

                    this.isConnected = true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("BaslerViewModel.cs: Open()" + ex.Message);
                throw ex;
            }

            LogManager.Info("BaslerViewModel.cs: Open()");
        }

        /// <summary>
        /// Close the image provider.
        /// </summary>
        private void Close()
        {
            try
            {
                LogManager.Info("BaslerViewModel.cs: Close(): Begin");
                if (this.isConnected)
                {
                    this.imageProvider.Close();
                    this.isConnected = false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("BaslerViewModel.cs: Close()" + ex.Message);
                throw ex;
            }

            LogManager.Info("BaslerViewModel.cs: Close(): End");
        }

        /// <summary>
        /// Take one shot from camera device.
        /// </summary>
        private void OneShot()
        {
            try
            {
                LogManager.Info("BaslerViewModel.cs: OneShot(): Begin");
                this.imageProvider.OneShot();
            }
            catch (Exception ex)
            {
                LogManager.Error("BaslerViewModel.cs: OneShot()" + ex.Message);
                throw ex;
            }

            LogManager.Info("BaslerViewModel.cs: OneShot(): End");
        }
        #endregion

        #region Register event handler for live view control
        /// <summary>
        /// Handles the event related to the occurrence of an error while grabbing proceeds.
        /// </summary>
        /// <param name="grabException">grab exception</param>
        /// <param name="additionalErrorMessage">error message</param>
        private void OnGrabErrorEventCallback(Exception grabException, string additionalErrorMessage)
        {
            if (this.Frame == null)
            {
                return;
            }

            this.Frame.Dispatcher.BeginInvoke(
            new Action(() =>
            {
                try
                {
                    LogManager.Error("Grabbing frame from camera");
                    this.imageProvider.Stop();
                    this.imageProvider.ContinuousShot();
                }
                catch (Exception ex)
                {
                    LogManager.Error("BaslerViewModel.cs: OnGrabErrorEventCallback(). " + ex.Message);
                }
            }),
            System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>
        ///  Handles the event related to the removal of a currently open device. 
        /// </summary>
        private void OnDeviceRemovedEventCallback()
        {
            if (this.Frame == null)
            {
                return;
            }

            this.Frame.Dispatcher.BeginInvoke(
            new Action(() =>
            {
                try
                {
                    LogManager.Error("Camera device is removed.");
                    this.isConnected = false;
                    this.isStarted = false;
                }
                catch (Exception ex)
                {
                    LogManager.Error("BaslerViewModel.cs: OnDeviceRemovedEventCallback(). " + ex.Message);
                }
            }),
            System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Handles the event related to a device being open.
        /// </summary>
        private void OnDeviceOpenedEventCallback()
        {
            if (this.Frame == null)
            {
                return;
            }
        }

        /// <summary>
        /// Handles the event related to a device being closed.
        /// </summary>
        private void OnDeviceClosedEventCallback()
        {
            if (this.Frame == null)
            {
                return;
            }
        }

        /// <summary>
        /// Handles the event related to the image provider executing grabbing. 
        /// </summary>
        private void OnGrabbingStartedEventCallback()
        {
            if (this.Frame == null)
            {
                return;
            }
        }

#if CameraLFR
        /// <summary>
        /// Handles the event related to an image having been taken and waiting for processing.
        /// </summary>
        private void OnImageReadyEventCallback()
        {
            if (this.Frame == null)
            {
                return;
            }

            this.Frame.Dispatcher.BeginInvoke(
            new Action(() =>
            {
                try
                {
                    // Acquire the image from the image provider. Only show the latest image. The camera may acquire images faster than images can be displayed
                    ImageProvider.Image lastestFrame = this.imageProvider.GetLatestImage();

                    // Check if the image has been removed in the meantime. 
                    if (lastestFrame != null)
                    {
                        // Check if the image is compatible with the currently used bitmap or a new bitmap is required.
                        if (BitmapFactory.IsCompatible(this.lastestBitmap, lastestFrame.Width, lastestFrame.Height, lastestFrame.Color))
                        {
                            // Update the bitmap with the image data. 
                            BitmapFactory.UpdateBitmap(this.lastestBitmap, lastestFrame.Buffer, lastestFrame.Width, lastestFrame.Height, lastestFrame.Color);
                        }
                        else
                        {
                            BitmapFactory.CreateBitmap(out this.lastestBitmap, lastestFrame.Width, lastestFrame.Height, lastestFrame.Color);
                            BitmapFactory.UpdateBitmap(this.lastestBitmap, lastestFrame.Buffer, lastestFrame.Width, lastestFrame.Height, lastestFrame.Color);
                        }

                        // Rotate bitmap
                        this.lastestBitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);

                        Image frameToDisplay = null;

                        if (this.ZoomRatio <= 1.0)
                        {
                            frameToDisplay = this.lastestBitmap;
                        }
                        else
                        {
                            float newWidth = (float)(this.lastestBitmap.Width / this.ZoomRatio);
                            float newHeight = (float)(this.lastestBitmap.Height / this.ZoomRatio);
                            frameToDisplay = LMUtils.CropBitmap(
                                this.lastestBitmap,
                                new RectangleF((this.lastestBitmap.Width - newWidth) / 2.0f, (this.lastestBitmap.Height - newHeight) / 2.0f, newWidth, newHeight));
                        }

                        // Resize image before display to save resource
                        frameToDisplay = LMUtils.ResizeImage(frameToDisplay, (int)(frameToDisplay.Width / this.scale), (int)(frameToDisplay.Height / this.scale));

                        this.LiveViewImage = LMUtils.ImageToByte(frameToDisplay);

                        // The processing of the image is done. Release the image buffer. 
                        this.imageProvider.ReleaseImage();
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error("BaslerViewModel.cs: OnImageReadyEventCallback(). " + ex.Message);
                }
            }),
            System.Windows.Threading.DispatcherPriority.Loaded);
        }
#else
        /// <summary>
        /// Handles the event related to an image having been taken and waiting for processing.
        /// </summary>
        private void OnImageReadyEventCallback()
        {
            if (this.Frame == null)
            {
                // Run camera without live view
                return;
            }

            //this.Frame.Dispatcher.BeginInvoke(

            System.Threading.Tasks.Task.Run(() =>
            {
                //LogManager.Info("BaslerViewModel.cs: OnImageReadyEventCallback(). BEGIN");
                try
                {
                    // Acquire the image from the image provider. Only show the latest image. The camera may acquire images faster than images can be displayed
                    ImageProvider.Image lastestFrame = this.imageProvider.GetLatestImage();

                    // Check if the image has been removed in the meantime. 
                    if (lastestFrame != null)
                    {
                        Bitmap lastestBitmap = null;
                        // Check if the image is compatible with the currently used bitmap or a new bitmap is required.
                        if (BitmapFactory.IsCompatible(lastestBitmap, lastestFrame.Width, lastestFrame.Height, lastestFrame.Color))
                        {
                            // Update the bitmap with the image data. 
                            BitmapFactory.UpdateBitmap(lastestBitmap, lastestFrame.Buffer, lastestFrame.Width, lastestFrame.Height, lastestFrame.Color);
                        }
                        else
                        {
                            BitmapFactory.CreateBitmap(out lastestBitmap, lastestFrame.Width, lastestFrame.Height, lastestFrame.Color);
                            BitmapFactory.UpdateBitmap(lastestBitmap, lastestFrame.Buffer, lastestFrame.Width, lastestFrame.Height, lastestFrame.Color);
                        }

                        // Rotate bitmap
                        lastestBitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);

                        Image frameToDisplay = null;

                        if (this.ZoomRatio <= 1.0)
                        {
                            frameToDisplay = lastestBitmap;
                        }
                        else
                        {
                            float newWidth = (float)(lastestBitmap.Width / this.ZoomRatio);
                            float newHeight = (float)(lastestBitmap.Height / this.ZoomRatio);
                            frameToDisplay = LMUtils.CropBitmap(
                                lastestBitmap,
                                new RectangleF((lastestBitmap.Width - newWidth) / 2.0f, (lastestBitmap.Height - newHeight) / 2.0f, newWidth, newHeight));
                        }

                        // Resize image before display to save resource
                        frameToDisplay = LMUtils.ResizeImage(frameToDisplay, (int)(frameToDisplay.Width / this.scale), (int)(frameToDisplay.Height / this.scale));

                        this.LiveViewImage = LMUtils.ImageToByte(frameToDisplay);

                        // The processing of the image is done. Release the image buffer. 
                        this.imageProvider.ReleaseImage();
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error("BaslerViewModel.cs: OnImageReadyEventCallback(). " + ex.Message);
                }

                //LogManager.Info("BaslerViewModel.cs: OnImageReadyEventCallback(). END");
            });
            //, System.Windows.Threading.DispatcherPriority.Loaded);

        }
#endif







        /// <summary>
        ///  Handles the event related to the image provider having stopped grabbing. 
        /// </summary>
        private void OnGrabbingStoppedEventCallback()
        {
            if (this.Frame == null)
            {
                return;
            }
        }
        #endregion
    }
}
