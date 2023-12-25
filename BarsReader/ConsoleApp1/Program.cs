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

            COMPortDataExchange port = new COMPortDataExchange("2");// Console.ReadLine());
            port.MesGot += Port_MessageReceived;


            Console.ReadKey();
            port.Disconnect();
            Console.ReadKey();


        }
        private static void Port_MessageReceived(byte[] mes)
        {
            Console.WriteLine($"Event.Read bytes as: {string.Join(", ", mes)}");

        }
    }


}
