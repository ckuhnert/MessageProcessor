using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessor
{
    public interface IMessage
    {
        Dictionary<string, object> Properties { get; }

        long Size { get; }
        char[] GetRawMessage { get; }
        char[] GetChunk(long index, int size);
        char GetAt(long index);
        long Find(char[] toFind, long index);
        char[] GetUntil(char[] toFind);
    }
}
