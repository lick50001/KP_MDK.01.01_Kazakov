using System;

namespace Kazakov_KP_01._01.Models
{
    public class FunctionItem
    {
        public string Title { get; set; }
        public string Icon { get; set; } = "⚙";
        public string Description { get; set; }
        public bool IsRunning { get; set; } = false;
    
        public Action OnStart { get; set; }
        public Action OnStop { get; set; }
    }
}