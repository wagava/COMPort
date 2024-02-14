using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {


            Console.WriteLine("Port number: ");
            string portnum = Console.ReadLine();
            Console.WriteLine("Slave ID: ");
            string slaveid = Console.ReadLine();
            COMPortDataExchange port = new COMPortDataExchange(portnum, slaveid);

            Console.ReadKey();
            port.Disconnect();
            Console.ReadKey();
        }

    }


}
