namespace ConnectorSpace
{
    using System;
    using System.Diagnostics;

    internal class TickHelper
    {
        private static long StopwatchFrequencyMilliseconds = (Stopwatch.Frequency / 0x3e8L);

        public static long GetTickCount()
        {
            return (Stopwatch.GetTimestamp() / StopwatchFrequencyMilliseconds);
        }
    }
}

