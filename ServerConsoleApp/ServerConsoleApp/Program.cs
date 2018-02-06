namespace Server
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;

    using Microshaoft;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(RuntimeInformation.OSArchitecture.ToString());
            Console.WriteLine(RuntimeInformation.OSDescription);
            Console.WriteLine(RuntimeInformation.FrameworkDescription);
            Console.Title = "Server";
            IPAddress ipa;
            IPAddress.TryParse("127.0.0.1", out ipa);
            var receiveEncoding = Encoding.Default;
            var sendEncoding = Encoding.UTF8;
            sendEncoding = Encoding.Default;
            var decoder = receiveEncoding.GetDecoder();
            var es = new EchoServer<string>
                            (
                                new IPEndPoint(ipa, 18180)
                                , (x, y) =>
                                {
                                    var s = Encoding.UTF8.GetString(y);
                                    Console.Write(s);
                                    s = string
                                            .Format
                                                (
                                                    "Echo: {0}{1}{0}{2}{0}{3}{0}"
                                                    , "\r\n"
                                                    , s
                                                    , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                                                    , RuntimeInformation.OSDescription
                                                );
                                    var buffer = Encoding.UTF8.GetBytes(s);
                                    byte[] intBytes = BytesHelper.GetLengthHeaderBytes(y);
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
