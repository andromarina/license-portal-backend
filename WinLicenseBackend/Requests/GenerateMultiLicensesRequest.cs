namespace WinLicenseBackend.Requests
{
    public class GenerateMultiLicensesRequest
    {
        public int[] OrderIds { get; set; }
        public string[] Emails { get; set; }
        public int? NumDays { get; set; }
    }
}
