using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kazakov_KP_01._01.Models
{
    public class Items
    {
        public int Item_Id { get; set; }
        public string ItemName { get; set; }
        public int MaxBuyPrice { get; set; }
        public bool IsActive { get; set; }
    }
}
