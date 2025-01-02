using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyColorReducer.Core
{
    public class ReducerException : Exception
    {
        public ReducerException(string message) 
            : base(message) { }
    }
}
