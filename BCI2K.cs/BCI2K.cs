using System;
using System.Collections.Generic;
using WebSocketSharp;
using System.Text;
using System.Linq;
namespace BCI2K.cs
{
    public class BCI2K_OperatorConnection
    {
        public WebSocket operatorWS;
        private int messageCount = 0;
        public BCI2K_OperatorConnection(string address)
        {
            operatorWS = new WebSocket(address);
            operatorWS.OnMessage += (sender, e) => OnMessageReceived(e.Data);
        }

        private void OnMessageReceived(string message)
        {
            string[] arr = message.Split(' ');

            string opcode = arr[0];
            string id = arr[1];
            string[] msg = arr.SubArray(2, arr.Length - 2);
            switch (opcode)
            {
                case "S":
                    Console.WriteLine(opcode);
                    Console.WriteLine(id);
                    break;
                case "O":
                    Console.WriteLine(opcode);
                    Console.WriteLine(id);
                    StringBuilder builder = new StringBuilder();

                    foreach (var line in msg)
                    {
                        builder.Append(line);
                        builder.Append(" \n");
                    }
                    Console.WriteLine(builder.ToString());
                    break;
                case "D":
                    Console.WriteLine(opcode);
                    Console.WriteLine(id);
                    break;
                default:
                    break;
            }
            //messageCount++;
            //Console.WriteLine(messageCount);
            //Console.WriteLine(message);
        }

        public void getVersion()
        {
            operatorWS.Send($"E {messageCount} Version");
        }

        public void showWindow()
        {
            operatorWS.Send($"E {messageCount} Show Window");
        }

        public void hideWindow()
        {
            operatorWS.Send($"E {messageCount} Hide Window");
        }

        public void setWatch(string state, string ip, string port)
        {
            operatorWS.Send($"E {messageCount} Add watch " + state + " at " + ip + ":" + port);
        }

        public void resetSystem()
        {
            operatorWS.Send($"E {messageCount} Reset System");
        }

        public void setConfig()
        {
            operatorWS.Send($"E {messageCount} Set Config");
        }

        public void start()
        {
            operatorWS.Send($"E {messageCount} Start");
        }

        public void stop()
        {
            operatorWS.Send($"E {messageCount} Stop");
        }

        public void kill()
        {
            operatorWS.Send($"E {messageCount} Exit");
        }

        public void getSystemState()
        {
            operatorWS.Send($"E {messageCount} Get System State");
        }

        public void loadParameterFile(string prm)
        {
            operatorWS.Send($"E {messageCount} Load Parameterfile {prm}; ");
        }
        public void addParameter(string prm)
        {
            operatorWS.Send($"E {messageCount} Add Parameter {prm}; ");
        }
        public void setParameter(string prm1, string prm2)
        {
            operatorWS.Send($"E {messageCount} Set Parameter {prm1} {prm2}; ");
        }
        public void getParameter(string prm = "Stimuli(1,2) ")
        {
            operatorWS.Send($"E {messageCount} Get Parameter {prm};");
        }
        public void listParameter(string prm = "Stimuli")
        {
            operatorWS.Send($"E {messageCount} List Parameter {prm}; ");
        }
        public void setState(string name, int value)
        {
            operatorWS.Send($"E {messageCount} Set STATE {name} {value}; ");
        }
        public void addState(string name, int bitWidth, int initialVal)
        {
            operatorWS.Send($"E {messageCount} ADD STATE {name} {bitWidth} {initialVal}; ");
        }
        public void setEvent(string name, float value)
        {
            operatorWS.Send($"E {messageCount} Set EVENT {name} {value}; ");
        }
        public void addEvent(string name, int bitWidth, float initialVal)
        {
            operatorWS.Send($"E {messageCount} Add EVENT {name} {bitWidth} {initialVal}; ");
        }
        public void startExecutable(string name, string arguments)
        {
            operatorWS.Send($"E {messageCount} Start executable {name} --{arguments}; ");
        }
        public void startupSystem()
        {
            operatorWS.Send($"E {messageCount} Startup System; ");
        }
    }

    public class BCI2K_DataConnection
    {
        public WebSocket dataWS;
        List<string> stateName = new List<string>();
        List<int> bitWidth = new List<int>();
        List<int> defaultValue = new List<int>();
        List<int> byteLocation = new List<int>();
        List<int> bitLocation = new List<int>();
        List<string> signalChannels = new List<string>();
        List<string> signalElements = new List<string>();
        public int nElements;
        public int nChannels;
        public string signalName;

        public List<float> signal = new List<float>();
        Dictionary<string, int> vecOrder = new Dictionary<string, int>();
        Dictionary<string, int> stateOrder = new Dictionary<string, int>();
        public List<KeyValuePair<string, int>> stateFormat;
        public List<KeyValuePair<string, int>> stateVecOrder;

        public struct units
        {
            public int[] offset;
            public int[] gain;
            public string[] symbol;
            public int[] vmin;
            public int[] vmax;
        }
        public struct SignalProperties
        {
            public string name;
            public List<string> channels;
            public List<string> elements;
            public int numelements;
            public string signaltype;
            public units channelUnit;
            public units elementUnit;
            public units valueUnit;
        }
        public SignalProperties sig;
        
        public BCI2K_DataConnection(string address)
        {
            dataWS = new WebSocket(address);
            dataWS.OnMessage += (sender, e) => OnBinaryMessageReceived(e.RawData);
        }
        
        public event Action onGenericSignal;
        public event Action onSignalProperties;

        private void OnBinaryMessageReceived(byte[] msg)
        {
            if (msg[0] == 3)
            {
                decodeStateFormat(msg);
            }
            else if (msg[0] == 4)
            {
                if (msg[1] == 1)
                {
                    decodeGenericSignal(msg);
                }
                if (msg[1] == 3)
                {
                    decodeSignalProperties(msg);
                }
            }
            else if (msg[0] == 5)
            {
                //decodeStateVector(msg);
            }

        }
        private void decodeStateFormat(byte[] msg)
        {

            var message = Encoding.ASCII.GetString(msg).Split('\n');
            foreach (var mess in message)
            {
                if (mess != "")
                {
                    stateName.Add(mess.Split(' ')[0]);
                    bitWidth.Add(int.Parse(mess.Split(' ')[1]));
                    defaultValue.Add(int.Parse(mess.Split(' ')[2]));
                    byteLocation.Add(int.Parse(mess.Split(' ')[3]));
                    bitLocation.Add(int.Parse(mess.Split(' ')[4]));
                }
            }
            for (int i = 0; i < stateName.Count; i++)
            {
                vecOrder.Add(stateName[i], byteLocation[i] * 8 + bitLocation[i]);
                stateOrder.Add(stateName[i], bitWidth[i]);
            }
            stateFormat = vecOrder.ToList();
            stateVecOrder = stateOrder.ToList();

            //Sort the list based on key values
            stateFormat.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
        }
        private void decodeGenericSignal(byte[] msg)
        {
            byte signalType = msg[2];
            nChannels = msg[3];
            if (nChannels == 0)
            {
                nChannels = msg[4] + 255;
            }
            nElements = msg[5];
            if (nElements == 0)
            {
                nElements = msg[6] + 255;
            }
            byte[] signalArray = new byte[msg.Length - 7];
            Array.Copy(msg, 7, signalArray, 0, msg.Length - 7);

            signal.Clear();
            for (int i = 0; i < nChannels * nElements; i++)
            {
                byte[] newArr = new byte[4];
                Array.Copy(signalArray, (4 * i), newArr, 0, 4);
                float myFloat = BitConverter.ToSingle(newArr, 0);
                signal.Add(myFloat);
            }
            onGenericSignal?.Invoke();
        }
        private void decodeSignalProperties(byte[] message)
        {
            sig.channels = new List<string>();
            sig.elements = new List<string>();
            
            string propstr = Encoding.ASCII.GetString(message);
            propstr = propstr.Replace("{", " { ");
            propstr = propstr.Replace("}", " } ");
            var props = propstr.Split(' ').ToList();
            for (int i = 0; i < props.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(props[i]))
                {
                    props.Remove(props[i]);
                }
            }
            int pidx = 0;
            sig.name = props[pidx++];

            if (props[pidx] == "{")
            {
                while (props[++pidx] != "}")
                {
                    sig.channels.Add(props[pidx]);
                }
                pidx++;
            }
            else
            {
                int numChannels = Int32.Parse(props[pidx++]);
                for (int i = 0; i < numChannels; i++)
                {
                    sig.channels.Add((i + 1).ToString());
                }
            }
            if (props[pidx] == "{")
            {
                while (props[++pidx] != "}")
                {
                    sig.elements.Add(props[pidx]);
                }
                pidx++;
            }
            else
            {
                int numChannels = Int32.Parse(props[pidx++]);
                for (int i = 0; i < numChannels; i++)
                {
                    sig.elements.Add((i + 1).ToString());
                }
            }
            sig.numelements = sig.elements.Count;
            sig.signaltype = props[pidx++];
            Console.WriteLine("Channel Offset");
            Console.WriteLine(props[pidx++].Trim()); //offset
            Console.WriteLine("Channel gain");
            Console.WriteLine(props[pidx++].Trim()); //gain 
            Console.WriteLine("Channel symbol");
            Console.WriteLine(props[pidx++].Trim()); //symbol
            Console.WriteLine("Channel vmin");
            Console.WriteLine(props[pidx++].Trim()); //vmin
            Console.WriteLine("Channel vmax");
            Console.WriteLine(props[pidx++].Trim()); //vmax
            Console.WriteLine("Element Offset");
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine("Element gain");
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine("Element symbol");
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine("Element vmin");
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine("Element vmax");
            Console.WriteLine(props[pidx++].Trim());
            pidx++;
            for(int i = 0; i < sig.channels.Count; i++)
            {
                //sig.valueUnit.offset;
            }
            Console.WriteLine("Value Offset");
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine("Value gain");
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine("Value symbol");
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine("Value vmin");
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine("Value vmax");
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine("Value Offset");
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine(props[pidx++].Trim());
            Console.WriteLine(props[pidx++].Trim());
            

            //signalProps = new string[6 * signalChannels.Count];

            //for (int i = 0; i < signalChannels.Count; i++)
            //{
            //    signalProps[i * 6] = signalChannels[i];
            //    signalProps[i * 6 + 1] = signalElements[(i * 4) + i + 0];
            //    signalProps[i * 6 + 2] = signalElements[(i * 4) + i + 1];
            //    signalProps[i * 6 + 3] = signalElements[(i * 4) + i + 2];
            //    signalProps[i * 6 + 4] = signalElements[(i * 4) + i + 3];
            //    signalProps[i * 6 + 5] = signalElements[(i * 4) + i + 4];
            //}
            onSignalProperties?.Invoke();
        }
    }
        //private void decodeStateVector(byte[] msg)
        //{
        //    for (int j = 1; j < msg.Length; j++)
        //    {
        //        //Console.WriteLine(msg[j]);
        //    }
        //    int i = 1;
        //    List<byte> a = new List<byte>();
        //    while (msg[i] != 0)
        //    {
        //        a.Add(msg[i]);
        //        i++;
        //    }

        //    var zeroInd = 1;
        //    List<byte> stateVectorLength = new List<byte>();
        //    List<byte> subsStateVectors = new List<byte>();

        //    for (int k = 1; i < msg.Length; k++)
        //    {
        //        // print("" + i + ": "+ message[i]);
        //        while (msg[k] != 0 && zeroInd < 3)
        //        {
        //            // print("" + i + ": " + message[i]);

        //            if (zeroInd == 1)
        //            {
        //                stateVectorLength.Add(msg[i]);
        //            }
        //            else
        //            {
        //                subsStateVectors.Add(msg[i]);
        //            }
        //            k++;
        //        }
        //        zeroInd++;
        //    }

        //    //Console.WriteLine(System.Text.Encoding.ASCII.GetString(stateVectorLength.ToArray()));       //56        //wtf do these mean?
        //    //Console.WriteLine(System.Text.Encoding.ASCII.GetString(subsStateVectors.ToArray()));        //101

        //}
    }
    public class BCI2K_ConnectorConnection
    {
        public WebSocket connectorWS;

        public BCI2K_ConnectorConnection(string address)
        {
            connectorWS = new WebSocket(address);
            connectorWS.OnMessage += (sender, e) => OnMessageReceived(e.Data);

        }

        private void OnMessageReceived(string message)
        {
            //Console.WriteLine(message);
        }
    }

