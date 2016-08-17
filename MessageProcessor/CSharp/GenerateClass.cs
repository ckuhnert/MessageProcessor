using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessor.CSharp
{
    public class Generator
    {
        public enum PropertyType
        {
            GenerateFields,
            GenerateProperties,
            GenerateNotifyProperties,
        }

        private static Dictionary<string, string> baseTypes;

        static Generator()
        {
            baseTypes = new Dictionary<string, string>();
            baseTypes["byte"] = "byte";
            baseTypes["date"] = "System.DateTime";
            baseTypes["decimal"] = "decimal";
            baseTypes["integer"] = "int";
            baseTypes["long"] = "long";
            baseTypes["string"] = "string";
            baseTypes["boolean"] = "bool";
        }

        public static string MapCSharpType(string baseType)
        {
            if (baseTypes.ContainsKey(baseType.ToLowerInvariant()))
            {
                return baseTypes[baseType.ToLowerInvariant()];
            }

            return baseType;
        }

        public static string ResolveType(Schema.ProjectDefinition definition, string baseType)
        {
            if (IsNativeType(baseType))
            {
                return MapCSharpType(baseType);
            }
            else
            {
                foreach (var x in definition.Items)
                {
                    if (x is Schema.Type)
                    {
                        if ((x as Schema.Type).name == baseType)
                        {
                            return ResolveType(definition, (x as Schema.Type).@base);
                        }
                    }
                }

                return MapCSharpType(baseType);
            }
        }

        public static bool IsNativeType(string baseType)
        {
            return baseTypes.ContainsKey(baseType.ToLowerInvariant());
        }

        public PropertyType GeneratePropertyType { get; set; }

        private bool IsMultiple(Schema.Element e)
        {
            return e.maxOccurs != "1" && !string.IsNullOrEmpty(e.maxOccurs);
        }

        #region Data Mapper
        public string GenerateDataMapper(Schema.Project project)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var m in project.MessageDefinitions)
            {
                builder.AppendFormat("public class {0}Mapper\r\n", m.name);
                builder.AppendLine("{");

                foreach (var m1 in m.Items)
                {
                    if (m1 is Schema.Element)
                    {
                        GenerateTopLevelMapperElement(builder, m, m1 as Schema.Element);
                    }
                }

                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        private void GenerateMapperElementForList(StringBuilder builder, Schema.Element e)
        {
            builder.AppendFormat("   private bool Map{0}(CSVMessage message, DataObject obj)\r\n", e.name);

            builder.AppendLine  ("   {");
            builder.AppendLine  ("      DataObjectList list = new DataObjectList();");

            builder.AppendFormat("      while (Map{0}(message, list))\r\n", e.name);
            builder.AppendFormat("      {{\r\n");
            builder.AppendFormat("      }}\r\n");

            builder.AppendFormat("      obj.AddField(new DataField()\r\n");
            builder.AppendFormat("      {{\r\n");
            builder.AppendFormat("          Name = \"{0}\",\r\n", e.name);
            builder.AppendFormat("          Value = list,\r\n");
            builder.AppendFormat("          Type = typeof(DataObjectList)\r\n");
            builder.AppendFormat("      }});\r\n");
                                        
            builder.AppendLine("        return true;");
            builder.AppendLine("   }");
        }

        private void GenerateMapperElement(StringBuilder builder, Schema.ProjectDefinition definition, Schema.Element e)
        {
            builder.AppendFormat("   private bool Map{0}(CSVMessage message, ", e.name);
            if (IsMultiple(e))
            {
                builder.AppendFormat("DataObjectList obj)\r\n");
            }
            else
            {
                builder.AppendFormat("DataObject obj)\r\n");
            }

            builder.AppendLine("   {");
            builder.AppendLine("      // Get the line");
            builder.AppendLine("      if (message.GetLine())");
            builder.AppendLine("      {");
            builder.AppendLine("         DataObject structure = new DataObject();");

            int counter = 0;
            foreach (var el in e.Element1)
            {

                builder.AppendFormat("         if (message.Fields.Count > {0})\r\n", counter);
                builder.AppendFormat("         {{\r\n");
                builder.AppendFormat("             structure.AddField(new DataField()\r\n");
                builder.AppendFormat("             {{\r\n");
                builder.AppendFormat("                 Name = \"{0}\",\r\n", el.name);
                builder.AppendFormat("                 Type = typeof({0}),\r\n", ResolveType(definition, el.type));
                builder.AppendFormat("                 Value = message.Fields[{0}]\r\n", counter);
                builder.AppendFormat("             }});\r\n");
                builder.AppendFormat("         }}\r\n");
                counter++;
            }


            if (IsMultiple(e))
            {
                builder.AppendFormat("         obj.AddItem(structure);\r\n");
            }
            else
            {
                builder.AppendFormat("         obj.AddField(new DataField()\r\n");
                builder.AppendFormat("         {{\r\n");
                builder.AppendFormat("             Name = \"{0}\",\r\n", e.name);
                builder.AppendFormat("             Value = structure,\r\n");
                builder.AppendFormat("             Type = typeof(DataObject)\r\n");
                builder.AppendFormat("         }});\r\n");
            }

            builder.AppendLine("         return true;");
            builder.AppendLine("      }");
            builder.AppendLine("      return false;");
            builder.AppendLine("   }");
        }
       
        private void GenerateTopLevelMapperElement(StringBuilder builder, Schema.ProjectDefinition definition, Schema.Element e)
        {
            foreach (var el in e.Element1)
            {
                if (string.IsNullOrEmpty(el.type))
                {
                    if (IsMultiple(el))
                    {
                        GenerateMapperElementForList(builder, el);
                    }
                    GenerateMapperElement(builder, definition, el);
                }
            }

            builder.AppendLine("   public DataObject MapMessage(CSVMessage message)");
            builder.AppendLine("   {");
            builder.AppendLine("      DataObject obj = new DataObject();");
            foreach (var el in e.Element1)
            {
                builder.AppendFormat("      Map{0}(message, obj);\r\n", el.name);
            }
            builder.AppendLine("      return obj;");
            builder.AppendLine("   }");
        }


        #endregion

        #region Structure

        public string GenerateStructures(Schema.Project project)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var m in project.MessageDefinitions)
            {
                foreach (var m1 in m.Items)
                {
                    if (m1 is Schema.Element)
                    {
                        GenerateElement(builder, m, m1 as Schema.Element);
                    }
                }
            }

            return builder.ToString();
        }

        private void GenerateElement(StringBuilder builder, Schema.ProjectDefinition definition, Schema.Element e)
        {
            foreach (var el in e.Element1)
            {
                if (string.IsNullOrEmpty(el.type))
                {
                    GenerateElement(builder, definition, el);
                }
            }

            builder.AppendLine("public class " + e.name);
            builder.AppendLine("{");
            foreach (var el in e.Element1)
            {
                if (IsMultiple(el))
                {
                    if (string.IsNullOrEmpty(el.type))
                    {
                        builder.AppendFormat("   public List<{0}> {1}", el.name, el.name);
                    }
                    else
                    {
                        builder.AppendFormat("   public List<{0}> {1}", ResolveType(definition, el.type), el.name);
                    }

                    if (GeneratePropertyType == PropertyType.GenerateFields)
                    {
                        builder.AppendFormat(";\r\n");
                    }
                    else if (GeneratePropertyType == PropertyType.GenerateProperties)
                    {
                        builder.AppendFormat(" {{ get; set; }}\r\n");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(el.type))
                    {
                        builder.AppendFormat("   public {0} {1}", el.name, el.name);
                    }
                    else
                    {
                        builder.AppendFormat("   public {0} {1}", ResolveType(definition, el.type), el.name);
                    }
                    if (GeneratePropertyType == PropertyType.GenerateFields)
                    {
                        builder.AppendFormat(";\r\n");
                    }
                    else if (GeneratePropertyType == PropertyType.GenerateProperties)
                    {
                        builder.AppendFormat(" {{ get; set; }}\r\n");
                    }

                }
            }
            builder.AppendLine("}");
        }

        private void GenerateType(Schema.Type t)
        {
            if (!string.IsNullOrEmpty(t.@base) && !IsNativeType(t.@base))
            {

            }
        }

        #endregion
    }
}
