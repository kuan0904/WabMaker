using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace MyTool.Services
{
    public class _Xml
    {
        public static string ModelToXml(object obj)
        {
            XmlSerializer ser = new XmlSerializer(obj.GetType());
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            ser.Serialize(writer, obj);
            return sb.ToString();
        }

        public static T XmlToModel<T>(string str)
        {
            XmlDocument xdoc = new XmlDocument();

            try
            {
                xdoc.LoadXml(str);
                XmlNodeReader reader = new XmlNodeReader(xdoc.DocumentElement);
                XmlSerializer ser = new XmlSerializer(typeof(T));
                object obj = ser.Deserialize(reader);
                reader.Close();

                return (T)obj;
            }
            catch
            {
                return default(T);
            }
        }
    }
}
