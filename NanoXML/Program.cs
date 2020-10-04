using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using TObject.Shared;

namespace NanoXML
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length == 0)
      {
        Console.WriteLine("No input file specified");
        Console.ReadLine();
        return;
      }

      FileStream fs = new FileStream(args[0], FileMode.Open, FileAccess.Read);
      byte[] data = new byte[fs.Length];
      fs.Read(data, 0, (int) fs.Length);
      fs.Close();

      string strData = Encoding.UTF8.GetString(data);

      try
      {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        NanoXMLDocument xml = new NanoXMLDocument(strData);
        sw.Stop();
        Console.WriteLine("NanoXMLDocument loaded successfully in {0} ms", sw.ElapsedMilliseconds);

        XmlDocument xml2 = new XmlDocument();
        sw.Reset();
        sw.Start();
        xml2.LoadXml(strData);
        sw.Stop();
        Console.WriteLine("XmlDocument loaded successfully in {0} ms", sw.ElapsedMilliseconds);

        StringReader stringReader = new StringReader(strData);
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.ProhibitDtd = false;
        XmlReader xmlReader = XmlReader.Create(stringReader, settings);
        sw.Reset();
        sw.Start();
        while (xmlReader.Read()) ;
        sw.Stop();
        Console.WriteLine("XmlReader loaded successfully in {0} ms", sw.ElapsedMilliseconds);
      }
      catch (XMLParsingException e)
      {
        Console.WriteLine("XML Parsing error: {0}", e.Message);
      }
      catch (Exception e)
      {
        Console.WriteLine("General excaption: [{0}]{1}", e.GetType().Name, e.Message);
      }
      Console.ReadLine();
    }
  }
}
