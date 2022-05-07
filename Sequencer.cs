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

                // this operation poses a problem -> multiple threads will be reading and accessing this variable in parallel
                // .. for both read and write operations; technically, this could result in two parallel operations setting the
                // .. new max value at essentially the same time causing the incorrect max value to be returned
                maxCycleLength = Math.Max(maxCycleLength, cycleLength);
            });
            return maxCycleLength;
        }

        public int calculateMaxCycleLengthThreadSafe()
        {
            ConcurrentDictionary<long, int> cycleLengths = new ConcurrentDictionary<long, int>();

            ConcurrentBag<int> maxCyclesLengthsFromThreads = new ConcurrentBag<int>();

            Parallel.For(
                start, // inclusive start for the index
                end, // exclusive end for the index
                () => 0, // the initial value for thisThreadsRunningMax, which we want to be zero

                // below is our parallel action
                (loopIndex, loopStateThatWeDontUse, thisThreadsRunningMax) => {
                    int cycleLength = generator.calculateCycleLength(cycleLengths, loopIndex);

                    // NOTE: this value gets stored as thisThreadsRunningMax and passed in to the next run
                    // There will be several instances of thisThreadsRunningMax - one per working thread.
                    // The value we set the first time our parallel function runs on a thread will be passed
                    // .. to the next function run on the same thread.
                    // thisThreadsRunningMax is thread safe (because there is a separate instance for each thread
                    return Math.Max(thisThreadsRunningMax, cycleLength);
                },

                // below is the "finally" action that will be performed in parallel on each result (aka: thisThreadsRunningMax)
                // .. of each set of parallel actions; this function must be thread safe
                (eachThreadsRunningMax) => maxCyclesLengthsFromThreads.Add(eachThreadsRunningMax)
                );

            // now we need to take the few maxCyclesLengthsFromThreads and find the true maximum

            return maxCyclesLengthsFromThreads.Max(); ;
        }

        public int calculateMaxCycleLengthThreadSafeSupaFast()
        {
            ConcurrentDictionary<long, int> cycleLengths = new ConcurrentDictionary<long, int>();

            int maxCycleLength = 0;

            Parallel.For(
                start, // inclusive start for the index
                end, // exclusive end for the index
                // below is our parallel action
                (loopIndex) =>
                {
                    int thisCycleMax = generator.calculateCycleLength(cycleLengths, loopIndex);

                    bool failedToUpdateMaxBecauseItChanged = false;
                    do
                    {
                        // get the current max value into a new variable for this comparison
                        int oldRunningMax = maxCycleLength; // 64-bit system int reads are atomic

                        int newRunningMax = Math.Max(oldRunningMax, thisCycleMax);

                        // only try to set maxCycleLength if necessary (because we have to block to do so)
                        if (newRunningMax > oldRunningMax)
                        {
                            // maxCycleLength will only be updated if its current value still matches oldRunningMax
                            // .. Interlocked will block other parallel Interlocked call until this one completes
                            int valueOfMaxWhenExchangeAttempted =
                                Interlocked.CompareExchange(ref maxCycleLength, newRunningMax, oldRunningMax);

                            // if the value of max when we try to exchange it for our new max does not equal
                            // .. the value that we read into oldRunningMax, then this calculation is invalid
                            // .. because another parallel method has already set a new max value
                            // we need to re-run our max value calculation based on the new max
                            failedToUpdateMaxBecauseItChanged = valueOfMaxWhenExchangeAttempted != oldRunningMax;
                        }


                    } while (failedToUpdateMaxBecauseItChanged);

                });

            return maxCycleLength;
        }
    }
}