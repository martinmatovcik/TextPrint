using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using NodaTime;
using TextPrint;

public class Program
{
    static void Main()
    {
        GenereateCmrTextFile(GenerateCmrFormDataDto(), CultureInfo.CurrentCulture);
    }

    private static Encoding ibm852 = CodePagesEncodingProvider.Instance.GetEncoding(852);
    private static CultureInfo _cultureInfo { get; set; }

    //todo --- rm.GetString.... 
    private static string code = "CODE";
    private static string tara = "TARA";
    private static string clo = "CLO";
    private static string dekl = "DEKL";

    public static void GenereateCmrTextFile(CmrFormDataDto formData, CultureInfo cultureInfo)
    {
        _cultureInfo = cultureInfo;
        string filePath = "/Users/macbook/RiderProjects/TextPrint/TextPrint/files/cmr-test.txt";

        try
        {
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

            int outsideFormLeftMargin = 6;
            int insideFormLeftMargin = 1;
            int leftMargin = outsideFormLeftMargin + insideFormLeftMargin;
            int upperFieldWidth = 36;
            int upperFieldWidthUsable = upperFieldWidth - insideFormLeftMargin;

            int field1LeftStartingPoint = leftMargin + 10; //ideal is 17

            int upperRightFieldStartingPoint = upperFieldWidthUsable;
            int field1RightDriverStartingPoint = upperRightFieldStartingPoint + 10; //ideal is 51
            int field1RightStartingPoint = upperRightFieldStartingPoint + 6;
            int field6StartingPoint = upperRightFieldStartingPoint + insideFormLeftMargin;
            int field10StartingPointFromLeftMargin = 10;
            int field11StartingPointFromField10 = 14;
            int field12StartingPointFromField11 = 14;
            int field13StartingPointFromField12 = 20;
            int field13StartingPointFromLeftMargin = field10StartingPointFromLeftMargin +
                                                     field11StartingPointFromField10 + field12StartingPointFromField11 +
                                                     field13StartingPointFromField12;
            int field16_1StartingPointFromLeftMargin = 7;
            int field16_2StartingPointFromField16_1 =  36;

            int upperSpacesBetween = field1RightStartingPoint - field1LeftStartingPoint;

            var italicsOn = new byte[] { 27, 52 };
            var italicsOff = new byte[] { 27, 53 };
            var doubleWidthOn = new byte[] { 27, 87, 49 };
            var doubleWidthOff = new byte[] { 27, 87, 48 };
            var doubleHeightOn = new byte[] { 27, 119, 49 };
            var doubleHeightOff = new byte[] { 27, 119, 48 };
            var ibm852CharacterSet = new byte[] { 27, 82, 46 };
            var setLeftMargin = new byte[] { 27, 108, (byte)leftMargin };
            var formFeed = new byte[] { 12 };
            var lineFeed = new byte[] { 10 };
            var carriageReturn = new byte[] { 13 };
            var variableLineFeed = new byte[] { 27, 74, 30 };

            var initialFileCommands = JoinByteLists(new List<List<byte>>()
            {
                new(ibm852CharacterSet),
                new(italicsOff),
                new(doubleWidthOff),
                new(doubleHeightOff),
                new(setLeftMargin)
            });

            var cmrNumberFormattingOnCommand = JoinByteLists(new List<List<byte>>()
            {
                new(doubleWidthOn),
                new(doubleHeightOn),
            });

            var cmrNumberFormattingOffCommand = JoinByteLists(new List<List<byte>>()
            {
                new(doubleWidthOff),
                new(doubleHeightOff),
            });

            var endFileCommands = JoinByteLists(new List<List<byte>>()
            {
                new(formFeed),
            });

            var content =
                //Start
                AddControlCode(initialFileCommands) +
                AddNewLines(5) +

                //1
                AddSpaces(field1RightDriverStartingPoint) + formData.Ridic + EndLine() +
                AddSpaces(field1LeftStartingPoint) + formData.ObjednavkaCislo + EndLine() +
                AddSpaces(field1LeftStartingPoint) + formData.ReleaseCislo +
                AddSpaces(upperSpacesBetween - formData.ReleaseCislo.Length) +
                $"({formData.CisloRidice}) {formData.RegistracniZnackaTahac}/{formData.RegistracniZnackaNaves}" +
                EndLine() +
                AddSpaces(field1LeftStartingPoint) + formData.KontaktOsoba + EndLine() +
                AddSpaces(field1RightStartingPoint) +
                AddControlCode(cmrNumberFormattingOnCommand) + formData.CmrNumber +
                AddControlCode(cmrNumberFormattingOffCommand) + EndLine() +
                AddSpaces(field1LeftStartingPoint) + formData.KontaktTelefon1 + EndLine() +
                AddSpaces(field1LeftStartingPoint) + formData.KontaktTelefon2 + EndLine() +

                //2 + 6
                AddNewLines(2) +
                formData.OdesilatelPrijemce1 + AddSpaces(upperFieldWidth - formData.OdesilatelPrijemce1.Length) +
                formData.Dopravce1 + EndLine() +
                formData.OdesilatelPrijemce2 + AddSpaces(upperFieldWidth - formData.OdesilatelPrijemce2.Length) +
                formData.Dopravce2 + EndLine() +
                formData.OdesilatelPrijemce3 + AddSpaces(upperFieldWidth - formData.OdesilatelPrijemce3.Length) +
                formData.Dopravce3 + EndLine() +
                formData.OdesilatelPrijemce4 + AddSpaces(upperFieldWidth - formData.OdesilatelPrijemce4.Length) +
                formData.Dopravce4 + EndLine() +

                //3
                AddNewLines(3) +
                formData.AdresaNakladky1 + EndLine() +
                formData.AdresaNakladky2 + EndLine() +
                formData.AdresaNakladky3 + EndLine() +
                formData.AdresaNakladky4 + EndLine() +

                //4
                AddControlCode(variableLineFeed) +
                AddNewLines(2) +
                $"{formData.CasPristaveni} / {formData.MistoPristaveni}" + EndLine() +

                //5
                AddNewLines(1) +
                AddSpaces(21) + formData.CisloPlomby + EndLine() +

                //9,10,11,12,13
                AddNewLines(1) +
                formData.DruhKontejneru +
                AddSpaces(field10StartingPointFromLeftMargin - formData.DruhKontejneru.Length) +
                formData.CisloKontejneru +
                AddSpaces(field11StartingPointFromField10 - formData.CisloKontejneru.Length) +
                formData.Zbozi[0].DruhObalu +
                AddSpaces(field12StartingPointFromField11 - formData.Zbozi[0].DruhObalu.Length) +
                formData.Zbozi[0].Oznaceni +
                AddSpaces(field13StartingPointFromField12 - formData.Zbozi[0].Oznaceni.Length) +
                formData.ImportVaha + EndLine() +
                AddSpaces(field13StartingPointFromLeftMargin) + $"{tara}: {formData.TaraKontejneru}" + EndLine() +
                AddNewLines(2) +
                $"{code}: {formData.UnloadingCode}" +

                //14 
                GenerateField14(formData) +

                //16
                AddSpaces(field16_1StartingPointFromLeftMargin) + formData.VystaveneV +
                AddSpaces(field16_2StartingPointFromField16_1 - formData.VystaveneV.Length) + formData.VystaveneDne.ToString("dd.MM.yyyy", cultureInfo)+ EndLine() +

                //End
                AddControlCode(endFileCommands);

            File.WriteAllText(filePath, content, ibm852);
            Console.WriteLine("File wrote successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static string GenerateField14(CmrFormDataDto formData)
    {
        var prefix = "";
        var padding = 0;
        var output = "";

        if (!string.IsNullOrEmpty(formData.CloText1))
        {
            if (formData.CloText1Prefix)
            {
                prefix = $"{clo}: ";
                padding = prefix.Length;
            }

            output += AddField14Line(0, prefix + formData.CloText1) +
                      AddField14Line(padding, formData.CloText2) +
                      AddField14Line(padding, formData.CloText3) +
                      AddField14Line(padding, formData.CloText4);

            padding = 0;
        }

        if (!string.IsNullOrEmpty(formData.DeclText1))
        {
            if (formData.DeclText1Prefix)
            {
                prefix = $"{dekl}: ";
                padding = prefix.Length;
            }

            output += AddField14Line(padding, prefix + formData.DeclText1) +
                      AddField14Line(padding, formData.DeclText2) +
                      AddField14Line(padding, formData.DeclText3) +
                      AddField14Line(padding, formData.DeclText4);
        }

        var maxLines = 8;
        var actualLines = output.Count(c => c == '\n');
        output += AddNewLines(maxLines - actualLines);

        return output;
    }

    private static string AddField14Line(int cloTextPadding, string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

        return AddSpaces(cloTextPadding) + text + EndLine();
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

    private static string AddNewLines(int numberOfNewLines)
    {
        var newLines = "";
        for (var i = 0; i < numberOfNewLines; i++)
        {
            newLines += "\n";
        }

        return newLines;
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

    private static string EndLine()
    {
        return "\n";
    }

    private static CmrFormDataDto GenerateCmrFormDataDto()
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
            UnloadingCode = "Unloading code",

            //13
            TaraKontejneru = 4000,
            ImportVaha = 16625,
            AdrInstruction = "", //todo transport.ADRDgClass != null ? rm.GetString("adr-instruction", ci) : ""

            //14
            CloText1Prefix = true,
            CloText1 = "CZ590201 - PARDUBICE",
            CloText2 = "Palackého 2659",
            CloText3 = "asdsad",
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
        };
    }
}