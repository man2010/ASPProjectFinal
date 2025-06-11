using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPProjectFinal.Models;

public partial class Order_Details
{
	[Key]
	[Column(Order = 0)]
	public int OrderId { get; set; }

	[Key]
	[Column(Order = 1)]
	public int ProductId { get; set; }

	[Required]
	[Column(TypeName = "money")]
	public decimal UnitPrice { get; set; }

	[Required]
	public short Quantity { get; set; }

	[Required]
	public float Discount { get; set; }

	public virtual Order? Order { get; set; }

	public virtual Product? Product { get; set; }
}