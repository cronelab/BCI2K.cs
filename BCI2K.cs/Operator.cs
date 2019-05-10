using System;
using System.Collections.Generic;
using System.Linq;
using WebSocketSharp;

namespace BCI2K.cs
{
    public class Operator
    {
        public WebSocket ws;
        public Operator(string address)
        {
            ws = new WebSocket(address);
        }

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
        public WebSocket[] websockets;
        public WebSocket mainWebsocket, filterWebsocket, connectorWebsocket, sourceWebsocket;


        public void connect()
        {
            ws.Connect();
            ws.OnMessage += (sender, e) => OnBinaryMessageReceived(e.RawData);
            ws.OnOpen += (sender, e) => Console.WriteLine("Connected!");  
        }

        public void sendMsg(string msg)
        {
            ws.Send("E 1 " + msg);
        }

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

    }
}


//    public string showWin = "E 1 Show Window";
//    public string hideWin = "E 1 Hide Window";
//    public string resetSys = "E 1 Reset System";
//    public string startSys = "Startup system localhost";
//    public string setConf = "Set Config;";
//    public string exit = "Exit;";
//    public string stop = "Stop;";
//    public string start = "Start";
//    public string sysState = "Get System State";
//    public string startExecutable(string prm, string loc)
//    {
//        return string.Format("Start executable {0} --{1}; ", prm, loc);
//    }
//    public string waitFor(string prm)
//    {
//        return string.Format("Wait for {0}; ", prm);
//    }
//    public string setWatch(string prm, string ip, string port)
//    {
//        return string.Format("Add watch {0} at {1}:{2}; ", prm, ip, port);
//    }
//    public string loadParameterFile(string prm)
//    {
//        return string.Format("Load Parameterfile {0}; ", prm);
//    }
//    public string setParameter(string prm1, string prm2)
//    {
//        return string.Format("Set Parameter {0} {1}; ", prm1, prm2);
//    }
//    public string getParameter(string prm = "Stimuli(1,2) ")
//    {
//        return string.Format("Get Parameter {0};", prm);
//    }
//    public string listParameter(string prm = "Stimuli")
//    {
//        return string.Format("List Parameter {0}; ", prm);
//    }
//    public string setState(string name, int value)
//    {
//        return string.Format("Set STATE {0} {1}; ", name, value);
//    }
//    public string addState(string name, int bitWidth, int initialVal)
//    {
//        return string.Format("ADD STATE {0} {1} {2}; ", name, bitWidth, initialVal);
//    }
//    public string setEvent(string name, float value)
//    {
//        return string.Format("Set EVENT {0} {1}; ", name, value);
//    }
//    public string addEvent(string name, int bitWidth, float initialVal)
//    {
//        return string.Format("Add EVENT {0} {1} {2}; ", name, bitWidth, initialVal);
//    }
//    #endregion

//    public int ID;


//    public void Start()
//    {
//        websockets = new WebSocket[4];
//        websockets[0] = mainWebsocket;
//        websockets[1] = sourceWebsocket;
//        websockets[2] = filterWebsocket;
//        // websockets[3] = connectorWebsocket;
//        // mainWebsocket = new WebSocket(new Uri("ws://"+ IPaddress+":80"));
//        // mainWebsocket = websockets[0];
//        // sourceWebsocket = websockets[1];
//        openWS(websockets[0], "ws://" + IPaddress + ":80", 0);
//        openWS(websockets[1], "ws://" + IPaddress + ":20100", 1);
//        // openWS(websockets[2], "ws://" + IPaddress + ":20323", 2);

//    }

//    void openWS(WebSocket ws, string address, int ID)
//    {
//        if (ws == null)
//        {
//            websockets[ID] = new WebSocket(new Uri(address));
//            websockets[ID].OnOpen += OnOpen;
//            websockets[ID].OnClosed += OnClosed;
//            websockets[ID].OnError += OnError;
//            websockets[ID].OnMessage += OnMessageReceived;
//            websockets[ID].OnBinary += OnBinaryMessageReceived;
//            websockets[ID].Open();
//        }
//    }

//    public void sendWSmsg(string msg)
//    {
//        if (mainWebsocket != null && mainWebsocket.IsOpen)
//        {
//            Console.Write(mainWebsocket.IsOpen);
//            mainWebsocket.Send("E 1 " + msg);
//        }
//    }

//    void OnDestroy()
//    {
//        //mainWebsocket.Close();
//        //filterWebsocket.Close();
//        //sourceWebsocket.Close();
//        //connectorWebsocket.Close();
//    }



//    void OnClosed(WebSocket ws, UInt16 code, string message)
//    {
//        // print(string.Format("-WebSocket closed! Code: {0} Message: {1}\n", code, message));
//        mainWebsocket = null;
//        sourceWebsocket = null;
//        filterWebsocket = null;
//        connectorWebsocket = null;
//    }

//    void OnError(WebSocket ws, Exception ex)
//    {
//        string errorMsg = string.Empty;
//        if (!ws.IsOpen)
//        {
//            // print("A");
//            ws.Open();
//        }

//        // print(string.Format("-An error occured: {0}\n", (ex != null ? ex.Message : "Unknown Error " + errorMsg)));
//        mainWebsocket = null;
//    }

//    void OnOpen(WebSocket ws)
//    {
//        Console.Write("Websocket is open!");
//    }
//}
