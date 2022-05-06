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
            Thread thread = new Thread(() => calculateMaxCycleLength(arguments), STACK_SIZE);
            thread.Start();
            thread.Join();
            displayResults(arguments);
        }

        private static void calculateMaxCycleLength(string[] arguments)
        {
            sequencer = initializeSequencer(arguments);
            maxCycleLength = sequencer.calculateMaxCycleLength();
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