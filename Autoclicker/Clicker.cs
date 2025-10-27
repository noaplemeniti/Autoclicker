using System;
using System.Threading.Tasks;

namespace Autoclicker
{
    internal class Clicker
    {
        private bool _toggle;
        private bool _leftButton = true; // toggle this if you add a UI control
        private int _cps = 10;

        public Task Click()
        {
            if (!_toggle) return Task.CompletedTask;

            if (_leftButton) NativeClicker.LeftClick();
            else NativeClicker.RightClick();

            return Task.CompletedTask;
        }

        public void Toggle() => _toggle = !_toggle;

        public string IsOn()
        {
            if(_toggle) return "On";
            else return "Off";
        }
        public int Cps
        {
            get => _cps;
            set => _cps = Math.Clamp(value, 1, 20);
        }
    }
}
