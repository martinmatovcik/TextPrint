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
        var s = CmrTextFileGenerator.GenerateCmrTextAsStream(GenerateCmrFormDataDto(), CultureInfo.CurrentCulture, 4);
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
            UnloadingCode = "",

            //13
            TaraKontejneru = 4000,
            ImportVaha = 16625,
            AdrInstruction =
                "\"ADR - popis viz. Doplňkový list ADR, ADR - siehe in der Ergänzungsliste ADR\"",

            //14
            CloText1Prefix = true,
            CloText1 = "CZ590201 - PARDUBICE",
            CloText2 = "Palackého 2659",
            CloText3 = "asdsad",
            CloText4 = "",

            DeclText1Prefix = true,
            DeclText1 = "",
            DeclText2 = "",
            DeclText3 = "",
            DeclText4 = "",

            //16
            VystaveneDne = new LocalDate(2024, 1, 17),
            VystaveneV = "Česká Třebová",

            //20
            WeightingInstruction = "JET PO NAKLÁDCE NA VÁHU!/LITRY",
            PickUpInstruction =
                "Vyzvednout: Embrace the journey",
            ReturnToInstruction =
                "Vrátit: Embrace the journey, conquer challenges, and cherish",
            Services =
                "Služby: Embrace the journey, conquer challenges, and cherish the moments. Life is a canvas; paint it with passion. Shine brightly, the world awaits your brilliance.",
            Notes =
                "Poznámky: Embrace the journey, conquer challenges, and cherish the moments. Life is a canvas; paint it with pass"
        };
    }
}