using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.MultiThreading
{
    /// <summary>
    /// Progress model that holds information for common TaskProgress
    /// </summary>
    public class TaskProgressModel : ITaskProgress
    {

        public TaskStatus Status { get; set; }
    }
}
