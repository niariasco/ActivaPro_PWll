using Microsoft.EntityFrameworkCore;
using OrderProcessing;

namespace OrderProcessingTests
{
    /// <summary>
    /// Contexto de base de datos para las pruebas de integración
    /// </summary>
    public class OrderProcessingContext : DbContext
    {
        public OrderProcessingContext(DbContextOptions<OrderProcessingContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// DbSet para productos disponibles
        /// </summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>
        /// DbSet para órdenes procesadas
        /// </summary>
        public DbSet<ProcessedOrder> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar modelo Product
            modelBuilder.Entity<Product>()
                .HasKey(p => p.ProductId);

            modelBuilder.Entity<Product>()
                .Property(p => p.ProductId)
                .HasMaxLength(50);

            modelBuilder.Entity<Product>()
                .Property(p => p.ProductName)
                .HasMaxLength(100)
                .IsRequired();

            // Configurar modelo ProcessedOrder
            modelBuilder.Entity<ProcessedOrder>()
                .HasKey(o => o.OrderId);

            modelBuilder.Entity<ProcessedOrder>()
                .Property(o => o.ProductId)
                .HasMaxLength(50);

            modelBuilder.Entity<ProcessedOrder>()
                .Property(o => o.OrderDate)
                .HasDefaultValue(DateTime.UtcNow);
        }
    }

    /// <summary>
    /// Entidad para un producto con su cantidad disponible
    /// </summary>
    public class Product
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int AvailableQuantity { get; set; }
    }

    /// <summary>
    /// Entidad para una orden procesada
    /// </summary>
    public class ProcessedOrder
    {
        public int OrderId { get; set; }
        public string ProductId { get; set; }
        public int QuantityOrdered { get; set; }
        public bool IsSuccessful { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
