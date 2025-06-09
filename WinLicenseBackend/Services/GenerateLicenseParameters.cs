using WinLicenseBackend.Models;

namespace WinLicenseBackend.Services
{
    public class GenerateLicenseParameters
    {        
        public string LicenseHash { get; set; }
        public DateTime ExpDateSysTime { get; set; }
        public string Name { get; set; }
        public string Org { get; set; }
        public string Custom { get; set; }
        public string HardId { get; set; }
        public int NumDays { get; set; }
        public int NumExec {  get; set; }
        public bool ExpDateEnabled { get; set; }
        public string RegName { get; set; }
        public string RegValueName { get; set; }
        public string RegFileName { get; set; }
    }
}
