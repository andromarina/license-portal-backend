using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using WinLicenseBackend;
using WinLicenseBackend.DataProviders;
using WinLicenseBackend.Models;
using WinLicenseBackend.Requests;
using WinLicenseBackend.Services;

namespace CustomerApiProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LicenseController : ControllerBase
    {
        private readonly IDataProvider _dataProvider;
        private readonly LicenseGenerator _generator;

        public LicenseController(IDataProvider dataProvider,
            LicenseGenerator licenseGenerator)
        {
            _dataProvider = dataProvider;
            _generator = licenseGenerator;
        }

        [HttpGet("generateLicense")]
        public ActionResult GenerateLicense([FromQuery] int orderId)
        {          
            
            var order = _dataProvider.GetOrder(orderId);
            if(order == null)
            {
                return NotFound("Order not found");
            }
            var regContent = order.Order_RegistrationContent;
            var product = _dataProvider.GetProduct(order.ID_Product.Value);
            if (product == null)
            {
                return NotFound("Product not found");
            }
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
            MemoryStream licenseMS = _generator.GenerateLicense(licenseParameters);         
            var contentType = "application/octet-stream";
            var fileName = customInfo.LicenseFileBinaryName;

            return File(licenseMS, contentType, fileName);
        }

        [HttpPost("generateMultipleLicenses")]
        public ActionResult GenerateMultipleLicenses([FromBody] GenerateMultiLicensesRequest request)
        {
            int[] orderIds = request.OrderIds;
            if (orderIds == null || orderIds.Length == 0)
                return BadRequest("No order IDs provided.");

            var licenseStreams = new Dictionary<string, MemoryStream>();
            string userName = null;
            string company = null;

            foreach (var orderId in orderIds)
            {
                var order = _dataProvider.GetOrder(orderId);
                if (order == null)
                    return NotFound($"Order with ID {orderId} not found.");

                var product = _dataProvider.GetProduct(order.ID_Product ?? 0);
                if (product == null)
                    return NotFound($"Product for order {orderId} not found.");

                if (userName == null) // Set once from the first valid order
                {
                    userName = order.Order_RegistrationContent?.UserName ?? "UnknownUser";
                    company = order.Order_RegistrationContent?.Company ?? "UnknownCompany";
                }

                var licenseParams = _generator.GetLicenseParameters(order, product);
                var licenseStream = _generator.GenerateLicense(licenseParams);
                string fileName = product.Product_CustomInfo?.LicenseFileBinaryName ?? $"license_{orderId}.dat";

                licenseStreams[fileName] = licenseStream;
            }

            if (licenseStreams.Count == 0)
                return StatusCode(500, "No licenses were generated.");

            var zipStream = Utils.CreateZipFromStreams(licenseStreams);
            string zipFileName = $"{userName}_{company}.zip".Replace(" ", "_");

            return File(zipStream, "application/zip", zipFileName);
        }
    }
    
}