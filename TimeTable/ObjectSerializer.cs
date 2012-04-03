using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace TimeTable
{
   public static class ObjectSerializer
   {
      static public void SerializeToXML(ObservableCollection<Zeiten> zeiten)
      {
         var serializer = new XmlSerializer(typeof(ObservableCollection<Zeiten>));
         TextWriter textWriter = new StreamWriter(@"C:\movie.xml");
         serializer.Serialize(textWriter, zeiten);
         textWriter.Close();
      }

      static public ObservableCollection<Zeiten> DeserializeFromXML()
      {
         if (!File.Exists(@"C:\movie.xml"))
            return new ObservableCollection<Zeiten>();

         var deserializer = new XmlSerializer(typeof(ObservableCollection<Zeiten>));
         TextReader textReader = new StreamReader(@"C:\movie.xml");
         var zeiten = (ObservableCollection<Zeiten>)deserializer.Deserialize(textReader);
         textReader.Close();

         return zeiten;
      }
   }
}
