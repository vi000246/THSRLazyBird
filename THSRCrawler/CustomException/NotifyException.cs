using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace THSRCrawler.CustomException
{
    public class NotifyException: Exception
    {
        public NotifyException()
        {
        }
        public NotifyException(string message)
            : base(message)
        {
        }

        public NotifyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
