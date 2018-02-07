﻿//Client.cs
namespace Client
{
    using Microshaoft;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    class Class1
    {
        static void Main(string[] args)
        {
            Console.Title = "Client";
            var ipa = IPAddress.Parse("127.0.0.1");
            var socket = new Socket
                                (
                                    AddressFamily.InterNetwork
                                    , SocketType.Stream
                                    , ProtocolType.Tcp
                                );
            var ipep = new IPEndPoint(ipa, 18180);
            socket.Connect(ipep);

            //Console.ReadLine();
            var handler = new SocketAsyncDataHandler<string>
                                                        (
                                                            socket
                                                            , 1
                                                        );
            var sendEncoding = Encoding.UTF8;
            var receiveEncoding = Encoding.UTF8;

            handler
                .StartReceiveWholeDataPackets
                    (
                                    
                        4
                        , 0
                        , 4
                        , () =>
                        {
                            var saea = new SocketAsyncEventArgs();
                            saea.SetBuffer
                                    (
                                        new byte[64 * 1024]
                                        , 0
                                        , 64 * 1024
                                    );
                            return saea;
                        }
                        , (x, y, z) =>
                        {
                            var s = receiveEncoding.GetString(y);
                            //Console.WriteLine("SocketID: {1}{0}Length: {2}{0}Data: {2}", "\r\n", x.SocketID, y.Length ,s);
                            Console.Write(s);
                            return true;
                        }
                    );
            string input = string.Empty;
            while ((input = Console.ReadLine()) != "q")
            {
                try
                {
                    var buffer = sendEncoding.GetBytes(input);
                    var l = buffer.Length;
                    byte[] intBytes = BytesHelper.GetLengthHeaderBytes(buffer);
                    handler.SendDataSync(intBytes);
                    handler.SendDataSync(buffer);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        
    }
}