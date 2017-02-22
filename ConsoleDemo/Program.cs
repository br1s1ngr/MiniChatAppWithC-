using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleDemo
{
    class Program
    {
        static string[] _krgstrTestData = {
                                              "a test line", 
                                              "another test line",
                                              "a very long line of text that will be sent to the server, which will then echo it so that the client can receive it again",
                                              "the next line is blank",
                                              "",
                                              "the p[revious line was blank",
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
                threadServer.IsBackground = true;
                threadServer.Start();
                areServerStarted.WaitOne();

                client.Start(new IPEndPoint(IPAddress.Loopback, ep.Port), _krgstrTestData);
                threadServer.Join();

                Console.WriteLine("client-server test succeeded");
            }
            catch (Exception ex)
            {
                Console.WriteLine("client-server test failed. error: \"" + ex.Message + "\"");
            }

            Console.ReadLine();
        }
    }
}
