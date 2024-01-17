using NodaTime;

namespace TextPrint;

public class CmrFormDataDto
{
    public bool NoteTooLong { get; set; }
    public TransportTypeEnum ImportExport { get; set; }

    // 1
    public string CmrNumber { get; set; }

    public string Ridic { get; set; }

    public string RegistracniZnackaTahac { get; set; }
    public string RegistracniZnackaNaves { get; set; }
    public string CisloRidice { get; set; }

    public string ObjednavkaCislo { get; set; }
    public string ReleaseCislo { get; set; }
    public string KontaktOsoba { get; set; }
    public string KontaktTelefon1 { get; set; }
    public string KontaktTelefon2 { get; set; }

    // 2
    public string OdesilatelPrijemce1 { get; set; }

    public string OdesilatelPrijemce2 { get; set; }
    public string OdesilatelPrijemce3 { get; set; }
    public string OdesilatelPrijemce4 { get; set; }

    // 3
    public string AdresaNakladky1 { get; set; }

    public string AdresaNakladky2 { get; set; }
    public string AdresaNakladky3 { get; set; }
    public string AdresaNakladky4 { get; set; }

    // 4
    public LocalDateTime CasPristaveni { get; set; }

    public string MistoPristaveni { get; set; }

    // 5
    public string CisloPlomby { get; set; }

    // 6
    public string Dopravce1 { get; set; }

    public string Dopravce2 { get; set; }
    public string Dopravce3 { get; set; }
    public string Dopravce4 { get; set; }

    // 8
    public string PripojeneDoklady { get; set; }

    // 9 { get; set; } 10 { get; set; } 11 { get; set; } 12
    public string OwnerText { get; set; }

    public string WasteInstruction { get; set; }
    public string TemperatureInstruction { get; set; }
    public string DruhKontejneru { get; set; }
    public string CisloKontejneru { get; set; }
    public List<ZboziData> Zbozi { get; set; }

    public string? UnloadingCode { get; set; }

    // 13
    public float? TaraKontejneru { get; set; }

    public float? ImportVaha { get; set; }
    public string AdrInstruction { get; set; }

    // 14
    public bool CloText1Prefix { get; set; }

    public string? CloText1 { get; set; }
    public string? CloText2 { get; set; }
    public string? CloText3 { get; set; }
    public string? CloText4 { get; set; }
    public bool DeclText1Prefix { get; set; }
    public string? DeclText1 { get; set; }
    public string? DeclText2 { get; set; }
    public string? DeclText3 { get; set; }
    public string? DeclText4 { get; set; }

    // 16
    public string VystaveneV { get; set; }

    public LocalDate VystaveneDne { get; set; }

    // 20
    public string WeightingInstruction { get; set; }

    public string PickUpInstruction { get; set; }
    public string ReturnToInstruction { get; set; }
    public string Services { get; set; }
    public string Notes { get; set; }
}

public class ZboziData
{
    public string DruhObalu { get; set; } = "";
    public string Oznaceni { get; set; } = "";
}

public enum TransportTypeEnum
{
    Import = 0,
    Export = 1,
}