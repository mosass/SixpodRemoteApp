using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Threading;

namespace SixpodRemoteApp
{
    enum REMOTECMD
    {
        SET_STEP_TIME = 31,
        SET_STEP_UPZ = 32,
        SET_SELECT_POS = 33,
        GET_FILE_LIST = 34,
        WALKING_STOP = 40,
        WALKING_WAV = 41,
        WALKING_RIPP = 42,
        WALKING_TRI = 43
    }

    class SixpodSocket
    {
        private MainWindow win;
        private IPAddress remoteIpAddress;

        private Socket remoteSocket;

        private const int remotePort = 9000;
        private const int debugPort = 9001;

        public bool logs_recv_flag { get; set; }

        public SixpodSocket(String Ip, MainWindow Window)
        {
            win = Window;
            remoteIpAddress = IPAddress.Parse(Ip);

            logs_recv_flag = false;
        }

        public void setIpAddress(string Ip)
        {
            remoteIpAddress = IPAddress.Parse(Ip);
        }

        public void setIpAddress(IPAddress Ip)
        {
            remoteIpAddress = Ip;
        }

        private void logsLine(String msg, bool newLine = true)
        {
            if (newLine)
            {
                win.txt_log.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action(() => win.txt_log.AppendText(msg + "\r\n")));
            }
            else
            {
                win.txt_log.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action(() => win.txt_log.AppendText(msg)));
            }
        }

        private void setUIOnlineStatus(bool onlineStatus)
        {
            string lbl_status, btn_status;
            if (onlineStatus)
            {
                lbl_status = "Online";
                btn_status = "Disconnect";
                logs_recv_flag = true;
            }
            else
            {
                lbl_status = "Offline";
                btn_status = "Connect";
                logs_recv_flag = false;
            }

            win.txt_statusmsg.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action(() => win.txt_statusmsg.Content = lbl_status));

            win.btn_connect.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action(() => win.btn_connect.Content = btn_status));
        }

        public void StartClient()
        {
            // Connect to a remote device.  
            try
            {
                // Create a TCP/IP  socket.  
                remoteSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint remoteEP = new IPEndPoint(remoteIpAddress, remotePort);
                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    remoteSocket.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        remoteSocket.RemoteEndPoint.ToString());

                    logsLine("Socket connected to " + remoteSocket.RemoteEndPoint.ToString());
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    logsLine("ArgumentNullException : " + ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                    logsLine("SocketException : " + se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    logsLine("Unexpected exception : " + e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                logsLine(e.ToString());
            }
        }

        public void StopClient()
        {
            // Connect the socket to the remote endpoint. Catch any errors.  
            try
            {
                // Release the socket.  
                remoteSocket.Shutdown(SocketShutdown.Both);
                remoteSocket.Close();
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                logsLine("ArgumentNullException : " + ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
                logsLine("SocketException : " + se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
                logsLine("Unexpected exception : " + e.ToString());
            }
        }

        public string sendCmd(REMOTECMD cmd, float val = 0)
        {
            String sb;
            switch (cmd)
            {
                case REMOTECMD.SET_STEP_TIME:
                case REMOTECMD.SET_STEP_UPZ:
                    sb = String.Format("{0:D} {1:F}", cmd, val);
                    writeToSocket(sb);
                    return readFormSocket();
                case REMOTECMD.SET_SELECT_POS:
                    sb = String.Format("{0:D} {1:D}", cmd, (int) val);
                    writeToSocket(sb);
                    return readFormSocket();
                case REMOTECMD.GET_FILE_LIST:
                case REMOTECMD.WALKING_RIPP:
                case REMOTECMD.WALKING_TRI:
                case REMOTECMD.WALKING_WAV:
                case REMOTECMD.WALKING_STOP:
                    sb = String.Format("{0:D}", cmd);
                    writeToSocket(sb);
                    return readFormSocket();
                default:
                    return "NACK";
            }
        }

        private void writeToSocket(string buf)
        {
            // Connect the socket to the remote endpoint. Catch any errors.  
            try
            {
                // Send the data through the socket.  
                int bytesSent = remoteSocket.Send(Encoding.ASCII.GetBytes(buf));
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                logsLine("ArgumentNullException : " + ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
                logsLine("SocketException : " + se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
                logsLine("Unexpected exception : " + e.ToString());
            }
        }

        private string readFormSocket()
        {
            // Connect the socket to the remote endpoint. Catch any errors.  
            byte[] bytes = new byte[2048];
            try
            {
                // Send the data through the socket.  
                int bytesRec = remoteSocket.Receive(bytes);
                return Encoding.ASCII.GetString(bytes, 0, bytesRec);
            }
            catch (ArgumentNullException ane)
            {
                return String.Format("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                return String.Format("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                return String.Format("Unexpected exception : {0}", e.ToString());
            }
        }

        public void StartDebugClient()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                // Create a TCP/IP  socket.  
                Socket sender = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint remoteEP = new IPEndPoint(remoteIpAddress, debugPort);
                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    logsLine("Socket connected to " + sender.RemoteEndPoint.ToString());

                    setUIOnlineStatus(true);

                    // Encode the data string into a byte array.  
                    //byte[] msg = Encoding.ASCII.GetBytes("GetLogs" + Environment.NewLine);

                    // Send the data through the socket.  
                    //int bytesSent = sender.Send(msg);

                    while (logs_recv_flag)
                    {
                        // Receive the response from the remote device.

                        int bytesRec = sender.Receive(bytes);
                        Console.WriteLine("Echoed test = {0}",
                                    Encoding.ASCII.GetString(bytes, 0, bytesRec));

                        logsLine(Encoding.ASCII.GetString(bytes, 0, bytesRec));
                    }

                    // Release the socket.  
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    setUIOnlineStatus(false);
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    logsLine("ArgumentNullException : " + ane.ToString());
                    setUIOnlineStatus(false);
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                    logsLine("SocketException : " + se.ToString());
                    setUIOnlineStatus(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    logsLine("Unexpected exception : " + e.ToString());
                    setUIOnlineStatus(false);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                logsLine(e.ToString());
                setUIOnlineStatus(false);
            }
        }
    }
}
