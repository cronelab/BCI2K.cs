using System;
using BCI2K.cs;
using System.Threading;
using System.Threading.Tasks;

namespace Example
{
    class Program
    {
        public static BCI2K_OperatorConnection bci_Op = new BCI2K_OperatorConnection("ws://127.0.0.1:80");
        public static BCI2K_DataConnection bci_Source = new BCI2K_DataConnection("ws://127.0.0.1:20100");
        //public static BCI2K_DataConnection bci_Spect = new BCI2K_DataConnection("ws://127.0.0.1:20203");
        //public static BCI2K_DataConnection bci_Connector = new BCI2K_DataConnection("ws://127.0.0.1:20323");
        static void Main(string[] args)
        {
            //bci_Op.operatorWS.Connect();
            bci_Source.dataWS.Connect();
            //bci_Connector.dataWS.Connect();


            //bci_Source.onGenericSignal += () =>
            //{
            //    Console.WriteLine(bci_Source.signal[0]);
            //};
            bci_Source.onSignalProperties += () =>
            {
                //Console.WriteLine(bci_Source.sig.signaltype);

            };
            Console.ReadLine();
        }
    }
}
