namespace Test_Basler_Camera
{
    /// <summary>
    /// BaslerCameraParams class to define params for Basler camera
    /// </summary>
    public class BaslerParams
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the BaslerParams class
        /// </summary>
        public BaslerParams()
        {
        }
        #endregion

        #region Properties

        /// <summary>
        /// IP address
        /// </summary>
        //public string IpAddress { get; set; } = "10.92.200.201";
        public string IpAddress { get; set; } = "10.92.200.254";

        /// <summary>
        /// Subnet mask
        /// </summary>
        public string SubnetMask { get; set; } = "255.255.255.0";

        /// <summary>
        /// Default getway
        /// </summary>
        public string DefaultGetway { get; set; } = "0.0.0.0";

        /// <summary>
        /// Gain raw
        /// </summary>
        public int GainRaw { get; set; } = 51;

        /// <summary>
        /// Exposure time raw
        /// </summary>
        public int ExposureTimewRaw { get; set; } = 35000;

        /// <summary>
        /// Width of live view
        /// </summary>
        public int Width { get; set; } = 1602;

        /// <summary>
        /// Height of live view
        /// </summary>
        public int Height { get; set; } = 1202;
        #endregion
    }
}
