namespace WinLicenseBackend.Models
{
    public class Customer
    {
        public int ID_Customer { get; set; }
        public string Customer_Salutation { get; set; }
        public string Customer_FirstName { get; set; }
        public string Customer_LastName { get; set; }
        public string Customer_JobTitle { get; set; }
        public string Customer_Organization { get; set; }
        public string Customer_Email { get; set; }
        public string Customer_Phone { get; set; }
        public string Customer_Fax { get; set; }
        public string Customer_Address1 { get; set; }
        public string Customer_Address2 { get; set; }
        public string Customer_City { get; set; }
        public string Customer_Zip { get; set; }
        public string Customer_State { get; set; }
        public string Customer_Country { get; set; }
        public string Customer_Notes { get; set; }
        public bool Customer_IsAcceptingMail { get; set; }
        public bool Customer_IsRegistered { get; set; }
        public bool Customer_IsBlacklisted { get; set; }
        public DateTime Customer_DateEdit { get; set; }
    }

}
