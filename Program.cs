using System.CommandLine;
using System.IO;
using System.Threading;

namespace ThreeNplus1
{
    class Program
    {
        const int STACK_SIZE = 1000000;
        private static Sequencer? sequencer;
        private static int maxCycleLength = 0;

        public static void Main(string[] arguments)
        {
            sequencer = initializeSequencer(arguments);
            var watch = new System.Diagnostics.Stopwatch();

            int maxCycleLengthOriginal = 0;
            int maxCycleLengthRefactor = 0;

            // lets run over and over until we see a error caused by cross-thread updates
            // .. on the original method
            while (maxCycleLengthOriginal == maxCycleLengthRefactor)
            {
                watch.Restart();
                maxCycleLengthRefactor = sequencer.calculateMaxCycleLengthThreadSafe();
                watch.Stop();

                Console.WriteLine("Refactor: \r\n-- Max Cycle Length: {0}\r\n-- Run time (ticks): {1}",
                    maxCycleLengthRefactor, watch.ElapsedTicks);



                watch.Restart();
                maxCycleLengthOriginal = sequencer.calculateMaxCycleLength();
                watch.Stop();

                Console.WriteLine("Original: \r\n-- Max Cycle Length: {0}\r\n-- Run time (ticks): {1}",
                    maxCycleLengthOriginal, watch.ElapsedTicks);


                Console.WriteLine("\r\n");
            }

            // it's also interesting to note the difference in run times for the first run
            // .. and how the run time converges as the dotnet runtime optimization get better
            // .. and better at execution

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("AHA!!! The stars have aligned! You can see in the run above that something " +
                "bad happened with the running max cycle count.");

            System.Diagnostics.Debugger.Break();
            maxCycleLength = maxCycleLengthRefactor;
            displayResults(arguments);
        }

        private static Sequencer initializeSequencer(string[] arguments)
        {
            if (sequencer != null)
            {
                return sequencer;
            }
            validateArgumentCount(arguments);
            int first = castArgument(arguments[0]);
            int second = castArgument(arguments[1]);
            return new Sequencer(first, second, new SeriesGenerator());
        }

        private static void validateArgumentCount(string[] arguments)
        {
            if (arguments.Length != 2)
            {
                throw new ArgumentException("Program requires exactly two arguments");
            }
        }

        private static int castArgument(string argument)
        {
            int castedArgument = 0;
            if (!int.TryParse(argument, out castedArgument))
            {
                throw new ArgumentException("Argument is invalid.");
            }
            return castedArgument;
        }

        private static void displayResults(string[] arguments)
        {
            sequencer = initializeSequencer(arguments);
            Console.WriteLine("{0} {1} {2}", sequencer.start, sequencer.end, maxCycleLength);
        }
    }
}