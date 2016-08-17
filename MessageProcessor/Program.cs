using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessor
{
    class Program
    {
        static void Main(string[] args)
        {

            MessageStructure struc = new MessageStructure();

            struc.DoIt();



            var cls = new CSharp.Generator();
            cls.GeneratePropertyType = CSharp.Generator.PropertyType.GenerateProperties;
            var stt = cls.GenerateStructures(struc.Project);
            var stt2 = cls.GenerateDataMapper(struc.Project);
            FileSource src = new FileSource("C:\\Temp\\0859_REPSS_2016-06-10_Daily.csv");

            CSVMessage csv = new CSVMessage(src, Encoding.UTF8);

            RepssMapper mp = new RepssMapper();
            var dataObject = mp.MapMessage(csv);
            MessageMapper mapper = new MessageMapper();
            //var dataObject = mapper.MapMessage(csv);

            var realObject = mapper.ValidateMessage(dataObject);

            //string line ;
            //do
            //{
            //    line = csv.GetLine();
            //    Console.WriteLine("Found {0} Fields", csv.Fields.Count);
            //} while (!string.IsNullOrEmpty(line));
            //csv.GetLine();
            //csv.GetLine();
            //csv.GetLine();
            //csv.GetLine();
            //csv.GetLine();
            //int lines = 0;
            //char[] pos = null;
            //do
            //{
            //    pos = msg.GetUntil(new char[] { '\r', '\n' });

            //    string foundString = new string(pos);

            //    //Console.Write(foundString);
            //    lines++;

            //} while (pos != null && pos.Count() > 0);

            //Console.WriteLine("There are " + lines.ToString() + " lines");

        }
    }
}
