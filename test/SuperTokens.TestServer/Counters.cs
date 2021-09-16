using System.Threading;

namespace SuperTokens.TestServer
{
    public sealed class Counters
    {
        private int _noOfTimesGetSessionCalledDuringTest;
        private int _noOfTimesRefreshAttemptedDuringTest;
        private int _noOfTimesRefreshCalledDuringTest;

        public int NoOfTimesGetSessionCalledDuringTest =>
            _noOfTimesGetSessionCalledDuringTest;

        public int NoOfTimesRefreshAttemptedDuringTest =>
            _noOfTimesRefreshAttemptedDuringTest;

        public int NoOfTimesRefreshCalledDuringTest =>
                            _noOfTimesRefreshCalledDuringTest;

        public void IncrementNoOfTimesGetSessionCalledDuringTest() =>
            Interlocked.Increment(ref _noOfTimesGetSessionCalledDuringTest);

        public void IncrementNoOfTimesRefreshAttemptedDuringTest() =>
            Interlocked.Increment(ref _noOfTimesRefreshAttemptedDuringTest);

        public void IncrementNoOfTimesRefreshCalledDuringTest() =>
                            Interlocked.Increment(ref _noOfTimesRefreshCalledDuringTest);

        public void Reset()
        {
            Interlocked.Exchange(ref _noOfTimesGetSessionCalledDuringTest, 0);
            Interlocked.Exchange(ref _noOfTimesRefreshAttemptedDuringTest, 0);
            Interlocked.Exchange(ref _noOfTimesRefreshCalledDuringTest, 0);
        }
    }
}
