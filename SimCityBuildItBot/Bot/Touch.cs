namespace SimCityBuildItBot.Bot
{
    using Common.Logging;
    using Managed.Adb;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using Tesseract;
    using Emgu.CV;
    using Emgu.CV.CvEnum;
    using Emgu.CV.Structure;
    using System.Diagnostics;
    using System.Data;
    using System;
    public class Touch
    {
        private ConsoleOutputReceiver creciever = new ConsoleOutputReceiver();
        private Device device;
        private ILog log;
        public TesseractEngine engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        public Touch(ILog log)
        {
            this.log = log;

            device = MemuChooser.GetDevice();



        }
        public void Execute(string cmd)
        {
            device.ExecuteShellCommand(cmd, creciever);



        }


        public void GotoTradeDepot()
        {
            //650 500 650 0
            Execute("input touchscreen swipe 200 600 0 0");

            Execute("input touchscreen swipe 200 200 200 0");

            // BotApplication.Wait(1 * 1000);
            this.ClickAt(Bot.Location.TradeDepot);

        }
        // 950 237
        public void GotoCenter()
        {
            for (var i = 0; i < 4; i++)
            {

                GotoTopLeft();
            }


            Execute("input touchscreen swipe 500 610 100 110");
            BotApplication.Wait(2 * 1000);
        }
        public void Swipe(Point from, Point to)
        {
            Execute("input touchscreen swipe " + from.X + " " + from.Y + " " + to.X + " " + to.Y);

        }
        public void GotoTopLeft()
        {
            Execute("input touchscreen swipe 100 110 530 520");
            BotApplication.Wait(1 * 1000);
        }
        public Image Snapshot(string name = "snapshot")
        {
            Image image = device.Screenshot.ToImage();
            image.Save(@"E:\botscreensave\" + name + ".png");

            return image;
        }
        public Bitmap Snapshot(int x, int y, int width, int height, string name = "snapshot")
        {
            return Crop(x, y, width, height);


        }


        public bool CheckPixel(int x, int y, Color c)
        {
            Image image = device.Screenshot.ToImage();

            using (Bitmap bmp = new Bitmap(image))
            {
                Color clr = bmp.GetPixel(x, y); // Get the color of pixel at position 5,5
                log.DebugFormat("color at {0},{1} is {2} and brightness : {3:C2} ", x, y, clr, clr.GetBrightness());

                return clr.Equals(c);
            }

        }
        public double compareColor(Color c, Color k)
        {
            ColorMine.ColorSpaces.Rgb cRgb = new ColorMine.ColorSpaces.Rgb(c.R, c.G, c.B);
            ColorMine.ColorSpaces.Rgb kRgb = new ColorMine.ColorSpaces.Rgb(k.R, k.G, k.B);

            double delta = cRgb.Compare(kRgb, new ColorMine.ColorSpaces.Comparisons.Cie1976Comparison());
            log.Info("delta  :" + delta + " color : " + c + " and " + k);
            return delta;
        }
        public bool CheckPixel(int x, int y, Color c,int delta)
        {
            Image image = device.Screenshot.ToImage();

            using (Bitmap bmp = new Bitmap(image))
            {
                Color clr = bmp.GetPixel(x, y); // Get the color of pixel at position 5,5
                log.DebugFormat("color at {0},{1} is {2} and brightness : {3:C2} ", x, y, clr, clr.GetBrightness());
                
                return delta > compareColor(c, clr);
            }

        }
        // c = touch.GetPixel(428 + 146 * col, 260 + 182 * row);
        //delta = compareColor(c, Color.FromArgb(255, 255, 255, 255));
        public Color GetPixel(int x, int y)
        {
            Image image = device.Screenshot.ToImage();

            using (Bitmap bmp = new Bitmap(image))
            {
                Color clr = bmp.GetPixel(x, y); // Get the color of pixel at position 5,5
                log.DebugFormat("color at {0},{1} is {2}", x, y, clr, clr.GetBrightness());

                return clr;
            }

        }
        public int CheckFactorySlotStatus(int x, int y)
        {
            Image image = device.Screenshot.ToImage();

            using (Bitmap bmp = new Bitmap(image))
            {
                Color clr = bmp.GetPixel(x, y); // Get the color of pixel at position x,y
                log.DebugFormat("color at {0},{1} is  {2} ", x, y, clr);
                int sum = clr.R + clr.G + clr.B;
                //768,466,132

                if (sum > 600) return 1; // ready
                if (sum < 130) return 0; // empty
                return -1; //processing
            }

        }
        public int[] CheckFactoryAllSlotStatus()
        {
            int[]   rel = new int[5];
            //int[] rel = new int[4];
            Image image = device.Screenshot.ToImage();

            using (Bitmap bmp = new Bitmap(image))
            {
                
                for (int i = 0; i < 5; i++)
                {
                    int x = 358 + 142 * i;
                    int y = 675;
                    Color clr = bmp.GetPixel(x, y); // Get the color of pixel at position x,y
                    log.DebugFormat("color at {0},{1} is  {2} ", x, y, clr);
                    int sum = clr.R + clr.G + clr.B;
                    //768,466,132

                    if (sum > 600) rel[i] = 1; // ready
                    else if (sum < 130) rel[i] = 0; // empty
                    else rel[i] = -1;
                }
                return rel;
                /*

                for (int i = 0; i < 4; i++)
              {
                  int x = 429 + 142 * i;
                  int y = 675;
                  Color clr = bmp.GetPixel(x, y); // Get the color of pixel at position x,y
                  log.DebugFormat("color at {0},{1} is  {2} ", x, y, clr);
                  int sum = clr.R + clr.G + clr.B;
                  //768,466,132

                  if (sum > 600) rel[i] = 1; // ready
                  else if (sum < 130) rel[i] = 0; // empty
                  else rel[i] = -1;
              }
              return rel;
               */

            }

        }
        public int getAvaiableStorage()
        {
            Bitmap bitmap = Crop(352, 19, 75, 25);
            int rel = 0;
            using (var pix = new BitmapToPixConverter().Convert(bitmap))
            {
                using (var page = engine.Process(pix, PageSegMode.SingleLine))
                {
                    try
                    {
                        log.Info(page.GetText().Trim().ToLower());
                        rel = int.Parse(page.GetText().Trim().Split('/')[1]) - int.Parse(page.GetText().Trim().Split('/')[0]);

                    }
                    catch (System.Exception ex)
                    {
                        log.Info(ex);
                    }



                }
            }
            return rel;
        }
        public Bitmap Crop(int x, int y, int width, int height, string name = "croped")
        {
            Image image = device.Screenshot.ToImage();

            Rectangle cropRect = new Rectangle(x, y, width, height);
            Bitmap src = new Bitmap(image);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }
             target.Save(@"E:\botscreensave\" + name + ".png", System.Drawing.Imaging.ImageFormat.Png);
            return target;



        }
        //396 614







        public void sendRawEvent(string raw)
        {
            device.ExecuteShellCommand("sendevent /dev/input/event7 " + raw, creciever);
            //device.ExecuteShellCommand("sendevent /dev/input/event7 0003 0036 230", creciever);
            //device.ExecuteShellCommand("sendevent /dev/input/event7 0000 0002 00000000", creciever);
            //device.ExecuteShellCommand("sendevent /dev/input/event7 0000 0002 00000000", creciever);


        }
        public void TouchDown()
        {
            device.ExecuteShellCommand("sendevent /dev/input/event7 1 330 1", creciever);
            device.ExecuteShellCommand("sendevent /dev/input/event7 3 58 1", creciever);
        }

        public void MoveTo(Point point)
        {
            device.ExecuteShellCommand("sendevent /dev/input/event7 3 53 " + point.X, creciever);
            device.ExecuteShellCommand("sendevent /dev/input/event7 3 54 " + point.Y, creciever);
            EndTouchData();
        }

        public void TouchUp()
        {
            device.ExecuteShellCommand("sendevent /dev/input/event7 1 330 0", creciever);
            device.ExecuteShellCommand("sendevent /dev/input/event7 3 58 0", creciever);
        }

        public void EndTouchData()
        {
            device.ExecuteShellCommand("sendevent /dev/input/event7 0 2 0", creciever);
            device.ExecuteShellCommand("sendevent /dev/input/event7 0 0 0", creciever);
        }

        public void ClickAt(Location location)
        {
            ClickAt(location, 400);
        }

        public void ClickAt(Location location, int waitms)
        {
            //log.Info("clicking " + location.ToString());

            var point = Constants.GetPoint(location);

            //this.MoveTo(point);

            System.Diagnostics.Debug.WriteLine("Clicking " + location.ToString());

            ClickAt(point, waitms);
        }

        public void LongPress(Location location, int seconds)
        {
            var point = Constants.GetPoint(location);

            this.TouchDown();
            this.MoveTo(point);

            BotApplication.Wait(seconds * 1000);

            this.TouchUp();
            this.EndTouchData();

            BotApplication.Wait(1000);
        }

        public void ClickAt(Point point)
        {
            log.Info("Click at " + point.ToString());

            ClickAt(point, 400);
        }
        public void ClickAtAndHold(Point point, int waitms)
        {
            this.MoveTo(point);
            this.TouchDown();
            this.MoveTo(point);
            BotApplication.Wait(waitms);
            this.TouchUp();
            this.EndTouchData();


        }
        public void ClickAt(Point point, int waitms)
        {
            this.TouchDown();
            this.MoveTo(point);

            this.TouchUp();
            this.EndTouchData();

            BotApplication.Wait(waitms);
        }
        public int FindLine()
        {
            var bmp = this.Crop(360, 300, 145, 30);


            //Load the image from file and resize it for display
            Image<Bgr, Byte> img =
               new Image<Bgr, byte>(bmp);
            ;
           
            //Convert the image to grayscale and filter out the noise
            UMat uimage = new UMat();
            CvInvoke.CvtColor(img, uimage, ColorConversion.Bgr2Gray);

            //use image pyr to remove noise
            UMat pyrDown = new UMat();
            CvInvoke.PyrDown(uimage, pyrDown);
            CvInvoke.PyrUp(pyrDown, uimage);

            //Image<Gray, Byte> gray = img.Convert<Gray, Byte>().PyrDown().PyrUp();

          
            Stopwatch watch = Stopwatch.StartNew();
            double cannyThreshold = 180.0;






           

            double cannyThresholdLinking = 120.0;
            UMat cannyEdges = new UMat();
            CvInvoke.Canny(uimage, cannyEdges, cannyThreshold, cannyThresholdLinking);

            LineSegment2D[] lines = CvInvoke.HoughLinesP(
               cannyEdges,
               1, //Distance resolution in pixel-related units
               Math.PI, //Angle resolution measured in radians.
               20, //threshold
               6, //min Line width
               106); //gap between lines

            return lines[0].P1.X;

        }
            
        public void Swipe(Location downAt, Location from, Location to, int steps, bool touchUpAtTouchDownLocation)
        {
            var pointdownAt = Constants.GetPoint(downAt);
            var pointFrom = Constants.GetPoint(from);
            var pointTo = Constants.GetPoint(to);

            Swipe(pointdownAt, pointFrom, pointTo, steps, touchUpAtTouchDownLocation);
        }

        public void Swipe(Point pointdownAt, Point pointFrom, Point pointTo, int steps, bool touchUpAtTouchDownLocation)
        {
            var xStep = (pointTo.X - pointFrom.X) / steps;
            var yStep = (pointTo.Y - pointFrom.Y) / steps;

            //log.Info("Swiping from " + from.ToString() + " to " + to.ToString());
            TouchDown();
            this.MoveTo(pointdownAt);
            BotApplication.Wait(300);
            this.MoveTo(pointFrom);
            BotApplication.Wait(300);

            for (int i = 0; i < steps; i++)
            {
                pointFrom.X += xStep;
                pointFrom.Y += yStep;
                this.MoveTo(pointFrom);
            }

            if (!touchUpAtTouchDownLocation)
            {
                this.MoveTo(pointdownAt);
            }

            BotApplication.Wait(500);
            TouchUp();
            EndTouchData();
            //BotApplication.Wait(500);
        }
    }
}