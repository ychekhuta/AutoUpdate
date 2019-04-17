using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoUpdate
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //if (!AutoUpdater.IsNetworkDir())
            //{
            //    MessageBox.Show("You can only run AutoUpdater from the network folder. Please, create a shortcut to the Network Location.");
            //    return;
            //}   

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AutoUpdater());
        }
    }
}
