namespace Server
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using Microshaoft;
    class EchoServer<T>
    {
        //Socket _socketListener;
        private Action<SocketAsyncDataHandler<T>, byte[]> _onReceivedDataProcessAction;
        public EchoServer
                    (
                        IPEndPoint localPoint
                        , Action
                            <
                                SocketAsyncDataHandler<T>
                                , byte[]
                            >
                            onReceivedDataProcessAction
                    )
        {
            _onReceivedDataProcessAction = onReceivedDataProcessAction;
            var listener = new Socket
                            (
                                localPoint.AddressFamily
                                , SocketType.Stream
                                , ProtocolType.Tcp
                            );
            listener.Bind(localPoint);
            listener.Listen(5);
            AcceptSocketAsync(listener);
        }
        private void AcceptSocketAsync(Socket listener)
        {
            var acceptSocketAsyncEventArgs = new SocketAsyncEventArgs();
            acceptSocketAsyncEventArgs.Completed += acceptSocketAsyncEventArgs_AcceptOneCompleted;
            listener.AcceptAsync(acceptSocketAsyncEventArgs);
        }
        private int _socketID = 0;
        void acceptSocketAsyncEventArgs_AcceptOneCompleted(object sender, SocketAsyncEventArgs e)
        {
            e.Completed -= acceptSocketAsyncEventArgs_AcceptOneCompleted;
            var client = e.AcceptSocket;
            var listener = sender as Socket;
            AcceptSocketAsync(listener);
            Console.WriteLine("Accepted");
            var handler = new SocketAsyncDataHandler<T>
                                    (
                                        client
                                        , _socketID++
                                    );
            handler.StartReceiveWholeDataPackets
                                (
                                   
                                     4
                                    , 0
                                    , 4
                                    ,() =>
                                    {
                                        var saea = new SocketAsyncEventArgs();
                                        saea.SetBuffer
                                                (
                                                    new byte[64*1024]
                                                    , 0
                                                    , 64 * 1024
                                                );
                                        return saea;
                                    }
                                    , (x, y, z) =>
                                    {
                                        _onReceivedDataProcessAction?.Invoke(x, y);
                                        return true;
                                    }
                                );
        }
    }
}