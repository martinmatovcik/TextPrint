using System.Text;

namespace TextPrint;

public class Print
{
    public static void GenereateCmrTextFile()
    {
        string filePath = "/Users/macbook/RiderProjects/TextPrint/TextPrint/files/test.txt";

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine("File deleted successfully!");
            }

            int outsideFormLeftMargin = 6;
            int insideFormLeftPadding = 1;
            int printerLeftMargin = outsideFormLeftMargin + insideFormLeftPadding;

            //------PRINTER-CONTROL CHARACTERS-------
            //Italics ON - 27,52 -> OFF - 27,53
            //Double width ON - 27,87,49 -> OFF - 27,87,48
            //Double height ON - 27,119,49 -> OFF - 27,119,48
            //IBM 852 characterSet -> 27,82,46
            //Set left margin -> 27,108,n1 --- (n1 specifics in manual page 121)
            //Set right margin -> 27,81,n2 --- (n2 specifics in manual page 121)

            var italicsOn = new byte[] { 27, 52 };
            var italicsOff = new byte[] { 27, 53 };
            var doubleWidthOn = new byte[] { 27, 87, 49 };
            var doubleWidthOff = new byte[] { 27, 87, 48 };
            var doubleHeightOn = new byte[] { 27, 119, 49 };
            var doubleHeightOff = new byte[] { 27, 119, 48 };
            var ibm852CharacterSet = new byte[] { 27, 82, 46 };
            var setLeftMargin = new byte[] { 27, 108, (byte)printerLeftMargin };
            var formFeed = new byte[] { 12 };
            var lineFeed = new byte[] { 10 };
            var carriageReturn = new byte[] { 13 };
            var variableLineFeed28 = new byte[] { 27, 74, 28 };
            var variableLineFeedEndFile = new byte[] { 27, 74, 36 };
            var variableRevLineFeedEndFile = new byte[] { 27, 106, 216 };
            // var variableRevLineFeedEndFile = new byte[] { 27, 106, 144 };
            var highSpeed = new byte[] { 27, 35, 48 };
            var tof = new byte[] { 27, 52};

            //todo
            var pageLength = new byte[] { 27, 67, 0, 12 }; //73 - good tear position ; 72 - should be right

            var initialFilePrinterCommands = JoinByteLists(new List<List<byte>>()
            {
                new(pageLength), new(highSpeed), new(ibm852CharacterSet), new(italicsOff), new(doubleWidthOff), new(doubleHeightOff),
                new(setLeftMargin)
            });

            var endFilePrinterCommands = JoinByteLists(new List<List<byte>>()
            {
                new(formFeed),
            });

            var FileEncoding = CodePagesEncodingProvider.Instance.GetEncoding(852);
            var cmrText = FileEncoding.GetString(initialFilePrinterCommands) + "TEST TEXTU" + FileEncoding.GetString(endFilePrinterCommands);
               

            File.WriteAllText(filePath, cmrText, FileEncoding);
            Console.WriteLine("File wrote successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
    
    private static byte[] JoinByteLists(List<List<byte>> listOfLists)
    {
        var totalLength = listOfLists.Sum(list => list.Count);

        var result = new byte[totalLength];

        var currentIndex = 0;
        foreach (List<byte> list in listOfLists)
        {
            list.CopyTo(result, currentIndex);
            currentIndex += list.Count;
        }

        return result;
    }
}