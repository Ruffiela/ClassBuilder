using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ClassBuilderSybase
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Checking if changes were implemented in GIT. Test 2018.05.30 Test1
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmBuilder());
        }
    }
}