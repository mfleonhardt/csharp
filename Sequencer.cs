using System.Collections.Concurrent;

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
            ConcurrentDictionary<long, int> cycleLengths = new ConcurrentDictionary<long, int>();

            int maxCycleLength = 0;
            Parallel.For(start, end, item => {
                int cycleLength = generator.calculateCycleLength(cycleLengths, item);
                maxCycleLength = Math.Max(maxCycleLength, cycleLength);
            });
            return maxCycleLength;
        }
    }
}