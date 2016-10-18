using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFMediaKit.DirectShow.Controls
{
    public partial class MediaSeekingElement
    {
        /// <summary>
        /// Starts the media with
        /// </summary>
        public virtual void Start(DateTime syncStartTime)
        {
            MediaSeekingPlayer.EnsureThread(DefaultApartmentState);
                  MediaSeekingPlayer.Dispatcher.BeginInvoke((Action)(delegate
            {
                MediaSeekingPlayer.Start(syncStartTime);
                Dispatcher.BeginInvoke(((Action)(() => SetIsPlaying(true))));
            }));
        }


    }
}
