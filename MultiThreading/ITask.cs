using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.MultiThreading
{
    /// <summary>
    /// Defines any object that has a <see cref="TaskStatus"/> property
    /// </summary>
    public interface ITask
    {
        TaskStatus Status { get; }
    }
}
