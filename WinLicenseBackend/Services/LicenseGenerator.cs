using WinLicenseBackend.DataProviders;
using WinLicenseBackend.Models;

namespace WinLicenseBackend.Services
{
    public class LicenseGenerator
    {
        public MemoryStream GenerateLicense(GenerateLicenseParameters parameters)
        {
            byte[] LicenseKeyBuff = new byte[4000];
            string pNameEdit = parameters.Name;
            string pOrgEdit = parameters.Org;
            string pCustomEdit = parameters.Custom;
            string pHardIdEdit = parameters.HardId;
            int mNumDays = parameters.NumDays;
            int mNumExec = parameters.NumExec;
            SystemTime ExpDateSysTime = new SystemTime();
            SystemTime NullExpDateSysTime = null;
            int SizeKey;
            string RegName = "HKEY_LOCAL_MACHINE\\" + parameters.RegName;

            // get the current date from dateTimePicker1

            ExpDateSysTime.wYear = (short)parameters.ExpDateSysTime.Year;
            ExpDateSysTime.wMonth = (short)parameters.ExpDateSysTime.Month;
            ExpDateSysTime.wDay = (short)parameters.ExpDateSysTime.Day;

            // generate license file
         
            if (parameters.ExpDateEnabled == false)
            {
                SizeKey = WinlicenseSDK.WLGenLicenseFileKey(parameters.LicenseHash, pNameEdit, pOrgEdit, pCustomEdit, pHardIdEdit, mNumDays, mNumExec, NullExpDateSysTime, 0, 0, 0, LicenseKeyBuff);
            }
            else
            {
                SizeKey = WinlicenseSDK.WLGenLicenseFileKey(parameters.LicenseHash, pNameEdit, pOrgEdit, pCustomEdit, pHardIdEdit, mNumDays, mNumExec, ExpDateSysTime, 0, 0, 0, LicenseKeyBuff);
            }

            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            for (int i = 0; i < SizeKey; i++)
            {
                w.Write((byte)LicenseKeyBuff[i]);
            }

            w.Flush(); // Make sure everything is written
            ms.Position = 0; // Rewind the stream

            return ms;
        }

        public GenerateLicenseParameters GetLicenseParameters(Order order, Product product)
        {
            var regContent = order.Order_RegistrationContent;           
            ProductCustomInfo customInfo = product.Product_CustomInfo;

            GenerateLicenseParameters licenseParameters = new GenerateLicenseParameters()
            {
                LicenseHash = product.Product_LicenseHash,
                Name = regContent.UserName,
                Org = regContent.Company,
                Custom = "",
                HardId = regContent.HardwareId,
                NumDays = regContent.DaysExpiration,
                ExpDateEnabled = false,
                RegName = customInfo.LicenseRegistryName,
                RegFileName = customInfo.LicenseFileBinaryName,
                RegValueName = customInfo.LicenseRegistryValueName,
            };
            return licenseParameters;
        }
    }
}
