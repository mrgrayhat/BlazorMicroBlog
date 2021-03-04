using System;
using System.Timers;

namespace Blazor.WebAssembly.ClientApp.Services
{
    public class ToastService : IDisposable
    {
        public event Action<string, ToastLevel> OnShow;
        public event Action OnHide;

        private const int INTERVAL = 5000;
        private Timer Countdown;

        public void ShowToast(string message, ToastLevel level)
        {
            OnShow?.Invoke(message, level);
            StartCountdown();
        }

        private void StartCountdown()
        {
            SetCountdown();

            if (Countdown.Enabled)
            {
                Countdown.Stop();
                Countdown.Start();
            }
            else
            {
                Countdown.Start();
            }
        }

        private void SetCountdown()
        {
            if (Countdown == null)
            {
                Countdown = new Timer(INTERVAL);
                Countdown.Elapsed += HideToast;
            }
        }

        private void HideToast(object source, ElapsedEventArgs args)
        {
            OnHide?.Invoke();
        }

        public void Dispose()
        {
            Countdown?.Dispose();
        }
    }
}
