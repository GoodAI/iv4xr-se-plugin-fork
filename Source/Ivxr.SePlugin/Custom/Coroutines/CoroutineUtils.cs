using System.Collections;
using System.Diagnostics;

namespace Iv4xr.SePlugin.Custom.Coroutines
{
    public static class CoroutineUtils
    {
        public static IEnumerator WaitForSeconds(double seconds)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.Elapsed.TotalSeconds < seconds)
            {
                yield return null;
            }
        }
    }
}