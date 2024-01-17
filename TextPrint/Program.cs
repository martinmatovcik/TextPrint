using System.Text;
using NodaTime;
using TextPrint;

public class Program
{
    static void Main()
    {
        GenereateCmrTextFile();
    }

    private static Encoding ibm852 = CodePagesEncodingProvider.Instance.GetEncoding(852);

    public static void GenereateCmrTextFile()
    {
        string filePath = "/Users/macbook/RiderProjects/TextPrint/TextPrint/files/cmr-test.txt";

        try
        {
            // Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // var ibm852 = CodePagesEncodingProvider.Instance.GetEncoding(852);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine("File deleted successfully!");
            }

            //Italics ON - 27,52 -> OFF - 27,53
            //Double width ON - 27,87,49 -> OFF - 27,87,48
            //Double height ON - 27,119,49 -> OFF - 27,119,48
            //IBM 852 characterSet -> 27,82,46
            //Set left margin -> 27,108,n1 --- (n1 specifics in manual page 121)
            //Set right margin -> 27,81,n2 --- (n2 specifics in manual page 121)

            const int leftMarginInt = 8;
            int upperFieldWidth = 34;
            int field1LeftLeftMargin = 17 - leftMarginInt;

            var italicsOn = new byte[] { 27, 52 };
            var italicsOff = new byte[] { 27, 53 };
            var doubleWidthOn = new byte[] { 27, 87, 49 };
            var doubleWidthOff = new byte[] { 27, 87, 48 };
            var doubleHeightOn = new byte[] { 27, 119, 49 };
            var doubleHeightOff = new byte[] { 27, 119, 48 };
            var ibm852CharacterSet = new byte[] { 27, 82, 46, };
            var setLeftMargin = new byte[] { 27, 108, leftMarginInt };
            var formFeed = new byte[] { 12 };
            var lineFeed = new byte[] { 10 };
            var carriageReturn = new byte[] { 13 };

            var startupSettings = new List<List<byte>>()
            {
                new(ibm852CharacterSet),
                new(italicsOff),
                new(doubleWidthOff),
                new(doubleHeightOff),
                new(setLeftMargin)
            };

            var endSettings = new List<List<byte>>()
            {
                new(formFeed),
                new(lineFeed),
                new(carriageReturn),
            };

            var senderLine1 = "XPO SUPPLY CHAIN SERVICES CZECH s";
            var senderLine1Spaces = 36 - senderLine1.Length;

            var carrierLine1 = "Davmar s.r.o.";

            var spaces1 = 51 - leftMarginInt -;
            var spaces2 = 17 - leftMarginInt;

            var content =
                AddControlCode(JoinByteLists(startupSettings)) +
                "\n \n \n \n \n" +
                AddSpaces(spaces1) + "Ján Novák\n" +
                AddSpaces(field1LeftLeftMargin) + "SIGE7896541521\n" +
                AddSpaces(field1LeftLeftMargin) + "78945612378945                (994) 2SB 3587 / 2S25515\n" +
                AddSpaces(field1LeftLeftMargin) + "Fendrich Zikmund\n" +
                AddSpaces(field1LeftLeftMargin) + "Petr Pavel Obsolovic          " +
                AddControlCode(doubleWidthOn) + AddControlCode(doubleHeightOn) + "PRG2023616104" +
                AddControlCode(doubleWidthOff) + AddControlCode(doubleHeightOff) + "\n" +
                AddSpaces(field1LeftLeftMargin) + "+420 789 456 123\n" +
                AddSpaces(field1LeftLeftMargin) + "+420 789 456 123\n" +
                "\n \n" +
                senderLine1 + AddSpaces(senderLine1Spaces) + carrierLine1 + "\n" +
                "CT Park Bor Unit BR Nova Hospoda\n" +
                "BOR, u Tachova/HALA A5\n" +
                "5ty\n" +
                "SK: ľščťžýáíé ĽŠČŤŽÝÁÍÉ" +
                "CZ: ěščřžýáíé ĚŠČŘŽÝÁÍÉ"
                + AddControlCode(JoinByteLists(endSettings));

            File.WriteAllText(filePath, content, ibm852);
            Console.WriteLine("File wrote successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static string AddControlCode(byte[] controlCode)
    {
        return ibm852.GetString(controlCode);
    }

    private static string AddSpaces(int numberOfSpaces)
    {
        var spaces = "";
        for (var i = 0; i < numberOfSpaces; i++)
        {
            spaces += " ";
        }

        return spaces;
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

    private static CmrFormDataDto generateCmrFormDataDto()
    {
        return new CmrFormDataDto()
        {
            NoteTooLong = false,
            ImportExport = TransportTypeEnum.Import,

            //1
            CmrNumber = "CTR2024000882",
            Ridic = "Martin Maťovčík",
            RegistracniZnackaTahac = "3SD2546",
            RegistracniZnackaNaves = "3SD2546",
            CisloRidice = "98765424",
            ObjednavkaCislo = "JANI42521",
            ReleaseCislo = "23BRVCTR50323",
            KontaktOsoba = "Janatka M.",
            KontaktTelefon1 = "+420 2 67293134",
            KontaktTelefon2 = "",

            //2
            OdesilatelPrijemce1 = "WESTROCK PACKAGING, VYKL. KOCLÍŘOV",
            OdesilatelPrijemce2 = "SchumiTransport, Koclířov 258",
            OdesilatelPrijemce3 = "KOCLIROV,",
            OdesilatelPrijemce4 = "56911 CZ",

            //3
            AdresaNakladky1 = "WESTROCK PACKAGING, VYKL. KOCLÍŘOV",
            AdresaNakladky2 = "SchumiTransport, Koclířov 258",
            AdresaNakladky3 = "KOCLIROV, 56911 CZ",
            AdresaNakladky4 = "pan Paclík 737 515 907 F:",

            //4
            CasPristaveni = new LocalDateTime(2024, 2, 16, 13, 0),
            MistoPristaveni = "U příjemce",

            //5
            CisloPlomby = "524819",

            //6
            Dopravce1 = "ACKERMAN II. spol. s r.o.",
            Dopravce2 = "Papírníkova 612",
            Dopravce3 = "Praha 4",
            Dopravce4 = "142 00 CZ",

            //8
            PripojeneDoklady = "", //empty

            //9,10,11,12
            WasteInstruction = "", //todo

            TemperatureInstruction = "", //todo

            OwnerText = "", //todo - "{rm.GetString("note.owner", ci)} {transport.OwnerCode}"
            DruhKontejneru = "40hc",
            CisloKontejneru = "WFHU 518088-2",
            Zbozi = new List<ZboziData>
            {
                new()
                {
                    DruhObalu = "ROL",
                    Oznaceni = "48103210"
                },
                new()
                {
                    DruhObalu = "ROL",
                    Oznaceni = "48103210"
                },
            },
            UnloadingCode = "", //todo

            //13
            TaraKontejneru = 4000,
            ImportVaha = 16625,
            AdrInstruction = "", //todo transport.ADRDgClass != null ? rm.GetString("adr-instruction", ci) : ""

            //14
            CloText1Prefix = true,
            CloText1 = "CZ590201 - PARDUBICE",
            CloText2 = "Palackého 2659",
            CloText3 = "",
            CloText4 = "",

            DeclText1Prefix = true,
            DeclText1 = "schvaleny prijemce",
            DeclText2 = "CLENI NA VYKLADCE",
            DeclText3 = "",
            DeclText4 = "",

            //16
            VystaveneDne = new LocalDate(2024, 1, 17),
            VystaveneV = "Česká Třebová",

            //20
            WeightingInstruction = "JET PO NAKLÁDCE NA VÁHU!/LITRY",
            PickUpInstruction = "Vyzvednout: max 118 znaků včetně mezer",
            ReturnToInstruction = "Vrátit: max znaků 118 včetně mezer",
            Services = "Služby: Dva riadky (max 164 znakov vrátane medzier / - názov)",
            Notes = "Poznámky: (82 znakov na riadok, max rozsah 246 znakov vrátane medzier)"
        }
    }
}