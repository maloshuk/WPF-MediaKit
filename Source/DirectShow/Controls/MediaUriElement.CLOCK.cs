using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFMediaKit.DirectShow.Controls
{
    /// <summary>
    /// Defines method Start
    /// </summary>
    public partial class MediaUriElement
    {
        /// <summary>
        /// The Start adds possibility to assign sync start time 
        /// </summary>
        public override void Start(DateTime syncStartTime)
        {
            EnsurePlayerThread();
            
            MediaPosition = 0;
            base.Start(syncStartTime);
        }
    }
}
