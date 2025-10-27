using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Autoclicker
{
    public partial class MainWindow : Window
    {
        private readonly System.Windows.Threading.DispatcherTimer _timer = new();
        private readonly Clicker _clicker = new();

        // === Win32 global hotkey ===
        const int HOTKEY_ID = 1;              // any number you choose
        const uint MOD_NONE = 0x0000;         // no modifiers
        const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public MainWindow()
        {
            InitializeComponent();

            _timer.Tick += async (_, __) => await _clicker.Click();
            SetTimerFromCps(10);
            _timer.Start();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Hook window messages
            var src = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            src.AddHook(WndProc);

            // Register F6 as a global hotkey
            int vkF6 = KeyInterop.VirtualKeyFromKey(Key.F6);
            if (!RegisterHotKey(new WindowInteropHelper(this).Handle, HOTKEY_ID, MOD_NONE, (uint)vkF6))
            {
                MessageBox.Show("Failed to register global hotkey F6. It may already be in use.",
                                "Hotkey", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Clean up the hotkey
            var handle = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(handle, HOTKEY_ID);
            base.OnClosed(e);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                Toggle();          // toggle even when not focused
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void ToggleBtn_Pressed(object sender, RoutedEventArgs e) => Toggle();

        private void Toggle()
        {
            _clicker.Toggle();
            StatusText.Text = _clicker.IsOn() == "On" ? "Running" : "Stopped";
        }

        private void SetTimerFromCps(int cps)
            => _timer.Interval = TimeSpan.FromMilliseconds(1000.0 / Math.Max(1, cps));

        private void CpsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int cps = (int)Math.Round(CpsSlider.Value);
            _clicker.Cps = cps;
            _timer.Interval = TimeSpan.FromMilliseconds(1000.0 / cps);
        }

    }
}
