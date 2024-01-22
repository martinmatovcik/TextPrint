using System.Globalization;
using System.Text;
using NodaTime;

namespace TextPrint.cmr;

public class CmrTextFileGenerator
{
    private static Encoding? FileEncoding { get; set; } = CodePagesEncodingProvider.Instance.GetEncoding(852);
    private static CultureInfo? CultureInfo { get; set; }

    //todo --- rm.GetString.... 
    private static string code = "Code";
    private static string rejd = "Rejd";
    private static string tara = "TARA";
    private static string clo = "CLO";
    private static string dekl = "DEKL";

    public static void GenereateCmrTextFile(CmrFormDataDto formData, CultureInfo cultureInfo)
    {
        CultureInfo = cultureInfo;
        string filePath = "/Users/macbook/RiderProjects/TextPrint/TextPrint/files/cmr-test.txt";

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

            int fields1To8Width = 36;
            int fields1To8WidthUsable = fields1To8Width - insideFormLeftPadding;

            int field1_1StartingPointFromPrinterLeftMargin = 10;

            int field1_2StartingPointFromPrinterLeftMargin = fields1To8WidthUsable;
            int field1_2DriverStartingPointFromLeftMargin =
                field1_2StartingPointFromPrinterLeftMargin + 10; //ideal is 51
            int field1_2TextStartingPointFromLeftMargin = field1_2StartingPointFromPrinterLeftMargin + 6;
            int field5StartingPointFromLeftMargin = 21;
            int field8StartingPointFromField5 = 26;

            int field16_1StartingPointFromLeftMargin = 7;
            int field16_2StartingPointFromField16_1 = 38;

            int upperSpacesBetween =
                field1_2TextStartingPointFromLeftMargin - field1_1StartingPointFromPrinterLeftMargin;

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
            //possible chyba -> je mozne ze ako sa spravi var, chyba tam 36-n znakov riadku ->> pridat variable line feed s n=36-n
            var variableLineFeed28 = new byte[] { 27, 74, 28 };
            var variableLineFeedEndFile = new byte[] { 27, 74, 8 };
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

            var cmrNumberFormattingOnPrinterCommand = JoinByteLists(new List<List<byte>>()
            {
                new(doubleWidthOn), new(doubleHeightOn),
            });

            var cmrNumberFormattingOffPrinterCommand = JoinByteLists(new List<List<byte>>()
            {
                new(doubleWidthOff), new(doubleHeightOff),
            });

            var endFilePrinterCommands = JoinByteLists(new List<List<byte>>()
            {//todo next
                new(formFeed), new(variableLineFeedEndFile)
            });

            var cmrText =
                //Start
                AddControlCode(initialFilePrinterCommands) +
                // todo -- if continuous print -> 6, if first print -> 5
                AddNewLines(6) +

                //1
                AddSpaces(field1_2DriverStartingPointFromLeftMargin) + formData.Ridic + EndLine() +
                AddSpaces(field1_1StartingPointFromPrinterLeftMargin) + formData.ObjednavkaCislo + EndLine() +
                AddSpaces(field1_1StartingPointFromPrinterLeftMargin) + formData.ReleaseCislo +
                AddSpaces(upperSpacesBetween - formData.ReleaseCislo.Length) +
                $"({formData.CisloRidice}) {formData.RegistracniZnackaTahac}/{formData.RegistracniZnackaNaves}" +
                EndLine() +
                AddSpaces(field1_1StartingPointFromPrinterLeftMargin) + formData.KontaktOsoba + EndLine() +
                AddSpaces(field1_2TextStartingPointFromLeftMargin) +
                AddControlCode(cmrNumberFormattingOnPrinterCommand) + formData.CmrNumber +
                AddControlCode(cmrNumberFormattingOffPrinterCommand) + EndLine() +
                AddSpaces(field1_1StartingPointFromPrinterLeftMargin) + formData.KontaktTelefon1 + EndLine() +
                AddSpaces(field1_1StartingPointFromPrinterLeftMargin) + formData.KontaktTelefon2 + EndLine() +

                //2 + 6
                AddNewLines(2) +
                formData.OdesilatelPrijemce1 + AddSpaces(fields1To8Width - formData.OdesilatelPrijemce1.Length) +
                formData.Dopravce1 + EndLine() +
                formData.OdesilatelPrijemce2 + AddSpaces(fields1To8Width - formData.OdesilatelPrijemce2.Length) +
                formData.Dopravce2 + EndLine() +
                formData.OdesilatelPrijemce3 + AddSpaces(fields1To8Width - formData.OdesilatelPrijemce3.Length) +
                formData.Dopravce3 + EndLine() +
                formData.OdesilatelPrijemce4 + AddSpaces(fields1To8Width - formData.OdesilatelPrijemce4.Length) +
                formData.Dopravce4 + EndLine() +

                //3
                AddNewLines(3) +
                formData.AdresaNakladky1 + EndLine() +
                formData.AdresaNakladky2 + EndLine() +
                formData.AdresaNakladky3 + EndLine() +
                formData.AdresaNakladky4 + EndLine() +

                //4
                AddControlCode(variableLineFeed28) +
                AddNewLines(2) +
                $"{formData.CasPristaveni} / {formData.MistoPristaveni}" + EndLine() +

                //5,8
                AddNewLines(1) +
                AddSpaces(field5StartingPointFromLeftMargin) + formData.CisloPlomby +
                AddSpaces(field8StartingPointFromField5 - formData.CisloPlomby.Length) + formData.PripojeneDoklady +
                EndLine() +

                //9,10,11,12,13
                AddNewLines(1) +
                GenerateFields9To13(formData) +

                //14 
                GenerateField14(formData) +

                //16
                AddSpaces(field16_1StartingPointFromLeftMargin) + formData.VystaveneV +
                AddSpaces(field16_2StartingPointFromField16_1 - formData.VystaveneV.Length) +
                formData.VystaveneDne.ToString("dd.MM.yyyy", cultureInfo) + EndLine() +

                //20
                AddNewLines(7) +
                GenerateField20(formData) +

                //End
                AddControlCode(endFilePrinterCommands);

            File.WriteAllText(filePath, cmrText, FileEncoding);
            Console.WriteLine("File wrote successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static string GenerateFields9To13(CmrFormDataDto formData)
    {
        var output = "";

        int field10StartingPointFromLeftMargin = 8;
        int field11StartingPointFromField10 = 15;
        int field11StartingPointFromLeftMargin = field10StartingPointFromLeftMargin + field11StartingPointFromField10;
        int field13StartingPointFromField11 = 36;

        var goods = formData.Zbozi
            .DistinctBy(x => x.DruhObalu + x.Oznaceni)
            .GroupBy(x => x.DruhObalu)
            .Select(druhy => (druhy.Key + ' ').PadRight(4, ' ') + String.Join(", ", druhy.Select(zb => zb.Oznaceni)));

        var goodsFormatted = String.Join(Environment.NewLine, goods);

        for (var i = 0; i < goodsFormatted.Length; i++)
        {
            if (i != 0 && i % 34 == 0 && goodsFormatted[i - 1] != '\n')
            {
                goodsFormatted = goodsFormatted.Insert(i, "\n" + AddSpaces(4));
            }
        }

        var goodsLinesSplit = goodsFormatted.Split("\n");

        var goodsLines = new string[6];

        for (int i = 0; i < goodsLines.Length; i++)
        {
            goodsLines[i] = goodsLinesSplit.Length > i ? goodsLinesSplit[i] : "";
        }

        //Line 1
        output += formData.DruhKontejneru +
                  AddSpaces(field10StartingPointFromLeftMargin - formData.DruhKontejneru.Length) +
                  formData.CisloKontejneru +
                  AddSpaces(field11StartingPointFromField10 - formData.CisloKontejneru.Length) +
                  goodsLines[0] +
                  AddSpaces(field13StartingPointFromField11 - goodsLines[0].Length) +
                  formData.ImportVaha + EndLine();

        //Line 2
        var taraText = AppendLineWithPrefixOrEmptyString(tara, formData.TaraKontejneru.ToString());
        output += AddSpaces(field11StartingPointFromLeftMargin) + goodsLines[1] +
                  AddSpaces(field13StartingPointFromField11 - goodsLines[1].Length) + taraText;

        //Line 3
        output += formData.TemperatureInstruction +
                  AddSpaces(field11StartingPointFromLeftMargin - formData.TemperatureInstruction.Length) +
                  goodsLines[2] + EndLine();

        //Line 4
        output += formData.WasteInstruction +
                  AddSpaces(field11StartingPointFromLeftMargin - formData.WasteInstruction.Length) + goodsLines[3] +
                  EndLine();

        //Line 5
        var rejdText = AppendLineWithPrefixOrEmptyString(rejd, formData.OwnerText);
        rejdText = rejdText.Substring(0, rejdText.Length - 1);
        output += rejdText + AddSpaces(field11StartingPointFromLeftMargin - rejdText.Length) +
                  goodsLines[4] + EndLine();

        //Line 6
        var codeText = AppendLineWithPrefixOrEmptyString(code, formData.UnloadingCode);
        codeText = codeText.Substring(0, codeText.Length - 1);
        output += codeText + AddSpaces(field11StartingPointFromLeftMargin - codeText.Length) +
                  goodsLines[5] + EndLine();

        //Line 7
        output += AddNewLines(1);

        //Line 8 + 9 (?)
        var adrInstructionLine = formData.AdrInstruction;
        var lineLength = 55;
        if (adrInstructionLine.Length > lineLength)
        {
            adrInstructionLine = adrInstructionLine.Insert(lineLength, "\n");
        }

        if (adrInstructionLine.Length > lineLength * 2)
        {
            adrInstructionLine = adrInstructionLine.Substring(0, lineLength * 2 + 1);
        }

        output += adrInstructionLine + EndLine();

        var maxLines = 10;
        var actualLines = output.Count(c => c == '\n');
        output += AddNewLines(maxLines - actualLines);

        return output;
    }

    private static string GenerateField14(CmrFormDataDto formData)
    {
        var prefix = "";
        var padding = 6;
        var output = "";

        if (!string.IsNullOrEmpty(formData.CloText1))
        {
            if (formData.CloText1Prefix)
            {
                prefix = $"{clo}:  ";
            }

            output += AppendLineWithPadding(0, prefix + formData.CloText1) +
                      AppendLineWithPadding(padding, formData.CloText2) +
                      AppendLineWithPadding(padding, formData.CloText3) +
                      AppendLineWithPadding(padding, formData.CloText4);
        }

        if (!string.IsNullOrEmpty(formData.DeclText1))
        {
            if (formData.DeclText1Prefix)
            {
                prefix = $"{dekl}: ";
            }

            output += AppendLineWithPadding(0, prefix + formData.DeclText1) +
                      AppendLineWithPadding(padding, formData.DeclText2) +
                      AppendLineWithPadding(padding, formData.DeclText3) +
                      AppendLineWithPadding(padding, formData.DeclText4);
        }

        var maxLines = 8;
        var actualLines = output.Count(c => c == '\n');
        output += AddNewLines(maxLines - actualLines);

        return output;
    }

    private static string GenerateField20(CmrFormDataDto formData)
    {
        var lineLength = 66;
        var paragraphLineLength = lineLength / 2;

        var pickUpInstructionLine1 = formData.PickUpInstruction;
        var pickUpInstructionLine2 = "";
        var pickUpInstructionLine3 = "";
        var pickupInstructionLength = formData.PickUpInstruction.Length;

        if (pickupInstructionLength > paragraphLineLength)
        {
            pickUpInstructionLine2 =
                pickUpInstructionLine1.Substring(paragraphLineLength, pickupInstructionLength - paragraphLineLength);
            pickUpInstructionLine1 = pickUpInstructionLine1.Substring(0, paragraphLineLength);
        }

        if (pickupInstructionLength > paragraphLineLength * 2)
        {
            pickUpInstructionLine3 = pickUpInstructionLine2.Substring(paragraphLineLength, paragraphLineLength);
            pickUpInstructionLine2 = pickUpInstructionLine2.Substring(0, paragraphLineLength);
        }

        var returnToInstructionLine1 = formData.ReturnToInstruction;
        var returnToInstructionLine2 = "";
        var returnToInstructionLine3 = "";
        var returnToInstructionLength = formData.ReturnToInstruction.Length;

        if (returnToInstructionLength > paragraphLineLength)
        {
            returnToInstructionLine2 =
                returnToInstructionLine1.Substring(paragraphLineLength,
                    returnToInstructionLength - paragraphLineLength);
            returnToInstructionLine1 = returnToInstructionLine1.Substring(0, paragraphLineLength);
        }

        if (returnToInstructionLength > paragraphLineLength * 2)
        {
            returnToInstructionLine3 = returnToInstructionLine2.Substring(paragraphLineLength, paragraphLineLength);
            returnToInstructionLine2 = returnToInstructionLine2.Substring(0, paragraphLineLength);
        }

        //Weight Instruction
        var spacesBetween = 4;
        var output = AppendLineWithPadding(7, formData.WeightingInstruction) +
                     //Pickup / Return Instruction
                     pickUpInstructionLine1 +
                     AddSpaces(paragraphLineLength - pickUpInstructionLine1.Length + spacesBetween) +
                     returnToInstructionLine1 + EndLine() +
                     pickUpInstructionLine2 +
                     AddSpaces(paragraphLineLength - pickUpInstructionLine2.Length + spacesBetween) +
                     returnToInstructionLine2 + EndLine() +
                     pickUpInstructionLine3 +
                     AddSpaces(paragraphLineLength - pickUpInstructionLine3.Length + spacesBetween) +
                     returnToInstructionLine3 + EndLine() + AddNewLines(1);

        lineLength += spacesBetween;
        //Services
        var servicesLine = formData.Services;
        if (servicesLine.Length > lineLength)
        {
            servicesLine = servicesLine.Insert(lineLength, "\n");
        }

        if (servicesLine.Length > lineLength * 2)
        {
            servicesLine = servicesLine.Substring(0, lineLength * 2 + 1);
        }

        output += servicesLine + EndLine();

        //Notes
        var notesLine = formData.Notes;
        var notesLineLength = notesLine.Length;
        if (notesLine.Length > lineLength)
        {
            notesLine = notesLine.Insert(lineLength, "\n");
        }

        if (notesLine.Length > lineLength * 2)
        {
            notesLine = notesLine.Insert(lineLength * 2, "\n");
        }

        if (notesLine.Length > lineLength * 3)
        {
            notesLine = notesLine.Substring(0, lineLength * 3 + 2);
        }

        output += notesLine;
        return output;
    }

    private static string AddControlCode(byte[] controlCode)
    {
        return FileEncoding.GetString(controlCode);
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

    private static string AppendLineWithPrefixOrEmptyString(string prefix, string? text)
    {
        return !string.IsNullOrEmpty(text) ? $"{prefix}: {text}" + EndLine() : "" + EndLine();
    }

    private static string AppendLineWithPadding(int padding, string? text)
    {
        return !string.IsNullOrEmpty(text)
            ? AddSpaces(padding) + text + EndLine()
            : "";
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
            Ridic = "Martin MaœovŸ¡k",
            RegistracniZnackaTahac = "3SD2546",
            RegistracniZnackaNaves = "3SD2546",
            CisloRidice = "98765424",
            ObjednavkaCislo = "JANI42521",
            ReleaseCislo = "23BRVCTR50323",
            KontaktOsoba = "Janatka M.",
            KontaktTelefon1 = "+420 2 67293134",
            KontaktTelefon2 = "",

            //2
            OdesilatelPrijemce1 = "WESTROCK PACKAGING, VYKL. KOCLÖüOV",
            OdesilatelPrijemce2 = "SchumiTransport, Kocl¡ýov 258",
            OdesilatelPrijemce3 = "KOCLIROV,",
            OdesilatelPrijemce4 = "56911 CZ",

            //3
            AdresaNakladky1 = "WESTROCK PACKAGING, VYKL. KOCLÖüOV",
            AdresaNakladky2 = "SchumiTransport, Kocl¡ýov 258",
            AdresaNakladky3 = "KOCLIROV, 56911 CZ",
            AdresaNakladky4 = "pan Pacl¡k 737 515 907 F:",

            //4
            CasPristaveni = new LocalDateTime(2024, 2, 16, 13, 0),
            MistoPristaveni = "U pý¡jemce",

            //5
            CisloPlomby = "524819",

            //6
            Dopravce1 = "ACKERMAN II. spol. s r.o.",
            Dopravce2 = "Pap¡rn¡kova 612",
            Dopravce3 = "Praha 4",
            Dopravce4 = "142 00 CZ",

            //8
            PripojeneDoklady = "DL2316305",

            //9,10,11,12
            WasteInstruction = "Odpady....",

            TemperatureInstruction = "Teplota xz",

            OwnerText = "CNG",
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
                    Oznaceni = "48103211"
                },
                new()
                {
                    DruhObalu = "ROL",
                    Oznaceni = "48103212"
                },
                new()
                {
                    DruhObalu = "ROL",
                    Oznaceni = "48103213"
                },
                new()
                {
                    DruhObalu = "ROL",
                    Oznaceni = "48103214"
                },
                new()
                {
                    DruhObalu = "ROL",
                    Oznaceni = "48103215"
                },
                new()
                {
                    DruhObalu = "PAL",
                    Oznaceni = "48103210"
                },
            },
            UnloadingCode = "Unloading code",

            //13
            TaraKontejneru = 4000,
            ImportVaha = 16625,
            AdrInstruction =
                "\"ADR - popis viz. Doplåkovì list ADR, ADR - siehe in der Erg„nzungsliste ADR\"",

            //14
            CloText1Prefix = true,
            CloText1 = "CZ590201 - PARDUBICE",
            CloText2 = "Palack‚ho 2659",
            CloText3 = "asdsad",
            CloText4 = "",

            DeclText1Prefix = true,
            DeclText1 = "schvaleny prijemce",
            DeclText2 = "CLENI NA VYKLADCE",
            DeclText3 = "",
            DeclText4 = "",

            //16
            VystaveneDne = new LocalDate(2024, 1, 17),
            VystaveneV = "¬esk  Týebov ",

            //20
            WeightingInstruction = "JET PO NAKLµDCE NA VµHU!/LITRY",
            PickUpInstruction =
                "Vyzvednout: Embrace the journey, conquer challenges, and cherish the moments. Life is a canvas; paint it with passion. Shine brightly, the world awaits your brilliance.",
            ReturnToInstruction =
                "Vr tit: Embrace the journey, conquer challenges, and cherish the moments. Life is a canvas; paint it with passion. Shine brightly, the world awaits your brilliance.",
            Services =
                "Slu§by: Embrace the journey, conquer challenges, and cherish the moments. Life is a canvas; paint it with passion. Shine brightly, the world awaits your brilliance.",
            Notes =
                "Pozn mky: Embrace the journey, conquer challenges, and cherish the moments. Life is a canvas; paint it with passion. Shine brightly, the world awaits your brilliance."
        };
    }
}