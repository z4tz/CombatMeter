using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CombatMeter
{
    public class PeriodicTask
    {
        /// <summary>
        /// test
        /// </summary>
        /// <param name="action"></param>
        /// <param name="period"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task Run(Action action, TimeSpan period, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
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
