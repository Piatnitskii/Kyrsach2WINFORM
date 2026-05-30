using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kyrsach2WINFORM
{
    public class ActivityMessageFilter : IMessageFilter
    {
        private readonly Action _onActivity;

        public ActivityMessageFilter(Action onActivity)
        {
            _onActivity = onActivity;
        }

        public bool PreFilterMessage(ref Message m)
        {
            const int WM_MOUSEMOVE = 0x0200;
            const int WM_LBUTTONDOWN = 0x0201;
            const int WM_LBUTTONUP = 0x0202;
            const int WM_RBUTTONDOWN = 0x0204;
            const int WM_MBUTTONDOWN = 0x0207;
            const int WM_MOUSEWHEEL = 0x020A;
            const int WM_MOUSEACTIVATE = 0x0021;
            const int WM_KEYDOWN = 0x0100;
            const int WM_SYSKEYDOWN = 0x0104;
            const int WM_CHAR = 0x0102;
            const int WM_ACTIVATE = 0x0006;
            const int WM_ACTIVATEAPP = 0x0316;

            bool isInputMessage =
                m.Msg == WM_MOUSEMOVE ||
                m.Msg == WM_LBUTTONDOWN ||
                m.Msg == WM_LBUTTONUP ||
                m.Msg == WM_RBUTTONDOWN ||
                m.Msg == WM_MBUTTONDOWN ||
                m.Msg == WM_MOUSEWHEEL ||
                m.Msg == WM_MOUSEACTIVATE ||
                m.Msg == WM_KEYDOWN ||
                m.Msg == WM_SYSKEYDOWN ||
                m.Msg == WM_CHAR ||
                m.Msg == WM_ACTIVATE ||
                m.Msg == WM_ACTIVATEAPP;

            if (isInputMessage)
            {
                _onActivity?.Invoke();
            }

            // Не блокируем сообщение, позволяем нормальную обработку окон
            return false;
        }
    }
}
