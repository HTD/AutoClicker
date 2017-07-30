using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Woof.WinAPI {

    /// <summary>
    /// Global keyboard and mouse events access.
    /// </summary>
    /// <remarks>
    /// A part of the Woof Toolkit.
    /// </remarks>
    public sealed class GlobalInput : IDisposable {

        /// <summary>
        /// Occurs whenever any key is pressed or released, system-wide.
        /// </summary>
        public event EventHandler<GlobalKeyEventArgs> KeyEvent;

        /// <summary>
        /// Hooks a managed keyboard event to a low level keyboard event.
        /// </summary>
        public GlobalInput() { 
            KeyboardHookId = NativeMethods.SetWindowsHookEx(
                WH_KEYBOARD_LL,
                KeyboardHookInstance = KeyboardLowLevelHandler,
                NativeMethods.GetModuleHandle(MainModuleName), 0
            );
            GC.KeepAlive(KeyboardHookInstance);
        }

        /// <summary>
        /// Low level keyboard event handler. Invokes managed events.
        /// </summary>
        /// <param name="nCode">A code the hook procedure uses to determine how to process the message. If nCode is less than zero, the hook procedure must pass the message to the CallNextHookEx function without further processing and should return the value returned by CallNextHookEx.</param>
        /// <param name="wParam">The identifier of the keyboard message. This parameter can be one of the following messages: WM_KEYDOWN, WM_KEYUP, WM_SYSKEYDOWN, or WM_SYSKEYUP.</param>
        /// <param name="lParam">A pointer to a KBDLLHOOKSTRUCT structure.</param>
        /// <returns>Pointer to a low level keyboard event handler.</returns>
        private IntPtr KeyboardLowLevelHandler(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && KeyEvent != null && (int)wParam == WM_KEYDOWN) KeyEvent(null, new GlobalKeyEventArgs(Marshal.ReadInt32(lParam)));
            return NativeMethods.CallNextHookEx(KeyboardHookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// Sends left click event at current cursor position.
        /// </summary>
        public void LeftClick() => NativeMethods.mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);

        /// <summary>
        /// Sends left click event at specified position.
        /// </summary>
        /// <param name="position">Absolute cursor position.</param>
        public void LeftClick(Point position) {
            Cursor.Position = position;
            NativeMethods.mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
        }
        /// <summary>
        /// Removes native hook.
        /// </summary>
        public void Dispose() {
            IsDisposing = true;
            NativeMethods.UnhookWindowsHookEx(KeyboardHookId);
        }

        /// <summary>
        /// Disposes the hook on destruction.
        /// </summary>
        ~GlobalInput() {
            if (!IsDisposing) Dispose();
        }

        #region Constants

        /// <summary>
        /// Installs a hook procedure that monitors low-level keyboard input events.
        /// </summary>
        private const int WH_KEYBOARD_LL = 13;

        /// <summary>
        /// Posted to the window with the keyboard focus when a nonsystem key is pressed. A nonsystem key is a key that is pressed when the ALT key is not pressed.
        /// </summary>
        private const int WM_KEYDOWN = 0x0100;

        /// <summary>
        /// The dx and dy parameters contain normalized absolute coordinates. If not set, those parameters contain relative data: the change in position since the last reported position. This flag can be set, or not set, regardless of what kind of mouse or mouse-like device, if any, is connected to the system.
        /// </summary>
        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        /// <summary>
        /// Movement occurred.
        /// </summary>
        private const int MOUSEEVENTF_MOVE = 0x0001;

        /// <summary>
        /// The left button is down.
        /// </summary>
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;

        /// <summary>
        /// The left button is up.
        /// </summary>
        private const int MOUSEEVENTF_LEFTUP = 0x0004;

        /// <summary>
        /// The right button is down.
        /// </summary>
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;

        /// <summary>
        /// The right button is up.
        /// </summary>
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;

        #endregion

        #region WinAPI calls

        /// <summary>
        /// WinAPI native methods.
        /// </summary>
        class NativeMethods {

            /// <summary>
            /// Retrieves a module handle for the specified module. The module must have been loaded by the calling process.
            /// </summary>
            /// <param name="lpModuleName">
            /// The name of the loaded module (either a .dll or .exe file).
            /// If the file name extension is omitted, the default library extension .dll is appended.
            /// The file name string can include a trailing point character (.) to indicate that the module name has no extension.
            /// The string does not have to specify a path. When specifying a path, be sure to use backslashes (\), not forward slashes (/).
            /// The name is compared (case independently) to the names of modules currently mapped into the address space of the calling process.
            /// If this parameter is NULL, GetModuleHandle returns a handle to the file used to create the calling process (.exe file).</param>
            /// <returns>If the function succeeds, the return value is a handle to the specified module.</returns>
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            /// <summary>
            /// Installs an application-defined hook procedure into a hook chain. You would install a hook procedure to monitor the system for certain types of events. These events are associated either with a specific thread or with all threads in the same desktop as the calling thread.
            /// </summary>
            /// <param name="idHook">The type of hook procedure to be installed.</param>
            /// <param name="lpfn">A pointer to the hook procedure. If the dwThreadId parameter is zero or specifies the identifier of a thread created by a different process, the lpfn parameter must point to a hook procedure in a DLL. Otherwise, lpfn can point to a hook procedure in the code associated with the current process.</param>
            /// <param name="hMod">A handle to the DLL containing the hook procedure pointed to by the lpfn parameter. The hMod parameter must be set to NULL if the dwThreadId parameter specifies a thread created by the current process and if the hook procedure is within the code associated with the current process.</param>
            /// <param name="dwThreadId">The identifier of the thread with which the hook procedure is to be associated. For desktop apps, if this parameter is zero, the hook procedure is associated with all existing threads running in the same desktop as the calling thread.</param>
            /// <returns>If the function succeeds, the return value is the handle to the hook procedure. </returns>
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr SetWindowsHookEx(int idHook, KeyboardLowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

            /// <summary>
            /// Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function. 
            /// </summary>
            /// <param name="hhk">A handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to SetWindowsHookEx.</param>
            /// <returns>True if succeeded.</returns>
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnhookWindowsHookEx(IntPtr hhk);

            /// <summary>
            /// Passes the hook information to the next hook procedure in the current hook chain. A hook procedure can call this function either before or after processing the hook information.
            /// </summary>
            /// <param name="hhk">This parameter is ignored.</param>
            /// <param name="nCode">The hook code passed to the current hook procedure.The next hook procedure uses this code to determine how to process the hook information.</param>
            /// <param name="wParam">The wParam value passed to the current hook procedure. The meaning of this parameter depends on the type of hook associated with the current hook chain.</param>
            /// <param name="lParam">The lParam value passed to the current hook procedure. The meaning of this parameter depends on the type of hook associated with the current hook chain.</param>
            /// <returns>This value is returned by the next hook procedure in the chain. The current hook procedure must also return this value. The meaning of the return value depends on the hook type.</returns>
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            /// <summary>
            /// The mouse_event function synthesizes mouse motion and button clicks.
            /// </summary>
            /// <param name="dwFlags">Controls various aspects of mouse motion and button clicking.</param>
            /// <param name="dx">The mouse's absolute position along the x-axis or its amount of motion since the last mouse event was generated, depending on the setting of MOUSEEVENTF_ABSOLUTE. Absolute data is specified as the mouse's actual x-coordinate; relative data is specified as the number of minimal distances moved.</param>
            /// <param name="dy">The mouse's absolute position along the y-axis or its amount of motion since the last mouse event was generated, depending on the setting of MOUSEEVENTF_ABSOLUTE. Absolute data is specified as the mouse's actual y-coordinate; relative data is specified as the number of minimal distances moved.</param>
            /// <param name="dwData">
            /// If dwFlags contains MOUSEEVENTF_WHEEL, then dwData specifies the amount of wheel movement. A positive value indicates that the wheel was rotated forward, away from the user; a negative value indicates that the wheel was rotated backward, toward the user. One wheel click is defined as WHEEL_DELTA, which is 120.
            /// If dwFlags contains MOUSEEVENTF_HWHEEL, then dwData specifies the amount of wheel movement.A positive value indicates that the wheel was tilted to the right; a negative value indicates that the wheel was tilted to the left.
            /// If dwFlags contains MOUSEEVENTF_XDOWN or MOUSEEVENTF_XUP, then dwData specifies which X buttons were pressed or released. This value may be any combination of the following flags.
            /// If dwFlags is not MOUSEEVENTF_WHEEL, MOUSEEVENTF_XDOWN, or MOUSEEVENTF_XUP, then dwData should be zero.
            /// </param>
            /// <param name="dwExtraInfo">An additional value associated with the mouse event. An application calls GetMessageExtraInfo to obtain this extra information.</param>
            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);

        }

        #endregion

        #region State

        /// <summary>
        /// Delegate for low level keyboard event handler.
        /// </summary>
        /// <param name="nCode">A code the hook procedure uses to determine how to process the message. If nCode is less than zero, the hook procedure must pass the message to the CallNextHookEx function without further processing and should return the value returned by CallNextHookEx.</param>
        /// <param name="wParam">The identifier of the keyboard message. This parameter can be one of the following messages: WM_KEYDOWN, WM_KEYUP, WM_SYSKEYDOWN, or WM_SYSKEYUP.</param>
        /// <param name="lParam">A pointer to a KBDLLHOOKSTRUCT structure.</param>
        /// <returns>Pointer to a low level keyboard event handler.</returns>
        private delegate IntPtr KeyboardLowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Instance of the keyboard hook procedure to be kept alive during the application lifetime.
        /// </summary>
        private readonly KeyboardLowLevelProc KeyboardHookInstance;

        /// <summary>
        /// Hook identifier.
        /// </summary>
        private IntPtr KeyboardHookId = IntPtr.Zero;

        /// <summary>
        /// Gets the main module name of the current process.
        /// </summary>
        private string MainModuleName {
            get {
                using (var process = Process.GetCurrentProcess())
                using (var mainModule = process.MainModule) return mainModule.ModuleName;
            }
        }

        /// <summary>
        /// True if the disposal process was started.
        /// </summary>
        private bool IsDisposing;

        #endregion

    }

    /// <summary>
    /// Global keyboard event arguments.
    /// </summary>
    public class GlobalKeyEventArgs : EventArgs {

        /// <summary>
        /// Gets the keyboard code.
        /// </summary>
        public Keys KeyCode { get; }

        /// <summary>
        /// Creates new <see cref="GlobalKeyEventArgs"/> from integer value.
        /// </summary>
        /// <param name="value">Keyboard code value.</param>
        public GlobalKeyEventArgs(int value) => KeyCode = (Keys)value;

    }

}