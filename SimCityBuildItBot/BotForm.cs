

namespace SimCityBuildItBot
{
    using Common.Logging;
    using SimCityBuildItBot.Bot;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using static Bot.Salesman;
    using Common.Logging.Simple;
    using System.Drawing;
    using Emgu.CV;
    using Emgu.CV.CvEnum;
    using Emgu.CV.Features2D;
    using Emgu.CV.Structure;
    using Emgu.CV.Util;
    using Emgu.CV.Cuda;
    using Emgu.CV.XFeatures2D;
    using ColorMine;
    using System.Drawing.Imaging;


    public partial class BotForm : Form
    {
        private ILog log;

        private BuildingSelector buildingSelector;
        private NavigateToBuilding navigateToBuilding;
        private Touch touch;
        private CommerceResourceReader resourceReader;
        private List<CommerceItemBuild> buildItemList;
        private TradeWindow tradeWindow;
        private CaptureScreen captureScreen;
        private ItemHashes itemHashes;
        private Salesman salesman;
        private TradePanelCapture tradePanelCapture;
        private Craftsman craftsman;

        public BotForm()
        {
            InitializeComponent();
            log = new LogToText(this.txtLog, this);
            touch = new Touch(log);
            buildingSelector = new BuildingSelector(log, touch);

            captureScreen = new CaptureScreen(log);
            tradeWindow = new TradeWindow(captureScreen, log);

            navigateToBuilding = new NavigateToBuilding(log, touch, buildingSelector, tradeWindow);
            resourceReader = new CommerceResourceReader(log, touch);
            buildItemList = CommerceItemBuild.CreateResourceList();
            tradePanelCapture = new TradePanelCapture(tradeWindow);

            itemHashes = new ItemHashes(new List<PictureBox>(), new List<TextBox>());
            itemHashes.ReadHashes();
            salesman = new Salesman(touch, tradeWindow, tradePanelCapture, itemHashes, navigateToBuilding, log);

            craftsman = new Craftsman(log, buildingSelector, navigateToBuilding, touch, resourceReader, buildItemList);
        }
        private void WaitNSeconds(int segundos)
        {
            if (segundos < 1) return;
            DateTime _desired = DateTime.Now.AddSeconds(segundos);
            while (DateTime.Now < _desired)
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }
        private void WaitNMiliSeconds(int segundos)
        {
            if (segundos < 1) return;
            DateTime _desired = DateTime.Now.AddMilliseconds(segundos);
            while (DateTime.Now < _desired)
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }

        public void CheckReadyFactoryItemAndCollect(int n)
        {
            int x = 358 + 142 * n;
            int y = 675;
            int status = touch.CheckFactorySlotStatus(x, y);


            if (status == 1) //ready
            {
                touch.ClickAt(new Point(x, y)); //collect
                Bot.BotApplication.Wait(250);
                if(n%5==0)
                    touch.Swipe(Constants.GetPoint(Bot.Location.ButtonLeftInner1), Constants.GetPoint(Bot.Location.CentreMap)); // procedure
                if (n % 5 == 1)
                    touch.Swipe(Constants.GetPoint(Bot.Location.ButtonRightInner1), Constants.GetPoint(Bot.Location.CentreMap)); // procedure
                if (n % 5 == 2)
                    touch.Swipe(Constants.GetPoint(Bot.Location.ButtonLeftInner3), Constants.GetPoint(Bot.Location.CentreMap)); // procedure
                if (n % 5 == 3)
                    touch.Swipe(Constants.GetPoint(Bot.Location.ButtonRightInner2), Constants.GetPoint(Bot.Location.CentreMap)); // procedure
                else
                    touch.Swipe(Constants.GetPoint(Bot.Location.ButtonLeftInner2), Constants.GetPoint(Bot.Location.CentreMap)); // procedure

            }
            if (status == 0) //empty
            {
                touch.Swipe(Constants.GetPoint(Bot.Location.ButtonLeftInner3), Constants.GetPoint(Bot.Location.CentreMap)); // procedure
            }
            if (status == -1) //processing
            {
                // donothing
               // touch.Swipe(Constants.GetPoint(Bot.Location.ButtonLeftInner2), Constants.GetPoint(Bot.Location.CentreMap)); // procedure
            }

        }
        public void CollectFactoryItem(int factorySize)
        {
            for (var i = 0; i < factorySize; i++)
            {
                BotApplication.Wait(10);
                CheckReadyFactoryItemAndCollect(i);
                BotApplication.Wait(50);
            }

        }
        public void nextFactory()
        {
            // 861 156

            touch.ClickAt(new Point(861, 156));
        }
        public bool checkEmptyFactorySlot()
        {
            Bitmap bmp = touch.Crop(396, 614, 65, 22);
            //bmp.Save(@"E:\botscreensave\crop.png", ImageFormat.Png);

            return true;
        }
        public void ProcedureFactoryItem(int factorySize)
        {
            for (var i = 0; i < factorySize; i++)
            {
                touch.Swipe(Constants.GetPoint(Bot.Location.FactoryResource1), Constants.GetPoint(Bot.Location.CentreMap));
                BotApplication.Wait(100);
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
        public bool CheckIfInFactory()
        {
            Color c = touch.GetPixel(861, 156);




            double delta = compareColor(c, Color.FromArgb(255, 250, 196, 1));
            log.Info("delta  :" + delta);
            return delta < 1;


        }
        public bool CheckIfInTradeDepot()
        {

            Bitmap bmp = touch.Crop(295, 104, 78, 68);

            bool isvisible = tradeWindow.IsImageVisible(bmp, "TradeDepot Logo");
            log.Info("TradeDepot Logo isvisible:" + isvisible);
            return isvisible;
        }
        public void SetPriceAndQty()
        {
            for (int i = 0; i < 5; i++)
            {


                touch.ClickAtAndHold(new Point(958, 399), 1000);
                Bot.BotApplication.Wait(10);

            }
            for (int i = 0; i < 5; i++)
            {

                Bot.BotApplication.Wait(1);
                touch.ClickAt(new Point(958, 284), 10);


            }

            Bot.BotApplication.Wait(50);
            touch.ClickAt(new Point(883, 569));
        }
        public static DateTime lastAdvertise = DateTime.Now;
        public bool CreateSellItem(int n, int at)
        {
            int row = 0;
            int col = 0;
            row = (int)Math.Floor((double)at / 4);
            row = row % 2;
            col = at % 4;

            if (n == 1)
            {
                touch.ClickAt(new Point(428 + 146 * col, 260 + 182 * row)); // click bua` hi vong se trung o 
                Bot.BotApplication.Wait(100);
                if (touch.CheckPixel(1033, 131, Color.White, 10)) // if create sell form are show
                {
                    
                    log.Info("Sell form are showed start selling at  [" + col + "," + row + "] ");
                    touch.ClickAt(new Point(325, 297));
                    Bot.BotApplication.Wait(100);
                    SetPriceAndQty();
                    Bot.BotApplication.Wait(200);
                }
                else
                {
                    // 808 116
                    if (touch.CheckPixel(808, 116, Color.White, 10)) //if edit sell form are show
                    {
                        touch.ClickAt(new Point(808, 116));
                        Bot.BotApplication.Wait(100);
                    }
                    else { // if nothing happend then phat click bua thanh ra collect cmnr
                        touch.ClickAt(new Point(428 + 146 * col, 260 + 182 * row));
                        Bot.BotApplication.Wait(100);
                        if (touch.CheckPixel(1033, 131, Color.White, 10)) // if create sell form are show
                        {

                            log.Info("Sell form are showed start selling at  [" + col + "," + row + "] ");
                            touch.ClickAt(new Point(325, 297));
                            Bot.BotApplication.Wait(100);
                            SetPriceAndQty();
                            Bot.BotApplication.Wait(200);
                        }
                        else
                        {
                            touch.ClickAt(new Point(873, 183));
                            Bot.BotApplication.Wait(100);

                            return false;
                        }
                    }
                }
                
                
                

                return true;

            }

            

            int x = 409 + 146 * col;
            int y = 305 + 182 * row;

            Color c = touch.GetPixel(409 + 146 * col, 305 + 182 * row);
            double delta = compareColor(c, Color.FromArgb(255, 119, 173, 45));
            if (delta < 10) // if sold then collect gold
            {

                log.Info(" Slot [" + col + "," + row + "] are sold");

                touch.ClickAt(new Point(409 + 146 * col, 305 + 182 * row)); //collect
                Bot.BotApplication.Wait(100);

                // then create sell
                touch.ClickAt(new Point(409 + 146 * col, 305 + 182 * row));
                Bot.BotApplication.Wait(100);
                touch.ClickAt(new Point(325, 297));
                Bot.BotApplication.Wait(100);
                SetPriceAndQty();
                Bot.BotApplication.Wait(200);







            }
            else    
            {

                c = touch.GetPixel(428 + 146 * col, 260 + 182 * row);
                delta = compareColor(c, Color.FromArgb(255, 255, 255, 255));

                if (delta < 10)
                {
                    log.Info("Slot [" + col + "," + row + "] selling");
                   

                        // click for edit sell
                        touch.ClickAt(new Point(428 + 146 * col, 260 + 182 * row));

                        // 723 492 
                        //rgb 40 , 122 , 161
                        BotApplication.Wait(50);
                        c = touch.GetPixel(723, 492);
                        delta = compareColor(c, Color.FromArgb(255,40, 122, 161 ));
                             log.Info("adv status "+ delta);
                        if (delta < 50)  // check if can advsertise
                        {
                            touch.ClickAt(new Point(723, 492));
                            
                        }
                        // then close
                        BotApplication.Wait(50);
                        touch.ClickAt(new Point(802, 124));
                        BotApplication.Wait(20);
                    
                }
                else
                {
                    log.Info("Slot [" + col + "," + row + "] avaiable");



                    // then create sell
                    touch.ClickAt(new Point(409 + 146 * col, 305 + 182 * row));
                    Bot.BotApplication.Wait(200);
                    touch.ClickAt(new Point(325, 297));
                    Bot.BotApplication.Wait(200);
                    SetPriceAndQty();
                    Bot.BotApplication.Wait(200);



                }

            }

            /*if ((at + 1) % 8 == 0)
            {
                //touch.Execute("input touchscreen swipe 923 300 339 300");
                touch.Execute("input touchscreen swipe 923 300 384 300");

                Bot.BotApplication.Wait(150);
            }*/
            return true;
        }
        public void SellItems(int n =0)
        {

            for (var i = 0; i < 5; i++)
            {

                touch.GotoTopLeft();
            }

            touch.GotoTradeDepot();
            BotApplication.Wait(500);

            if (CheckIfInTradeDepot())
            {
                // 409 305 x 150 px
                for (var i = 0; i < 8; i++)
                {
                    bool result = CreateSellItem(0, i);
                    if (!result) break;

                }
                 
                Bot.BotApplication.Wait(150);
                touch.Execute("input touchscreen swipe 923 300 384 300");
                //touch.Swipe(new Point(900, 300), new Point(900, 300), new Point(384, 300), 1, true);
                Bot.BotApplication.Wait(150);
                for (var i = 0; i < 8; i++)
                {
                    CreateSellItem(1, i);

                }

                //Bot.BotApplication.Wait(150);
                //touch.Swipe(new Point(923, 300), new Point(923, 300), new Point(384, 300), 1, true);
                //Bot.BotApplication.Wait(150);
                //for (var i = 0; i < 8; i++)
                //{
                //    CreateSellItem(1, i);

                //}


                //// Color c = touch.GetPixel(409,305);
                if (CheckIfInTradeDepot())
                {
                   touch.ClickAt(new Point(958, 135));
                }
                this.touch.GotoCenter();
            }
            //else {
            //    if(n<3)
            //    SellItems(n++);
            //}
            
        }
        public void ProcedureItems()
        {


            this.touch.GotoCenter();
             Random rnd = new Random();
            List<Point> items = new List<Point>();
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner1));
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner1));
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner1));
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner1));
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner1));
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner1));
        
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner1));
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner2));
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner1));
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner2));
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner1));
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner2));
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner1));
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner2));
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner1));
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner2));
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner2));
            items.Add(Constants.GetPoint(Bot.Location.ButtonLeftInner3));
            items.Add(Constants.GetPoint(Bot.Location.ButtonRightInner1));
           

            int cnt = 0;
            while (!CheckIfInFactory())
            {
                touch.ClickAt(Bot.Location.FirstFactory);
                BotApplication.Wait(200);
                cnt++;
                if (cnt > 10) break;

            }
            //cnt = 0;
            //while (!CheckIfInFactory())
            //{
            //    touch.ClickAt(new Point(830, 538));
            //    BotApplication.Wait(200);
            //    cnt++;
            //    if (cnt > 10) break;

            //}
           
            if (!CheckIfInFactory())
            {
                ProcedureItems();
                return;
            }
            //touch.Swipe(Constants.GetPoint(Bot.Location.FactoryResource1), Constants.GetPoint(Bot.Location.CentreMap));
            //424 678
            for (int j = 0; j < 6; j++) // 5 factories
            {
                //CollectFactoryItem(5);
                int[] AllStatus = touch.CheckFactoryAllSlotStatus();

                log.Info("Status of all 5 slots :" + string.Join(",", AllStatus));
                int emptyCount = 0;
               
                for (int i = 0; i < AllStatus.Length; i++)
                {
                    int x = 358 + 142 * i;
                    int y = 675;
                    if (AllStatus[i] == 1)
                    {
                        touch.ClickAt(new Point(x, y));
                        BotApplication.Wait(20);
                        emptyCount++;
                    }
                    if (AllStatus[i] == 0)
                    {
                        emptyCount++;
                    }
                }
                /*

                for (int i = 0; i < AllStatus.Length; i++)
                {
                    int x = 429 + 142 * i;
                    int y = 675;
                    if (AllStatus[i] == 1)
                    {
                        touch.ClickAt(new Point(x, y));
                        BotApplication.Wait(20);
                        emptyCount++;
                    }
                    if (AllStatus[i] == 0)
                    {
                        emptyCount++;
                    }
                }
                */
                for (int i = 0; i < emptyCount; i++)
                {
                    Point  pr = items[rnd.Next(items.Count)];
                    touch.Swipe(pr, Constants.GetPoint(Bot.Location.CentreMap)); // procedure

                    BotApplication.Wait(30);
                }
                Bot.BotApplication.Wait(100);
                nextFactory();
                Bot.BotApplication.Wait(100);
            }
            if (CheckIfInFactory())
            {
                touch.sendRawEvent("1 1 1");
                touch.sendRawEvent("0 0 0");
               // touch.ClickAt(new Point(655, 77));
                /*
                 /dev/input/event7: 0001 0001 00000001
/dev/input/event7: 0000 0000 00000000
                 */


            }
        }
        public void CraftItem()
        {
            this.touch.GotoCenter();

            int cnt = 0;
            while (!CheckIfInFactory())
            {
                touch.ClickAt(Bot.Location.FirstStore);


                BotApplication.Wait(200);
                cnt++;
                if (cnt > 10) break;

            }
            if (!CheckIfInFactory())
            {
                while (!CheckIfInFactory())
                {
                    touch.ClickAt(Bot.Location.SecondStore);


                    BotApplication.Wait(200);
                    cnt++;
                    if (cnt > 10) break;

                }
            }
            if (!CheckIfInFactory())
            {
                return;
            }
            for (int i = 0; i < 4; i++) // number of store
            {
                for (int j = 0; j < 4; j++)
                {
                    touch.ClickAt(Bot.Location.CenterFactory);
                    Bot.BotApplication.Wait(100);

                }
                for (int j = 0; j < 2; j++)
                {
                    
                    touch.Swipe(Constants.GetPoint(Bot.Location.ButtonLeftInner1), Constants.GetPoint(Bot.Location.CentreMap)); // procedure
                    Bot.BotApplication.Wait(100);
                    // check if not enouch resource
                    //510 480
                    if (touch.CheckPixel(510, 480, Color.White, 10))
                    {
                        touch.ClickAt(new Point(510, 480));
                        Bot.BotApplication.Wait(200);
                        break;
                    }
                }
                /*
                for (int j = 0; j < 2; j++)
                {

                    touch.Swipe(Constants.GetPoint(Bot.Location.ButtonLeftInner2), Constants.GetPoint(Bot.Location.CentreMap)); // procedure
                    Bot.BotApplication.Wait(100);
                    // check if not enouch resource
                    //510 480
                    if (touch.CheckPixel(510, 480, Color.White, 10))
                    {
                        touch.ClickAt(new Point(510, 480));
                        Bot.BotApplication.Wait(200);
                        break;
                    }
                }
                for (int j = 0; j < 2; j++)
                {

                    touch.Swipe(Constants.GetPoint(Bot.Location.ButtonLeftInner3), Constants.GetPoint(Bot.Location.CentreMap)); // procedure
                    Bot.BotApplication.Wait(100);
                    // check if not enouch resource
                    //510 480
                    if (touch.CheckPixel(510, 480, Color.White, 10))
                    {
                        touch.ClickAt(new Point(510, 480));
                        Bot.BotApplication.Wait(200);
                        break;
                    }
                }
                */
                Bot.BotApplication.Wait(100);
                nextFactory();
                Bot.BotApplication.Wait(100);
            }
            if (CheckIfInFactory())
            {
                touch.sendRawEvent("1 1 1");
                touch.sendRawEvent("0 0 0");
                //touch.ClickAt(new Point(150, 150));
                /*
                 /dev/input/event7: 0001 0001 00000001
/dev/input/event7: 0000 0000 00000000
                 */


            }

        }
       
        private void btnBuildAvailableItems_Click(object sender, EventArgs e)
        {
            /*
            Bitmap bmp = touch.Crop(336, 206, 551, 336, "test" );

            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 4; i++)
                {

                    string name = "square-" + i+"-"+j+"-";
                    Rectangle cropRect = new Rectangle(20 + 148 * i, 50 + 182 * j, 40, 60);

                    Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

                    using (Graphics g = Graphics.FromImage(target))
                    {
                        g.DrawImage(bmp, new Rectangle(0, 0, target.Width, target.Height),
                                         cropRect,
                                         GraphicsUnit.Pixel);
                    }
                    target.Save(@"E:\botscreensave\" + name + ".png", System.Drawing.Imaging.ImageFormat.Png);
                }
            }
              
            *///bool isvisible = tradeWindow.IsImageVisible(bmp, "test");
            //log.Info("TradeDepot Logo isvisible:" + isvisible);


            //20 50 40 60

            
            int cnt = 0;
            while (1 == 1)
            {
                cnt++;
                int avaiableStorageSlot = touch.getAvaiableStorage();
                log.Info("avaiableStorageSlot  :" + avaiableStorageSlot);




                ProcedureItems();
               
                    CraftItem();
               //if(avaiableStorageSlot<25) 
                    SellItems(); 

               // if (cnt > 3) cnt = 0;



                


            }

            

            //650 500 650 0
            /*
             int cnt = 0;
             while (!CheckIfInFactory())
             {
                 touch.ClickAt(Bot.Location.FirstFactory);
                 WaitNSeconds(1);
                 cnt++;
                 if (cnt > 10) break;
             }

            //touch.Swipe(Constants.GetPoint(Bot.Location.FactoryResource1), Constants.GetPoint(Bot.Location.CentreMap));
            //424 678
            for (int i = 0; i < 5; i++)
            {
                CollectFactoryItem(4);
                WaitNSeconds(1);
                nextFactory();
                WaitNSeconds(1);
            }


            */

            //captureScreen.LeftMouseClick(1298, 290);
            //WaitNSeconds(1);
            //captureScreen.LeftMouseClick(777, 350);
            //WaitNSeconds(10);
            //captureScreen.LeftMouseClick(777, 398);
            //touch.Swipe(new Point(120,140), new Point(500, 420), new Point(500, 420), 100, true);
            //touch.ClickAt(new Point(200, 200),100);
            /*
            while (true)
            {
                bool buildingItems = craftsman.Craft();
                //salesman.Sell();

                if (buildingItems)
                {
                    log.Info("Sleeping for 1 mins");
                    Bot.BotApplication.Wait(1000 * 60 * 1); // sleep for 2 mins
                }
                else
                {
                    log.Info("Sleeping for 4 mins");
                    Bot.BotApplication.Wait(1000 * 60 * 4); // sleep for 4 mins
                    this.txtLog.Text = "";
                }
            }*/
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new DebugForm().ShowDialog();
        }

        private void btnBuy_Click(object sender, EventArgs e)
        {
            this.Hide();
            new BuyForm().ShowDialog();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();

            Process.GetCurrentProcess().Kill();
        }

        private void btnSellTheCraft_Click(object sender, EventArgs e)
        {

            touch.Execute("input touchscreen swipe 923 300 384 300");
            //touch.Swipe(new Point(900, 300), new Point(900, 300), new Point(384, 300), 1, true);
            //while (true)
            //{
            //    if (this.IsDisposed)
            //    {
            //        return;
            //    }

            //    var sw = new Stopwatch();
            //    sw.Start();

            //    bool buildingItems = craftsman.Craft();
            //    if (this.IsDisposed)
            //    {
            //        return;
            //    }

            //    log.Info("Sleeping for 1 mins");
            //    var millisecond = 60 * 1000;

            //    while (sw.ElapsedMilliseconds < millisecond)
            //    {
            //        log.Trace("Sleep remaining = " + (int)((millisecond - sw.ElapsedMilliseconds) / 1000) + " seconds");
            //        Application.DoEvents();
            //        System.Threading.Thread.Sleep(50);
            //        if (this.IsDisposed)
            //        {
            //            return;
            //        }
            //    }

            //    this.txtLog.Text = "";

            //    string itemSold;

            //    salesman.Sell(out itemSold);
            //    if (this.IsDisposed)
            //    {
            //        return;
            //    }
            //}
        }

        private void L12Craft_Click(object sender, EventArgs e)
        {
            //this.touch.GotoCenter();

            this.SellItems();
            //int x = 358 + 142 * n;
            //int y = 675;
            
            // 660 510
            //touch.FindLine();
            // touch.Swipe(new Point(900, 380), new Point(900, 380), new Point(490, 380), 1, true);
            //  this.log.Info
            //  ("Found first Line at "+ touch.FindLine());
            // BotApplication.Wait(5000);

            // captureScreen.LeftMouseDown(900, 380);
            //captureScreen.LeftMouseMoveTo(490, 380);
            //captureScreen.LeftMouseUp(490, 380);



            //touch.Execute("input touchscreen swipe 923 300 385 300");
            //touch.MoveTo(new Point(900, 380));
            //touch.MoveTo(new Point(900, 380));
            //touch.MoveTo(new Point(900, 380));
            //touch.TouchDown();
            //touch.MoveTo(new Point(800, 380));
            //touch.MoveTo(new Point(762, 380));
            //touch.MoveTo(new Point(662, 380));
            //touch.MoveTo(new Point(562, 380));
            ////touch.TouchDown();
            //touch.MoveTo(new Point(462, 380));
            //touch.MoveTo(new Point(462, 380));
            //touch.MoveTo(new Point(462, 380));
            //touch.MoveTo(new Point(462, 380));
            //touch.MoveTo(new Point(462, 380));
            //touch.TouchUp();
            //touch.EndTouchData();
            //touch.ClickAt(new Point(958, 135));
            /*NavigateToBuilding.FactorySwitch = Building.BasicFactory;

            while (true)
            {
                bool buildingItems = craftsman.CraftLevel12();
                //salesman.Sell();

                log.Info("Sleeping for 1 mins");
                Bot.BotApplication.Wait(1000 * 60 * 1); // sleep for 2 mins
                this.txtLog.Text = "";
            }*/
        }


        private void L12SellAndCraft_Click(object sender, EventArgs e)
        {
            CraftItem();

            /*NavigateToBuilding.FactorySwitch = Building.BasicFactory;

            while (true)
            {
                if (this.IsDisposed)
                {
                    return;
                }

                var sw = new Stopwatch();
                sw.Start();

                bool buildingItems = craftsman.CraftLevel12();
                if (this.IsDisposed)
                {
                    return;
                }

                log.Info("Sleeping for 1 mins");
                var millisecond = 60 * 1000;

                while (sw.ElapsedMilliseconds < millisecond)
                {
                    log.Trace("Sleep remaining = " + (int)((millisecond - sw.ElapsedMilliseconds) / 1000) + " seconds");
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(50);
                    if (this.IsDisposed)
                    {
                        return;
                    }
                }

                this.txtLog.Text = "";

                string itemSold;

                salesman.Sell(out itemSold);
                if (this.IsDisposed)
                {
                    return;
                }
            }*/
        }

        private void ExecuteBtn_Click(object sender, EventArgs e)
        {
            this.touch.Execute(comnandTxt.Text);
        }

        private void BotForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();

            Process.GetCurrentProcess().Kill();
        }
    }
}