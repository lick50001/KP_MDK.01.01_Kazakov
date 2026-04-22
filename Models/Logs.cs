using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kazakov_KP_01._01.Models
{
    public class Logs
    {
        public int Log_Id { get; set; }
        public string LogType { get; set; }
        public string Message { get; set; }
        public DateTime EventTime { get; set; }
    }
}
