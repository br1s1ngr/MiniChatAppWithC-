using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatAppWithSingleSocket
{
    class Program
    {
        static void Main(string[] args)
        {

            TcpClient client = new TcpClient("127.0.0.1", 8088);


            //Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.Bind(new IPEndPoint(IPAddress.Any, 8888));
            //socket.Listen(3);


        }
    }
}
