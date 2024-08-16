using System;
using System.Collections.Generic;
using System.Text;

namespace EmuWarface.Core
{
    public class QueryException : Exception
    {
        public int CustomCode { get; }

        public QueryException(int custom_code)
        {
            CustomCode = custom_code;
        }

        public QueryException(object custom_code)
        {
            CustomCode = (int)custom_code;
        }
    }
}
