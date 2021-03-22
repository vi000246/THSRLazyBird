using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace THSRCrawler
{
    public class CritialPageErrorException : Exception
    {
        public CritialPageErrorException()
        {
        }
        public CritialPageErrorException(string message)
            : base(message)
        {
        }

        public CritialPageErrorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
