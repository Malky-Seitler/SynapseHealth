using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynapseInterview.Code.Models
{
    public class Item
    {
        public string Description { get; set; }
        public int DeliveryNotification { get; set; }
        public Status Status { get; set; }
    }

    public enum Status
    {
        Delivered,
        In_Progress
    }
}
