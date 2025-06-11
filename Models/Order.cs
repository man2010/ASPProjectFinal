using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPProjectFinal.Models;

public partial class Order
{
    public int OrderId { get; set; }

    [Required(ErrorMessage = "L'ID client est requis.")]
    public string? CustomerId { get; set; }

    public int? EmployeeId { get; set; }

    [Required(ErrorMessage = "La date de commande est requise.")]
    [DataType(DataType.Date)]
    public DateTime? OrderDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? RequiredDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ShippedDate { get; set; }

    [ForeignKey("ShipViaNavigation")]
    public int? ShipVia { get; set; }

    public decimal? Freight { get; set; }
    public string? ShipName { get; set; }
    public string? ShipAddress { get; set; }
    public string? ShipCity { get; set; }
    public string? ShipRegion { get; set; }
    public string? ShipPostalCode { get; set; }
    public string? ShipCountry { get; set; }

    public virtual Customer? Customer { get; set; }
    public virtual Employee? Employee { get; set; }
    public virtual Shipper? ShipViaNavigation { get; set; }
    public virtual ICollection<Order_Details> Order_Details { get; set; } = new List<Order_Details>();
}