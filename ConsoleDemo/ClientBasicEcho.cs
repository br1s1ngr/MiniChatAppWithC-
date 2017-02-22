using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ConsoleDemo
{
    class ClientBasicEcho
    {
        private Encoding _code = Encoding.UTF8;
        private string _strMessage = "";
        private List<string> _lstrMessages = new List<string>();

        internal void Start(System.Net.IPEndPoint iPEndPoint, string[] rgstrMessages)
        {
            Socket sockConnect = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sockConnect.Connect(iPEndPoint);

            foreach (string strMessage in rgstrMessages)
                sockConnect.Send(_code.GetBytes(strMessage + "\0"));

            sockConnect.Shutdown(SocketShutdown.Send);
            byte[] rgb = new byte[8192];
            int cb;

            while ((cb = sockConnect.Receive(rgb)) > 0)
                _RecievedBytes(rgb, cb);

            sockConnect.Close();

            if (rgstrMessages.Length != _lstrMessages.Count)
                throw new Exception("different number of strings");

            for (int istr = 0; istr < rgstrMessages.Length; istr++)
                if (rgstrMessages[istr] != _lstrMessages[istr])
                    throw new Exception(string.Format("string pair not equal (\"{0}\" and \"{1}\")", rgstrMessages[istr], _lstrMessages[istr]));

        }

        private void _RecievedBytes(byte[] rgb, int cb)
        {
            int ichNull;

            _strMessage += _code.GetString(rgb, 0, cb);
            while ((ichNull = _strMessage.IndexOf("\0")) >= 0)
            {
                _lstrMessages.Add(_strMessage.Substring(0, ichNull));
                _strMessage = _strMessage.Substring(ichNull + 1);
            }
        }
    }
}
