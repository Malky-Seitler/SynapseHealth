using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynapseInterview.Code.Models
{
    public class Order
    {
        public string OrderId { get; set; }
        public List<Item> Items { get; set; }
    }

}
