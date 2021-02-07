using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSS.Mapping;

namespace PSS.MultiThreading
{
    public static class Factory
    {
        public static void Log(string Message)
        {
            Mapping.Factory.Log(Message);
        }
        public static void LogInnerExceptions(AggregateException e) 
        {
            Mapping.Factory.LogInnerExceptions(e);
        }
    }
}
