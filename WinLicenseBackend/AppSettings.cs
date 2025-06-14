﻿namespace WinLicenseBackend
{
    public class AppSettings
    {
        public string Domain { get; set; }
        public string EmailFrom { get; set; }
        public string EmailTitle { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSMTPssl { get; set; }
        public string PrivateKey { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPass { get; set; }
        public string DkimPrivateKey { get; set; }
    }
}
