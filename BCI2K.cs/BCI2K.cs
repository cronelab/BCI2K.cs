using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace BCI2K.cs
{
    public class BCI2K_OperatorConnection
    {
        Uri operatorUri;
        public ClientWebSocket operatorWS;

        public BCI2K_OperatorConnection(string address)
        {
            operatorUri = new Uri(address);
            operatorWS = new ClientWebSocket();
        }

        public async Task Connect()
        {
            try
            {
                await operatorWS.ConnectAsync(operatorUri, CancellationToken.None);
                Console.WriteLine("Connected");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }
        }


        public async Task Send(string msg)
        {

            if (operatorWS.State == WebSocketState.Open)
            {

                ArraySegment<byte> b = new ArraySegment<byte>(Encoding.UTF8.GetBytes("E 1 "+msg));
                await operatorWS.SendAsync(b, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task Receive()
        {
            ArraySegment<byte> buf = new ArraySegment<byte>(new byte[1024]);
            if (operatorWS.State == WebSocketState.Open)
            {

                var result = await operatorWS.ReceiveAsync(buf, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await operatorWS.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    Console.WriteLine(Encoding.UTF8.GetString(buf.Array, 0, result.Count));
                }
            }
        }



        public async void getVersion()
        {
            await Send("Version");        }

        public async void showWindow()
        {
            await Send("Show Window");
        }

        public async void hideWindow()
        {
            await Send("Hide Window");
        }

        public async void setWatch(string state, string ip, string port)
        {
            await Send("Add watch " + state + " at " + ip + ":" + port);
        }

        public async void resetSystem()
        {
            await Send("Reset System");
        }

        public async void setConfig()
        {
            await Send("Set Config");
        }

        public async void start()
        {
            await Send("Start");
        }

        public async void stop()
        {
            await Send("Stop");
        }

        public async void kill()
        {
            await Send("Exit");
        }

        public async void getSystemState()
        {
            await Send("Get System State");
        }

        public async void loadParameterFile(string prm)
        {
            await Send(string.Format("Load Parameterfile {0}; ", prm));
        }
        public async void setParameter(string prm1, string prm2)
        {
            await Send(string.Format("Set Parameter {0} {1}; ", prm1, prm2));
        }
        public async void getParameter(string prm = "Stimuli(1,2) ")
        {
            await Send(string.Format("Get Parameter {0};", prm));
        }
        public async void listParameter(string prm = "Stimuli")
        {
            await Send(string.Format("List Parameter {0}; ", prm));
        }
        public async void setState(string name, int value)
        {
            await Send(string.Format("Set STATE {0} {1}; ", name, value));
        }
        public async void addState(string name, int bitWidth, int initialVal)
        {
            await Send(string.Format("ADD STATE {0} {1} {2}; ", name, bitWidth, initialVal));
        }
        public async void setEvent(string name, float value)
        {
            await Send(string.Format("Set EVENT {0} {1}; ", name, value));
        }
        public async void addEvent(string name, int bitWidth, float initialVal)
        {
            await Send(string.Format("Add EVENT {0} {1} {2}; ", name, bitWidth, initialVal));
        }
    }
    public class BCI2K_DataConnection
    {
        Uri dataUri;
        ClientWebSocket dataWS;

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
        string[] signalProps;
        public List<float> signal = new List<float>();
        Dictionary<string, int> vecOrder = new Dictionary<string, int>();
        Dictionary<string, int> stateOrder = new Dictionary<string, int>();
        public List<KeyValuePair<string, int>> stateFormat;
        public List<KeyValuePair<string, int>> stateVecOrder;
        public BCI2K_DataConnection(string address)
        {
            dataUri = new Uri(address);
            dataWS = new ClientWebSocket();
        }


        ArraySegment<byte> buf = new ArraySegment<byte>(new byte[1024]);

        public async Task Connect()
        {
            try
            {
                await dataWS.ConnectAsync(dataUri, CancellationToken.None);
                Console.WriteLine("Connected");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }
        }

        public async Task Receive()
        {
            ArraySegment<byte> buf = new ArraySegment<byte>(new byte[1024]);
            if (dataWS.State == WebSocketState.Open)
            {
                var result = await dataWS.ReceiveAsync(buf, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await dataWS.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    if (buf.Array[0] == 3)
                    {
                        var msg = System.Text.Encoding.ASCII.GetString(buf.Array).Split('\n');
                        foreach (var mess in msg)
                        {

                            if (true)
                            {
                                Console.WriteLine(mess.Trim());
                                //                            Console.WriteLine(mess);
                                //                           Console.WriteLine(mess.Split(' ')[0]);
                                //                                Console.WriteLine(mess.Split(' ')[1]);
                                //                         Console.WriteLine("A");
                                //    stateName.Add(mess.Split(' ')[0]);
                                //  bitWidth.Add(int.Parse(mess.Split(' ')[1]));
                                //       defaultValue.Add(int.Parse(mess.Split(' ')[2]));
                                //     byteLocation.Add(int.Parse(mess.Split(' ')[3]));
                                //   bitLocation.Add(int.Parse(mess.Split(' ')[4]));
                            }
                        }
                        /*
                        for (int i = 0; i < stateName.Count; i++)
                        {
                            vecOrder.Add(stateName[i], byteLocation[i] * 8 + bitLocation[i]);
                            stateOrder.Add(stateName[i], bitWidth[i]);
                        }
                        stateFormat = vecOrder.ToList();
                        stateVecOrder = stateOrder.ToList();

                        //Sort the list based on key values
                        stateFormat.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
                        */
                    }
                    /*
                    if (buf.Array[0] == 4)
                    {
                        if(buf.Array[1] == 1)
                        {
                            byte signalType = buf.Array[2];
                            nChannels = buf.Array[3];
                            nChannels += buf.Array[4]; //if #channels > 255
                            nElements = buf.Array[5];
                            nElements += buf.Array[6]; //if #elements > 255
                        }
                    }
                    */
                    /*
        private void decodeGenericSignal(byte[] message)
        {
            byte signalType = message[2];
            nChannels = message[3];
            nChannels += message[4]; //if #channels > 255
            nElements = message[5];
            nElements += message[6]; //if #elements > 255

            byte[] signalArray = new byte[message.Length - 7];
            Array.Copy(message, 7, signalArray, 0, message.Length - 7);

            signal.Clear();
            for (int i = 0; i < nChannels * nElements; i++)
            {
                byte[] newArr = new byte[4];
                Array.Copy(signalArray, (4 * i), newArr, 0, 4);
                float myFloat = BitConverter.ToSingle(newArr, 0);
                signal.Add(myFloat);
            }
        }

        private void decodeStateFormat(byte[] message)
        {
            var msg = System.Text.Encoding.ASCII.GetString(message).Split('\n');
            foreach (var mess in msg)
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

        private void decodeSignalProperties(byte[] message)
        {
            string strMessage = System.Text.Encoding.ASCII.GetString(message);
            strMessage = strMessage.Replace("{", " { ");
            strMessage = strMessage.Replace("}", " } ");


            var msg = strMessage.Split(' ').ToList();
            for (int i = 0; i < msg.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(msg[i]))
                {
                    msg.Remove(msg[i]);
                }
            }


            signalName = msg[0];

            var count = 1;
            for (int i = 0; i < msg.Count; i++)
            {
                if (msg[i] == "{")
                {
                    i++;
                    while (msg[i] != "}")
                    {
                        if (count == 1)
                        {
                            signalChannels.Add(msg[i]);
                        }
                        else if (count == 2)
                        {
                            signalElements.Add(msg[i]);
                        }
                        i++;
                    }
                    count++;
                }
            }
            signalProps = new string[6 * signalChannels.Count];

            for (int i = 0; i < signalChannels.Count; i++)
            {
                signalProps[i * 6] = signalChannels[i];
                signalProps[i * 6 + 1] = signalElements[(i * 4) + i + 0];
                signalProps[i * 6 + 2] = signalElements[(i * 4) + i + 1];
                signalProps[i * 6 + 3] = signalElements[(i * 4) + i + 2];
                signalProps[i * 6 + 4] = signalElements[(i * 4) + i + 3];
                signalProps[i * 6 + 5] = signalElements[(i * 4) + i + 4];
            }
        }

        private void decodeStateVector(byte[] message)
        {
            for (int j = 1; j < message.Length; j++)
            {
                Console.WriteLine(message[j]);
            }
            int i = 1;
            List<byte> a = new List<byte>();
            while (message[i] != 0)
            {
                a.Add(message[i]);
                i++;
            }

            var zeroInd = 1;
            List<byte> stateVectorLength = new List<byte>();
            List<byte> subsStateVectors = new List<byte>();

            for (int k = 1; i < message.Length; k++)
            {
                // print("" + i + ": "+ message[i]);
                while (message[k] != 0 && zeroInd < 3)
                {
                    // print("" + i + ": " + message[i]);

                    if (zeroInd == 1)
                    {
                        stateVectorLength.Add(message[i]);
                    }
                    else
                    {
                        subsStateVectors.Add(message[i]);
                    }
                    k++;
                }
                zeroInd++;
            }

            // print(System.Text.Encoding.ASCII.GetString(stateVectorLength.ToArray()));       //56        //wtf do these mean?
            // print(System.Text.Encoding.ASCII.GetString(subsStateVectors.ToArray()));        //101

        }

        void OnBinaryMessageReceived(byte[] message)
        {
            // print(message[0]);
            if (message[0] == 3)
            {
                //Runs once upon open to give you the byte location and bit width of the states.
                decodeStateFormat(message);
            }
            else if (message[0] == 4)
            {
                if (message[1] == 1)
                {
                    decodeGenericSignal(message);
                }
                if (message[1] == 3)
                {
                    decodeSignalProperties(message);
                    Console.WriteLine("Channel: " + signalProps[0] + " " + "Offset: " + signalProps[1]);
                }
                else
                {
                    // UnityEngine.Debug.Log("This supplement is not currently supported");
                }
            }
            else if (message[0] == 5)
            {
                decodeStateVector(message);
            }
            else
            {
                // UnityEngine.Debug.Log("Unsupported descriptor");
            }
}
     */
                }
            }
        }










    }
}

