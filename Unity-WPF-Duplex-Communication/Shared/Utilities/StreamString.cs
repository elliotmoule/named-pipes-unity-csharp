using System;
using System.IO;
using System.Text;

namespace Shared.Utilities
{
    // Defines the data protocol for reading and writing strings on our stream
    // https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-use-named-pipes-for-network-interprocess-communication
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            string retVal = string.Empty;
            int len = ioStream.ReadByte();
            if (len > -1)
            {
                len *= 256;
                len += ioStream.ReadByte();
                byte[] inBuffer = new byte[len];
                ioStream.Read(inBuffer, 0, len);

                retVal = streamEncoding.GetString(inBuffer);
            }

            return retVal;
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }

            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();
            return outBuffer.Length + 2;
        }
    }
}
