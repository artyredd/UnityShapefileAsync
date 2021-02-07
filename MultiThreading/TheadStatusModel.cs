using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS
{
    /// <summary>
    /// General use model used to track progress of threads
    /// </summary>
    public class ThreadStatusModel
    {
        public bool Working { get; set; } = false;
        public bool Waiting { get; set; } = false;
        public bool ThreadStarted { get; set; } = false;
        public bool ThreadEnded { get; set; } = false;
        public bool ThreadCancelled { get; set; } = false;
        public int Status { get; set; } = 0;
    }
}
