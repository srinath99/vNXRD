using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace vNXRD
{
    public static class WindowsServices
    {
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int WS_EX_LAYERED = 0x80000;
        const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }

        public static void SetWindowExLayered(IntPtr hwnd)
        {
            var style = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, -20, style & ~(0x80000 | 0x20));
        }
    }
}

namespace vWXR.KeyboardHelper
{
    /// <summary>
    /// Listens keyboard globally.
    /// 
    /// <remarks>Uses WH_KEYBOARD_LL.</remarks>
    /// </summary>
    public class KeyboardListener : IDisposable
    {
        /// <summary>
        /// Creates global keyboard listener.
        /// </summary>
        public KeyboardListener()
        {
            // We have to store the HookCallback, so that it is not garbage collected runtime
            hookedCallback = (InterceptKeys.LowLevelKeyboardProc)HookCallback;

            // Set the hook
            hookId = InterceptKeys.SetHook(hookedCallback);

            // Assign the asynchronous callback event
            asyncCallback += new HookCallbackAsync(KeyboardListener_asyncCallback);
        }

        /// <summary>
        /// Destroys global keyboard listener.
        /// </summary>
        ~KeyboardListener()
        {
            Dispose();
        }

        /// <summary>
        /// Fired when any of the keys is pressed down.
        /// </summary>
        public event RawKeyEventHandler KeyDown;

        /// <summary>
        /// Fired when any of the keys is released.
        /// </summary>
        public event RawKeyEventHandler KeyUp;

        #region Inner workings
        /// <summary>
        /// Hook ID
        /// </summary>
        private IntPtr hookId = IntPtr.Zero;

        /// <summary>
        /// Asynchronous callback hook.
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        private delegate void HookCallbackAsync(int nCode, uint wParam, int lParam);

        /// <summary>
        /// Actual callback hook.
        /// 
        /// <remarks>Calls asynchronously the asyncCallback.</remarks>
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private IntPtr HookCallback(
            int nCode, UIntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
                if (asyncCallback != null)
                    if (wParam.ToUInt32() == InterceptKeys.WM_KEYDOWN ||
                        wParam.ToUInt32() == InterceptKeys.WM_KEYUP ||
                        wParam.ToUInt32() == InterceptKeys.WM_SYSKEYDOWN ||
                        wParam.ToUInt32() == InterceptKeys.WM_SYSKEYUP)
                        asyncCallback.BeginInvoke(nCode, wParam.ToUInt32(), Marshal.ReadInt32(lParam), null, null);

            return InterceptKeys.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// Event to be invoked asynchronously each time key is pressed.
        /// </summary>
        private event HookCallbackAsync asyncCallback;

        /// <summary>
        /// Contains the hooked callback in runtime.
        /// </summary>
        private InterceptKeys.LowLevelKeyboardProc hookedCallback;

        /// <summary>
        /// Asynccallback that calls accordingly the KeyDown or KeyUp events.
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        void KeyboardListener_asyncCallback(int nCode, uint wParam, int lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = lParam;

                if (wParam == InterceptKeys.WM_KEYDOWN)
                    if (KeyDown != null)
                        KeyDown(this, new RawKeyEventArgs(vkCode, false));

                if (wParam == InterceptKeys.WM_KEYUP)
                    if (KeyUp != null)
                        KeyUp(this, new RawKeyEventArgs(vkCode, false));

                if (wParam == InterceptKeys.WM_SYSKEYDOWN)
                    if (KeyDown != null)
                        KeyDown(this, new RawKeyEventArgs(vkCode, true));

                if (wParam == InterceptKeys.WM_SYSKEYUP)
                    if (KeyUp != null)
                        KeyUp(this, new RawKeyEventArgs(vkCode, true));
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes the hook.
        /// <remarks>This call is required as it calls the UnhookWindowsHookEx.</remarks>
        /// </summary>
        public void Dispose()
        {
            InterceptKeys.UnhookWindowsHookEx(hookId);
        }

        #endregion
    }
    /// <summary>
    /// Raw KeyEvent arguments.
    /// </summary>
    public class RawKeyEventArgs : EventArgs
    {
        /// <summary>
        /// VKCode of the key.
        /// </summary>
        public int VKCode;

        /// <summary>
        /// WPF Key of the key.
        /// </summary>
        public Key Key;

        /// <summary>
        /// Is the key a system key.
        /// </summary>
        public bool IsSysKey;

        /// <summary>
        /// Create raw keyevent arguments.
        /// </summary>
        /// <param name="VKCode"></param>
        /// <param name="isSysKey"></param>
        public RawKeyEventArgs(int VKCode, bool isSysKey)
        {
            this.VKCode = VKCode;
            this.IsSysKey = isSysKey;
            this.Key = System.Windows.Input.KeyInterop.KeyFromVirtualKey(VKCode);
           
        }
    }

    /// <summary>
    /// Raw keyevent handler.
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="args">raw keyevent arguments</param>
    public delegate void RawKeyEventHandler(object sender, RawKeyEventArgs args);

    #region WINAPI Helper class
    /// <summary>
    /// Winapi Key interception helper class.
    /// </summary>
    internal static class InterceptKeys
    {
        public delegate IntPtr LowLevelKeyboardProc(
            int nCode, UIntPtr wParam, IntPtr lParam);

        public static int WH_KEYBOARD_LL = 13;
        public static int WM_KEYDOWN = 256;
        public static int WM_KEYUP = 257;
        public static int WM_SYSKEYUP = 261;
        public static int WM_SYSKEYDOWN = 260;

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            UIntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
    }
    #endregion

}    