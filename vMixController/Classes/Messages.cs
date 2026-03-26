using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vMixController.Widgets;

namespace vMixController.Messages
{
    public class LoadingMessage
    {
        public bool Loading { get; set; }
    }
    public class LIVEToggleMessage
    {
        public int State { get; set; }
    }

    public class SetGlobalVariable
    {
        public int Index { get; set; } = -1;
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public enum WidgetEditAction
    {
        Move,
        Resize
    }

    public class WidgetEditMessage
    {
        public vMixControl Widget { get; set; }
        public WidgetEditAction Action { get; set; }
        public bool IsStarted { get; set; }
    }

    public class HoveredWidgetMessage
    {
        public vMixControl Widget { get; set; }
    }

    public class HotkeyLinkMessage
    {
        public string Link { get; set; }
        public object Parameter { get; set; }
    }

    public class WidgetMoveStateMessage
    {
        public vMixControl Widget { get; set; }
        public bool IsStarted { get; set; }
    }

    public class WidgetMoveDeltaMessage
    {
        public vMixControl Widget { get; set; }
        public double DeltaX { get; set; }
        public double DeltaY { get; set; }
    }

    public class HotkeysEnabledMessage
    {
        public bool IsEnabled { get; set; }
    }

    public class SyncStateRequestMessage
    {
        public bool Force { get; set; } = true;
    }

    public enum PageNavigationMode
    {
        Next,
        Previous,
        SetIndex
    }

    public class PageNavigationMessage
    {
        public PageNavigationMode Mode { get; set; }
        public int PageIndex { get; set; }
    }

}
