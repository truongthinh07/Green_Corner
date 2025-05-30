﻿using System;
using System.Collections.Generic;

namespace GreenCorner.EcommerceAPI.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public string UserId { get; set; } = null!;

    public string Status { get; set; } = null!;

    public decimal TotalMoney { get; set; }

    public string? PaymentMethod { get; set; }

    public string Name { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? ShippingAddress { get; set; }

    public string? Email { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
