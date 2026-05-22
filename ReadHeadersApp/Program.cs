using System;
using System.IO;
using System.Xml;

class Program
{
    static void Main()
    {
        var path = @"d:\TudfConverter\docs\sharedStrings.xml";
        using var reader = XmlReader.Create(path);
        using var writer = new StreamWriter(@"d:\TudfConverter\docs\strings.txt");
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "t")
            {
                var text = reader.ReadElementContentAsString();
                writer.WriteLine(text);
            }
        }
        Console.WriteLine("Done");
    }
}
