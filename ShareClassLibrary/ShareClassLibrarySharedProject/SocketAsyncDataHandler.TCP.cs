namespace Microshaoft
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    public partial class SocketAsyncDataHandler<T>
    {
        private bool _isUdp = false;
        public bool IsUdp
        {
            get
            {
                return _isUdp;
            }
        }
        private Socket _socket;
        public Socket WorkingSocket
        {
            get
            {
                return _socket;
            }
        }
        public int ReceiveDataBufferLength
        {
            get
            {
                return 128 * 1024;
            }
        }
        public SocketAsyncEventArgs ReceiveSocketAsyncEventArgs
        {
            get;
            private set;
        }
        public T Context
        {
            get;
            set;
        }
        public IPAddress RemoteIPAddress
        {
            get
            {
                return 
                    (
                        (IPEndPoint) _socket.RemoteEndPoint
                    ).Address;
            }
        }
        public IPAddress LocalIPAddress
        {
            get
            {
                return
                    (
                        (IPEndPoint) _socket.LocalEndPoint
                    ).Address;
            }
        }
        public int SocketID
        {
            get;
            private set;
        }
        public bool HasNoWorkingSocket
        {
            get
            {
                return (_socket == null);
            }
        }
        public SocketAsyncDataHandler
                            (
                                Socket socket
                                , int socketID
                            )
        {
            _socket = socket;
            _isUdp = (_socket.ProtocolType == ProtocolType.Udp);
            _sendSocketAsyncEventArgs = new SocketAsyncEventArgs();
            SocketID = socketID;
        }
        private SocketAsyncEventArgs _sendSocketAsyncEventArgs;
        public int HeaderBytesLength
        {
            get;
            private set;
        }
        public int HeaderBytesOffset
        {
            get;
            private set;
        }
        private long _receivedCount = 0;
        public long ReceivedCount
        {
            get
            {
                return _receivedCount;
            }
        }
        public int HeaderBytesCount
        {
            get;
            private set;
        }
        private bool _isStartedReceiveData = false;
        private bool _isHeader = true;
        public bool StartReceiveWholeDataPackets
                            (
                                int headerBytesLength
                                , int headerBytesOffset
                                , int headerBytesCount
                                , Func<SocketAsyncEventArgs> getReceiveSocketAsyncEventArgsProcessFunc
                                , Func
                                    <
                                        SocketAsyncDataHandler<T>
                                        , byte[]
                                        , SocketAsyncEventArgs
                                        , bool
                                    > onOneWholeDataPacketReceivedProcessFunc
                                , Func
                                    <
                                        SocketAsyncDataHandler<T>
                                        , byte[]
                                        , SocketAsyncEventArgs
                                        , bool
                                    > onReceivedDataPacketErrorProcessFunc = null
                                , Func
                                    <
                                        SocketAsyncDataHandler<T>
                                        , SocketAsyncEventArgs
                                        , Exception
                                        , bool
                                    > onCaughtExceptionProcessFunc = null
                            )
        {
            return
                StartReceiveWholeDataPackets
                    (
                        headerBytesLength
                        , headerBytesOffset
                        , headerBytesCount
                        , getReceiveSocketAsyncEventArgsProcessFunc()
                        , onOneWholeDataPacketReceivedProcessFunc
                        , onReceivedDataPacketErrorProcessFunc
                        , onCaughtExceptionProcessFunc
                    );
        }

        private EventHandler<SocketAsyncEventArgs> _receiveSocketAsyncEventArgsCompletedEventHandlerProcessAction = null;
        public bool StartReceiveWholeDataPackets
                            (
                                int headerBytesLength
                                , int headerBytesOffset
                                , int headerBytesCount
                                , SocketAsyncEventArgs receiveSocketAsyncEventArgs
                                , Func
                                    <
                                        SocketAsyncDataHandler<T>
                                        , byte[]
                                        , SocketAsyncEventArgs
                                        , bool
                                    > onOneWholeDataPacketReceivedProcessFunc
                                , Func
                                    <
                                        SocketAsyncDataHandler<T>
                                        , byte[]
                                        , SocketAsyncEventArgs
                                        , bool
                                    > onReceivedDataPacketErrorProcessFunc = null
                                , Func
                                    <
                                        SocketAsyncDataHandler<T>
                                        , SocketAsyncEventArgs
                                        , Exception
                                        , bool
                                    > onCaughtExceptionProcessFunc = null
                            )
        {
            if (!_isStartedReceiveData)
            {
                HeaderBytesLength = headerBytesLength;
                HeaderBytesOffset = headerBytesOffset;
                HeaderBytesCount = headerBytesCount;
                ReceiveSocketAsyncEventArgs = receiveSocketAsyncEventArgs;
                int bodyLength = 0;
                if (ReceiveSocketAsyncEventArgs.Buffer == null)
                {
                    ReceiveSocketAsyncEventArgs
                            .SetBuffer
                                (
                                    new byte[ReceiveDataBufferLength]
                                    , 0
                                    , HeaderBytesLength
                                );
                }
                if
                    (
                        ReceiveSocketAsyncEventArgs.Count != 0
                        ||
                        ReceiveSocketAsyncEventArgs.Offset != HeaderBytesLength
                    )
                {
                    ReceiveSocketAsyncEventArgs
                                            .SetBuffer
                                                (
                                                    0
                                                    , HeaderBytesLength
                                                );
                }
                ReceiveSocketAsyncEventArgs
                                .Completed +=
                                    (
                                        (sender, e) =>
                                        {
                                            var socket = sender as Socket;
                                            if (e.BytesTransferred >= 0)
                                            {
                                                byte[] buffer = e.Buffer;
                                                int r = e.BytesTransferred;
                                                int p = e.Offset;
                                                int l = e.Count;
                                                if (r < l)
                                                {
                                                    p += r;
                                                    // issue: reset buffer's Offset property and Count Property
                                                    e.SetBuffer(p, l - r);
                                                }
                                                else if (r == l)
                                                {
                                                    if (_isHeader)
                                                    {
                                                        byte[] data = new byte[headerBytesCount];
                                                        Buffer
                                                            .BlockCopy
                                                                (
                                                                    buffer
                                                                    , HeaderBytesOffset
                                                                    , data
                                                                    , 0
                                                                    , data.Length
                                                                );
                                                        byte[] intBytes = new byte[4];
                                                        l =
                                                                (
                                                                    intBytes.Length < HeaderBytesCount
                                                                    ?
                                                                    intBytes.Length
                                                                    :
                                                                    HeaderBytesCount
                                                                );
                                                        Buffer
                                                            .BlockCopy
                                                                (
                                                                    data
                                                                    , 0
                                                                    , intBytes
                                                                    , 0
                                                                    , l
                                                                );
                                                        //Array.Reverse(intBytes);
                                                        bodyLength = BitConverter.ToInt32(intBytes, 0);
                                                                        p += r;
                                                        // issue: reset buffer's Offset property and Count Property
                                                        e.SetBuffer(p, bodyLength);
                                                                        _isHeader = false;
                                                    }
                                                    else
                                                    {
                                                        byte[] data = new byte[bodyLength + HeaderBytesLength];
                                                        bodyLength = 0;
                                                        Buffer
                                                            .BlockCopy
                                                                (
                                                                    buffer
                                                                    , 0
                                                                    , data
                                                                    , 0
                                                                    , data.Length
                                                                );
                                                        _isHeader = true;
                                                        // issue: reset buffer's Offset property and Count Property
                                                        e.SetBuffer(0, HeaderBytesLength);
                                                                        onOneWholeDataPacketReceivedProcessFunc?
                                                                            .Invoke
                                                                                (
                                                                                    this
                                                                                    , data
                                                                                    , e
                                                                                );
                                                    }
                                                }
                                                else
                                                {
                                                    if (onReceivedDataPacketErrorProcessFunc != null)
                                                    {
                                                        byte[] data = new byte[p + r + HeaderBytesLength];
                                                        Buffer
                                                            .BlockCopy
                                                                (
                                                                    buffer
                                                                    , 0
                                                                    , data
                                                                    , 0
                                                                    , data.Length
                                                                );
                                                        bool b = onReceivedDataPacketErrorProcessFunc
                                                                    (
                                                                        this
                                                                        , data
                                                                        , e
                                                                    );
                                                        if (b)
                                                        {
                                                            bool i = DestoryWorkingSocket();
                                                        }
                                                        else
                                                        {
                                                            _isHeader = true;
                                                            // issue: reset buffer's Offset property and Count Property
                                                            e.SetBuffer(0, HeaderBytesLength);
                                                        }
                                                    }
                                                }
                                            }
                                            if (!_isWorkingSocketDestoryed)
                                            {
                                                try
                                                {
                                                    // loop ReceiveAsync
                                                    // issue: after reset SocketAsyncEventArgs.Buffer's offset property and count property
                                                    // , can't raise completed event
                                                    //socket.ReceiveAsync(e);
                                                    ReceiveAsyncCompleted(_socket, ReceiveSocketAsyncEventArgs);
                                                }
                                                catch (Exception exception)
                                                {
                                                    var r = false;
                                                    if (onCaughtExceptionProcessFunc != null)
                                                    {
                                                        r = onCaughtExceptionProcessFunc
                                                    (
                                                        this
                                                        , e
                                                        , exception
                                                    );
                                                    }
                                                    if (r)
                                                    {
                                                        DestoryWorkingSocket();
                                                    }
                                                }
                                            }
                                        }
                                    );
                //_socket.ReceiveAsync(ReceiveSocketAsyncEventArgs);
                ReceiveAsyncCompleted(_socket, ReceiveSocketAsyncEventArgs);
                _isStartedReceiveData = true;
            }
            return _isStartedReceiveData;
        }


        private void ReceiveAsyncCompleted(Socket socket, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            var willRaiseEvent = false;
            do
            {
                r = socket.ReceiveAsync(ReceiveSocketAsyncEventArgs);
            }
            while (!willRaiseEvent);
        }

        private bool _isWorkingSocketDestoryed = false;
        public bool DestoryWorkingSocket()
        {
            //bool r = false;
            try
            {
                ReceiveSocketAsyncEventArgs.Completed -= _receiveSocketAsyncEventArgsCompletedEventHandlerProcessAction;

                if (_socket.Connected)
                {
                    _socket.Disconnect(false);
                }
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket.Dispose();
                _socket = null;
                _isWorkingSocketDestoryed = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                //r = false;
            }
            return _isWorkingSocketDestoryed;
        }
        private object _sendSyncLockObject = new object();
        public int SendDataSync(byte[] data)
        {
            var length = data.Length;
            var sentOffset = 0;
            if (!_isUdp)
            {
                lock (_sendSyncLockObject)
                {
                    while (length > sentOffset)
                    {
                        try
                        {
                            sentOffset +=
                                            _socket
                                                .Send
                                                    (
                                                        data
                                                        , sentOffset
                                                        , length - sentOffset
                                                        , SocketFlags.None
                                                    //, out socketError
                                                    );
                        }
                        catch //(SocketException se)
                        {
                            //socketError = socketError + se.SocketErrorCode;
                            break;
                        }
                        //catch (Exception e)
                        //{
                        //    break;
                        //}
                    }
                }
            }
            return sentOffset;
        }
        public int SendDataSyncWithRetry
                (
                    byte[] data
                    , int retry = 3
                    , int sleepInMilliseconds = 0
                )
        {
            //增加就地重试机制
            int r = -1;
            int i = 0;
            int l = data.Length;

            while (i < retry)
            {
                r = -1;
                //lock (_sendSyncLockObject)
                {
                    try
                    {
                        if (_socket != null)
                        {
                            r = SendDataSync(data);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                i++;
                if (r == l || i == retry)
                {
                    break;
                }
                else
                {
                    if (sleepInMilliseconds > 0)
                    {
                        Thread.Sleep(sleepInMilliseconds);
                    }
                }
            }
            return r;
        }
    }
}

