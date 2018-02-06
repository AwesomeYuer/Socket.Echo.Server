namespace Microshaoft
{
    using System;
    public static class BytesHelper
    {
     

        public static byte[] GetLengthHeaderBytes(byte[] bodyBytes, int headerBytesLength = 4)
        {
            byte[] headerBytes = new byte[headerBytesLength];
            var l = bodyBytes.Length;
            var lengthBytes = BitConverter.GetBytes(l);
            Buffer.BlockCopy
                        (
                            lengthBytes
                            , 0
                            , headerBytes
                            , 0
                            , headerBytes.Length
                        );
            return headerBytes;
        }
    }
}
