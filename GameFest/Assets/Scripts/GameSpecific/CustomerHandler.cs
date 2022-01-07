using System.Collections.Generic;

/// <summary>
/// Class for holding customer orders
/// </summary>
class CustomerHandler
{
    // constants
    const int CUSTOMER_COUNT = 30;
    
    // fields
    List<CustomerOrder> _customers = new List<CustomerOrder>();

    /// <summary>
    /// Initialises a list of orders
    /// </summary>
    public void Initialise()
    {
        for (var i = 0; i < CUSTOMER_COUNT; i++)
        {
            var customerName = /*NameFetcher.GetName()*/"";
            var order = OrderFactory.GetOrder();
            _customers.Add(new CustomerOrder(customerName, order));
        }
    }

    /// <summary>
    /// Returns a list of orders from customers
    /// </summary>
    /// <returns>List of customers orders</returns>
    public List<CustomerOrder> GetOrders()
    {
        return _customers;
    }
}
