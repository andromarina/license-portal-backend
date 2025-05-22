namespace WinLicenseBackend.Models
{
    public class Order
    {
        public int ID_Order { get; set; }
        public DateTime? Order_Date { get; set; }
        public DateTime? Order_ExpireDate { get; set; }
        public bool? Order_MarkedAsExpired { get; set; }
        public string Order_DeliveryMethod { get; set; }
        public string Order_HowFound { get; set; }
        public string Order_RegistrationName { get; set; }
        public string Order_ShippingAddress { get; set; }
        public string Order_ShippingCity { get; set; }
        public string Order_ShippingZip { get; set; }
        public string Order_ShippingState { get; set; }
        public string Order_ShippingCountry { get; set; }
        public int? ID_Product { get; set; }
        public double? Order_UnitPrice { get; set; }
        public double? Order_ShippingFee { get; set; }
        public double? Order_ServiceFee { get; set; }
        public int? Order_Quantity { get; set; }
        public double? Order_Total { get; set; }
        public string Order_PaymentMethod { get; set; }
        public bool? Order_WasPaid { get; set; }
        public int? ID_Customer { get; set; }
        public ParsedLicense Order_RegistrationContent { get; set; }
        public byte? Order_LastRegistrationType { get; set; }
        public string Order_Notes { get; set; }
    }

}
