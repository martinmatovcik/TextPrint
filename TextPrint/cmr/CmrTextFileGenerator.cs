using System.Globalization;
using System.Resources;
using System.Text;
using NodaTime;

namespace TextPrint.cmr;

public class CmrTextFileGenerator
{
    private static readonly Encoding? Encoding = CodePagesEncodingProvider.Instance.GetEncoding(852);

    //Document margin and padding
    private const int leftMarginSTX = 2;

    private const int OutsideFormLeftMargin = 3;
    private const int InsideFormLeftPadding = 1;
    private const int PrinterLeftMargin = OutsideFormLeftMargin + InsideFormLeftPadding + leftMarginSTX;
    private const int TopMarginInLines = 2;

    //CMR fields widths and starting points
    private const int Fields1To8Width = 36;
    private const int Fields1To8WidthUsable = Fields1To8Width - InsideFormLeftPadding;
    private const int Field11StartingPointFromPrinterLeftMargin = 10;
    private const int Field12StartingPointFromPrinterLeftMargin = Fields1To8WidthUsable;

    private const int Field12DriverStartingPointFromLeftMargin =
        Field12StartingPointFromPrinterLeftMargin + 10;

    private const int Field12TextStartingPointFromLeftMargin = Field12StartingPointFromPrinterLeftMargin + 6;
    private const int Field5StartingPointFromLeftMargin = 21;
    private const int Field8StartingPointFromField5 = 26;
    private const int Field161StartingPointFromLeftMargin = 7;
    private const int Field162StartingPointFromField161 = 38;

    private const int Field1SpacesBetween =
        Field12TextStartingPointFromLeftMargin - Field11StartingPointFromPrinterLeftMargin;

    private const int MillimetersToInchConvention = 9;
    private const int LineHeight = 36; //value is in n/216inch

    //Printer control codes
    private static readonly byte[] ItalicsOff = { 27, 53 };
    private static readonly byte[] DoubleWidthOn = { 27, 87, 49 };
    private static readonly byte[] DoubleWidthOff = { 27, 87, 48 };
    private static readonly byte[] DoubleHeightOn = { 27, 119, 49 };
    private static readonly byte[] DoubleHeightOff = { 27, 119, 48 };
    private static readonly byte[] Ibm852CharacterSet = { 27, 82, 46 };
    private static readonly byte[] SetLeftMargin = { 27, 108, PrinterLeftMargin };
    private static readonly byte[] FormFeed = { 12 };
    private static readonly byte[] HighSpeed = { 27, 35, 48 };
    private static readonly byte[] PageLength = { 27, 67, 0, 12 };
    private static readonly byte[] EmulationMode = { 27, 123, 64 };
    private static readonly byte[] SixLpi = { 27, 50 };
    private static readonly byte[] EightLpi = { 27, 48 };
    private static readonly byte[] TenCpi = { 27, 80 };

    //variable line-feed command { 27, 74, n } -> n = posuv (n/216inch); 0 <= n <= 255
    //variable reverse-line-feed command { 27, 106, n } -> n = posuv (n/216inch); 0 <= n <= 255
    private const int Field4VariableLineFeedValue = 28;
    private static readonly byte[] Field4VariableLineFeedCommand = { 27, 74, Field4VariableLineFeedValue };

    //CAREFULL!!!!!
    private const int StartVariableRevLineFeedValue = 35;
    private const int EndVariableRevLineFeedValue = 25;
    private static readonly byte[] StartVariableRevLineFeedCommand = { 27, 106, StartVariableRevLineFeedValue };
    // private static readonly byte[] VariableLineFeedToZeroCommand = { 27, 74, 35 };
    private static readonly byte[] VariableLineFeedToZeroCommand = { 27, 106, 0 };

    private const char EndOfLine = '\n';
    private const char Space = ' ';

    //Prefixes codes
    private const string Code = "CODE:";
    private const string Rejd = "REJD:";
    private const string Tara = "TARA:";
    private const string Clo = "CLO:";
    private const string Dekl = "DEKL:";

    //Field 9to13
    private const int Field10StartingPointFromLeftMargin = 8;
    private const int Field11StartingPointFromField10 = 15;

    private const int Field11StartingPointFromLeftMargin =
        Field10StartingPointFromLeftMargin + Field11StartingPointFromField10;

    private const int Field13StartingPointFromField11 = 33;
    private const int Field11And12MaxNumberOfLines = 6;
    private const int Field12Padding = 4;
    private const int Field11And12FullWidth = 31;

    //Field 14
    private const int Field14Padding = 6;
    private const int Field14MaxLines = 8;

    //Field 20
    private const int Field20PickupReturnLineLength = 66;
    private const int Field20ParagraphLineLength = Field20PickupReturnLineLength / 2;
    private const int Field20PickupReturnSpacesBetween = 4;
    private const int Field20LineLength = Field20PickupReturnLineLength + Field20PickupReturnSpacesBetween;

    public static Stream GenerateCmrTextAsStream(CmrFormDataDto formData, CultureInfo cultureInfo,
        int verticalOffSetLines)
    {
        var verticalOffSetMilimeters = 0; //ready for vertical-MICRO-Offset feature
        int verticalOffsetInches =
            verticalOffSetMilimeters * MillimetersToInchConvention; //ready for vertical-MICRO-Offset feature
        int sumVariableLineFeeds = verticalOffsetInches + Field4VariableLineFeedValue;
        byte[] variableLineFeedStart = { 27, 74, (byte)verticalOffsetInches };
        byte[] variableLineFeedEndFile = { 27, 74, (byte)(LineHeight - sumVariableLineFeeds + 2) };

        var initialFilePrinterCommands = JoinByteLists(new List<List<byte>>()
        {
            new(EmulationMode), new(SixLpi), new(TenCpi), new(PageLength), new(HighSpeed), new(Ibm852CharacterSet),
            new(ItalicsOff), new(DoubleWidthOff),
            new(DoubleHeightOff), new(SetLeftMargin)
        });

        var cmrNumberFormattingOnPrinterCommand = JoinByteLists(new List<List<byte>>()
        {
            new(DoubleWidthOn), new(DoubleHeightOn),
        });

        var cmrNumberFormattingOffPrinterCommand = JoinByteLists(new List<List<byte>>()
        {
            new(DoubleWidthOff), new(DoubleHeightOff),
        });

        var endFilePrinterCommands = JoinByteLists(new List<List<byte>>()
        {
            // new(FormFeed),
            // new(VariableLineFeedToZeroCommand),
            new(variableLineFeedEndFile)
        });

        var cmrText =
            //Start
            AddControlCode(initialFilePrinterCommands) +
            AddNewLines(TopMarginInLines + verticalOffSetLines - 1) +
            AddControlCode(StartVariableRevLineFeedCommand) +

            //1
            AddSpaces(Field12DriverStartingPointFromLeftMargin) + formData.Ridic + EndOfLine +
            AddSpaces(Field11StartingPointFromPrinterLeftMargin) + formData.ObjednavkaCislo + EndOfLine +
            AddSpaces(Field11StartingPointFromPrinterLeftMargin) + formData.ReleaseCislo +
            AddSpaces(Field1SpacesBetween - formData.ReleaseCislo.Length) +
            $"({formData.CisloRidice}) {formData.RegistracniZnackaTahac}/{formData.RegistracniZnackaNaves}" +
            EndOfLine +
            AddSpaces(Field11StartingPointFromPrinterLeftMargin) + formData.KontaktOsoba + EndOfLine +
            AddSpaces(Field12TextStartingPointFromLeftMargin) +
            AddControlCode(cmrNumberFormattingOnPrinterCommand) + formData.CmrNumber +
            AddControlCode(cmrNumberFormattingOffPrinterCommand) + EndOfLine +
            AddSpaces(Field11StartingPointFromPrinterLeftMargin) + formData.KontaktTelefon1 + EndOfLine +
            AddSpaces(Field11StartingPointFromPrinterLeftMargin) + formData.KontaktTelefon2 + EndOfLine;

        var odesilatelPrijemce1 = TrimTextToMaxLengthIfTooLong(Fields1To8WidthUsable, formData.OdesilatelPrijemce1);
        var odesilatelPrijemce2 = TrimTextToMaxLengthIfTooLong(Fields1To8WidthUsable, formData.OdesilatelPrijemce2);
        var odesilatelPrijemce3 = TrimTextToMaxLengthIfTooLong(Fields1To8WidthUsable, formData.OdesilatelPrijemce3);
        var odesilatelPrijemce4 = TrimTextToMaxLengthIfTooLong(Fields1To8WidthUsable, formData.OdesilatelPrijemce4);

        var dopravce1 = TrimTextToMaxLengthIfTooLong(Fields1To8WidthUsable, formData.Dopravce1);
        var dopravce2 = TrimTextToMaxLengthIfTooLong(Fields1To8WidthUsable, formData.Dopravce2);
        var dopravce3 = TrimTextToMaxLengthIfTooLong(Fields1To8WidthUsable, formData.Dopravce3);
        var dopravce4 = TrimTextToMaxLengthIfTooLong(Fields1To8WidthUsable, formData.Dopravce4);


        cmrText +=
            //2 + 6
            AddNewLines(2) +
            odesilatelPrijemce1 +
            AddSpaces(Fields1To8Width - odesilatelPrijemce1.Length) +
            dopravce1 + EndOfLine +
            odesilatelPrijemce2 +
            AddSpaces(Fields1To8Width - odesilatelPrijemce2.Length) +
            dopravce2 + EndOfLine +
            odesilatelPrijemce3 +
            AddSpaces(Fields1To8Width - odesilatelPrijemce3.Length) +
            dopravce3 + EndOfLine +
            odesilatelPrijemce4 +
            AddSpaces(Fields1To8Width - odesilatelPrijemce4.Length) +
            dopravce4 + EndOfLine +

            //3
            AddNewLines(3) +
            TrimTextToMaxLengthIfTooLong(Fields1To8WidthUsable, formData.AdresaNakladky1) + EndOfLine +
            TrimTextToMaxLengthIfTooLong(Fields1To8WidthUsable, formData.AdresaNakladky2) + EndOfLine +
            TrimTextToMaxLengthIfTooLong(Fields1To8WidthUsable, formData.AdresaNakladky3) + EndOfLine +
            TrimTextToMaxLengthIfTooLong(Fields1To8WidthUsable, formData.AdresaNakladky4) + EndOfLine +

            //4
            AddControlCode(Field4VariableLineFeedCommand) +
            AddNewLines(2) +
            formData.CasPristaveni.ToString("g", cultureInfo) + " / " + formData.MistoPristaveni + EndOfLine +

            //5,8
            AddNewLines(1) +
            AddSpaces(Field5StartingPointFromLeftMargin) + formData.CisloPlomby +
            AddSpaces(Field8StartingPointFromField5 - formData.CisloPlomby.Length) +
            formData.PripojeneDoklady +
            EndOfLine +

            //9,10,11,12,13
            AddNewLines(1) +
            GenerateFields9To13(formData, cultureInfo) +

            //14 
            GenerateField14(formData, cultureInfo) +

            //16
            AddSpaces(Field161StartingPointFromLeftMargin) + formData.VystaveneV +
            AddSpaces(Field162StartingPointFromField161 - formData.VystaveneV.Length) +
            formData.VystaveneDne.ToString("d.M.yyyy", cultureInfo) + EndOfLine +

            //20
            AddNewLines(7) +
            GenerateField20(formData) +

            //End
            AddControlCode(endFilePrinterCommands) + AddControlCode(VariableLineFeedToZeroCommand) + AddControlCode(FormFeed);

        return new MemoryStream(Encoding.GetBytes(cmrText));
    }

    private static string GenerateFields9To13(CmrFormDataDto formData, CultureInfo cultureInfo)
    {
        var output = string.Empty;

        var goods = formData.Zbozi
            .DistinctBy(x => x.DruhObalu + x.Oznaceni)
            .GroupBy(x => x.DruhObalu)
            .Select(druhy =>
                (druhy.Key + Space).PadRight(4, Space) + String.Join(",", druhy.Select(zb => zb.Oznaceni)));

        var goodsFormatted = String.Join(Environment.NewLine, goods);

        for (var i = 1; i < goodsFormatted.Length; i++)
        {
            if (i % Field11And12FullWidth == 0 && goodsFormatted[i - 1] != EndOfLine)
            {
                goodsFormatted = goodsFormatted.Insert(i, EndOfLine + AddSpaces(Field12Padding));
            }
        }

        var goodsLinesSplit = goodsFormatted.Split(EndOfLine);

        var goodsLines = new string[Field11And12MaxNumberOfLines];

        for (int i = 0; i < goodsLines.Length; i++)
        {
            goodsLines[i] = goodsLinesSplit.Length > i ? goodsLinesSplit[i] : string.Empty;
        }

        //Line 1
        output += formData.DruhKontejneru +
                  AddSpaces(Field10StartingPointFromLeftMargin - formData.DruhKontejneru.Length) +
                  formData.CisloKontejneru +
                  AddSpaces(Field11StartingPointFromField10 - formData.CisloKontejneru.Length) +
                  goodsLines[0] +
                  AddSpaces(Field13StartingPointFromField11 - goodsLines[0].Length) +
                  formData.ImportVaha + EndOfLine;

        //Line 2
        var taraText = AppendLineWithPrefixOrEmptyString(Tara,
            formData.TaraKontejneru.ToString());
        output += AddSpaces(Field11StartingPointFromLeftMargin) + goodsLines[1] +
                  AddSpaces(Field13StartingPointFromField11 - goodsLines[1].Length) + taraText;

        //Line 3
        output += formData.TemperatureInstruction +
                  AddSpaces(Field11StartingPointFromLeftMargin - formData.TemperatureInstruction.Length) +
                  goodsLines[2] + EndOfLine;

        //Line 4
        output += formData.WasteInstruction +
                  AddSpaces(Field11StartingPointFromLeftMargin - formData.WasteInstruction.Length) +
                  goodsLines[3] + EndOfLine;

        //Line 5
        var rejdText =
            AppendLineWithPrefixOrEmptyString(Rejd, formData.OwnerText);
        rejdText = rejdText.Substring(0, rejdText.Length - 1);
        output += rejdText + AddSpaces(Field11StartingPointFromLeftMargin - rejdText.Length) +
                  goodsLines[4] + EndOfLine;

        //Line 6
        var codeText = AppendLineWithPrefixOrEmptyString(Code, formData.UnloadingCode);
        codeText = codeText.Substring(0, codeText.Length - 1);
        output += codeText + AddSpaces(Field11StartingPointFromLeftMargin - codeText.Length) +
                  goodsLines[5] + EndOfLine;

        //Line 7
        output += EndOfLine;

        //Line 8 + 9
        var adrInstructionLine = formData.AdrInstruction;
        var lineLength = 55;
        if (adrInstructionLine.Length > lineLength)
        {
            adrInstructionLine = adrInstructionLine.Insert(lineLength, EndOfLine.ToString());
        }

        if (adrInstructionLine.Length > lineLength * 2)
        {
            adrInstructionLine = adrInstructionLine.Substring(0, lineLength * 2 + 1);
        }

        output += adrInstructionLine + EndOfLine;

        var maxLines = 10;
        var actualLines = output.Count(c => c == EndOfLine);
        output += AddNewLines(maxLines - actualLines);

        return output;
    }

    private static string GenerateField14(CmrFormDataDto formData, CultureInfo cultureInfo)
    {
        var prefix = string.Empty;
        var output = string.Empty;

        if (!string.IsNullOrEmpty(formData.CloText1))
        {
            if (formData.CloText1Prefix)
            {
                prefix = $"{Clo}  ";
            }

            output += AppendLineWithPaddingOrEmptyString(0, prefix + formData.CloText1) +
                      AppendLineWithPaddingOrEmptyString(Field14Padding, formData.CloText2) +
                      AppendLineWithPaddingOrEmptyString(Field14Padding, formData.CloText3) +
                      AppendLineWithPaddingOrEmptyString(Field14Padding, formData.CloText4);
        }

        if (!string.IsNullOrEmpty(formData.DeclText1))
        {
            if (formData.DeclText1Prefix)
            {
                prefix = $"{Dekl} ";
            }

            output += AppendLineWithPaddingOrEmptyString(0, prefix + formData.DeclText1) +
                      AppendLineWithPaddingOrEmptyString(Field14Padding, formData.DeclText2) +
                      AppendLineWithPaddingOrEmptyString(Field14Padding, formData.DeclText3) +
                      AppendLineWithPaddingOrEmptyString(Field14Padding, formData.DeclText4);
        }

        var actualLines = output.Count(c => c == EndOfLine);
        output += AddNewLines(Field14MaxLines - actualLines);

        return output;
    }

    private static string GenerateField20(CmrFormDataDto formData)
    {
        var field20ParagraphLineLengthTimesTwo = Field20ParagraphLineLength * 2;

        var pickUpInstructionLine1 = formData.PickUpInstruction;
        var pickUpInstructionLine2 = string.Empty;
        var pickUpInstructionLine3 = string.Empty;
        var pickupInstructionLength = formData.PickUpInstruction.Length;

        if (pickupInstructionLength > Field20ParagraphLineLength)
        {
            pickUpInstructionLine2 =
                pickUpInstructionLine1.Substring(Field20ParagraphLineLength,
                    pickupInstructionLength - Field20ParagraphLineLength);
            pickUpInstructionLine1 = pickUpInstructionLine1.Substring(0, Field20ParagraphLineLength);
        }

        if (pickupInstructionLength > field20ParagraphLineLengthTimesTwo)
        {
            var pickUpInstructionLine2Length = pickUpInstructionLine2.Length;
            pickUpInstructionLine3 =
                pickUpInstructionLine2.Substring(Field20ParagraphLineLength,
                    pickUpInstructionLine2Length >= field20ParagraphLineLengthTimesTwo
                        ? Field20ParagraphLineLength
                        : pickUpInstructionLine2Length - Field20ParagraphLineLength);
            pickUpInstructionLine2 = pickUpInstructionLine2.Substring(0, Field20ParagraphLineLength);
        }

        var returnToInstructionLine1 = formData.ReturnToInstruction;
        var returnToInstructionLine2 = string.Empty;
        var returnToInstructionLine3 = string.Empty;
        var returnToInstructionLength = formData.ReturnToInstruction.Length;

        if (returnToInstructionLength > Field20ParagraphLineLength)
        {
            returnToInstructionLine2 =
                returnToInstructionLine1.Substring(Field20ParagraphLineLength,
                    returnToInstructionLength - Field20ParagraphLineLength);
            returnToInstructionLine1 = returnToInstructionLine1.Substring(0, Field20ParagraphLineLength);
        }

        if (returnToInstructionLength > Field20ParagraphLineLength * 2)
        {
            var returnToInstructionLine2Length = returnToInstructionLine2.Length;
            returnToInstructionLine3 =
                returnToInstructionLine2.Substring(Field20ParagraphLineLength,
                    returnToInstructionLine2Length >= field20ParagraphLineLengthTimesTwo
                        ? Field20ParagraphLineLength
                        : returnToInstructionLine2Length - Field20ParagraphLineLength);
            returnToInstructionLine2 = returnToInstructionLine2.Substring(0, Field20ParagraphLineLength);
        }

        //Weight Instruction
        var output = string.IsNullOrEmpty(formData.WeightingInstruction)
            ? EndOfLine.ToString()
            : AppendLineWithPaddingOrEmptyString(7, formData.WeightingInstruction);

        //Pickup / Return Instruction
        output += pickUpInstructionLine1 +
                  AddSpaces(Field20ParagraphLineLength - pickUpInstructionLine1.Length +
                            Field20PickupReturnSpacesBetween) +
                  returnToInstructionLine1 + EndOfLine +
                  pickUpInstructionLine2 +
                  AddSpaces(Field20ParagraphLineLength - pickUpInstructionLine2.Length +
                            Field20PickupReturnSpacesBetween) +
                  returnToInstructionLine2 + EndOfLine +
                  pickUpInstructionLine3 +
                  AddSpaces(Field20ParagraphLineLength - pickUpInstructionLine3.Length +
                            Field20PickupReturnSpacesBetween) +
                  returnToInstructionLine3 + AddNewLines(2);


        //Services
        var servicesLine = formData.Services;
        if (servicesLine.Length > Field20LineLength)
        {
            servicesLine = servicesLine.Insert(Field20LineLength, EndOfLine.ToString());
        }

        if (servicesLine.Length > Field20LineLength * 2)
        {
            servicesLine = servicesLine.Substring(0, Field20LineLength * 2 + 1);
        }

        output += servicesLine + EndOfLine;

        //Notes
        var notesLine = formData.Notes;
        var notesLineLength = notesLine.Length;
        if (notesLineLength > Field20LineLength)
        {
            notesLine = notesLine.Insert(Field20LineLength, EndOfLine.ToString());
        }

        if (notesLineLength > Field20LineLength * 2)
        {
            notesLine = notesLine.Insert(Field20LineLength * 2, EndOfLine.ToString());
        }

        if (notesLineLength > Field20LineLength * 3)
        {
            notesLine = notesLine.Substring(0, Field20LineLength * 3 + 2);
        }

        output += notesLine;
        return output;
    }

    private static string AddControlCode(byte[] controlCode)
    {
        return Encoding.GetString(controlCode);
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
        return string.IsNullOrEmpty(text)
            ? string.Empty + EndOfLine
            : $"{prefix} {text}" + EndOfLine;
    }

    private static string AppendLineWithPaddingOrEmptyString(int padding, string? text)
    {
        return string.IsNullOrEmpty(text)
            ? string.Empty
            : AddSpaces(padding) + text + EndOfLine;
    }

    private static string TrimTextToMaxLengthIfTooLong(int maxLength, string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        return text.Length > maxLength ? text.Substring(0, maxLength) : text;
    }

    private static string AddSpaces(int numberOfSpaces)
    {
        if (numberOfSpaces < 0) numberOfSpaces = 0;
        return new string(Space, numberOfSpaces);
    }

    private static string AddNewLines(int numberOfNewLines)
    {
        return new string(EndOfLine, numberOfNewLines);
    }
}