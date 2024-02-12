using System.Net.Sockets;
using System.Text;

namespace TextPrint.lpr;

public class PrintClient
{
    public static async Task PrintStreamAsync(Stream stream, Encoding encoding)
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
            string cmrText = await reader.ReadToEndAsync();
            reader.Close();
            await writer.WriteAsync(cmrText);
            await writer.FlushAsync();
            writer.Close();
            client.Close();
            
            Console.WriteLine("LPR command success.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}