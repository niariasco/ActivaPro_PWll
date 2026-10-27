using Xunit;
using Microsoft.EntityFrameworkCore;
using OrderProcessing;

namespace OrderProcessingTests
{
    /// <summary>
    /// Pruebas de Integración para OrderService con base de datos SQLite en memoria
    /// </summary>
    public class OrderServiceIntegrationTests : IDisposable
    {
        private readonly OrderProcessingContext _dbContext;
        private readonly DatabaseInventoryService _inventoryService;
        private readonly OrderService _orderService;

        public OrderServiceIntegrationTests()
        {
            // Crear opciones para SQLite en memoria
            var options = new DbContextOptionsBuilder<OrderProcessingContext>()
                .UseSqlite("Data Source=:memory:")
                .Options;

            // Crear contexto
            _dbContext = new OrderProcessingContext(options);
            
            // Crear base de datos en memoria
            _dbContext.Database.OpenConnection();
            _dbContext.Database.EnsureCreated();

            // Crear servicio de inventario y OrderService
            _inventoryService = new DatabaseInventoryService(_dbContext);
            _orderService = new OrderService(_inventoryService);
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }

        /// <summary>
        /// Prueba 1: Agregar producto y procesar orden exitosa
        /// </summary>
        [Fact]
        public void PlaceOrder_WithProductInDatabase_ReturnsTrue()
        {
            // Arrange
            var product = new Product
            {
                ProductId = "LAPTOP-001",
                ProductName = "Laptop Dell",
                AvailableQuantity = 50
            };
            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            var order = new Order { ProductId = "LAPTOP-001", Quantity = 10 };

            // Act
            bool result = _orderService.PlaceOrder(order);

            // Assert
            Assert.True(result);
            Assert.Equal(40, _inventoryService.GetAvailableQuantity("LAPTOP-001"));
        }

        /// <summary>
        /// Prueba 2: Procesar orden sin inventario suficiente
        /// </summary>
        [Fact]
        public void PlaceOrder_WithInsufficientInventory_ReturnsFalse()
        {
            // Arrange
            var product = new Product
            {
                ProductId = "MOUSE-001",
                ProductName = "Mouse Logitech",
                AvailableQuantity = 5
            };
            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            var order = new Order { ProductId = "MOUSE-001", Quantity = 10 };

            // Act
            bool result = _orderService.PlaceOrder(order);

            // Assert
            Assert.False(result);
            Assert.Equal(5, _inventoryService.GetAvailableQuantity("MOUSE-001"));
        }

        /// <summary>
        /// Prueba 3: Procesar orden de producto inexistente
        /// </summary>
        [Fact]
        public void PlaceOrder_WithProductNotInDatabase_ReturnsFalse()
        {
            // Arrange
            var order = new Order { ProductId = "NONEXISTENT-001", Quantity = 5 };

            // Act
            bool result = _orderService.PlaceOrder(order);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Prueba 4: Múltiples órdenes del mismo producto
        /// </summary>
        [Fact]
        public void PlaceOrder_MultipleOrdersSameProduct_ReducesInventory()
        {
            // Arrange
            var product = new Product
            {
                ProductId = "KEYBOARD-001",
                ProductName = "Keyboard Mecánico",
                AvailableQuantity = 100
            };
            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            var order1 = new Order { ProductId = "KEYBOARD-001", Quantity = 25 };
            var order2 = new Order { ProductId = "KEYBOARD-001", Quantity = 30 };
            var order3 = new Order { ProductId = "KEYBOARD-001", Quantity = 20 };

            // Act
            bool result1 = _orderService.PlaceOrder(order1);
            bool result2 = _orderService.PlaceOrder(order2);
            bool result3 = _orderService.PlaceOrder(order3);

            // Assert
            Assert.True(result1);
            Assert.True(result2);
            Assert.True(result3);
            Assert.Equal(25, _inventoryService.GetAvailableQuantity("KEYBOARD-001"));
        }

        /// <summary>
        /// Prueba 5: Orden que agota el inventario exactamente
        /// </summary>
        [Fact]
        public void PlaceOrder_ExactInventoryAmount_ReturnsTrue()
        {
            // Arrange
            var product = new Product
            {
                ProductId = "MONITOR-001",
                ProductName = "Monitor LG",
                AvailableQuantity = 15
            };
            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            var order = new Order { ProductId = "MONITOR-001", Quantity = 15 };

            // Act
            bool result = _orderService.PlaceOrder(order);

            // Assert
            Assert.True(result);
            Assert.Equal(0, _inventoryService.GetAvailableQuantity("MONITOR-001"));
        }

        /// <summary>
        /// Prueba 6: Múltiples productos diferentes
        /// </summary>
        [Fact]
        public void PlaceOrder_MultipleProducts_HandlesIndependently()
        {
            // Arrange
            var products = new[]
            {
                new Product { ProductId = "PROD-A", ProductName = "Producto A", AvailableQuantity = 100 },
                new Product { ProductId = "PROD-B", ProductName = "Producto B", AvailableQuantity = 50 },
                new Product { ProductId = "PROD-C", ProductName = "Producto C", AvailableQuantity = 30 }
            };
            _dbContext.Products.AddRange(products);
            _dbContext.SaveChanges();

            var orderA = new Order { ProductId = "PROD-A", Quantity = 40 };
            var orderB = new Order { ProductId = "PROD-B", Quantity = 25 };
            var orderC = new Order { ProductId = "PROD-C", Quantity = 10 };

            // Act
            bool resultA = _orderService.PlaceOrder(orderA);
            bool resultB = _orderService.PlaceOrder(orderB);
            bool resultC = _orderService.PlaceOrder(orderC);

            // Assert
            Assert.True(resultA);
            Assert.True(resultB);
            Assert.True(resultC);
            Assert.Equal(60, _inventoryService.GetAvailableQuantity("PROD-A"));
            Assert.Equal(25, _inventoryService.GetAvailableQuantity("PROD-B"));
            Assert.Equal(20, _inventoryService.GetAvailableQuantity("PROD-C"));
        }

        /// <summary>
        /// Prueba 7: Verificar persistencia de datos en BD
        /// </summary>
        [Fact]
        public void Database_PersistsProductData_AfterMultipleOperations()
        {
            // Arrange
            var product = new Product
            {
                ProductId = "TEST-PERSIST",
                ProductName = "Test Persistence",
                AvailableQuantity = 200
            };
            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            // Act
            var retrievedProduct = _dbContext.Products.FirstOrDefault(p => p.ProductId == "TEST-PERSIST");

            // Assert
            Assert.NotNull(retrievedProduct);
            Assert.Equal("Test Persistence", retrievedProduct.ProductName);
            Assert.Equal(200, retrievedProduct.AvailableQuantity);
        }

        /// <summary>
        /// Prueba 8: Intentar procesar orden con cantidad cero
        /// </summary>
        [Fact]
        public void PlaceOrder_WithZeroQuantity_ReturnsFalse()
        {
            // Arrange
            var product = new Product
            {
                ProductId = "PROD-ZERO",
                ProductName = "Producto Zero",
                AvailableQuantity = 100
            };
            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            var order = new Order { ProductId = "PROD-ZERO", Quantity = 0 };

            // Act
            bool result = _orderService.PlaceOrder(order);

            // Assert
            Assert.False(result);
            Assert.Equal(100, _inventoryService.GetAvailableQuantity("PROD-ZERO"));
        }

        /// <summary>
        /// Prueba 9: Orden después de agotar inventario
        /// </summary>
        [Fact]
        public void PlaceOrder_AfterInventoryExhausted_ReturnsFalse()
        {
            // Arrange
            var product = new Product
            {
                ProductId = "LIMITED-STOCK",
                ProductName = "Stock Limitado",
                AvailableQuantity = 10
            };
            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            var order1 = new Order { ProductId = "LIMITED-STOCK", Quantity = 10 };
            var order2 = new Order { ProductId = "LIMITED-STOCK", Quantity = 5 };

            // Act
            bool result1 = _orderService.PlaceOrder(order1);
            bool result2 = _orderService.PlaceOrder(order2);

            // Assert
            Assert.True(result1);
            Assert.False(result2);
            Assert.Equal(0, _inventoryService.GetAvailableQuantity("LIMITED-STOCK"));
        }

        /// <summary>
        /// Prueba 10: Escenario completo de flujo de pedidos
        /// </summary>
        [Fact]
        public void PlaceOrder_CompleteOrderFlow_ProcessesCorrectly()
        {
            // Arrange - Crear múltiples productos
            var products = new[]
            {
                new Product { ProductId = "ITEM-1", ProductName = "Item 1", AvailableQuantity = 500 },
                new Product { ProductId = "ITEM-2", ProductName = "Item 2", AvailableQuantity = 300 },
                new Product { ProductId = "ITEM-3", ProductName = "Item 3", AvailableQuantity = 200 }
            };
            _dbContext.Products.AddRange(products);
            _dbContext.SaveChanges();

            // Act - Procesar múltiples órdenes
            var orders = new[]
            {
                new Order { ProductId = "ITEM-1", Quantity = 100 },
                new Order { ProductId = "ITEM-2", Quantity = 75 },
                new Order { ProductId = "ITEM-3", Quantity = 200 },
                new Order { ProductId = "ITEM-1", Quantity = 250 },
                new Order { ProductId = "ITEM-2", Quantity = 225 }
            };

            var results = orders.Select(o => _orderService.PlaceOrder(o)).ToList();

            // Assert
            Assert.Equal(new[] { true, true, true, true, false }, results);
            Assert.Equal(150, _inventoryService.GetAvailableQuantity("ITEM-1"));
            Assert.Equal(0, _inventoryService.GetAvailableQuantity("ITEM-2"));
            Assert.Equal(0, _inventoryService.GetAvailableQuantity("ITEM-3"));
        }
    }
}
