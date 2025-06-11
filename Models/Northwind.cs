using Microsoft.EntityFrameworkCore;
using ASPProjectFinal.Models;

namespace ASPProjectFinal.Models
{
    public partial class Northwind : DbContext
    {
        public Northwind(DbContextOptions<Northwind> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<Order_Details> Order_Details { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Shipper> Shippers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Remove hardcoded connection string to use appsettings.json
            // Only use for fallback in development
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=Khadija--1\\SQLEXPRESS;Database=Northwind;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure composite primary key for Order_Details
            modelBuilder.Entity<Order_Details>()
                .HasKey(od => new { od.OrderId, od.ProductId });

            // Specify table name for Order Details
            modelBuilder.Entity<Order_Details>()
                .ToTable("Order Details");

            // Configure relationships for Order_Details
            modelBuilder.Entity<Order_Details>()
                .HasOne(od => od.Order)
                .WithMany(o => o.Order_Details)
                .HasForeignKey(od => od.OrderId);

            modelBuilder.Entity<Order_Details>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductId);

            // Configure relationship between Order and Shipper
            modelBuilder.Entity<Order>()
                .HasOne(o => o.ShipViaNavigation)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.ShipVia)
                .HasConstraintName("FK_Orders_Shippers");

            // Configure hierarchical relationship for Employee
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.ReportsToNavigation)
                .WithMany(e => e.InverseReportsToNavigation)
                .HasForeignKey(e => e.ReportsTo)
                .HasConstraintName("FK_Employees_Employees");

            // Configure decimal precision for Freight in Order
            modelBuilder.Entity<Order>()
                .Property(o => o.Freight)
                .HasPrecision(18, 2);

            // Configure decimal precision for UnitPrice in Product
            modelBuilder.Entity<Product>()
                .Property(p => p.UnitPrice)
                .HasPrecision(18, 2);

            // Configure decimal precision for UnitPrice in Order_Details
            modelBuilder.Entity<Order_Details>()
                .Property(od => od.UnitPrice)
                .HasPrecision(18, 2);

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}