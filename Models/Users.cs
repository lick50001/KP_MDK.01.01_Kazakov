using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kazakov_KP_01._01.Models
{
    public class Users
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string PwdHash { get; set; }
    }
}