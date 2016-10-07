using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightOrganApp.Model
{
    public enum PlaybackState
    {
        Buffering = 6,    
        Connecting = 8,    
        Error = 7,    
        FastForwarding = 4,  
        None = 0,   
        Paused = 2,   
        Playing = 3,   
        Rewinding = 5,   
        SkippingToNext = 10,    
        SkippingToPrevious = 9,
        SkippingToQueueItem = 11,
        Stopped = 1
    }
}
