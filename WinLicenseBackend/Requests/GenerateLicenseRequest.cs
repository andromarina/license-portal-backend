namespace WinLicenseBackend.Requests
{
    public class GenerateLicenseRequest
    {
        public int ProductId {  get; set; } 
        public int OrderId { get; set; }
    }
}
