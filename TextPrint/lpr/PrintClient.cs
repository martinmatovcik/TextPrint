using System.Net.Sockets;
using System.Text;

namespace TextPrint.lpr;

public class PrintClient
{
    public static void PrintFile(string filePath)
    {
        string ipAddress = "192.168.50.159";
        int port = 9100;
        var encoding = CodePagesEncodingProvider.Instance.GetEncoding(852);
        
        try
        {
            // Open connection
            TcpClient client = new TcpClient();
            client.Connect(ipAddress, port);

            // Write ZPL String to connection
            StreamReader reader = new StreamReader(filePath, encoding);
            StreamWriter writer = new StreamWriter(client.GetStream(), encoding);
            string file = reader.ReadToEnd();
            reader.Close();
            writer.Write(file);
            writer.Flush();
            writer.Close();
            client.Close();
            
            Console.WriteLine("LPR command success.");
        }
        catch (Exception ex)
        {
            // Catch Exception
        }
    }
}