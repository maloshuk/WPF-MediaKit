using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFMediaKit.DirectShow.Controls
{

    public partial class MediaElementBase
    {
        /// <summary>
        /// Starts the media with
        /// </summary>
        public virtual void Start(DateTime syncStartTime)
        {
            MediaPlayerBase.EnsureThread(DefaultApartmentState);
            MediaPlayerBase.Dispatcher.BeginInvoke((Action)(delegate
            {
                MediaPlayerBase.Start(syncStartTime);
                Dispatcher.BeginInvoke(((Action)(() => SetIsPlaying(true))));
            }));
        }

    }
}
