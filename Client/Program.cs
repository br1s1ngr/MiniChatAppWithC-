using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static TcpListener client;
        static NetworkStream stream;
        static Socket s;

        static void Main(string[] args)
        {
            Console.WriteLine("Waiting.....");

            //IPHostEntry entryPt = Dns.GetHostEntry(Dns.GetHostName());
            //IPEndPoint endPt = new IPEndPoint(entryPt.AddressList[0], 18888);
             
            client = new TcpListener(IPAddress.Any, 18888);
            //Thread senderThd = new Thread(new ThreadStart(senderHandler));
            //senderThd.Start();
            //Thread recieverThd = new Thread(new ThreadStart(recieverHandler));
            //recieverThd.Start();
            client.Start();
            while (true)
            {
                //if (client.AcceptSocket())
                //s.Listen(-1);
                s = client.AcceptSocket();
                AsyncCallback acceptCallBack = new AsyncCallback(acceptHandler);
                
                s.BeginAccept(acceptCallBack, null);

                

                //receive(s);
                //sendBytes(s);
            }
            client.Stop();
        }

        private static void recieveHandler(IAsyncResult ar)
        {
            s.EndSend(ar);
        }

        private static void acceptHandler(IAsyncResult ar)
        {
            byte[] bytes = new byte[1];

            int bytesREcieved = 0;
            //do
            //{
            //bytesREcieved = stream.Read(bytes, 0, bytes.Length);
            //recievedBytesL.AddRange(bytes.Take(bytesREcieved));

            bytesREcieved = s.Receive(bytes);//(bytes, 0, bytes.Length);
            Console.WriteLine(Encoding.ASCII.GetString(bytes));
            //} while (bytesREcieved != 0);

            byte[] b = Encoding.ASCII.GetBytes("recieved");
            
            AsyncCallback recieveCallBack = new AsyncCallback(recieveHandler);
            s.BeginSend(b, 0, b.Length, SocketFlags.None, recieveCallBack, null);
        }

        private static void receive(Socket s)
        {
            //stream = s.Receive()
            //stream = client.GetStream();
            
            List<byte> recievedBytesL = new List<byte>();
            
            Console.WriteLine(Encoding.ASCII.GetString(recievedBytesL.ToArray()));
            //sendBytes();
        }

        private static void sendBytes(Socket s)
        {
            //byte[] b = Encoding.ASCII.GetBytes("recieved");
            //s.Send(b);
            //stream.Write(b, 0, b.Length);
            //s.
            //stream.Flush();
            //stream.Close();
        }
    }
}
