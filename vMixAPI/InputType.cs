using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMixAPI
{
    public enum InputType
    {
        Video = 0,
        Image = 1,
        Photos = 2,
        PowerPoint = 3,
        DVD = 4,
        Capture = 5,
        DesktopCapture = 6,
        Audio = 7,
        Flash = 8,
        Xaml = 9,
        VideoDelay = 10,
        Title = 11,
        Colour = 12,
        AudioFile = 13,
        VideoList = 14,
        VirtualSet = 15,
        VideoLoop = 16,
        Stream = 20,
        ImageSequence = 21,
        Virtual = 22,
        Blank = 100,
        Signal = 1000,
        Placeholder = 2000,
        Replay = 3000,
        ReplayPreview = 3001,
        ReplayOverlay = 3002,
        NDI = 4000,
        Browser = 5000,
        VideoCall = 6000,
    }
}
