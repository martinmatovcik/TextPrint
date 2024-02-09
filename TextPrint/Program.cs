using System.Globalization;
using System.Text;
using NodaTime;
using TextPrint.cmr;
using TextPrint.lpr;

namespace TextPrint;

public class Program
{
    private static readonly Encoding? Encoding = CodePagesEncodingProvider.Instance.GetEncoding(852);

    static void Main()
    {
        var s = CmrTextFileGenerator.GenerateCmrTextAsStream(GenerateCmrFormDataDto(), CultureInfo.CurrentCulture, 0);
        PrintClient.PrintStream(s, Encoding);
        
        // FileCreator.SaveToFile(s, Encoding);
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
            CisloPlomby = "",

            //6
            Dopravce1 = "ACKERMAN II. spol. s r.o.",
            Dopravce2 = "Papírníkova 612",
            Dopravce3 = "Praha 4",
            Dopravce4 = "142 00 CZ",

            //8
            PripojeneDoklady = "",

            //9,10,11,12
            WasteInstruction = "",

            TemperatureInstruction = "",

            OwnerText = "",
            DruhKontejneru = "",
            CisloKontejneru = "",
            Zbozi = new List<ZboziData>
            {
            },
            UnloadingCode = "",

            //13
            TaraKontejneru = 4000,
            ImportVaha = 16625,
            AdrInstruction =
                "",

            //14
            CloText1Prefix = false,
            CloText1 = "",
            CloText2 = "",
            CloText3 = "",
            CloText4 = "",

            DeclText1Prefix = false,
            DeclText1 = "",
            DeclText2 = "",
            DeclText3 = "",
            DeclText4 = "",

            //16
            VystaveneDne = new LocalDate(2024, 1, 17),
            VystaveneV = "",

            //20
            WeightingInstruction = "",
            PickUpInstruction =
                "",
            ReturnToInstruction =
                "",
            Services =
                "",
            Notes =
                ""
        };
    }
}