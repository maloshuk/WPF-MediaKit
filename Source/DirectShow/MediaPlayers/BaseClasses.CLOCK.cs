using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DirectShowLib;

namespace WPFMediaKit.DirectShow.MediaPlayers
{
    public partial class MediaPlayerBase
    {
        protected ReferenceClock m_syncClock;

        /// <summary>
        /// Plays the media
        /// </summary>
        public virtual void Start(DateTime syncStartTime)
        {
            VerifyAccess();
            Play();            
            m_syncClock.SetStartTime(syncStartTime);
        }

        /// <summary>
        /// Sets clock for the graph
        /// </summary>
        protected void SetNewGraphClock(IFilterGraph graph)
        {
            m_syncClock = new ReferenceClock();
            ((IMediaFilter) graph).SetSyncSource(m_syncClock);
        }
    }


    #region ReferenceClock

    public class ReferenceClock : IReferenceClock
    {
        #region "Private Members"

        [DllImport("Kernel32.DLL")]
        private static extern bool SetEvent(IntPtr hEvent);

        /// <summary>
        /// A constant to convert seconds into 100 ns units.
        /// 1 sec = 10 000 000 (ns unit)
        /// </summary>
        /// <remarks></remarks>
        private const long TIME_FACTOR = 10000000;

        /// <summary>
        /// Time difference with used clock and time returned in GetTime() request
        /// </summary>
        private long _deltaNsUnits;
        private long _prevTime = 0;

        #endregion

        /// <summary>
        /// Get time of the clock in 100 nanosecond units
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private long GetClockTime()
        {
            DateTime time = DateTime.Now;
            var nsUnits = TimeToNsUnits(time);
            return nsUnits;
        }

        /// <summary>
        /// Applies _deltaNsUnits correct to the time in the argument
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private long CorrectTime(long time)
        {
            return time + _deltaNsUnits;
        }
        #region "IReferenceClock Members"

        public int GetTime(out long pTime)
        {
            long corTime = CorrectTime(GetClockTime());
            //never return time less than previous
            _prevTime = corTime > _prevTime ? corTime : _prevTime;
            pTime = _prevTime;
            return 0;
        }


        /// <summary>
        /// Set desired start time to adjust clock to. 
        /// If pass default(DateTime), clock is not updated.
        /// </summary>
        /// <remarks></remarks>
        public void SetStartTime(DateTime time)
        {
            if (time == default(DateTime))
            {
                return;
            }
            long prevDelta = _deltaNsUnits;

            _deltaNsUnits = GetClockTime() - TimeToNsUnits(time);

            if ((prevDelta > _deltaNsUnits))
            {
                //reset previose time overwise it will be lower than corrected time permanently
                _prevTime = 0;
            }
        }

        private long TimeToNsUnits(DateTime time)
        {
            return (long)((double)time.Ticks/TimeSpan.TicksPerSecond*TIME_FACTOR);
        }
        private long TimeToNsUnits(TimeSpan time)
        {
            return (long)((double)time.Ticks/TimeSpan.TicksPerSecond*TIME_FACTOR);
        }

        private long _refTime;
        private IntPtr _hEvent;

        public int AdviseTime(long baseTime, long streamTime, System.IntPtr hEvent, out int pdwAdviseCookie)
        {
            pdwAdviseCookie = 0;
            long refTime = baseTime + streamTime;
            //_refTime = refTime;
            //_hEvent = hEvent;
            long time;
            GetTime(out time);

            var worker = new BackgroundWorker();
            worker.DoWork += AdviseTimeWorker_DoWork;
            worker.RunWorkerAsync(string.Format("{0}|{1}", refTime, hEvent));


            //Task.Run((Action)CallSetEvent);

            return 0;
        }

        private void AdviseTimeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            string[] args = ((string) e.Argument).Split('|');
            long refTime = long.Parse(args[0]);
            IntPtr hEvent = (IntPtr) int.Parse(args[1]);

            while (true)
            {
                long privateTime = 0;
                GetTime(out privateTime);
                if (privateTime >= refTime)
                {
                    try
                    {
                        SetEvent(hEvent);
                        break;
                    }
                    catch (Exception)
                    {}
                }
                Thread.Sleep(2);
            }

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

    #endregion

}
