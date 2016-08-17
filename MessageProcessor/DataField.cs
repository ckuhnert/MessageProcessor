using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageProcessor
{
    public class DataField
    {
        public object Value { get; set; }
        public Type Type { get; set; }
        public string Name { get; set; }
        public bool IsNull { get; set; }

        public override string ToString()
        {
            return Name + ": " + Value.ToString();
        }
    }
}
