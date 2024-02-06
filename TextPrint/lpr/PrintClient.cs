using System.Net.Sockets;
using System.Text;

namespace TextPrint.lpr;

public class PrintClient
{
    public static void PrintStream(Stream stream, Encoding encoding)
    {
        string ipAddress = "192.168.50.159";
        int port = 9100;
        
        try
        {
            // Open connection
            TcpClient client = new TcpClient();
            client.Connect(ipAddress, port);

            // Write ZPL String to connection
            StreamReader reader = new StreamReader(stream, encoding);
            StreamWriter writer = new StreamWriter(client.GetStream(), encoding);
            string cmrText = reader.ReadToEnd();
            reader.Close();
            writer.Write(cmrText);
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