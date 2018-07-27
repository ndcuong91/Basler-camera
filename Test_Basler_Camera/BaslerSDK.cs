using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PylonC.NET;
using PylonC.NETSupportLibrary;
using System.Windows;
using System.Drawing;
using OpenCvSharp;

namespace Test_Basler_Camera
{
    public class BaslerSDK
    {

        /// <summary>
        /// Check if Pylon sdk is initialized
        /// </summary>
        private bool isInitImageProvider = false;

        /// <summary>
        /// Check if camera is calibrated
        /// </summary>
        private static bool isCalibrated = false;

        /// <summary>
        /// Width resolution of camera image
        /// </summary>
        public static int cameraWidth = 1600;

        /// <summary>
        /// Height resolution of camera image
        /// </summary>
        /// 
        public static int cameraHeight = 1200;

        public string sFrameRate;


        /// <summary>
        /// Get list of camera device
        /// </summary>
        private static List<DeviceEnumerator.Device> list = null;

        /// <summary>
        /// Check if camera is connected
        /// </summary>
        private bool isConnected = false;

        /// <summary>
        /// To get frame
        /// </summary>
        private ImageProvider imageProvider;

        /// <summary>
        /// Camera index to connect
        /// </summary>
        private static int cameraIndex = 0;

        /// <summary>
        /// Check if camera has started
        /// </summary>
        private bool isStarted = false;

        /// <summary>
        /// Check if Pylon sdk is initialized
        /// </summary>
        private static bool isInitPylon = false;
        public Image Frame { get; set; }

        public void InitializeImageProvider()
        {
            try
            {
                if (!this.isInitImageProvider)
                {
                    this.imageProvider = new ImageProvider();
                    //this.imageProvider.GrabErrorEvent += new ImageProvider.GrabErrorEventHandler(this.OnGrabErrorEventCallback);
                    //this.imageProvider.DeviceRemovedEvent += new ImageProvider.DeviceRemovedEventHandler(this.OnDeviceRemovedEventCallback);
                    //this.imageProvider.DeviceOpenedEvent += new ImageProvider.DeviceOpenedEventHandler(this.OnDeviceOpenedEventCallback);
                    //this.imageProvider.DeviceClosedEvent += new ImageProvider.DeviceClosedEventHandler(this.OnDeviceClosedEventCallback);
                    //this.imageProvider.GrabbingStartedEvent += new ImageProvider.GrabbingStartedEventHandler(this.OnGrabbingStartedEventCallback);
                    //this.imageProvider.ImageReadyEvent += new ImageProvider.ImageReadyEventHandler(this.OnImageReadyEventCallback);
                    //this.imageProvider.GrabbingStoppedEvent += new ImageProvider.GrabbingStoppedEventHandler(this.OnGrabbingStoppedEventCallback);
                    this.isInitImageProvider = true;
                }
            }
            catch (Exception ex)
            {
                Pylon.Terminate();
                throw ex;
            }
        }
        /// 
        /// <summary>
        /// Start live view camera.
        /// </summary>
        public void Start()
        {
            try
            {
                if (!this.isStarted)
                {
                    this.Connect();
                    this.ContinuousShot();
                    this.isStarted = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Connect to the device.
        /// </summary>
        /// 
        private void Connect()
        {
            try
            {
                this.Open();
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                    if (!IsReadyDevice(false))
                    {
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
                throw ex;
            }

        }

        /// <summary>
        /// Start live view stream.
        /// </summary>
        private void ContinuousShot()
        {
            try
            {
                this.imageProvider.ContinuousShot();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Close the image provider.
        /// </summary>
        private void Close()
        {
            try
            {
                if (this.isConnected)
                {
                    this.imageProvider.Close();
                    this.isConnected = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        #region Private static methods
        /// <summary>
        /// Check if device is ready
        /// </summary>
        /// <returns>true if device is ready</returns>
        /// <param name="isUpdateListDevice">true if update list of devices</param>
        private static bool IsReadyDevice(bool isUpdateListDevice)
        {
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
                    return false;
                }

                PYLON_DEVICE_HANDLE device = null;
                BaslerParams baslerParams = new BaslerParams();

                for (int i = 0; i < list.Count; i++)
                {
                    // Check ip address
                    try
                    {
                        device = Pylon.CreateDeviceByIndex((uint)i);
                        string ipAddress = Pylon.DeviceInfoGetPropertyValueByName(Pylon.DeviceGetDeviceInfoHandle(device), Pylon.cPylonDeviceInfoIpAddressKey);
                        if (string.Compare(baslerParams.IpAddress, ipAddress) == 0)
                        {
                            cameraIndex = i;
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        isReady = false;
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
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isReady = false;
            }

            return isReady;
        }

        /// <summary>
        /// Initialize Pylon environment.
        /// </summary>
        public void InitializePylon()
        {
            try
            {
                if (!isInitPylon)
                {
                    Pylon.Initialize();
                    isInitPylon = true;
                }
            }
            catch (Exception ex)
            {
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
        #endregion


        /// <summary>
        /// Acquire the image from the image provider.
        /// </summary>
        /// <returns>current image</returns>
        public Bitmap GetCurrentFrame()
        {
            Bitmap currentFrame = null;
            try
            {
                int timeoutFrame = Environment.TickCount;
                ImageProvider.Image lastestFrame = null;
                while (lastestFrame == null)
                {
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

                currentFrame = tmpFrame;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return currentFrame;
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
                    this.StopShot();
                    this.Disconnect();
                    this.isStarted = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Stop live view stream.
        /// </summary>
        private void StopShot()
        {
            try
            {
                // Release image buffer in case of without live view
                //if (this.frame == null)
                this.imageProvider.ReleaseImage();
                this.imageProvider.Stop();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Disconnect the device.
        /// </summary>
        private void Disconnect()
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        /// <summary>
        /// Calibrate intrinsic parameters.
        /// </summary>
        public void Calibrate(int w = 1200, int h = 800, bool color = false, bool setFrameRate = false, int frameRate = 2)
        {
            if (!isCalibrated)
            {
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

                        // Check to see if the Mono8 pixel format can be set
                        if (!Pylon.DeviceFeatureIsAvailable(device, "EnumEntry_PixelFormat_Mono8"))
                        {
                            throw new Exception("Invalid camera pixel format.");
                        }
                        string width = "1980";
                        string height = "1080";
                        int xOffset = (int.Parse(width) - w) / 2;
                        int yOffset = (int.Parse(height) - h) / 2;
                        this.SetConfig(device, "Width", w);
                        this.SetConfig(device, "Height", h);
                        this.SetConfig(device, "XOffset", xOffset);
                        this.SetConfig(device, "YOffset", yOffset);
                        this.SetConfig(device, "AcquisitionFrameRateEnable", "1");
                        // string fps = this.GetConfig(device, "ResultingFrameRateAbs");
                        if (setFrameRate)
                        {
                            this.SetConfig(device, "AcquisitionFrameRateAbs", frameRate.ToString());
                        }

                        if (!color)
                            this.SetConfig(device, "PixelFormat", "Mono8");

                    }
                }
                catch (Exception ex)
                {
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
                throw ex;
            }

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
                value = Pylon.DeviceFeatureToString(device, featureName);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return value;
        }

        public string GetCurrentFPS()
        {
            //PYLON_DEVICE_HANDLE device = null;
            try
            {
                // Must Enumerate all camera devices before creating a device.
                //uint numDevices = Pylon.EnumerateDevices();
                //if (numDevices > 0)
                //{
                //    // Get a handle for the first device found. 
                //    device = Pylon.CreateDeviceByIndex(0);
                //    this.SetConfig(device, "AcquisitionFrameRateEnable", "1");
                //string fps = this.GetConfig(device, "ResultingFrameRateAbs");
                return "";
                //}

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                try
                {
                    // Clean up. Close and release the pylon device.
                    //if (Pylon.DeviceIsOpen(device))
                    //{
                    //    Pylon.DeviceClose(device);
                    //}

                    //Pylon.DestroyDevice(device);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "";
        }
    }
}
