using WinLicenseBackend.DataProviders;
using WinLicenseBackend.Models;
using Microsoft.Extensions.Logging;
using System.IO;

namespace WinLicenseBackend.Services
{
    public class LicenseGenerator
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();


        public MemoryStream GenerateLicense(GenerateLicenseParameters parameters)
        {
            _logger.Info("Generating license for user '{Name}', hardware ID: {HardId}", parameters.Name, parameters.HardId);

            try
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

                if (parameters.ExpDateEnabled)
                {
                    ExpDateSysTime.wYear = (short)parameters.ExpDateSysTime.Year;
                    ExpDateSysTime.wMonth = (short)parameters.ExpDateSysTime.Month;
                    ExpDateSysTime.wDay = (short)parameters.ExpDateSysTime.Day;

                    _logger.Info("License expiration set to {Date}", parameters.ExpDateSysTime.ToShortDateString());
                    SizeKey = WinlicenseSDK.WLGenLicenseFileKey(
                        parameters.LicenseHash,
                        pNameEdit, pOrgEdit, pCustomEdit, pHardIdEdit,
                        mNumDays, mNumExec,
                        ExpDateSysTime, 0, 0, 0, LicenseKeyBuff
                    );
                }
                else
                {
                    _logger.Info("No expiration date set for license.");
                    SizeKey = WinlicenseSDK.WLGenLicenseFileKey(
                        parameters.LicenseHash,
                        pNameEdit, pOrgEdit, pCustomEdit, pHardIdEdit,
                        mNumDays, mNumExec,
                        NullExpDateSysTime, 0, 0, 0, LicenseKeyBuff
                    );
                }

                MemoryStream ms = new MemoryStream();
                using BinaryWriter w = new BinaryWriter(ms);
                for (int i = 0; i < SizeKey; i++)
                {
                    w.Write((byte)LicenseKeyBuff[i]);
                }

                w.Flush();
                ms.Position = 0;

                _logger.Info("License successfully generated. Size: {Size} bytes", SizeKey);
                return ms;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to generate license for user '{Name}'", parameters.Name);
                throw;
            }
        }

        public GenerateLicenseParameters GetLicenseParameters(Order order, Product product)
        {
            var regContent = order.Order_RegistrationContent;
            ProductCustomInfo customInfo = product.Product_CustomInfo;

            var licenseParameters = new GenerateLicenseParameters()
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

            _logger.Info("License parameters extracted for user '{UserName}', product: {ProductName}", regContent.UserName, product.Product_Name);
            return licenseParameters;
        }
    }
}
