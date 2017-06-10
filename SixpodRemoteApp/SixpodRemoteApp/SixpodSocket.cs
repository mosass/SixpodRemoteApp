using Microsoft.Win32;
using System;
using System.IO;
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
        public bool remote_flag { get; set; }

        public SixpodSocket(String Ip, MainWindow Window)
        {
            win = Window;
            remoteIpAddress = IPAddress.Parse(Ip);

            logs_recv_flag = false;
            remote_flag = false;
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

            win.btn_connectLogs.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action(() => win.btn_connectLogs.Content = btn_status));
        }

        private void setUIRemoteOnlineStatus(bool onlineStatus)
        {
            string btn_status;
            if (onlineStatus)
            {
                btn_status = "Disconnect Remote";
                remote_flag = true;
            }
            else
            {
                btn_status = "Connect Remote";
                remote_flag = false;
            }

            win.btn_connectRemote.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action(() => win.btn_connectRemote.Content = btn_status));
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

                    setUIRemoteOnlineStatus(true);

                    logsLine("Remote connected");
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    logsLine("Remote connect error");
                    setUIRemoteOnlineStatus(false);
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                    logsLine("Remote connect error");
                    setUIRemoteOnlineStatus(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    logsLine("Remote connect error");
                    setUIRemoteOnlineStatus(false);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                logsLine("Remote connect error");
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
                logsLine("Remote disconnect");
                setUIRemoteOnlineStatus(false);
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                logsLine("Remote connect error");
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
                logsLine("Remote connect error");
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
                logsLine("Remote connect error");
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
                    SaveFileDialog svfile = new SaveFileDialog();
                    svfile.Filter = "CSV Files |*.csv";
                    svfile.ShowDialog();
                    StreamWriter swfile;
                    swfile = new StreamWriter(svfile.FileName, true);

                    sender.Connect(remoteEP);

                    Console.WriteLine("Log connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    logsLine("Log connected");

                    setUIOnlineStatus(true);

                    while (logs_recv_flag)
                    {
                        // Receive the response from the remote device.
                        int bytesRec = sender.Receive(bytes);
                        Console.WriteLine("{0}",
                                    Encoding.ASCII.GetString(bytes, 0, bytesRec));
                        logsLine(Encoding.ASCII.GetString(bytes, 0, bytesRec));

                        swfile.Write(Encoding.ASCII.GetString(bytes, 0, bytesRec));
                    }

                    swfile.Close();
                    // Release the socket.  
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    setUIOnlineStatus(false);
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    logsLine("Log connect error");
                    setUIOnlineStatus(false);
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                    logsLine("Log connect error");
                    setUIOnlineStatus(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    logsLine("Log connect error");
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
