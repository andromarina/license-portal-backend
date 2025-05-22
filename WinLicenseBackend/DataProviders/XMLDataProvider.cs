using System.Collections;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WinLicenseBackend.Models;

namespace WinLicenseBackend.DataProviders
{
    public class XmlDataProvider : IDataProvider
    {
        private readonly string _projectsPath;
        private readonly string _activationDevicesPath;
        private readonly string _customersPath;
        private readonly string _ordersPath;
        private readonly string _productsPath;
        private readonly string _activationsPath;

        public XmlDataProvider(string projectsPath, string activationDevicesPath, string customersPath, string ordersPath, string productsPath, string activationsPath)
        {
            _projectsPath = projectsPath;
            _activationDevicesPath = activationDevicesPath;
            _customersPath = customersPath;
            _ordersPath = ordersPath;
            _productsPath = productsPath;
            _activationsPath = activationsPath;
        }

        public IEnumerable<Project> GetProjects()
        {
            var doc = XDocument.Load(_projectsPath);
            return doc.Descendants().Where(x => x.Name.LocalName == "row").Select(x => new Project
            {
                ID_Project = (int)x.Attribute("ID_Project"),
                Project_Name = (string)x.Attribute("Project_Name"),
                Project_Settings = (string)x.Attribute("Project_Settings"),
                ID_Product = (int)x.Attribute("ID_Product"),
                Project_DateEdit = DateTime.Parse((string)x.Attribute("Project_DateEdit"))
            });
        }

        public IEnumerable<ActivationDevice> GetActivationDevices()
        {
            var doc = XDocument.Load(_activationDevicesPath);
            return doc.Descendants().Where(x => x.Name.LocalName == "row").Select(x => new ActivationDevice
            {
                ID_ActivationDevice = (int)x.Attribute("ID_ActivationDevice"),
                ID_Activation = (int)x.Attribute("ID_Activation"),
                ActivationDevice_HardwareId = (string)x.Attribute("ActivationDevice_HardwareId"),
                ActivationDevice_IsRevoked = (string)x.Attribute("ActivationDevice_IsRevoked") == "1",
                ActivationDevice_Date = DateTime.Parse((string)x.Attribute("ActivationDevice_Date"))
            });
        }

        public IEnumerable<Customer> GetCustomers()
        {
            var doc = XDocument.Load(_customersPath);
            return doc.Descendants().Where(x => x.Name.LocalName == "row").Select(x => new Customer
            {
                ID_Customer = (int)x.Attribute("ID_Customer"),
                Customer_Salutation = (string)x.Attribute("Customer_Salutation"),
                Customer_FirstName = (string)x.Attribute("Customer_FirstName"),
                Customer_LastName = (string)x.Attribute("Customer_LastName"),
                Customer_JobTitle = (string)x.Attribute("Customer_JobTitle"),
                Customer_Organization = (string)x.Attribute("Customer_Organization"),
                Customer_Email = (string)x.Attribute("Customer_Email"),
                Customer_Phone = (string)x.Attribute("Customer_Phone"),
                Customer_Fax = (string)x.Attribute("Customer_Fax"),
                Customer_Address1 = (string)x.Attribute("Customer_Address1"),
                Customer_Address2 = (string)x.Attribute("Customer_Address2"),
                Customer_City = (string)x.Attribute("Customer_City"),
                Customer_Zip = (string)x.Attribute("Customer_Zip"),
                Customer_State = (string)x.Attribute("Customer_State"),
                Customer_Country = (string)x.Attribute("Customer_Country"),
                Customer_Notes = (string)x.Attribute("Customer_Notes"),
                Customer_IsAcceptingMail = (string)x.Attribute("Customer_IsAcceptingMail") == "1",
                Customer_IsRegistered = (string)x.Attribute("Customer_IsRegistered") == "1",
                Customer_IsBlacklisted = (string)x.Attribute("Customer_IsBlacklisted") == "1",
                Customer_DateEdit = DateTime.Parse((string)x.Attribute("Customer_DateEdit"))
            });
        }
        public Customer GetCustomer(int customerId)
        {
            return GetCustomers().Where(c => customerId == c.ID_Customer).SingleOrDefault();
        }
             
        public IEnumerable<Product> GetProducts()
        {
            var doc = XDocument.Load(_productsPath);
            return doc.Descendants().Where(row => row.Name.LocalName == "row").Select(row => new Product
            {
                ID_Product = (int)row.Attribute("ID_Product"),
                Product_LicenseHash = (string)row.Attribute("Product_LicenseHash"),
                Product_TrialHash = (string)row.Attribute("Product_TrialHash"),
                Product_PasswordHash = (string)row.Attribute("Product_PasswordHash"),
                Product_InputFilename = (string)row.Attribute("Product_InputFilename"),
                Product_OutputFilename = (string)row.Attribute("Product_OutputFilename"),
                Product_Type = ParseInt(row.Attribute("Product_Type")?.Value),
                Product_Name = (string)row.Attribute("Product_Name"),
                Product_Version = (string)row.Attribute("Product_Version"),
                Product_Description = (string)row.Attribute("Product_Description"),
                Product_Icon = HexToBase64((string)row.Attribute("Product_Icon")), 
                Product_UnitPrice = ParseDouble(row.Attribute("Product_UnitPrice")?.Value),
                Product_ShippingFee = ParseDouble(row.Attribute("Product_ShippingFee")?.Value),
                Product_ServiceFee = ParseDouble(row.Attribute("Product_ServiceFee")?.Value),
                Product_CustomInfo = ParseProductCustomInfo((string)row.Attribute("Product_CustomInfo")),
                Product_InvoiceTemplatePath = (string)row.Attribute("Product_InvoiceTemplatePath"),
                Product_Notes = (string)row.Attribute("Product_Notes"),
                Product_KeyGenDLL = (string)row.Attribute("Product_KeyGenDLL"),
                Product_KeyGenFunction = (string)row.Attribute("Product_KeyGenFunction")
            });
        }
        public Product GetProduct(int productId)
        {
            return GetProducts().Where(p => p.ID_Product == productId).SingleOrDefault();
        }

        public IEnumerable<Order> GetOrders()
        {
            var doc = XDocument.Load(_ordersPath);
            return doc.Descendants().Where(row => row.Name.LocalName == "row").Select(row => new Order
            {
                ID_Order = (int)row.Attribute("ID_Order"),
                Order_Date = ParseDate(row.Attribute("Order_Date")?.Value),
                Order_ExpireDate = ParseDate(row.Attribute("Order_ExpireDate")?.Value),
                Order_MarkedAsExpired = ParseBool(row.Attribute("Order_MarkedAsExpired")?.Value),
                Order_DeliveryMethod = (string)row.Attribute("Order_DeliveryMethod"),
                Order_HowFound = (string)row.Attribute("Order_HowFound"),
                Order_RegistrationName = (string)row.Attribute("Order_RegistrationName"),
                Order_ShippingAddress = (string)row.Attribute("Order_ShippingAddress"),
                Order_ShippingCity = (string)row.Attribute("Order_ShippingCity"),
                Order_ShippingZip = (string)row.Attribute("Order_ShippingZip"),
                Order_ShippingState = (string)row.Attribute("Order_ShippingState"),
                Order_ShippingCountry = (string)row.Attribute("Order_ShippingCountry"),
                ID_Product = ParseInt(row.Attribute("ID_Product")?.Value),
                Order_UnitPrice = ParseDouble(row.Attribute("Order_UnitPrice")?.Value),
                Order_ShippingFee = ParseDouble(row.Attribute("Order_ShippingFee")?.Value),
                Order_ServiceFee = ParseDouble(row.Attribute("Order_ServiceFee")?.Value),
                Order_Quantity = ParseInt(row.Attribute("Order_Quantity")?.Value),
                Order_Total = ParseDouble(row.Attribute("Order_Total")?.Value),
                Order_PaymentMethod = (string)row.Attribute("Order_PaymentMethod"),
                Order_WasPaid = ParseBool(row.Attribute("Order_WasPaid")?.Value),
                ID_Customer = ParseInt(row.Attribute("ID_Customer")?.Value),
                Order_RegistrationContent = ParseOrderRegistrationContent((string)row.Attribute("Order_RegistrationContent")),
                Order_LastRegistrationType = ParseByte(row.Attribute("Order_LastRegistrationType")?.Value),
                Order_Notes = (string)row.Attribute("Order_Notes")
            });
        }
        public Order GetOrder(int orderId)
        {
            return GetOrders().Where(o => o.ID_Order == orderId).SingleOrDefault();
        }

        public IEnumerable<Activation> GetActivations()
        {
            var doc = XDocument.Load(_activationsPath);
            return doc.Descendants().Where(row => row.Name.LocalName == "row").Select(row => new Activation
            {
                ID_Activation = ParseInt(row.Attribute("ID_Activation")?.Value) ?? 0,
                Activation_String = (string)row.Attribute("Activation_String"),
                Activation_ID_Order = ParseInt(row.Attribute("Activation_ID_Order")?.Value) ?? 0,
                Activation_CreationDate = ParseDate(row.Attribute("Activation_CreationDate")?.Value) ?? DateTime.MinValue,
                Activation_IsDisabled = ParseBool(row.Attribute("Activation_IsDisabled")?.Value) ?? false,
                Activation_CurrentDeactivations = ParseInt(row.Attribute("Activation_CurrentDeactivations")?.Value) ?? 0,
                Activation_CurrentActivations = ParseInt(row.Attribute("Activation_CurrentActivations")?.Value) ?? 0
            });
        }

        private static DateTime? ParseDate(string value) =>
       DateTime.TryParse(value, out var date) ? date : (DateTime?)null;

        private static int? ParseInt(string value) =>
            int.TryParse(value, out var i) ? i : (int?)null;

        private static double? ParseDouble(string value) =>
            double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : (double?)null;

        private static bool? ParseBool(string value) =>
            value == "1" ? true : value == "0" ? false : (bool?)null;

        private static byte? ParseByte(string value) =>
            byte.TryParse(value, out var b) ? b : (byte?)null;

        private string HexToBase64(string hex)
        {
            // Remove optional "0x" prefix
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hex = hex.Substring(2);

            // Convert hex to byte[]
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            // Convert byte[] to base64
            return Convert.ToBase64String(bytes);
        }

        private ParsedLicense ParseOrderRegistrationContent(string input)
        {

            var parsed = new ParsedLicense();

            // Normalize whitespace
            input = Regex.Replace(input, @"\s+", " ");

            parsed.UserName = MatchField(input, "User Name");
            parsed.Company = MatchField(input, "Company");
            parsed.HardwareId = MatchField(input, "Hardware ID");

            var daysStr = MatchField(input, "Days Expiration");
            parsed.DaysExpiration = int.TryParse(daysStr, out var days) ? days : 0;

            // Extract the license data between <license_start> and <license_end>
            var match = Regex.Match(input, @"<license_start>\s*(.*?)\s*<license_end>", RegexOptions.Singleline);
            parsed.LicenseDataBase64 = match.Success ? match.Groups[1].Value.Trim() : null;

            return parsed;
        }

        private static string MatchField(string input, string fieldName)
        {
            var pattern = $@"\b{Regex.Escape(fieldName)}\s*=\s*(.*?)(?=\s+\S[^=]*\s*=\s*|<|$)";
            var match = Regex.Match(input, pattern, RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        private ProductCustomInfo ParseProductCustomInfo(string content)
        {
            var info = new ProductCustomInfo();
            var regex = new Regex(@"(?<key>[^=\s]+)=(?<value>.*?)(?=\s[^=\s]+=|$)", RegexOptions.Singleline);
            var matches = regex.Matches(content);

            var dict = matches.Cast<Match>()
                              .ToDictionary(m => m.Groups["key"].Value.Trim(),
                                            m => m.Groups["value"].Value.Trim());

            return new ProductCustomInfo
            {
                DefaultActivationSimultaneousDevices = int.Parse(dict.GetValueOrDefault("DefaultActivationSimultaneousDevices", "0")),
                DefaultRegistrationType = dict.GetValueOrDefault("DefaultRegistrationType", ""),
                DefaultActivationLimitActivation = int.Parse(dict.GetValueOrDefault("DefaultActivationLimitActivation", "0")),
                DefaultActivationLimitDeactivation = int.Parse(dict.GetValueOrDefault("DefaultActivationLimitDeactivation", "0")),
                DefaultActivationMaxDifferentDevices = int.Parse(dict.GetValueOrDefault("DefaultActivationMaxDifferentDevices", "0")),
                DefaultActivationFormat = dict.GetValueOrDefault("DefaultActivationFormat", ""),
                OpenFolderWhenGeneratingLicense = dict.GetValueOrDefault("OpenFolderWhenGeneratingLicense", "NO").ToUpper() == "YES",
                AutoGenerateInFolder = dict.GetValueOrDefault("AutoGenerateInFolder", "NO").ToUpper() == "YES",
                LicenseFileBinaryName = dict.GetValueOrDefault("LicenseFileBinaryName", ""),
                LicenseFileTextName = dict.GetValueOrDefault("LicenseFileTextName", ""),
                LicenseRegistryHive = dict.GetValueOrDefault("LicenseRegistryHive", ""),
                LicenseRegistryName = dict.GetValueOrDefault("LicenseRegistryName", ""),
                GenerateLicensesInFolder = dict.GetValueOrDefault("GenerateLicensesInFolder", ""),
                LicenseRegistryValueName = dict.GetValueOrDefault("LicenseRegistryValueName", "")
            };
        }
    }
}
