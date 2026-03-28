using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMixController.Classes
{
    public static class Constants
    {
        public const string BUTTON_STYLE_MOMENTARY = "Momentary";
        public const string BUTTON_STYLE_TOGGLE = "Toggle";
        public const string BUTTON_STYLE_PRESS = "Press";
        public static string[] ButtonStyle { get; } = new string[] { BUTTON_STYLE_MOMENTARY, BUTTON_STYLE_TOGGLE, BUTTON_STYLE_PRESS };

        public const string BUTTON_IMAGE_TYPE_DEFAULT = "Default";
        public const string BUTTON_IMAGE_TYPE_DEFAULTPRESSED = "Default+Pressed";
        public static string[] ButtonImageType { get; } = new string[] { BUTTON_IMAGE_TYPE_DEFAULT, BUTTON_IMAGE_TYPE_DEFAULTPRESSED };

        public const string TEXTFIELD_STYLE_FILE = "File";
        public const string TEXTFIELD_STYLE_TEXT = "Text";

        public static string[] TextFieldStyle { get; } = new string[] { TEXTFIELD_STYLE_FILE, TEXTFIELD_STYLE_TEXT };

        public const string SCORE_BASIC = "Basic";
        public const string SCORE_BASKETBALL = "Basketball";
        public const string SCORE_RUGBY = "Rugby";
        public const string SCORE_AMERICANFOOTBALL = "American Football";
        public const string SCORE_CUSTOM = "Custom";
        public static string[] ScoreStyle { get; } = new string[] { SCORE_BASIC, SCORE_BASKETBALL, SCORE_RUGBY, SCORE_AMERICANFOOTBALL, SCORE_CUSTOM };

        public const string HORIZONTAL = "Horizontal";
        public const string VERTICAL = "Vertical";
        public static string[] HorizontalVertical { get; } = new string[] { HORIZONTAL, VERTICAL };

        public const string TBAR_MODE_AB = "A/B";
        public const string TBAR_MODE_SNAPBACK = "Snap Back";
        public static string[] TBarMode {  get; } = new string[] { TBAR_MODE_AB, TBAR_MODE_SNAPBACK };

        public const string BUS_A = "Bus A";
        public const string BUS_B = "Bus B";
        public const string BUS_C = "Bus C";
        public const string BUS_D = "Bus D";
        public const string BUS_E = "Bus E";
        public const string BUS_F = "Bus F";
        public const string BUS_G = "Bus G";
        public const string BUS_MASTER = "Master";
        public const string BUS_INPUT = "Input";

        public static string[] AudioBusses { get; } = new string[] {
            BUS_INPUT,
            BUS_MASTER,
            BUS_A,
            BUS_B,
            BUS_C,
            BUS_D,
            BUS_E,
            BUS_F,
            BUS_G
        };

        public static byte[] XORKEY = new byte[] { 0x44, 0x77, 0xCC, 0xA, 0x10,  0x33, 0x01, 0x05 };


        public const string LINKS_ALL = "All";
        public const string LINKS_HOVER = "Hover";
        public const string LINKS_NONE = "None";
        public static string[] Links { get; } = new string[] { LINKS_ALL, LINKS_HOVER, LINKS_NONE };

        public const string TIMER_EVENT_ONSTART = "On Start";
        public const string TIMER_EVENT_ONPAUSE = "On Pause";
        public const string TIMER_EVENT_ONSTOP = "On Stop";
        public const string TIMER_EVENT_ONCOMPLETION = "On Completion";
        public const string TIMER_EVENT_ONTICK = "On Tick";
        public static string[] TimerEvents = new string[] { TIMER_EVENT_ONSTART, TIMER_EVENT_ONPAUSE, TIMER_EVENT_ONSTOP, TIMER_EVENT_ONCOMPLETION, TIMER_EVENT_ONTICK };
        //public static byte[] DESIV = new byte[] { 0x10, 0x33, 0xFA, 0x6E, 0x3F, 0xAA, 0x90, 0x5D };
    }
}