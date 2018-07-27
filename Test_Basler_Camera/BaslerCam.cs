using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;
using System.IO;

namespace Test_Basler_Camera
{
    public partial class BaslerCam : Form
    {
        public BaslerSDK cam;
        Bitmap display;
        bool isRunning = false;
        ZXingReader reader;
        HalconReader halconReader;
        float fScaleX, fScaleY;
        PointF[] ZXingPt, HalconPt;
        bool bDrawCode = false;
        bool bHalconDrawCode = false;
        public BaslerCam()
        {
            InitializeComponent();
            reader = new ZXingReader();
            halconReader = new HalconReader();
            cam = new BaslerSDK();
            cam.InitializePylon();
            cam.InitializeImageProvider();
            cam.Calibrate(1200,800,false,true,20);
            //pbMain.Image = display;
            ZXingPt = new PointF[2];
            HalconPt = new PointF[4];
            fScaleX = (float)1200 / (float)pbMain.Width;
            fScaleY = (float)800 / (float)pbMain.Height;
            //cam.Stop();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            if (bgrWorker.IsBusy != true)
            {
                isRunning = true;
                bgrWorker.RunWorkerAsync();
            }
        }

        private void pbMain_Paint(object sender, PaintEventArgs e)
        {
            if (bDrawCode)
            {
                e.Graphics.DrawLine(new Pen(Color.Orange, 2f), ZXingPt[0].X / fScaleX, ZXingPt[0].Y / fScaleY, ZXingPt[1].X / fScaleX, ZXingPt[1].Y / fScaleY);
                bDrawCode = false;
            }

            if (bHalconDrawCode)
            {
                e.Graphics.DrawLine(new Pen(Color.Blue, 2f), HalconPt[0].X / fScaleX, HalconPt[0].Y / fScaleY, HalconPt[1].X / fScaleX, HalconPt[1].Y / fScaleY);
                e.Graphics.DrawLine(new Pen(Color.Blue, 2f), HalconPt[1].X / fScaleX, HalconPt[1].Y / fScaleY, HalconPt[2].X / fScaleX, HalconPt[2].Y / fScaleY);
                e.Graphics.DrawLine(new Pen(Color.Blue, 2f), HalconPt[2].X / fScaleX, HalconPt[2].Y / fScaleY, HalconPt[3].X / fScaleX, HalconPt[3].Y / fScaleY);
                e.Graphics.DrawLine(new Pen(Color.Blue, 2f), HalconPt[3].X / fScaleX, HalconPt[3].Y / fScaleY, HalconPt[0].X / fScaleX, HalconPt[0].Y / fScaleY);
                bHalconDrawCode = false;
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            if (bgrWorker.WorkerSupportsCancellation == true)
            {
                isRunning = false;
                bgrWorker.CancelAsync();
                cam.Stop();
            }
        }

        private void bgrWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            cam.Start();
            while (isRunning)
            {
                try
                {
                    display = cam.GetCurrentFrame();
                    if (display != null)
                    {
                        reader.Decode(display);
                        if (reader.IsFound)
                        {
                            ZXingPt[0] = new PointF(reader.resultPoints[0].X, reader.resultPoints[0].Y);
                            ZXingPt[1] = new PointF(reader.resultPoints[1].X, reader.resultPoints[1].Y);
                            bDrawCode = true;
                        }

                        halconReader.Decode(display);
                        if (halconReader.IsFound)
                        {
                            HalconPt[0] = halconReader.resultPoints[0];
                            HalconPt[1] = halconReader.resultPoints[1];
                            HalconPt[2] = halconReader.resultPoints[2];
                            HalconPt[3] = halconReader.resultPoints[3];
                            bHalconDrawCode = true;
                        }

                        this.Invoke(new Action(() =>
                        {
                            //labelStatus.Text = "FPS: " + fps;
                            // labelStatus.Text = "time: " + sw.ElapsedMilliseconds.ToString();
                            pbMain.Image = display;
                            pbMain.Refresh();
                            labelStatus.Text = reader.Time + " ms\n " + reader.Result;
                            labelStatus2.Text = halconReader.Time + " ms\n " + halconReader.Result;
                        }));
                    }
                }
                catch (Exception ex)
                {
                    cam.Stop();
                }
                //Thread.Sleep(50);
            }
        }

        private void bgrWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int a = 0;
        }

        private void BaslerCam_FormClosed(object sender, FormClosedEventArgs e)
        {
            isRunning = false;
            bgrWorker.CancelAsync();
            cam.Stop();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Bitmap saveImg = (Bitmap)pbMain.Image;
            SaveFileDialog saveFileDlg = new SaveFileDialog();
            saveFileDlg.Title = "Save image";
            saveFileDlg.Filter = "Jpeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDlg.OverwritePrompt = true;
            if (saveFileDlg.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDlg.FileName;
                if (File.Exists(fileName))
                    File.Delete(fileName);

                switch (saveFileDlg.FilterIndex)
                {
                    case 1:
                        saveImg.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case 2:
                        saveImg.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                    case 3:
                        saveImg.Save(fileName, System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                }
                saveImg.Dispose();
            }
            else
                saveImg.Dispose();
        }
    }
}
