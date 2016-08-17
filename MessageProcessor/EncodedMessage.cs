using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessor
{
    public class EncodedMessage : IMessage
    {
        private ISource source;
        private Encoding encoding;
        private char[] buffer;
        private int preambleSize;

        public EncodedMessage(ISource source, Encoding encoding)
        {
            this.source = source;
            this.encoding = encoding;
            preambleSize = 0;
        }

        public EncodedMessage(ISource source, Encoding encoding, bool detectEncoding)
        {
            this.source = source;
            this.encoding = encoding;

            preambleSize = 0;
            if (detectEncoding)
            {
                var buffer = source.GetChunk(0, 4);
                DetectEncoding(buffer);
            }
        }

        public Dictionary<string, object> Properties
        {
            get { return source.Properties; }
        }

        public long Size
        {
            get { return source.Size - preambleSize; }
        }

        public char[] GetRawMessage
        {
            get
            {
                return encoding.GetChars(source.GetRawMessage, preambleSize, (int)Size);
            }
        }

        public char[] GetChunk(long index, int size)
        {
            if (buffer == null)
            {
                buffer = GetRawMessage;
            }

            char[] chunk = new char[size];
            Array.Copy(buffer, index, chunk, 0, size);
            return chunk;
        }

        public char GetAt(long index)
        {
            if (buffer == null)
            {
                buffer = GetRawMessage;
            }

            return buffer[index];
        }

        public long Find(char[] toFind, long index)
        {
            var bytes = encoding.GetBytes(toFind);

            return source.Find(bytes, index + preambleSize);
        }

        public char[] GetUntil(char[] toFind)
        {
            var bytes = encoding.GetBytes(toFind);

            var data = source.GetBytesUntil(bytes);

            if (data == null)
            {
                return null;
            }
            return encoding.GetChars(data);
        }

        private void DetectEncoding(byte[] buffer)
        {
            if (buffer.Length < 2)
                return;

            bool changedEncoding = false;
            if (buffer[0] == 0xFE && buffer[1] == 0xFF)
            {
                // Big Endian Unicode

                encoding = new UnicodeEncoding(true, true);
                preambleSize = 2;
                changedEncoding = true;
            }

            else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
            {
                // Little Endian Unicode, or possibly little endian UTF32
                if (buffer.Length < 4 || buffer[2] != 0 || buffer[3] != 0)
                {
                    encoding = new UnicodeEncoding(false, true);
                    preambleSize = 2;
                    changedEncoding = true;
                }
                else
                {
                    encoding = new UTF32Encoding(false, true);
                    preambleSize = 4;
                    changedEncoding = true;
                }
            }

            else if (buffer.Length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
            {
                // UTF-8
                encoding = Encoding.UTF8;
                preambleSize = 3;
                changedEncoding = true;
            }
            else if (buffer.Length >= 4 && buffer[0] == 0 && buffer[1] == 0 &&
                     buffer[2] == 0xFE && buffer[3] == 0xFF)
            {
                // Big Endian UTF32
                encoding = new UTF32Encoding(true, true);
                preambleSize = 4;
                changedEncoding = true;
            }
           
            if (changedEncoding)
            {
                source.Seek(preambleSize);
            }
            else
            {
                source.Seek(0);
            }
        }

    }
}
