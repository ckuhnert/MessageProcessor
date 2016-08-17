using System.Collections.Generic;

namespace MessageProcessor
{
    public interface IRawMessage
    {
        Dictionary<string, object> Properties { get; }

        long Size { get; }
        byte[] GetRawMessage { get; }
        byte[] GetChunk(long index, int size);
        byte[] GetNextBytes(int size);
        byte GetAt(long index);
        void Seek(long index);
        long Find(byte[] toFind, long index);
        byte[] GetBytesUntil(byte[] toFind);
    }
}
