using System.Text;

namespace TextPrint;

public static class FileCreator
{
    public static void SaveToFile(Stream stream, Encoding encoding)
    {
        var filePath = "/Users/macbook/RiderProjects/TextPrint/TextPrint/files/cmr-test.txt";
        StreamReader reader = new StreamReader(stream, encoding);
        string cmrText =  reader.ReadToEnd();
        reader.Close();
        File.WriteAllText(filePath, cmrText, encoding);
    }
}