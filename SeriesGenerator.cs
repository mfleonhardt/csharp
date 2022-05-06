namespace ThreeNplus1
{
    class SeriesGenerator
    {
        private static Dictionary<Int64, Int32> cycleLengths = new Dictionary<Int64, Int32>();

        public int calculateCycleLength(Int64 value, int cycleLength = 0)
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

            int totalCycleLength = calculateCycleLength(value % 2 == 0 ? value / 2 : 3 * value + 1, cycleLength);
            int localCycleLength = totalCycleLength - cycleLength + 1;
            cycleLengths.Add(value, localCycleLength);
            return totalCycleLength;
        }
    }
}