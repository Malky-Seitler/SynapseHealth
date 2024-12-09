using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynapseInterview.Code.Models
{
    public class Log
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public  bool IsErrorLog { get; set; }
        public Log()
        {
            Date = DateTime.Now;
        }
    }
}
