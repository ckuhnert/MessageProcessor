using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MessageProcessor
{
    public class FileSource : ISource
    {
        const int BUFSIZE = 1024;
        private FileStream file;
        private long currentPos;
        private Dictionary<string, object> properties;

        public FileSource(string filename)
        {
            file = File.OpenRead(filename);


            properties = new Dictionary<string, object>();
            properties["Name"] = Path.GetFileNameWithoutExtension(filename);
            properties["Extension"] = Path.GetExtension(filename);
            properties["Path"] = Path.GetDirectoryName(filename);
            properties["Size"] = file.Length;
        }

        public Dictionary<string, object> Properties
        {
            get { return properties; }
        }

        public long Size
        {
            get { return file.Length; }
        }

        public byte[] GetRawMessage
        {
            get
            {
                byte[] bytes = new byte[Size];
                file.Read(bytes, 0, (int)Size);

                return bytes;
            }
        }

        public byte[] GetChunk(long index, int size)
        {
            byte[] bytes = new byte[size];

            if (currentPos != index)
            {
                file.Seek(index, SeekOrigin.Begin);
            }

            int actualSize = file.Read(bytes, 0, (int)size);
            if (actualSize != size)
            {
                byte[] actual = new byte[actualSize];
                Array.Copy(bytes, actual, actualSize);
                currentPos = index + actualSize;
                return actual;
            }
            currentPos = index + actualSize;
            return bytes;

        }

        public byte[] GetNextBytes(int size)
        {
            return GetChunk((int)currentPos, size);
        }


        public byte GetAt(long index)
        {
            file.Seek(index, SeekOrigin.Begin);
            currentPos = index + 1;
            return (byte)file.ReadByte();
        }

         public void Seek(long index)
        {
            file.Seek(index, SeekOrigin.Begin);
            currentPos = index;
        }

        public long Find(byte[] toFind, long index)
        {
            bool hasFound = false;
            int continueOnNextBuf = 0;
            long pos = index;
            var buf = GetChunk(index, BUFSIZE);

            while (buf.Length > 0 && !hasFound)
            {
                for (int b = 0; b < buf.Length; b++)
                {
                    if (continueOnNextBuf > 0 || buf[b] == toFind[0])
                    {
                        bool found = true;
                        for (int i = continueOnNextBuf; i < toFind.Length; i++)
                        {
                            if (b + i >= buf.Length)
                            {
                                continueOnNextBuf = i;
                                break;
                            }

                            if (buf[b+i] != toFind[i])
                            {
                                found = false;
                                continueOnNextBuf = 0;
                                b = b + i;
                                break;
                            }
                        }

                        if (found)
                        {
                            hasFound = true;
                            pos = pos + b;
                            break;
                        }
                    }
                }

                if (!hasFound)
                {
                    buf = GetNextBytes(BUFSIZE);
                }
            }

            if (!hasFound)
            {
                return -1;
            }

            return pos;
        }

        public byte[] GetBytesUntil(byte[] toFind)
        {
            bool hasFound = false;
            int continueOnNextBuf = 0;
            int pos = 0;
            var buf = GetNextBytes(BUFSIZE);

            List<byte> results = new List<byte>();

            while (buf.Length > 0 && !hasFound)
            {
                for (int b = 0; b < buf.Length; b++)
                {
                    if (continueOnNextBuf > 0 || buf[b] == toFind[0])
                    {
                        bool found = true;
                        for (int i = continueOnNextBuf; i < toFind.Length; i++)
                        {
                            if (b + i >= buf.Length)
                            {
                                continueOnNextBuf = i;
                                break;
                            }

                            if (buf[b + i] != toFind[i])
                            {
                                found = false;
                                continueOnNextBuf = 0;
                                b = b + i;
                                break;
                            }
                        }

                        if (found)
                        {
                            hasFound = true;
                            pos = b;
                            break;
                        }
                    }
                }

                if (!hasFound)
                {
                    results.AddRange(buf);
                    buf = GetNextBytes(BUFSIZE);
                }
            }

            if (hasFound)
            {
                currentPos -= (buf.Length - pos);
                currentPos += toFind.Length;
                file.Seek(currentPos, SeekOrigin.Begin);
                results.AddRange(buf.Take(pos + toFind.Length));
            }

            if (buf.Length == 0)
            {
                return null;
            }

            return results.ToArray();
        }

    }
}
