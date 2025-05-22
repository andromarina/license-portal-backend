namespace WinLicenseBackend.Models
{
    public class Project
    {
        public int ID_Project { get; set; }
        public string Project_Name { get; set; }
        public string Project_Settings { get; set; }
        public int ID_Product { get; set; }
        public DateTime Project_DateEdit { get; set; }
    }

}
