using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CombatMeter
{
    public class PeriodicTask
    {

        public static async Task Run(Action action, TimeSpan period, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Debug.WriteLine("hello");
                    await Task.Delay(period, cancellationToken);
                    action();

            }
        }

        public static Task Run(Action action, TimeSpan period)
        {
            return Run(action, period, CancellationToken.None);
        }
    }
}
