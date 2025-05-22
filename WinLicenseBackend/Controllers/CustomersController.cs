using Microsoft.AspNetCore.Mvc;
using WinLicenseBackend;
using WinLicenseBackend.DataProviders;
using WinLicenseBackend.Models;

namespace CustomerApiProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly IDataProvider _dataProvider;

        public CustomersController(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        [HttpGet("getCustomers")]
        public ActionResult<IEnumerable<Customer>> GetAllCustomers()
        {
            var customers = _dataProvider.GetCustomers();
            return Ok(customers);
        }

        [HttpGet("getCustomerOrders")]
        public ActionResult<IEnumerable<OrderDTO>> GetCustomerOrders([FromQuery] int customerId)
        {
            IEnumerable<OrderDTO> orders = _dataProvider.GetOrders()
                .Where(o => o.ID_Customer == customerId)
                .Select(c => new OrderDTO
                {
                    ID_Order = c.ID_Order,
                    Order_RegistrationContent = c.Order_RegistrationContent,
                    Product = _dataProvider.GetProduct(c.ID_Product.Value)
                });
            return Ok(orders);
        }
    }
}