using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class for holding customer orders
/// </summary>
class CustomerHandler
{
    // constants
    const int CUSTOMER_COUNT = 20;
    const int CUSTOMER_TOPUP = 10;

    // fields
    List<CustomerOrder> _customers = new List<CustomerOrder>();
    int _customerIndex = 0;

    /// <summary>
    /// Constructor
    /// </summary>
    public CustomerHandler()
    {
        Initialise_(CUSTOMER_COUNT);
    }

    /// <summary>
    /// Initialises a list of orders
    /// </summary>
    void Initialise_(int count)
    {
        var names = new NameHandler();

        for (var i = 0; i < count; i++)
        {
            // generate a customer
            var customerName = names.GetName();
            var order = OrderFactory.GetOrder();
            _customers.Add(new CustomerOrder(customerName, order));
        }
    }

    /// <summary>
    /// Returns a list of orders from customers
    /// </summary>
    /// <returns>List of customers orders</returns>
    public List<CustomerOrder> GetAllOrders()
    {
        return _customers;
    }

    /// <summary>
    /// Returns a list of orders from customers, starting on the current one
    /// </summary>
    /// <param name="number">How many to take</param>
    /// <returns>List of customers orders</returns>
    public List<CustomerOrder> GetNextOrders(int number)
    {
        return _customers.GetRange(_customerIndex, number);
    }

    /// <summary>
    /// When a burger is served to the customer
    /// </summary>
    /// <param name="burgerSelections">Items included in the burger</param>
    internal void CustomerServed(List<object> burgerSelections)
    {
        // the customer has received their burger
        _customers[_customerIndex].BurgerReceived(burgerSelections);
        _customerIndex++;

        // if running out of customers, add more
        if (_customerIndex > _customers.Count - 5)
            Initialise_(CUSTOMER_TOPUP);
    }
}
