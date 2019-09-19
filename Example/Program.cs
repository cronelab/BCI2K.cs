using System;
using BCI2K.cs;
using System.Threading;
using System.Threading.Tasks;

namespace Example
{
    class Program
    {
        public static BCI2K_OperatorConnection bci_Op = new BCI2K_OperatorConnection("ws://127.0.0.1:80");
  //      public static BCI2K_DataConnection bci_Source = new BCI2K_DataConnection("ws://127.0.0.1:20100");
        static async Task Main(string[] args)
        {
            Thread.Sleep(1000);
            await bci_Op.Connect();
            while (bci_Op.operatorWS.State == System.Net.WebSockets.WebSocketState.Open)
            {
                //await bci_Op.Send("Get System State");
                bci_Op.getVersion();
                await bci_Op.Receive();
                Thread.Sleep(1000);


            }
            Console.ReadLine();
        }
    }
}
