using SimCityBuildItBot.ImageHashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimCityBuildItBot
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BotForm());
            //MessageBox.Show("score:"+ImageHashing.ImageHashing.Similarity(@"E:\Downloads\SimCityBuildItBot-master\SimCityBuildItBot-master\sample\logo\Home Button.png", @"E:\botscreensave\Home Button.png"));
            
            //Application.Run(new HashingDemoForm());
            //Application.Run(new TradeDepot());
            //Application.Run(new ShoppingListsForm());
            //Application.Run(new TestForm());
        }
    }
}
