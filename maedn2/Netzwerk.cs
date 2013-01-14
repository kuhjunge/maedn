using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

// Version 1.0.0.1
namespace maedn2
{
    // ---------- Server --------
    public class Server
    {
        // Variablen
        private string typ = "udp";
        EndPoint Remote = null; //udp
        private Thread thrMessaging; //udp
        private Thread thrBroad; //udp
        private string fehler = ""; 
        private ArrayList _workerList = ArrayList.Synchronized(new ArrayList()); //tcp
        private ManualResetEvent _allDone = new ManualResetEvent(false); //tcp
        private Socket _socBroad; //
        private Socket _socListen; //
        private Socket _socAccept; //tcp
        private const int BACKLOG = 3;  //tcp Größe der ausstehenden Queue
        private string sport = null;

        public string connect(string status = "udp", string port = "3663")                                              // !! Verbinden !!
        {
            typ = status;
            try
            {
                // Verwende den angegebenen Port, ansonsten nimm den Standardwert 3663
                int servPort = (port.Length > 0) ? Convert.ToInt32(port) : 3663;
                sport = port; 
                // Erzeuge neuen Socket / Erzeuge den Server Socket, der auf eingehende Verbindungen lauscht
                if (status == "v6")
                {
                    _socListen = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                    _socListen.Bind(new IPEndPoint(IPAddress.IPv6Loopback, servPort));
                    _socListen.Listen(BACKLOG); // Starte das Listening
                    _socListen.BeginAccept(new AsyncCallback(AcceptCallback), _socListen); // Rückruf für jeden Clienten
                    if (_socListen.Connected)
                    {
                        status = "Verbunden!";
                    }
                }
                else if (status == "v4") // ipv4
                {
                    _socListen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _socListen.Bind(new IPEndPoint(IPAddress.Any, servPort));
                    _socListen.Listen(BACKLOG); // Starte das Listening
                    _socListen.BeginAccept(new AsyncCallback(AcceptCallback), _socListen); // Rückruf für jeden Clienten
                    if (_socListen.Connected)
                    {
                        status = "Verbunden!";
                    }
                }
                else if (status == "udpv6") // udp
                {
                    IPEndPoint ip = new IPEndPoint(IPAddress.Any, servPort);
                    _socListen = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                    _socListen.Bind(ip);
                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, servPort);
                    Remote = (EndPoint)(sender);
                    thrMessaging = new Thread(new ThreadStart(receive));
                    thrMessaging.Start();
                    status = "Verbunden!";
                }
                else // udp
                {
                    IPEndPoint ip = new IPEndPoint(IPAddress.Any, servPort);
                    _socListen = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    _socListen.Bind(ip);
                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, servPort);
                    Remote = (EndPoint)(sender);
                    thrMessaging = new Thread(new ThreadStart(receive));
                    thrMessaging.Start();
                    status = "Verbunden!";
                }
            }
            catch (SocketException se)
            {
                status = se.Message;
            }
            return status;
        }

        public void close()
        {
            //MessageBox.Show(thrBroad.ThreadState.ToString());
          //  try { if (thrBroad.IsAlive) thrBroad.Abort(); }
          //  catch { }
          //  try { if (thrMessaging.IsAlive) thrMessaging.Abort(); }
           // catch { }
            _socListen.Close();
            if (_socBroad != null) _socBroad.Close();
            _socBroad = null;
            _socListen = null;
            _socAccept = null;

        }

        private void receive() // udp
        {
            try
            {
                while (_socListen != null)
                {
                    byte[] byteBuffer = new byte[1024];
                    int bytesRcvd = _socListen.ReceiveFrom(byteBuffer, ref Remote);
                    char[] chars = new char[bytesRcvd];

                    if (sport == "7")// Für Broadcast sonderlocke
                    {
                        OnUpdateText(new TextEventArgs(Remote.ToString()));
                    }
                    else
                    {
                        Decoder dec = Encoding.UTF8.GetDecoder();

                        int charLen = dec.GetChars(byteBuffer, 0, bytesRcvd, chars, 0);
                        string newData = new string(chars);
                        OnUpdateText(new TextEventArgs(newData)); // daten übergeben - invoke wird dort verarbeitet 
                    }
                }
            }
            catch
            {
                thrMessaging.Abort();
            }
        }

        public string send(string data) //beide
        {
            try
            {
                
                if (typ == "udp")
                {
                    byte[] byteBuffer = System.Text.Encoding.ASCII.GetBytes(data);
                    _socListen.SendTo(byteBuffer, 0, byteBuffer.Length, SocketFlags.None, Remote);
                }
                else
                {
                    
                    byte[] byteBuffer = System.Text.Encoding.ASCII.GetBytes(data + "~");
                    _socAccept.Send(byteBuffer);
                }
                try
                {
                    if (thrBroad.IsAlive) thrBroad.Abort();
                }
                catch { MessageBox.Show("Interner Fehler beim Senden!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                return data;
            }
            catch (Exception se)
            {
                return se.Message;
            }
        }

        private void AcceptCallback(IAsyncResult asyncResult) // nur für TCP
        {
            try
            {
                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                //The epSender identifies the incoming clients
                EndPoint epSender = (EndPoint)ipeSender;
                // Rückruf
                _socAccept = _socListen.EndAccept(asyncResult);
                thrMessaging = new Thread(new ThreadStart(WaitForData));
                thrMessaging.Start();
                OnUpdateText(new TextEventArgs("Eingehende Verbindung \r\n")); // Ereignis-Methode aufrufen
            }
            catch (Exception se)
            {
                fehler = "AcceptCallback: " + se.Message;
            }
        }

        private void WaitForData() // tcp
        {
            try
            {
                while (_socListen != null)
                {
                    // Empfange Daten
                    byte[] byteBuffer = new byte[1024]; //2048
                    int bytesRcvd;

                   bytesRcvd =_socAccept.Receive(byteBuffer); //tcp

                    char[] chars = new char[bytesRcvd];

                    Decoder dec = Encoding.UTF8.GetDecoder();
                    int charLen = dec.GetChars(byteBuffer, 0, bytesRcvd, chars, 0);
                    string newData = new string(chars);
                    string[] strarr = newData.Split('~');
                    foreach (string s in strarr)
                    {
                        OnUpdateText(new TextEventArgs(s)); // daten übergeben - invoke wird dort verarbeitet
                    }
                }
            }
            catch (SocketException se)
            {
                fehler = "WaitForData: " + se.Message;
            }
        }

      #region Broadcast
        // -- Broadcast Methoden -- 

        public void online(bool on = true)
        {
            try
            {
                if (on)
                {
                    IPEndPoint ip = new IPEndPoint(IPAddress.Any, 7);
                    _socBroad = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    _socBroad.Bind(ip);
                    thrBroad = new Thread(new ThreadStart(onlinerec));
                    thrBroad.Start();
                }
                else
                {
                     try { if (thrBroad.IsAlive) thrBroad.Abort(); }
                      catch { }
                    if (_socBroad != null) _socBroad.Close();
                    _socBroad = null;
                }

            }
            catch { }
        }

        private void onlinerec() // udp
        {
            try
            {
                while (_socBroad != null)
                {
                    EndPoint Broadc = null; //broadcast
                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 7);
                    Broadc = (EndPoint)(sender);
                    byte[] byteBuffer = new byte[1024];
                    int bytesRcvd = _socBroad.ReceiveFrom(byteBuffer, ref Broadc);
                    char[] chars = new char[bytesRcvd];
                    Decoder dec = Encoding.UTF8.GetDecoder();
                    int charLen = dec.GetChars(byteBuffer, 0, bytesRcvd, chars, 0);
                    string newData = new string(chars);
                    if (newData == "hello Server")
                    {
                        byteBuffer = System.Text.Encoding.ASCII.GetBytes("hello Client");
                        System.Threading.Thread.Sleep(200);
                        BroadCastSend(byteBuffer, 7);
                        OnUpdateText(new TextEventArgs("Antworte auf Scan")); // daten übergeben - invoke wird dort verarbeitet
                    }
                }
            }
            catch (Exception e)
            {
                OnUpdateText(new TextEventArgs("Fehler Broadcast: " + e.Message)); // Fehler Broadcast
            }
        }

        // -- Broadcast --
        /// <summary>
        /// Methode, um einen Broadcast im lokalen Netzwerk abzusetzen
        /// </summary>
        /// <param name="data">Daten, welche beim Broadcast mitgesendet werden</param>
        /// <param name="port">Zielport</param>
        public void BroadCastSend(byte[] data, int port)
        {
            this.BroadCastSend(data, IPAddress.Broadcast, port);
        }

        /// <summary>
        /// Methode, um einen Broadcast im lokalen Netzwerk abzusetzen
        /// </summary>
        /// <param name="data">Daten, welche beim Broadcast mitgesendet werden</param>
        /// <param name="ip">Protokollart</param>
        /// <param name="port">Zielport</param>
        public void BroadCastSend(byte[] data, IPAddress ip, int port)
        {//Socket definieren
            Socket bcSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//EndPoint definieren bzw. Ziel des Broadcastes
            IPEndPoint iep1 = new IPEndPoint(ip, port);
            bcSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);//Optionen auf den Socket binden
            bcSocket.SendTo(data, iep1);//Broadcast senden
            bcSocket.Close(); //Socket schliessen, nach erfolgreichem Senden des Broadcastes
        }
        #endregion 

        #region event
        // -- Event Managemant --
        public class TextEventArgs : EventArgs
        {
            public TextEventArgs(string text)
            {
                Text = text; // Eigenschaft setzen
            }
            public string Text { get; set; } // Text als Eigenschaft definieren
        }

        public event EventHandler<TextEventArgs> UpdateText; // Ereignis deklarieren

        protected virtual void OnUpdateText(TextEventArgs e)
        {
            EventHandler<TextEventArgs> ev = UpdateText;
            if (ev != null)
                ev(this, e); // abonnierte Ereignismethode(n) aufrufen
        }
        #endregion

        #region ipabfrage
        // -- IP Abfrage --
        // Zeige die lokale IP-Adresse des Servers
        //GetLocalAddresses()[0];
        /// <summary>
        /// Ermittelt die lokale IP-Adresse.
        /// </summary>
        /// <returns>Rückgabe einer Liste mit der lokalen IP.</returns>
        public static List<string> GetLocalAddresses(string hostName = "")
        {
            List<string> addresses = new List<string>();
            try
            {
                // Rufe den lokalen Hostnamen ab
                if (hostName == "") hostName = Dns.GetHostName();
                addresses.Add(hostName);
                // Find host by name
                IPHostEntry hostEntry = Dns.GetHostEntry(hostName);

                foreach (IPAddress ipaddr in hostEntry.AddressList)
                {
                    addresses.Add(ipaddr.ToString());
                }
            }
            catch (Exception e)
            {
                addresses.Add(e.Message);
            }
            return addresses;
        }
        #endregion
    }

    //  ------ Client --------
    public class Client
    {
        // variablen
        private Socket _clientSocket;
        private Thread thrMessaging;
        private bool client = false;
        private string typ = "udp";
        IPEndPoint udptarget = null;

        public string connect(string ipadresse, string port, string type = "udp", bool ipv4 = true)                                  // Verbinden
        {
            string message = null;
            // Sind wir bereits verbunden?
            if (_clientSocket != null && _clientSocket.Connected)
            {
                _clientSocket.Close();
            }
            try
            {
                // Verwende den angegebenen Host, ansonsten nimm den lokalen Host   string server = (ipadresse.Length > 0) ? ipadresse : "::1"; 
                typ = type; // UDP oder TCP setzen
                int servPort = (port.Length > 0) ? Convert.ToInt32(port) : 3663;
                IPAddress servIPAddress = IPAddress.Parse("127.0.0.1");
                try
                {
                    servIPAddress = IPAddress.Parse(ipadresse);
                }
                catch (FormatException)
                {
                        OnUpdateText(new TextEventArgs("Namensauflösung von " + ipadresse)); // daten übergeben - invoke wird dort verarbeitet
                       if (ipv4) servIPAddress = IPAddress.Parse(Server.GetLocalAddresses(ipadresse)[2]);
                       else servIPAddress = IPAddress.Parse(Server.GetLocalAddresses(ipadresse)[1]);
                }
                // Erzeuge neuen Socket
                if (typ == "tcp")
                {
                    IPEndPoint servEndPoint = new IPEndPoint(servIPAddress, servPort);
                    _clientSocket = new Socket(servIPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    _clientSocket.Connect(servEndPoint);
                    // Zeige dem User das wir verbunden sind
                    if (_clientSocket.Connected)
                    {
                        message = "Sie wurden zu " + ipadresse + " erfolgreich verbunden!";
                        client = true;
                        thrMessaging = new Thread(new ThreadStart(listening));
                        thrMessaging.Start();
                    }
                }
                else // udp
                {
                    IPEndPoint servEndPoint = new IPEndPoint(servIPAddress, servPort);
                    udptarget = new IPEndPoint(servIPAddress, servPort);
                    servEndPoint = new IPEndPoint(IPAddress.Any, servPort);
                    _clientSocket = new Socket(servIPAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    _clientSocket.Bind(servEndPoint);
                    message = "UDP Verbindung aufgebaut";
                    message = "Sie wurden zu " + ipadresse + " erfolgreich verbunden!";
                    client = true;
                    thrMessaging = new Thread(new ThreadStart(listening));
                    thrMessaging.Start();
                }
            }
            catch (SocketException se)
            {
                message = se.Message;
            }
            return message;
        }

        public string disconnect()                                   // Verbindung trennen
        {
            if (_clientSocket == null)
            {
                return "You must first connect to a Server!";
            }

            // Schließe den Socket
            if (_clientSocket.Connected)
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
                client = false;
            }

            _clientSocket.Close();

            return "Disconnected!";
        }

        public string send(string data)                                     // Daten senden
        {
            if (typ == "udp")
            {
                try
                {
                    byte[] byteBuffer = System.Text.Encoding.ASCII.GetBytes(data);
                    _clientSocket.SendTo(byteBuffer, 0, byteBuffer.Length, SocketFlags.None, udptarget);
                    return data;
                }
                catch (SocketException se)
                {
                    return se.Message;
                }
            }
            else if (_clientSocket == null || !_clientSocket.Connected)
            {
                return "You must first connect to a Server!";
            }
            else
            {
                try
                {
                    // Sende Daten
                    byte[] byteBuffer = System.Text.Encoding.ASCII.GetBytes(data + "~");
                    _clientSocket.Send(byteBuffer, 0, byteBuffer.Length, SocketFlags.None);
                    return data;
                }
                catch (SocketException se)
                {
                    return se.Message;
                }
            }
        }

        public string receive()
        {
            string rec = "Prozess existiert bereits.";
            Thread.Sleep(20);
            if (!thrMessaging.IsAlive)
            {
                thrMessaging = new Thread(new ThreadStart(listening));
                thrMessaging.Start();
                rec = "neues Listening gestartet.";
            }
            return rec;
        }

        public void listening()                                  // Daten empfangen
        {
            if ((_clientSocket == null || !_clientSocket.Connected) && typ != "udp")
            {
                OnUpdateText(new TextEventArgs("Erst den Server verbinden!")); // daten übergeben - invoke wird dort verarbeitet
                thrMessaging.Abort();
            }
            while (client)
            {
                try
                {
                    // Empfange Daten
                    EndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                    byte[] byteBuffer = new byte[1024]; //2048
                    int bytesRcvd;

                    if (typ == "udp")
                    {
                        bytesRcvd = _clientSocket.ReceiveFrom(byteBuffer, ref sender);
                        char[] chars = new char[bytesRcvd];

                        Decoder dec = Encoding.UTF8.GetDecoder();
                        int charLen = dec.GetChars(byteBuffer, 0, bytesRcvd, chars, 0);
                        string newData = new string(chars);
                        OnUpdateText(new TextEventArgs(newData)); // daten übergeben - invoke wird dort verarbeitet
                    }
                    else
                    {
                        bytesRcvd = _clientSocket.Receive(byteBuffer); //tcp
                        char[] chars = new char[bytesRcvd];

                        Decoder dec = Encoding.UTF8.GetDecoder();
                        int charLen = dec.GetChars(byteBuffer, 0, bytesRcvd, chars, 0);
                        string newData = new string(chars);
                        string[] strarr = newData.Split('~');
                        foreach (string s in strarr)
                        {
                            OnUpdateText(new TextEventArgs(s)); // daten übergeben - invoke wird dort verarbeitet
                        }
                    }

                }
                catch (Exception se)
                {
                    OnUpdateText(new TextEventArgs(se.Message)); // daten übergeben - invoke wird dort verarbeitet
                    break;
                }
            }
            thrMessaging.Abort();
        }

        // -- Event Managemant --
        public class TextEventArgs : EventArgs
        {
            public TextEventArgs(string text)
            {
                Text = text; // Eigenschaft setzen
            }
            public string Text { get; set; } // Text als Eigenschaft definieren
        }

        public event EventHandler<TextEventArgs> UpdateText; // Ereignis deklarieren

        protected virtual void OnUpdateText(TextEventArgs e)
        {
            EventHandler<TextEventArgs> ev = UpdateText;
            if (ev != null)
                ev(this, e); // abonnierte Ereignismethode(n) aufrufen
        }
    } // Client ende
}