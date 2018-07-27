using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Test_Basler_Camera
{
    public class HalconReader
    {
        HImage source;
        public string Result;
        public PointF[] resultPoints;
        public bool IsFound = false;
        public int Time;

        public HalconReader()
        {
            resultPoints = new PointF[4];
        }
        public void Decode(Bitmap input)
        {
            Bitmap2HImage(input);

            HObject ho_SymbolRegions = null;
            HOperatorSet.GenEmptyObj(out ho_SymbolRegions);
            ho_SymbolRegions.Dispose();

            HTuple hv_BarCodeHandle = null, hv_DecodedDataStrings = new HTuple();
            HOperatorSet.CreateBarCodeModel(new HTuple(), new HTuple(), out hv_BarCodeHandle);

            //HObject test_inv;
            //HOperatorSet.InvertImage(test, out test_inv);
            //HOperatorSet.FindBarCode(test_inv, out ho_SymbolRegions, hv_BarCodeHandle, "auto", out hv_DecodedDataStrings);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                HOperatorSet.FindBarCode(this.source, out ho_SymbolRegions, hv_BarCodeHandle, "auto", out hv_DecodedDataStrings);
            }
            catch (Exception)
            {
            }
            watch.Stop();
            Time = (int)watch.ElapsedMilliseconds;

            HTuple hv_Reference = new HTuple(), hv_String = new HTuple(), hv_J = new HTuple(), hv_Char = new HTuple();

            if (hv_DecodedDataStrings.Length > 0)
            {
                IsFound = true;
                using (HRegion region = new HRegion(ho_SymbolRegions))
                {
                    HTuple row = new HTuple(), column = new HTuple(), phi = new HTuple(), length1 = new HTuple(), length2 = new HTuple();
                    region.SmallestRectangle2(out row, out column, out phi, out length1, out length2);

                    for (int i = 0; i < 1; i++)
                    {
                        double angle = phi.DArr[i] / 0.0174532F;
                        // if (angle < 1)
                        // {
                        int x = (int)(column.DArr[i] - length1.DArr[i]);
                        int y = (int)(row.DArr[i] - length2.DArr[i]);
                        int width = (int)(2 * length1.DArr[i]);
                        int height = (int)(2 * length2.DArr[i]);
                        Rectangle roi = new Rectangle(x, y, width, height);

                        resultPoints[0] = new PointF(roi.Left, roi.Top);
                        resultPoints[1] = new PointF(roi.Right, roi.Top);
                        resultPoints[2] = new PointF(roi.Right, roi.Bottom);
                        resultPoints[3] = new PointF(roi.Left, roi.Bottom);

                        //HOperatorSet.GetBarCodeResult(hv_BarCodeHandle, 0, "decoded_types", out hv_Reference);
                        Result = hv_DecodedDataStrings[0].S;
                    }

                }
            }
            else
            {
                IsFound = false;
            }

            HOperatorSet.ClearBarCodeModel(hv_BarCodeHandle);
            //test.Dispose();
            ho_SymbolRegions.Dispose();
        }

        private void Bitmap2HImage(Bitmap input)
        {
            IntPtr ptr = IntPtr.Zero;
            ptr = BitmapToIntPtr(input);
            SetImagePtr(input.Width, input.Height, ptr);
        }
        private IntPtr BitmapToIntPtr(Bitmap bitmap)
        {
            IntPtr pointer = IntPtr.Zero;
            BitmapData bitmapData = null;
            try
            {
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
                pointer = bitmapData.Scan0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                try
                {
                    if (bitmapData == null)
                    {
                    }

                    bitmap.UnlockBits(bitmapData);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }

            return pointer;
        }

        public void SetImagePtr(int width, int height, IntPtr imagePtr)
        {
            try
            {
                // HImage temp = new HImage("byte", Stride(width), height, IntPtr.Zero);
                HImage temp = new HImage("byte", Stride(width), height, imagePtr);
                this.source = temp.CropPart(0, 0, width, height);
                temp.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private int Stride(int nWidth)
        {
            return 4 * (1 + (nWidth - 1) / 4);
        }
        public Bitmap ResizeBitmap(Bitmap original, int newWidth, int newHeight)
        {
            Bitmap bitmap = null;
            try
            {
                bitmap = new Bitmap(newWidth, newHeight);
                using (Graphics gr = Graphics.FromImage(bitmap))
                {
                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    gr.DrawImage(original, new Rectangle(0, 0, newWidth, newHeight));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bitmap;
        }
    }
}
