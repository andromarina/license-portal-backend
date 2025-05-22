namespace WinLicenseBackend.Models
{
    public class ProductCustomInfo
    {
        public int DefaultActivationSimultaneousDevices { get; set; }
        public string DefaultRegistrationType { get; set; }
        public int DefaultActivationLimitActivation { get; set; }
        public int DefaultActivationLimitDeactivation { get; set; }
        public int DefaultActivationMaxDifferentDevices { get; set; }
        public string DefaultActivationFormat { get; set; }
        public bool OpenFolderWhenGeneratingLicense { get; set; }
        public bool AutoGenerateInFolder { get; set; }
        public string LicenseFileBinaryName { get; set; }
        public string LicenseFileTextName { get; set; }
        public string LicenseRegistryHive { get; set; }
        public string LicenseRegistryName { get; set; }
        public string GenerateLicensesInFolder { get; set; }
        public string LicenseRegistryValueName { get; set; }
    }
}
