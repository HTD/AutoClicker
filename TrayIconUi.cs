using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Woof.WinAPI;

namespace AutoClicker {

    /// <summary>
    /// Auto clicker tray icon application.
    /// </summary>
    class TrayIconUi : ApplicationContext {

        /// <summary>
        /// StartStop Keys available settings.
        /// </summary>
        Dictionary<Keys, string> StartStopKeysAvailable = new Dictionary<Keys, string>() {
            {Keys.Scroll, "Scroll Lock" },
            {Keys.RControlKey, "Right Ctrl" }
        };

        /// <summary>
        /// Clicks Per Second available settings.
        /// </summary>
        int[] CpsAvailable { get; } = new[] { 5, 15, 30, 60 };

        /// <summary>
        /// Gets or sets StartStopKey setting.
        /// </summary>
        Keys StartStopKey
        {
            get => Properties.Settings.Default.StartStopKey;
            set
            {
                var keysItems = KeysMenu.DropDownItems.OfType<ToolStripMenuItem>();
                foreach (var i in keysItems) i.Checked = (Keys)i.Tag == value;
                Properties.Settings.Default.StartStopKey = value;
            }
        }

        /// <summary>
        /// Gets or sets Clicks Per Second setting.
        /// </summary>
        int CPS {
            get => Properties.Settings.Default.CPS;
            set {
                var cpsItems = CPSMenu.DropDownItems.OfType<ToolStripMenuItem>();
                foreach (var i in cpsItems) i.Checked = (int)i.Tag == value;
                Properties.Settings.Default.CPS = value;
                Timer.Interval = 1000 / value;
            }
        }

        /// <summary>
        /// Gets or sets cursor pin state.
        /// </summary>
        bool IsCursorPinned {
            get => Properties.Settings.Default.PinCursor;
            set {
                (TrayMenu.Items[4] as ToolStripMenuItem).Checked = Properties.Settings.Default.PinCursor = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Creates and initializes tray menu and events.
        /// </summary>
        public TrayIconUi() {
            foreach (var i in StartStopKeysAvailable) KeysMenu.DropDownItems.Add(new ToolStripMenuItem(i.Value, null, SetStartStopKeyClick) { Tag = i.Key });
            TrayMenu.Items.Add(KeysMenu);
            TrayMenu.Items.Add("-");
            foreach (var i in CpsAvailable) CPSMenu.DropDownItems.Add(new ToolStripMenuItem($"{i} CPS", Properties.Resources.Time, SetCpsClick) { Tag = i });
            TrayMenu.Items.Add(CPSMenu);
            TrayMenu.Items.Add("-");
            TrayMenu.Items.Add("Pin cursor", Properties.Resources.Pin, TogglePinClick);
            TrayMenu.Items.Add("-");
            TrayMenu.Items.Add("Exit", Properties.Resources.Exit, ExitClick);
            CPS = Properties.Settings.Default.CPS;
            StartStopKey = Properties.Settings.Default.StartStopKey;
            IsCursorPinned = Properties.Settings.Default.PinCursor;
            TrayIcon.Icon = Properties.Resources.Fire;
            TrayIcon.ContextMenuStrip = TrayMenu;
            TrayIcon.Text = $"AutoClicker :: Press [{StartStopKeysAvailable[Properties.Settings.Default.StartStopKey]}] to activate";
            TrayIcon.Visible = true;
            Timer.Tick += (s, e) => {
                if (Properties.Settings.Default.PinCursor) GlobalInput.LeftClick(Position);
                else GlobalInput.LeftClick();
            };
            Timer.Interval = Properties.Settings.Default.CPS;
            GlobalInput.KeyEvent += (s, e) => {
                if (e.KeyCode == Properties.Settings.Default.StartStopKey) lock (SwitchLock) {
                        if (!Timer.Enabled) Position = Cursor.Position;
                        Timer.Enabled = !Timer.Enabled;
                    }
            };
            ShowHelp();
        }

        /// <summary>
        /// Disposes disposable objects.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing) {
            Timer.Dispose();
            GlobalInput.Dispose();
            TrayMenu.Dispose();
            TrayIcon.Visible = false;
            TrayIcon.Dispose();
            base.Dispose(disposing);
        }

        void ShowHelp() => TrayIcon.ShowBalloonTip(2500, "AutoClicker", $"Press [{StartStopKeysAvailable[Properties.Settings.Default.StartStopKey]}]\r\nto activate and deactivate AutoClicker.", ToolTipIcon.Info);

        /// <summary>
        /// Set StartStopKey menu item click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SetStartStopKeyClick(object sender, EventArgs e) => StartStopKey = (Keys)(sender as ToolStripMenuItem).Tag;

        /// <summary>
        /// Set CPS menu item click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SetCpsClick(object sender, EventArgs e) => CPS = (int)(sender as ToolStripMenuItem).Tag;

        /// <summary>
        /// Pin cursor menu item click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TogglePinClick(object sender, EventArgs e) => IsCursorPinned = !(sender as ToolStripMenuItem).Checked;

        /// <summary>
        /// Exit menu item handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ExitClick(object sender, EventArgs e) => Application.Exit();

        #region Private data

        /// <summary>
        /// Global input events access.
        /// </summary>
        readonly GlobalInput GlobalInput = new GlobalInput();

        /// <summary>
        /// Tray icon instance.
        /// </summary>
        readonly NotifyIcon TrayIcon = new NotifyIcon();

        /// <summary>
        /// Menu associated with the tray icon.
        /// </summary>
        private ContextMenuStrip TrayMenu = new ContextMenuStrip();

        /// <summary>
        /// Submenu for CPS.
        /// </summary>
        private ToolStripMenuItem CPSMenu = new ToolStripMenuItem("Set CPS", Properties.Resources.Time);

        /// <summary>
        /// Submenu for StartStopKeys.
        /// </summary>
        private ToolStripMenuItem KeysMenu = new ToolStripMenuItem("Set Start/Stop key");

        /// <summary>
        /// Click timer
        /// </summary>
        private System.Windows.Forms.Timer Timer = new System.Windows.Forms.Timer();

        /// <summary>
        /// Cursor position for clicking
        /// </summary>
        private Point Position = Point.Empty;

        /// <summary>
        /// A lock to prevent multiple switching
        /// </summary>
        private readonly object SwitchLock = new Object();

        #endregion

    }

}
