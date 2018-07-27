using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace Test_Basler_Camera
{
    public class ZXingReader
    {
        public string Result;
        public ResultPoint[] resultPoints;
        public bool IsFound = false;
        public int Time;
        public ZXingReader()
        {

        }
        public void Decode(Bitmap input)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            IBarcodeReader reader = new BarcodeReader();
           // IBarcodeWriter writer
            var result = reader.Decode(input);
            
            if (result != null)
            {
                IsFound = true;
                Result = result.Text;
                resultPoints = result.ResultPoints;
            }
            else
            {
                IsFound = false;
                Result = "Cannot decode!";
                resultPoints = new ResultPoint[0];
            }
            sw.Stop();
            Time = (int)sw.ElapsedMilliseconds;
        }
    }
}
