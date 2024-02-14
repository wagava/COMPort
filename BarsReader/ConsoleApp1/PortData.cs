using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class PortData
    {
        /// <summary>
        /// Class <c>PortData</c> contains port work states.
        /// </summary>

        public enum ConnectionStatus
        {
            Connected,
            Disconnected
        }
        public enum ExchangeState
        {
            Free,
            Sending,
            Idle,
            Receiving,
            Timeout,
            Error
        }
    }
}
