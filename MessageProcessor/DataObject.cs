using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessor
{
    public class DataObject
    {
        private List<DataField> Fields;

        public DataObject()
        {
            Fields = new List<DataField>();
        }

        public object GetField(string name)
        {
            var f = Fields.FirstOrDefault(x => x.Name == name);
            if (f != null)
            {
                return f.Value;
            }

            return null;
        }

        public bool IsFieldNull(string name)
        {
            var f = Fields.FirstOrDefault(x => x.Name == name);
            if (f != null)
            {
                return f.IsNull;
            }

            return true;
        }

        public DataObject GetSection(string name)
        {

            var f = Fields.FirstOrDefault(x => x.Name == name);
            if (f != null && f.Type == typeof(DataObject))
            {
                return (DataObject)f.Value;
            }

            return null;
        }

        public void AddField(DataField field)
        {
            Fields.Add(field);
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            foreach (var f in Fields)
            {
                if (b.Length > 0)
                {
                    b.Append(",");
                }
                b.Append(f.Value);
            }
            return "Structure: " + b.ToString();
        }
    }

    class DataObjectList
    {
        private List<DataObject> list;

        public DataObjectList()
        {
            list = new List<DataObject>();
        }

        public DataObject GetAt(int index)
        {
            return list[index];
        }

        public void AddItem(DataObject item)
        {
            list.Add(item);
        }

        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        public List<DataObject> Items
        {
            get
            {
                return list;
            }
        }

        public override string ToString()
        {
            return "List: " + list.Count.ToString() + " items";
        }
    }
}
