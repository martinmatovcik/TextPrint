using System.Globalization;
using System.Text;
using NodaTime;
using TextPrint.cmr;
using TextPrint.lpr;

namespace TextPrint;

public class Program
{
    private static readonly Encoding? Encoding = CodePagesEncodingProvider.Instance.GetEncoding(852);

    static async Task Main()
    {

        for (int i = 0; i < 10; i++)
        {
            var s = CmrTextFileGenerator.GenerateCmrTextAsStream(GenerateCmrFormDataDto(), CultureInfo.CurrentCulture, 0);
            await PrintClient.PrintStreamAsync(s, Encoding);
            await Task.Delay(2000);
            Console.WriteLine("Iteration: " + i);
        }

        // FileCreator.SaveToFile(s, Encoding);
    }

    private static CmrFormDataDto GenerateCmrFormDataDto()
    {
        return new CmrFormDataDto()
        {
            NoteTooLong = false,
            ImportExport = TransportTypeEnum.Import,

            //1
            CmrNumber = "AAA2024000999",
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
            OdesilatelPrijemce2 = "",
            OdesilatelPrijemce3 = "",
            OdesilatelPrijemce4 = "",

            // //2
            // OdesilatelPrijemce1 = "",
            // OdesilatelPrijemce2 = "SchumiTransport, Koclířov 258",
            // OdesilatelPrijemce3 = "",
            // OdesilatelPrijemce4 = "",
            
            // //2
            // OdesilatelPrijemce1 = "",
            // OdesilatelPrijemce2 = "",
            // OdesilatelPrijemce3 = "KOCLIROV,",
            // OdesilatelPrijemce4 = "",
            
            // //2
            // OdesilatelPrijemce1 = "",
            // OdesilatelPrijemce2 = "",
            // OdesilatelPrijemce3 = "",
            // OdesilatelPrijemce4 = "56911 CZ",

            //3
            AdresaNakladky1 = "WESTROCK PACKAGING, VYKL. KOCLÍŘOV",
            AdresaNakladky2 = "",
            AdresaNakladky3 = "",
            AdresaNakladky4 = "",

            // //3
            // AdresaNakladky1 = "",
            // AdresaNakladky2 = "SchumiTransport, Koclířov 258",
            // AdresaNakladky3 = "",
            // AdresaNakladky4 = "",
            //
            // //3
            // AdresaNakladky1 = "",
            // AdresaNakladky2 = "",
            // AdresaNakladky3 = "KOCLIROV, 56911 CZ",
            // AdresaNakladky4 = "",
            //
            // //3
            // AdresaNakladky1 = "",
            // AdresaNakladky2 = "",
            // AdresaNakladky3 = "",
            // AdresaNakladky4 = "pan Paclík 737 515 907 F:",

            //4
            CasPristaveni = new LocalDateTime(2024, 2, 16, 13, 0),
            MistoPristaveni = "U příjemce__1__2__3",

            //5
            CisloPlomby = "",

            //6
            Dopravce1 = "ACKERMAN II. spol. s r.o.",
            Dopravce2 = "Papírníkova 612",
            Dopravce3 = "Praha 4",
            Dopravce4 = "142 00 CZ",

            // //6
            // Dopravce1 = "ACKERMAN II. spol. s r.o.__1",
            // Dopravce2 = "Papírníkova 612__1",
            // Dopravce3 = "Praha 4__1",
            // Dopravce4 = "142 00 CZ__1",

            // //6
            // Dopravce1 = "ACKERMAN II. spol. s r.o.__1__2",
            // Dopravce2 = "Papírníkova 612__1__2",
            // Dopravce3 = "Praha 4__1__2",
            // Dopravce4 = "142 00 CZ__1__2",

            // //6
            // Dopravce1 = "ACKERMAN II. spol. s r.o.__1__2__3",
            // Dopravce2 = "Papírníkova 612__1__2__3",
            // Dopravce3 = "Praha 4__1__2__3",
            // Dopravce4 = "142 00 CZ__1__2__3",

            //8
            PripojeneDoklady = "ABCDEFGHIJKLMNO",

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
            VystaveneDne = new LocalDate(2025, 3, 17),
            VystaveneV = "111111______11111",

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