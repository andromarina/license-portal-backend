namespace WinLicenseBackend.Models
{
    public class Product
    {
        public int ID_Product { get; set; }
        public string Product_LicenseHash { get; set; }
        public string Product_TrialHash { get; set; }
        public string Product_PasswordHash { get; set; }
        public string Product_InputFilename { get; set; }
        public string Product_OutputFilename { get; set; }
        public int? Product_Type { get; set; }
        public string Product_Name { get; set; }
        public string Product_Version { get; set; }
        public string Product_Description { get; set; }
        public string Product_Icon { get; set; }
        public double? Product_UnitPrice { get; set; }
        public double? Product_ShippingFee { get; set; }
        public double? Product_ServiceFee { get; set; }
        public ProductCustomInfo Product_CustomInfo { get; set; }
        public string Product_InvoiceTemplatePath { get; set; }
        public string Product_Notes { get; set; }
        public string Product_KeyGenDLL { get; set; }
        public string Product_KeyGenFunction { get; set; }
    }

}
