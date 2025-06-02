using WinLicenseBackend.Models;

namespace WinLicenseBackend.DataProviders
{
    public interface IDataProvider
    {
        IEnumerable<Project> GetProjects();
        IEnumerable<ActivationDevice> GetActivationDevices();
        IEnumerable<Customer> GetCustomers();
        Customer GetCustomer(int customerId);
        IEnumerable<Product> GetProducts();
        IEnumerable<Product> GetProductsFromOrders(int[] orderId);
        Product GetProduct(int productId);
        IEnumerable<Order> GetOrders();
        Order GetOrder(int orderId);
        IEnumerable<Activation> GetActivations();
    }

}
