using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SouqScrapper.LinkGenerators
{
    public class PagingInfo
    {
        public int Page { get; set; }
        public int Section { get; set; }
    }

    public class PagingLinkGenerator : IEnumerable<PagingInfo>
    {
        //private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private readonly int _batchNumber;
        private int _currentBatchNumber = 0;
        private bool _isRunning = true;
        private int _lastPage = 1;
        private int _lastSection = 1;

        public PagingLinkGenerator(int batchNumber)
        {
            _batchNumber = batchNumber;
        }

        public void Reset()
        {
            _lastPage = 1;
            _lastSection = 1;
            _currentBatchNumber = 0;
            //_autoResetEvent.Set();
        }

        public void Terminate()
        {
            //Console.WriteLine("Terminate: " + _debug);
            _isRunning = false;
            //_autoResetEvent.Set();
        }

        public void Next()
        {
            _currentBatchNumber--;
            //_autoResetEvent.Set();
        }

        public IEnumerator<PagingInfo> GetEnumerator()
        {
            do
            {
                _currentBatchNumber++;

                yield return new PagingInfo()
                {
                    Page = _lastPage,
                    Section = _lastSection
                };

                if (_lastSection == 2)
                {
                    _lastPage++;
                    _lastSection = 1;
                }
                else
                {
                    _lastSection++;
                }

                if (_currentBatchNumber >= _batchNumber - 1)
                {
                    //var hasSignal = _autoResetEvent.WaitOne(TimeSpan.FromMinutes(20));

                    //if (!hasSignal)
                    _isRunning = false;
                }
            } while (_isRunning);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}