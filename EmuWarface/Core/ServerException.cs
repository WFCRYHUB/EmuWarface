using System;
using System.Collections.Generic;
using System.Text;

namespace EmuWarface.Core
{
    public class ServerException : Exception
    {
        public ServerException(string message)
            : base (message)
        {

        }
    }
}
