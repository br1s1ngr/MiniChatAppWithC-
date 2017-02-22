using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
                static string[] _krgstrTestData =
        {
            "a test line",
            "another test line",
            "a very long line of text that will be sent to the server, which will then echo it so that the client can receive it again",
            "the next line is blank",
            "",
            "the previous line was blank",
            "one last line of text"
        };

        static void Main(string[] args)
        {
            try
            {
                ServerBasicEcho server = new ServerBasicEcho();
                ClientBasicEcho client = new ClientBasicEcho();
                AutoResetEvent areServerStarted = new AutoResetEvent(false);
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, 5005);

                Thread threadServer = new Thread(delegate() { server.Start(ep, areServerStarted); });

                // Setting IsBackground to true ensures that even if something
                // goes wrong, the server thread will exit when the main thread
                // does.  In a more sophisticated program, there'd be a mechanism
                // for explicitly shutting the server down, but this suffices
                // for this sample.
                threadServer.IsBackground = true;
                threadServer.Start();

                // Wait here for the server to start.  Otherwise, it's
                // possible the client could start before the server and
                // thus not be able to connect to it.
                areServerStarted.WaitOne();

                client.Start(new IPEndPoint(IPAddress.Loopback, ep.Port), _krgstrTestData);

                // Wait here until the server actually exits.  That way,
                // we can easily tell if the server isn't behaving as
                // expected.
                threadServer.Join();

                Console.WriteLine("client-server test succeeded!");
            }
            catch (Exception exc)
            {
                Console.WriteLine("client-server test failed. error: \"" + exc.Message + "\"");
            }

            Console.ReadLine();
        }
    }

    /// <summary>
    /// A very basic echo server.  It runs in a single thread, and serves
    /// just a single connection before terminating.  It receives null-
    /// terminated strings, and when the last string has been received from
    /// the client, it sends all of the strings back to the client.
    /// </summary>
    public class ServerBasicEcho
    {
        /// <summary>
        /// The text encoding used to convert between bytes and strings
        /// for the network i/o
        /// </summary>
        private Encoding _code = Encoding.UTF8;

        /// <summary>
        /// The string representing the message received so far
        /// </summary>
        private string _strMessage = "";

        /// <summary>
        /// The list of strings representing the complete, null-terminated
        /// messages we've received so far
        /// </summary>
        private List<string> _lstrMessages = new List<string>();

        /// <summary>
        /// Starts the server.  Returns when the server has completed all
        /// processing for a single client.
        /// </summary>
        /// <param name="epListen">The endpoint address on which to host the server</param>
        /// <param name="areStarted">The event for the caller to wait on so that it knows when the server's ready to accept a connection</param>
        public void Start(EndPoint epListen, AutoResetEvent areStarted)
        {
            Socket sockListen = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Sets up the listening socket, to which a client will make a connection request
            sockListen.Bind(epListen);
            sockListen.Listen(1);

            areStarted.Set();

            // Waits for a client to make a connection request
            Socket sockConnect = sockListen.Accept();

            // Only serving a single client, so close the listening socket
            // once we have a client to serve
            sockListen.Close();

            // We need a place to put bytes we've received and the count of
            // bytes actually received
            byte[] rgb = new byte[8192];
            int cb;

            // Socket.Receive() will return 0 when the client has finished
            // sending all of its data.  Just keep processing bytes until then.
            while ((cb = sockConnect.Receive(rgb)) > 0)
            {
                _ReceivedBytes(rgb, cb);
            }

            // Now, send each of the strings back to the client.
            foreach (string strMessage in _lstrMessages)
            {
                sockConnect.Send(_code.GetBytes(strMessage + ""));
            }

            // Complete the "graceful closure" of the socket
            sockConnect.Shutdown(SocketShutdown.Both);
            sockConnect.Close();
        }

        /// <summary>
        /// Processes any bytes that have been received
        /// </summary>
        /// <param name="rgb">The buffer containing the bytes</param>
        /// <param name="cb">The actual count of bytes received</param>
        private void _ReceivedBytes(byte[] rgb, int cb)
        {
            int ichNull;

            // Convert the current bytes to a string and append the
            // result to string accumulator
            _strMessage += _code.GetString(rgb, 0, cb);

            // Now, extract each null-terminated string from the
            // string accumulator
            while ((ichNull = _strMessage.IndexOf("")) >= 0)
            {
                _lstrMessages.Add(_strMessage.Substring(0, ichNull));

                _strMessage = _strMessage.Substring(ichNull + 1);
            }
        }
    }

    /// <summary>
    /// A very basic client.  It connects to the given server, sends
    /// each string in the array of strings passed to it, and then
    /// receives the echoed list of strings, comparing the received
    /// strings with the original list to make sure they are the same.
    /// </summary>
    public class ClientBasicEcho
    {
        /// <summary>
        /// The text encoding used to convert between bytes and strings
        /// for the network i/o
        /// </summary>
        private Encoding _code = Encoding.UTF8;

        /// <summary>
        /// The string representing the message received so far
        /// </summary>
        private string _strMessage = "";

        /// <summary>
        /// The list of strings representing the complete, null-terminated
        /// messages we've received so far
        /// </summary>
        private List<string> _lstrMessages = new List<string>();

        /// <summary>
        /// Starts the client.  Returns when it has successfully sent
        /// all of the strings passed to it, and the server has replied
        /// with an identical list.
        /// </summary>
        /// <param name="epConnect">The endpoint address of the server to connect to</param>
        /// <param name="rgstrMessages">The list of strings to send to the server</param>
        public void Start(EndPoint epConnect, string[] rgstrMessages)
        {
            Socket sockConnect = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            sockConnect.Connect(epConnect);

            foreach (string strMessage in rgstrMessages)
            {
                sockConnect.Send(_code.GetBytes(strMessage + ""));
            }

            // Calling Socket.Shutdown() with SocketShutdown.Send causes
            // the network driver to signal to the remote endpoint that
            // we are done _sending_ data.  We are still free to receive
            // any data the remote endpoint might send.
            sockConnect.Shutdown(SocketShutdown.Send);

            byte[] rgb = new byte[8192];
            int cb;

            // Socket.Receive() will return 0 when the server has finished
            // sending all of its data.  Just keep processing bytes until then.
            while ((cb = sockConnect.Receive(rgb)) > 0)
            {
                _ReceivedBytes(rgb, cb);
            }

            // We're done sending, so once the server is also done sending
            // we're done with the socket and can close it.
            sockConnect.Close();

            // Check the echoed strings against our originals
            if (rgstrMessages.Length != _lstrMessages.Count)
            {
                throw new Exception("different number of strings");
            }

            for (int istr = 0; istr < rgstrMessages.Length; istr++)
            {
                if (rgstrMessages[istr] != _lstrMessages[istr])
                {
                    throw new Exception(
                        string.Format("string pair not equal (\"{0}\" and \"{1}\")",
                        rgstrMessages[istr], _lstrMessages[istr]));
                }
            }
        }

        /// <summary>
        /// Processes any bytes that have been received
        /// </summary>
        /// <param name="rgb">The buffer containing the bytes</param>
        /// <param name="cb">The actual count of bytes received</param>
        private void _ReceivedBytes(byte[] rgb, int cb)
        {
            int ichNull;

            // Convert the current bytes to a string and append the
            // result to string accumulator
            _strMessage += _code.GetString(rgb, 0, cb);

            // Now, extract each null-terminated string from the
            // string accumulator
            while ((ichNull = _strMessage.IndexOf("")) >= 0)
            {
                _lstrMessages.Add(_strMessage.Substring(0, ichNull));

                _strMessage = _strMessage.Substring(ichNull + 1);
            }
        }
    }
}
