namespace WinLicenseBackend.Models
{
    public class Activation
    {
        public int ID_Activation { get; set; }
        public string Activation_String { get; set; }
        public int Activation_ID_Order { get; set; }
        public DateTime Activation_CreationDate { get; set; }
        public bool Activation_IsDisabled { get; set; }
        public int Activation_CurrentDeactivations { get; set; }
        public int Activation_CurrentActivations {  get; set; }
    }
}
