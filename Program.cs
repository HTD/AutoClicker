using System;
using System.Windows.Forms;

namespace AutoClicker {

    /// <summary>
    /// AutoClicker application startup.
    /// </summary>
    static class Program {

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            using (var ui = new TrayIconUi()) Application.Run(ui);
        }

    }

}