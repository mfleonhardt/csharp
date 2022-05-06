using System.Collections.Concurrent;

namespace ThreeNplus1
{
    class SeriesGenerator
    {
        public int calculateCycleLength(ConcurrentDictionary<long, int> cycleLengths, long value, int cycleLength = 0)
        {
            int knownCycleLength;
            if (cycleLengths.TryGetValue(value, out knownCycleLength))
            {
                return knownCycleLength + cycleLength;
            }

            ++cycleLength;

            if (value == 1)
            {
                return cycleLength;
            }

            int totalCycleLength = calculateCycleLength(cycleLengths, value % 2 == 0 ? value / 2 : 3 * value + 1, cycleLength);
            int localCycleLength = totalCycleLength - cycleLength + 1;
            cycleLengths.TryAdd(value, localCycleLength);
            return totalCycleLength;
        }
    }
}