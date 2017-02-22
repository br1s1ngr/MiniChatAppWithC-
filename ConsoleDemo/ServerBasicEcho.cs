using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ConsoleDemo
{
    class ServerBasicEcho
    {
        private Encoding _code = Encoding.UTF8;
        private string _strMessage = "";
        private List<string> _lstrMessages = new List<string>();

        internal void Start(System.Net.IPEndPoint epListen, System.Threading.AutoResetEvent areServerStarted)
        {
            Socket sockListen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            sockListen.Bind(epListen);
            sockListen.Listen(1);

            areServerStarted.Set();
            Socket sockConnect = sockListen.Accept();
            sockListen.Close();

            byte[] rgb = new byte[8192];
            int cb;

            while ((cb = sockConnect.Receive(rgb)) > 0)
                _RecievedBytes(rgb, cb);

            foreach (string strMessage in _lstrMessages)
                sockConnect.Send(_code.GetBytes(strMessage + "\0"));

            sockConnect.Shutdown(SocketShutdown.Both);
            sockConnect.Close();
        }

        private void _RecievedBytes(byte[] rgb, int cb)
        {
            int ichNull;

            _strMessage += _code.GetString(rgb, 0, cb);

            while ((ichNull = _strMessage.IndexOf("\0")) >= 0)
            {
                _lstrMessages.Add(_strMessage.Substring(0, ichNull));

                if (_strMessage.Length == 0)
                    break;
                
                _strMessage = _strMessage.Substring(ichNull + 1);
            }
        }
     
    }
}
