namespace Server
{
    using Microshaoft;
    using System;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;
    class Program
    {
        static void Main(string[] args)
        {
#if NETCOREAPP2_0
            Console.WriteLine(RuntimeInformation.OSArchitecture.ToString());
            Console.WriteLine(RuntimeInformation.OSDescription);
            Console.WriteLine(RuntimeInformation.FrameworkDescription);
# endif
            Console.Title = "Server";
            IPAddress ipa;
            IPAddress.TryParse("127.0.0.1", out ipa);
            var receiveEncoding = Encoding.UTF8;
            var sendEncoding = Encoding.UTF8;
            var es = new EchoServer<string>
                            (
                                new IPEndPoint(ipa, 18180)
                                , (x, y) =>
                                {
                                    var s = receiveEncoding.GetString(y);
                                    s = string
                                            .Format
                                                (
                                                    "Echo: {0}{1}{0}{2}{0}{3}{0}"
                                                    , "\r\n"
                                                    , s
                                                    , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                                                    , ""
                                                );
                                    Console.WriteLine(s);
                                    var buffer = sendEncoding.GetBytes(s);
                                    byte[] intBytes = BytesHelper.GetLengthHeaderBytes(buffer);
                                    x.SendDataSync(intBytes);
                                    x.SendDataSync(buffer);
                                }
                            );
            Console.WriteLine("Hello World");
            Console.WriteLine(Environment.Version.ToString());
            Console.ReadLine();
        }
    }
}
