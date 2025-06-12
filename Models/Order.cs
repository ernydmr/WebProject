using System;
using System.Collections.Generic;

namespace WebProject.Models;

public class Order
{
    public int Id { get; set; }
    
    public string UserId { get; set; }
    
    public ApplicationUser User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<OrderItem> Items { get; set; } = new();

    public int AddressId { get; set; }

    public Address Address { get; set; }

    public decimal TotalPrice { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

}
public enum OrderStatus
{
    Pending,
    Approved,
    Preparing,
    Shipped,
    Delivered,
    Cancelled
}