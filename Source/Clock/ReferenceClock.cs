using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DirectShowLib;

namespace WPFMediaKit.Clock
{
    public class ReferenceClock : IReferenceClock
    {

        #region "Private Members"
        [DllImport("Kernel32.DLL")]
        private static extern bool SetEvent(IntPtr hEvent);

        private long _correctionNsUnits;
        /// <summary>
        /// A constant to convert seconds into 100 ns units.
        /// 1 sec = 10 000 000 (ns unit)
        /// </summary>
        /// <remarks></remarks>
        private const long TimeFactor = 10000000;
        private DateTime _firstRequestTime;
        private DateTime _desiredStartTime;

        long _prevTime = 0;
        #endregion

        #region "Constructors"

        public ReferenceClock()
        {
        }

        #endregion

        /// <summary>
        /// Get time of the timer in 100 nanosecond units
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private long GetPrivateTime()
        {
            DateTime currentTime = DateTime.Now;
            if ((_firstRequestTime == DateTime.MinValue))
            {
                _firstRequestTime = currentTime;
            }

            TimeSpan privateTime = default(TimeSpan);

            if ((_desiredStartTime == DateTime.MinValue))
            {
                privateTime = new TimeSpan(0);
            }
            else
            {
                privateTime = currentTime.Subtract(_desiredStartTime);
            }
            return TimeSpanToNsUnits(privateTime);
        }

        private long TimeSpanToNsUnits(TimeSpan time)
        {
            return (long)((double)time.Ticks / TimeSpan.TicksPerSecond * TimeFactor);
        }

        private TimeSpan NsUnitsToTimeSpan(long nsUnits)
        {
            return TimeSpan.FromTicks((long)((double)nsUnits / TimeFactor * TimeSpan.TicksPerSecond));
        }

        /// <summary>
        /// Set desired start time.
        /// </summary>
        /// <remarks></remarks>
        public void SetStartTime(DateTime time)
        {
            if ((time == DateTime.MinValue))
            {
                return;
            }
            _desiredStartTime = time;
            _firstRequestTime = DateTime.MinValue;
            _prevTime = 0;
        }

        #region "IReferenceClock Members"
        public int GetTime(out long pTime)
        {
            dynamic privateTime = GetPrivateTime();
            //Dim corTime As Long = CorrectTime(privateTime)

            _prevTime = privateTime > _prevTime ? privateTime : _prevTime;
            //never go backward
            pTime = _prevTime;

            return 0;
        }

        public int AdviseTime(long baseTime, long streamTime, System.IntPtr hEvent, out int pdwAdviseCookie)
        {
            pdwAdviseCookie = 0;

            long refTime = baseTime + streamTime;
            IntPtr evnt = hEvent;


            //SetEvent(evnt)
            //Return 0

            Task.Run(() =>
            {
                while (true)
                {
                    long privateTime = 0;
                    GetTime(out privateTime);

                    if (refTime <= privateTime)
                    {
                        SetEvent(evnt);
                        break; // TODO: might not be correct. Was : Exit While
                    }

                    Thread.Sleep(2);
                }
            });
            return 0;
        }


        public int Unadvise(int dwAdviseCookie)
        {
            // TODO: Add RefrenceClock.Unadvise implementation 
            return 0;
        }

        public int AdvisePeriodic(long startTime, long periodTime, System.IntPtr hSemaphore, out int pdwAdviseCookie)
        {
            // TODO: Add RefrenceClock.AdvisePeriodic implementation 
            pdwAdviseCookie = 0;
            return 0;
        }

        #endregion

    }

}

