namespace WinLicenseBackend.Models
{
    public class ParsedLicense
    {
        public string UserName { get; set; }
        public string Company { get; set; }
        public string HardwareId { get; set; }
        public int DaysExpiration { get; set; }
        public string LicenseDataBase64 { get; set; }
    }
}
