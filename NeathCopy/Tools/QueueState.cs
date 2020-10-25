using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeathCopy.Tools
{
    /// <summary>
    /// Expose a VisualCopy queve state
    /// </summary>
    public enum QueueState
    {
        /// <summary>
        /// The VisualCopy below to a queve, but is not been started.
        /// </summary>
        Waiting,
        /// <summary>
        /// The VisualCopy below to a queve and was started.
        /// </summary>
        StartedRuning,
        /// <summary>
        /// The VisualCopy below to a queve and was started, but them was paused.
        /// </summary>
        StartedPaused,
        /// <summary>
        /// The VisualCopy do not below to any queve.
        /// </summary>
        None,
        /// <summary>
        /// The VisualCopy below to a queve and and is thisplaying not enough space window.
        /// </summary>
        DisplayingNotEnoughSpace
    }
}
