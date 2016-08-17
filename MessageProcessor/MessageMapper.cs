using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessor
{
    public class Header
    {
        public string Date { get; set; }
        public string UserNumber { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        public string Branch { get; set; }
        public string OnUsSpecialInstruction { get; set; }
    }
    public class Row
    {
        public string Date { get; set; }
        public string UserNumber { get; set; }
        public string UserName { get; set; }
        public int UserType { get; set; }
    }

    public class Repss
    {
        public Header Header { get; set; }
        public List<Row> Row { get; set; }
    }

    class MessageMapper
    {
        public DataObject MapMessage(CSVMessage message)
        {
            DataObject obj = new DataObject();

            ExtractHeader(message, obj);
            ExtractRows(message, obj);

            return obj;
        }

        public Repss ValidateMessage(DataObject obj)
        {
            Repss newObj = new Repss();

            if (obj != null)
            {
                DataObject header = (DataObject)obj.GetField("Header");
                ValidateHeader(newObj, header);

                DataObjectList rows = (DataObjectList)obj.GetField("Rows");
                ValidateRows(newObj, rows);
            }
            return newObj;
        }

        private static void ValidateRows(Repss baseObj, DataObjectList rows)
        {
            if (rows != null)
            {
                List<Row> newObj = new List<Row>();

                foreach (var l in rows.Items)
                {
                    ValidateRow(newObj, l);
                }
                baseObj.Row = newObj;
            }
        }

        private static void ValidateRow(List<Row> baseObj, DataObject row)
        {
            if (row != null)
            {
                Row newObj = new Row();

                object f;
                f = row.GetField("Date");
                if (f != null)
                {
                    newObj.Date = f.ToString();
                }

                f = row.GetField("UserNumber");
                if (f != null)
                {
                    newObj.UserNumber = f.ToString();
                }

                f = row.GetField("UserName");
                if (f != null)
                {
                    newObj.UserName = f.ToString();
                }

                f = row.GetField("UserType");
                if (f != null)
                {
                    newObj.UserType = int.Parse(f.ToString());
                }

                baseObj.Add(newObj);
            }
        }
        private static void ValidateHeader(Repss newObj, DataObject header)
        {
            if (header != null)
            {
                newObj.Header = new Header();
                object f;
                f = header.GetField("Date");
                if (f != null)
                {
                    newObj.Header.Date = f.ToString();
                }

                f = header.GetField("UserNumber");
                if (f != null)
                {
                    newObj.Header.UserNumber = f.ToString();
                }

                f = header.GetField("UserName");
                if (f != null)
                {
                    newObj.Header.UserName = f.ToString();
                }

                f = header.GetField("UserType");
                if (f != null)
                {
                    newObj.Header.UserType = f.ToString();
                }

            }
        }

        private void ExtractRows(CSVMessage message, DataObject obj)
        {
            DataObjectList rows = new DataObjectList();
            while (message.GetLine())
            {
                ExtractRow(message, rows);
            }

            obj.AddField(new DataField()
            {
                Name = "Rows",
                Value = rows,
                Type = typeof(DataObjectList)
            });
        }

        private static bool ExtractRow(CSVMessage message, DataObjectList rows)
        {
            if (message.GetLine())
            {
                DataObject row = new DataObject();
                if (message.Fields.Count > 0)
                {
                    row.AddField(new DataField()
                    {
                        Name = "Date",
                        Type = typeof(string),
                        Value = message.Fields[0]
                    });
                }

                if (message.Fields.Count > 1)
                {
                    row.AddField(new DataField()
                    {
                        Name = "UserNumber",
                        Type = typeof(string),
                        Value = message.Fields[1]
                    });
                }

                if (message.Fields.Count > 2)
                {
                    row.AddField(new DataField()
                    {
                        Name = "UserName",
                        Type = typeof(string),
                        Value = message.Fields[2]
                    });
                }

                if (message.Fields.Count > 3)
                {
                    row.AddField(new DataField()
                    {
                        Name = "UserType",
                        Type = typeof(int),
                        Value = message.Fields[3]
                    });
                }

                rows.AddItem(row);

                return true;
            }

            return false;
        }

        private static bool ExtractHeader(CSVMessage message, DataObject obj)
        {
            // Get the Header
            if (message.GetLine())
            {
                DataObject header = new DataObject();
                if (message.Fields.Count > 0)
                {
                    header.AddField(new DataField()
                    {
                        Name = "Date",
                        Type = typeof(string),
                        Value = message.Fields[0]
                    });
                }

                if (message.Fields.Count > 1)
                {
                    header.AddField(new DataField()
                    {
                        Name = "UserNumber",
                        Type = typeof(string),
                        Value = message.Fields[1]
                    });
                }

                if (message.Fields.Count > 2)
                {
                    header.AddField(new DataField()
                    {
                        Name = "UserName",
                        Type = typeof(string),
                        Value = message.Fields[2]
                    });
                }

                if (message.Fields.Count > 3)
                {
                    header.AddField(new DataField()
                    {
                        Name = "UserType",
                        Type = typeof(string),
                        Value = message.Fields[3]
                    });
                }

                if (message.Fields.Count > 4)
                {
                    header.AddField(new DataField()
                    {
                        Name = "Branch",
                        Type = typeof(string),
                        Value = message.Fields[4]
                    });
                }

                if (message.Fields.Count > 5)
                {
                    header.AddField(new DataField()
                    {
                        Name = "OnUsSpecialInstruction",
                        Type = typeof(string),
                        Value = message.Fields[5]
                    });
                }

                obj.AddField(new DataField()
                {
                    Name = "Header",
                    Value = header,
                    Type = typeof(DataObject)
                });

                return true;
            }

            return false;
        }
    }



    public class RepssMapper
    {
        private bool MapHeader(CSVMessage message, DataObject obj)
        {
            // Get the line
            if (message.GetLine())
            {
                DataObject structure = new DataObject();
                if (message.Fields.Count > 0)
                {
                    structure.AddField(new DataField()
                    {
                        Name = "Date",
                        Type = typeof(string),
                        Value = message.Fields[0]
                    });
                }
                if (message.Fields.Count > 1)
                {
                    structure.AddField(new DataField()
                    {
                        Name = "UserNumber",
                        Type = typeof(string),
                        Value = message.Fields[1]
                    });
                }
                if (message.Fields.Count > 2)
                {
                    structure.AddField(new DataField()
                    {
                        Name = "UserName",
                        Type = typeof(string),
                        Value = message.Fields[2]
                    });
                }
                if (message.Fields.Count > 3)
                {
                    structure.AddField(new DataField()
                    {
                        Name = "UserType",
                        Type = typeof(string),
                        Value = message.Fields[3]
                    });
                }
                if (message.Fields.Count > 4)
                {
                    structure.AddField(new DataField()
                    {
                        Name = "Branch",
                        Type = typeof(string),
                        Value = message.Fields[4]
                    });
                }
                if (message.Fields.Count > 5)
                {
                    structure.AddField(new DataField()
                    {
                        Name = "OnUsSpecialInstruction",
                        Type = typeof(string),
                        Value = message.Fields[5]
                    });
                }
                obj.AddField(new DataField()
                {
                    Name = "Header",
                    Value = structure,
                    Type = typeof(DataObject)
                });
                return true;
            }
            return false;
        }
        private bool MapRow(CSVMessage message, DataObject obj)
        {
            DataObjectList list = new DataObjectList();
            while (MapRow(message, list))
            {
            }
            obj.AddField(new DataField()
            {
                Name = "Row",
                Value = list,
                Type = typeof(DataObjectList)
            });
            return true;
        }
        private bool MapRow(CSVMessage message, DataObjectList obj)
        {
            // Get the line
            if (message.GetLine())
            {
                DataObject structure = new DataObject();
                if (message.Fields.Count > 0)
                {
                    structure.AddField(new DataField()
                    {
                        Name = "Date",
                        Type = typeof(string),
                        Value = message.Fields[0]
                    });
                }
                if (message.Fields.Count > 1)
                {
                    structure.AddField(new DataField()
                    {
                        Name = "UserNumber",
                        Type = typeof(string),
                        Value = message.Fields[1]
                    });
                }
                if (message.Fields.Count > 2)
                {
                    structure.AddField(new DataField()
                    {
                        Name = "UserName",
                        Type = typeof(string),
                        Value = message.Fields[2]
                    });
                }
                if (message.Fields.Count > 3)
                {
                    structure.AddField(new DataField()
                    {
                        Name = "UserType",
                        Type = typeof(int),
                        Value = message.Fields[3]
                    });
                }
                obj.AddItem(structure);
                return true;
            }
            return false;
        }
        public DataObject MapMessage(CSVMessage message)
        {
            DataObject obj = new DataObject();
            MapHeader(message, obj);
            MapRow(message, obj);
            return obj;
        }
    }

}
