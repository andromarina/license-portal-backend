namespace WinLicenseBackend.Models
{
    public class ActivationDevice
    {
        public int ID_ActivationDevice { get; set; }
        public int ID_Activation { get; set; }
        public string ActivationDevice_HardwareId { get; set; }
        public bool ActivationDevice_IsRevoked { get; set; }
        public DateTime ActivationDevice_Date { get; set; }
    }

}
