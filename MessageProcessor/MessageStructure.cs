using System.IO;
using System.Xml.Serialization;

namespace MessageProcessor
{
    class MessageStructure
    {
        public Schema.Project Project;
        public void DoIt()
        {

            var txt = File.ReadAllText(@"C:\Work\Projects\CS\MessageProcessor\MessageProcessor\Schema\formqarch.xml");

            XmlSerializer serializer = new XmlSerializer(typeof(Schema.Project));
            StringReader reader = new StringReader(txt);
            Project = (Schema.Project)serializer.Deserialize(reader);
        }

      
    }
}
