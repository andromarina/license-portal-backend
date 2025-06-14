using MailKit.Search;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly EmailService _emailService;

        public LicenseController(
            IDataProvider dataProvider,
            LicenseGenerator licenseGenerator,
            EmailService emailService)
        {
            _dataProvider = dataProvider;
            _generator = licenseGenerator;
            _emailService = emailService;
        }

        [HttpGet("generateLicense"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult GenerateLicense([FromQuery] int orderId)
        {

            var order = _dataProvider.GetOrder(orderId);
            if (order == null)
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

        [HttpPost("generateMultipleLicenses"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
                if(request.NumDays > 0)
                {
                    licenseParams.NumDays = request.NumDays.Value;
                }
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

        [HttpPost("generateAndSend"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GenerateAndSendAsync([FromBody] GenerateMultiLicensesRequest request)
        {
            if (request.Emails == null || request.Emails.Length == 0)
            {
                throw new BadHttpRequestException("At least 1 email address should be added");
            }
            
            // 1. Generate the file result
            var fileResult = (FileStreamResult)GenerateMultipleLicenses(request);

            // 2. Extract bytes + metadata
            var (bytes, name, type) = await GetFileFromActionResult(fileResult);

            string[] productsList = _dataProvider.GetProductsFromOrders(request.OrderIds).Select(o => o.Product_Name).ToArray();
            Order order = _dataProvider.GetOrder(request.OrderIds[0]);
            string userName = order.Order_RegistrationContent?.UserName ?? "Customer";
            // 3. Send mail (choose one)
            _emailService.SendFileWithSmtpClient(bytes, name, productsList,type, request.Emails[0], userName);
  
            // 4. Return success
            return Ok(new { message = "Sent by email!" });
        }

        private async Task<(byte[] fileBytes, string fileName, string contentType)> GetFileFromActionResult(IActionResult actionResult)
        {
            if (actionResult is FileContentResult contentResult)
            {
                return (contentResult.FileContents,
                        contentResult.FileDownloadName,
                        contentResult.ContentType);
            }
            else if (actionResult is FileStreamResult streamResult)
            {
                using var ms = new MemoryStream();
                await streamResult.FileStream.CopyToAsync(ms);
                return (ms.ToArray(),
                        streamResult.FileDownloadName,
                        streamResult.ContentType);
            }

            throw new InvalidOperationException("Unsupported result type");
        }
    }

}