using System;

namespace ThreeNplus1
{
    class Sequencer
    {
        public int start { get; private set; }
        public int end { get; private set; }
        private SeriesGenerator generator;

        public Sequencer(int start, int end, SeriesGenerator generator)
        {
            this.start = start;
            this.end = end;
            this.generator = generator;
            validateSequenceRange();
        }

        private void validateSequenceRange()
        {
            if (start < 0 || end > 1000000 || start >= end)
            {
                throw new ArgumentException("Arguments are not valid.");
            }
        }

        public int calculateMaxCycleLength()
        {
            int maxCycleLength = 0;
            for (int item = start; item <= end; ++item)
            {
                int cycleLength = generator.calculateCycleLength(item);
                maxCycleLength = Math.Max(maxCycleLength, cycleLength);
            }
            return maxCycleLength;
        }
    }
}