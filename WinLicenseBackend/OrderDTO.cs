using WinLicenseBackend.Models;

namespace WinLicenseBackend
{
    public class OrderDTO
    {
        public int ID_Order { get; set; }
        public ParsedLicense Order_RegistrationContent { get; set; }
        public Product Product { get; set; }
    }
}
