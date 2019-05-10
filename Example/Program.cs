using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCI2K.cs;
using WebSocketSharp;
namespace Example
{
    class Program
    {
        public static Operator bci = new Operator("ws://127.0.0.1:20100");
        static void Main(string[] args)
        {

            bci.connect();
            Console.Read();
            //while (true)
            //{
            //    Console.WriteLine("BCI Command: ");
            //    bci.sendMsg(Console.ReadLine());
            //}
        }
    }
}
