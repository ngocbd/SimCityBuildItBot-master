﻿namespace SimCityBuildItBot.Bot
{
    using Common.Logging;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using System.IO;

    public class TradeWindow
    {
        private CaptureScreen captureScreen;
        private ILog log;
        public bool Debug { get; set; }
        public PictureBox PictureBox { get; set; }

        public bool StopCapture { get; set; }

        public TradeWindow(CaptureScreen captureScreen, ILog log)
        {
            this.captureScreen = captureScreen;
            this.log = log;
        }
        public string projectPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
        
        public int X = 287;
        public int Y = 165;
        public Size Size = new Size(1300, 800);

        public List<int> imageTopsGlobalTrade = new List<int>() { 93, 444 };
        public List<int> imageTopsTradeDepot = new List<int>() { 154, 505 };

        public int clickXExtra = 272;
        public List<int> clickY = new List<int>() { 374, 738 };

        public const ulong RefreshGlobalTrade = 17521127647493644;

        public const ulong ConfigButtonHash = 35869570894094686;
        public const ulong TradeDepotLogoHash = 4071112760807423612;
        public const ulong CreateSaleHash = 35887226814411264;
        public Bitmap CaptureTradeWindow()
        {
            return captureScreen.SnapShot(X, Y, Size);
        }

        public Bitmap CaptureRefreshGlobalTradeButton()
        {
            return captureScreen.SnapShot(835, 935, new Size(55, 55));
        }

        public Bitmap CaptureConfigButton()
        {
            return captureScreen.SnapShot(60, 275, new Size(90, 90));
        }

        public Bitmap CaptureTradeDepotLogo()
        {
            return captureScreen.SnapShot(300, 140, new Size(85, 70), "TradeDepot Logo");
        }

        public Bitmap CaptureEditSaleCloseButton()
        {
            return captureScreen.SnapShot(1525, 95, new Size(55, 55));
        }

        public Point CalcClickPointGlobalTrade(PanelLocation loc)
        {
            var y = loc.Start.Y >= imageTopsGlobalTrade[1] ? clickY[1] : clickY[0];
            var x = clickXExtra + loc.Start.X + 120;

            return new Point(x, y);
        }

        public Point CalcClickPointTradeDepot(PanelLocation loc)
        {
            var y = loc.Start.Y >= imageTopsTradeDepot[1] ? clickY[1] : clickY[0];
            var x = clickXExtra + loc.Start.X + 120;

            return new Point(x, y);
        }

        public Point CalcClickPointCreateSaleItem(PanelLocation loc)
        {
            return new Point(520 + loc.Column * 280, 408 + loc.Row * 176);
        }

        public Func<bool> IsRefreshGlobalTradeButtonVisible
        {
            get
            {
                return () =>
                {
                    Func<Bitmap> getBitmap = () => { return this.CaptureRefreshGlobalTradeButton(); };

                    bool isvisible = IsImageVisible(getBitmap, TradeWindow.RefreshGlobalTrade);
                    return isvisible;
                };
            }
        }

        public Func<bool> IsConfigButtonVisible
        {
            get
            {
                return () =>
            {
                Func<Bitmap> getBitmap = () => { return this.CaptureConfigButton(); };

                bool isvisible = IsImageVisible(getBitmap, TradeWindow.ConfigButtonHash);
                return isvisible;
            };
            }
        }

        public Func<bool> IsTradeDepotLogoVisible
        {
            get
            {
                return () =>
            {
                Func<Bitmap> getBitmap = () => { return this.CaptureTradeDepotLogo(); };
                bool isvisible = IsImageVisible(getBitmap, "TradeDepot Logo");
               // bool isvisible = IsImageVisible(getBitmap, 8971012661670477438);
                return isvisible;
            };
            }
        }
        public bool IsImageVisible(Func<Bitmap> getImage, string name)
        {
            return IsImageVisible(getImage,ImageHashing.ImageHashing.AverageHash(Path.Combine(projectPath, @"Resources\sample\logo\" + name + ".png")));
        }
        public bool IsImageVisible(Bitmap getImage, string name)
        {
            var image = getImage;

            if (this.PictureBox != null)
            {
                ImageViewer.ShowBitmap(image, this.PictureBox);
            }

            ulong hash = ImageHashing.ImageHashing.AverageHash(image);
            ulong desiredHash = ImageHashing.ImageHashing.AverageHash(Path.Combine(projectPath, @"Resources\sample\logo\" + name + ".png"));
            var score = ImageHashing.ImageHashing.Similarity(desiredHash, hash);
            var isImageVisible = score > 90;
            if (!isImageVisible)
            {
                
                    log.Info("hash incorrect: " + hash+" and "+ desiredHash +" Similarity: "+ score);
                
            }

            return isImageVisible || this.StopCapture;
        }
        private bool IsImageVisible(Func<Bitmap> getImage, ulong desiredHash)
        {
            var image = getImage();

            if (this.PictureBox != null)
            {
                ImageViewer.ShowBitmap(image, this.PictureBox);
            }

            ulong hash = ImageHashing.ImageHashing.AverageHash(image);

            var isImageVisible = ImageHashing.ImageHashing.Similarity(desiredHash, hash) > 98;
            if (!isImageVisible)
            {
                if (Debug)
                {
                    log.Info("hash incorrect: " + hash);
                }
            }

            return isImageVisible || this.StopCapture;
        }

        public Func<bool> IsEditSaleCloseButtonVisible
        {
            get
            {
                return () =>
                {
                    Func<Bitmap> getBitmap = () => { return this.CaptureEditSaleCloseButton(); };

                    bool isvisible = IsImageVisible(getBitmap, TradeWindow.CreateSaleHash);
                    return isvisible;
                };
            }
        }

        public Func<bool> IsGlobalTradeVisible
        {
            get
            {
                return () =>
                {
                    Func<Bitmap> getBitmap = () => { return captureScreen.SnapShot(307, 142, new Size(60, 55), "GlobalTrade Logo"); };

                    bool isvisible = IsImageVisible(getBitmap, "GlobalTrade Logo");
                    return isvisible;
                };
            }
        }
        
        public Func<bool> IsHomeButtonVisible
        {
            get
            {
                return () =>
                {
                    Func<Bitmap> getBitmap = () => { return captureScreen.SnapShot(14, 63, new Size(65, 65),"Home Button"); ; };
                    bool isvisible = IsImageVisible(getBitmap, "Home Button");


                    //bool isvisible = IsImageVisible(getBitmap, 4503599627370463103) || IsImageVisible(getBitmap, 17591633037885440);
                    log.Debug("Home button isvisible = " + isvisible);
                    return isvisible;
                };
            }
        }

        internal Bitmap CaptureForSaleInventory()
        {
            int x = 400;
            int y = 345;
            var size = new Size(600, 600);

            return captureScreen.SnapShot(x, y, size);
        }

        public Func<bool> IsOfflineButtonVisible
        {
            get
            {
                return () =>
                {
                    Func<Bitmap> getBitmap = () => { return captureScreen.SnapShot(850, 800, new Size(130, 70));};

                    bool isvisible = IsImageVisible(getBitmap, 35887507618856960);
                    return isvisible;
                };
            }
        }
    }
}