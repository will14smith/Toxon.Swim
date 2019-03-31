using System;
using System.Threading;
using System.Threading.Tasks;

namespace Toxon.Swim
{
    public static class TimerUtils
    {
        public static CancellationTokenSource SetTimer(Action action, TimeSpan time)
        {
            var cts = new CancellationTokenSource();

            Task.Run(() =>
            {
                Task.Delay(time, cts.Token).ContinueWith(task =>
                {
                    if (task.IsCanceled) return;

                    action();
                });
            });

            return cts;
        }
    }
}
