using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DirectShowLib;
using WPFMediaKit.Clock;

namespace WPFMediaKit.DirectShow.MediaPlayers
{
    public partial class MediaSeekingPlayer
    {
        protected ReferenceClock m_syncClock;
        protected DateTime m_syncStartTime;

        protected System.Windows.Forms.Timer m_positionSyncTimer = null;
        const int PositionSyncTimerIntervalMs = 1000;
        const int PositionsyncMaxDeltaMs = 50;

        /// <summary>
        /// Plays the media
        /// </summary>
        public virtual void Start(DateTime syncStartTime)
        {
            VerifyAccess();
            Pause();

            /* update _syncStartTime and set pos if non zero */
            m_syncStartTime = syncStartTime;
            UpdateSyncedPositonAndClockStartTime(DateTime.Now);

            Play();

            StartPositionSyncTimerIfSynced();
        }

        public override void Pause()
        {
            StopPositionSyncTimer();
            base.Pause();
        }

        public override void Stop()
        {
            StopPositionSyncTimer();
            base.Stop();
        }

        ///<summary>
        /// Sets current movie position to the time calculated. If _syncStartTime was not set, nothign happens.
        /// </summary>
        /// <param name="currentTime">Usually the current time of the day</param>
        /// <remarks></remarks>
        private void UpdateSyncedPositonAndClockStartTime(DateTime currentTime)
        {
            long newPosMs;
            DateTime newSyncStartTime;
            if (m_syncStartTime == default(DateTime))
            {
                newPosMs = 0;
                newSyncStartTime = currentTime;
            }
            else
            {
                newPosMs = CalcSynchronizedPosition(currentTime);
                newSyncStartTime = CalcStartTimeSynchronizedToPosition(currentTime, newPosMs);
            }

            /* Update clock  startTime and movie position */
            m_syncClock.SetStartTime(newSyncStartTime);
            MediaPosition = newPosMs * 10000; 
        }

        /// <summary>
        /// Returns current movie synchronized position
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private long CalcSynchronizedPosition(DateTime currentTime)
        {
            long newPosMs = 0;
            long passedms = 0;
            var durationMs = Duration/10000;
            if (durationMs > 0)
            {
                passedms = (long)(currentTime - m_syncStartTime).TotalMilliseconds;
                if (passedms > 0)
                {
                    newPosMs = passedms % durationMs;
                }
            }
            return newPosMs;
        }

        private DateTime CalcStartTimeSynchronizedToPosition(DateTime currentTime, long newPosMs)
        {
            return currentTime.AddMilliseconds(-newPosMs);
        }

        /// <summary>
        /// Sets clock for the graph
        /// </summary>
        protected void SetNewGraphClock(IFilterGraph graph)
        {
            m_syncClock = new ReferenceClock();
            ((IMediaFilter)graph).SetSyncSource(m_syncClock);
        }

        /// <summary>
        /// Starts position sync timer if _syncStartTime not zero
        /// </summary>
        /// <remarks></remarks>
        private void StartPositionSyncTimerIfSynced()
        {
            if ((m_syncStartTime == DateTime.MinValue))
            {
                return;
            }
            if ((m_positionSyncTimer == null))
            {
                m_positionSyncTimer = new System.Windows.Forms.Timer();
                m_positionSyncTimer.Interval = PositionSyncTimerIntervalMs;
                m_positionSyncTimer.Tick += delegate { SyncPosition(); };
            }
            m_positionSyncTimer.Start();
        }

             
        private void StopPositionSyncTimer()
        {
            if ((m_positionSyncTimer != null))
            {
                m_positionSyncTimer.Stop();
            }
        }

        /// <summary>
        /// Compares actual (current) and desired (calciulated) movie position. 
        /// If difference is above delta inits playback restart with new sync start time. 
        /// </summary>
        /// <remarks></remarks>
        private void SyncPosition()
        {
            long curPosMs = MediaPosition / 10000;
            long desiredPosMs = CalcSynchronizedPosition(DateTime.Now);
            long posDelta = desiredPosMs - curPosMs;

            Debug.WriteLine("Delta:" + posDelta);

            if ((posDelta > PositionsyncMaxDeltaMs))
            {
                Start(m_syncStartTime);
            }
            else
            {
                Debug.WriteLine("Delta OK!");
                StopPositionSyncTimer();
            }
        }

    }
}
