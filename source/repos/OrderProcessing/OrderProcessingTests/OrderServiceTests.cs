using Xunit;
using Moq;
using OrderProcessing;

namespace OrderProcessingTests
{
    /// <summary>
    /// Pruebas unitarias para la clase OrderService
    /// </summary>
    public class OrderServiceTests
    {
        /// <summary>
        /// Prueba 1: Pedido procesado con éxito
        /// Verifica que cuando hay inventario disponible, PlaceOrder retorna true
        /// y que IsProductAvailable se invoca exactamente una vez
        /// </summary>
        [Fact]
        public void PlaceOrder_WithAvailableInventory_ReturnsTrue()
        {
            // Arrange
            // Configurar mock para IInventoryService que retorne true
            var mockInventoryService = new Mock<IInventoryService>();
            mockInventoryService
                .Setup(x => x.IsProductAvailable(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true);

            // Crear OrderService con el mock
            var orderService = new OrderService(mockInventoryService.Object);

            // Crear un objeto Order
            var order = new Order { ProductId = "P1", Quantity = 100 };

            // Act
            bool result = orderService.PlaceOrder(order);

            // Assert
            // Verificar que PlaceOrder retorna true
            Assert.True(result);

            // Verificar que IsProductAvailable se invocó exactamente una vez
            mockInventoryService.Verify(
                x => x.IsProductAvailable(It.IsAny<string>(), It.IsAny<int>()),
                Times.Once);
        }

        /// <summary>
        /// Prueba 2: Pedido rechazado por falta de inventario
        /// Verifica que cuando no hay inventario disponible, PlaceOrder retorna false
        /// y que IsProductAvailable se invoca exactamente una vez
        /// </summary>
        [Fact]
        public void PlaceOrder_WithoutAvailableInventory_ReturnsFalse()
        {
            // Arrange
            // Configurar mock para IInventoryService que retorne false
            var mockInventoryService = new Mock<IInventoryService>();
            mockInventoryService
                .Setup(x => x.IsProductAvailable(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(false);

            // Crear OrderService con el mock
            var orderService = new OrderService(mockInventoryService.Object);

            // Crear un objeto Order
            var order = new Order { ProductId = "P1", Quantity = 100 };

            // Act
            bool result = orderService.PlaceOrder(order);

            // Assert
            // Verificar que PlaceOrder retorna false
            Assert.False(result);

            // Verificar que IsProductAvailable se invocó exactamente una vez
            mockInventoryService.Verify(
                x => x.IsProductAvailable(It.IsAny<string>(), It.IsAny<int>()),
                Times.Once);
        }

        /// <summary>
        /// Prueba 3: Validación de parámetro nulo
        /// Verifica que llamar a PlaceOrder con un objeto Order nulo
        /// lanza una excepción de tipo ArgumentNullException
        /// </summary>
        [Fact]
        public void PlaceOrder_WithNullOrder_ThrowsArgumentNullException()
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();
            var orderService = new OrderService(mockInventoryService.Object);

            // Act & Assert
            // Verificar que se lanza ArgumentNullException cuando order es null
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var exception = Assert.Throws<ArgumentNullException>(() => orderService.PlaceOrder(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            
            // Verificar que el nombre del parámetro es correcto
            Assert.Equal("order", exception.ParamName);
        }

        /// <summary>
        /// Prueba 4: Verificar que el parámetro correcto se pasa al servicio de inventario
        /// </summary>
        [Fact]
        public void PlaceOrder_PassesCorrectProductIdAndQuantity()
        {
            // Arrange
            const string expectedProductId = "PROD-123";
            const int expectedQuantity = 50;
            
            var mockInventoryService = new Mock<IInventoryService>();
            mockInventoryService
                .Setup(x => x.IsProductAvailable(expectedProductId, expectedQuantity))
                .Returns(true);

            var orderService = new OrderService(mockInventoryService.Object);
            var order = new Order { ProductId = expectedProductId, Quantity = expectedQuantity };

            // Act
            orderService.PlaceOrder(order);

            // Assert
            mockInventoryService.Verify(
                x => x.IsProductAvailable(expectedProductId, expectedQuantity),
                Times.Once);
        }

        /// <summary>
        /// Prueba 5: Prueba con diferentes cantidades
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void PlaceOrder_WithDifferentQuantities_ReturnsTrue(int quantity)
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();
            mockInventoryService
                .Setup(x => x.IsProductAvailable(It.IsAny<string>(), quantity))
                .Returns(true);

            var orderService = new OrderService(mockInventoryService.Object);
            var order = new Order { ProductId = "P1", Quantity = quantity };

            // Act
            bool result = orderService.PlaceOrder(order);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Prueba 6: Prueba con diferentes IDs de producto
        /// </summary>
        [Theory]
        [InlineData("P1")]
        [InlineData("PRODUCT-001")]
        [InlineData("SKU-2024-001")]
        [InlineData("A")]
        public void PlaceOrder_WithDifferentProductIds_ReturnsTrue(string productId)
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();
            mockInventoryService
                .Setup(x => x.IsProductAvailable(productId, It.IsAny<int>()))
                .Returns(true);

            var orderService = new OrderService(mockInventoryService.Object);
            var order = new Order { ProductId = productId, Quantity = 100 };

            // Act
            bool result = orderService.PlaceOrder(order);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Prueba 7: Múltiples llamadas a PlaceOrder con el mismo servicio
        /// </summary>
        [Fact]
        public void PlaceOrder_MultipleCallsWithSameService_IndependentResults()
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();
            mockInventoryService
                .SetupSequence(x => x.IsProductAvailable(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true)
                .Returns(false)
                .Returns(true);

            var orderService = new OrderService(mockInventoryService.Object);
            var order1 = new Order { ProductId = "P1", Quantity = 100 };
            var order2 = new Order { ProductId = "P2", Quantity = 200 };
            var order3 = new Order { ProductId = "P3", Quantity = 300 };

            // Act
            bool result1 = orderService.PlaceOrder(order1);
            bool result2 = orderService.PlaceOrder(order2);
            bool result3 = orderService.PlaceOrder(order3);

            // Assert
            Assert.True(result1);
            Assert.False(result2);
            Assert.True(result3);
            
            mockInventoryService.Verify(
                x => x.IsProductAvailable(It.IsAny<string>(), It.IsAny<int>()),
                Times.Exactly(3));
        }

        /// <summary>
        /// Prueba 8: Verificar que el servicio de inventario se llama exactamente una vez
        /// </summary>
        [Fact]
        public void PlaceOrder_CallsInventoryServiceExactlyOnce()
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();
            mockInventoryService
                .Setup(x => x.IsProductAvailable(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true);

            var orderService = new OrderService(mockInventoryService.Object);
            var order = new Order { ProductId = "P1", Quantity = 100 };

            // Act
            orderService.PlaceOrder(order);

            // Assert
            mockInventoryService.Verify(
                x => x.IsProductAvailable(It.IsAny<string>(), It.IsAny<int>()),
                Times.Once,
                "El servicio de inventario debe ser llamado exactamente una vez");
        }

        /// <summary>
        /// Prueba 9: Verificar que no se llama al servicio después de recibir una orden nula
        /// </summary>
        [Fact]
        public void PlaceOrder_WithNullOrder_DoesNotCallInventoryService()
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();
            var orderService = new OrderService(mockInventoryService.Object);

            // Act & Assert
#pragma warning disable CS8625
            Assert.Throws<ArgumentNullException>(() => orderService.PlaceOrder(null));
#pragma warning restore CS8625

            // Verificar que el servicio NO fue llamado
            mockInventoryService.Verify(
                x => x.IsProductAvailable(It.IsAny<string>(), It.IsAny<int>()),
                Times.Never,
                "El servicio de inventario no debe ser llamado cuando la orden es nula");
        }

        /// <summary>
        /// Prueba 10: Verificar propiedades de Order
        /// </summary>
        [Fact]
        public void Order_Properties_CanBeSet()
        {
            // Arrange
            var order = new Order();

            // Act
            order.ProductId = "TEST-001";
            order.Quantity = 50;

            // Assert
            Assert.Equal("TEST-001", order.ProductId);
            Assert.Equal(50, order.Quantity);
        }

        /// <summary>
        /// Prueba 11: Constructor de OrderService con dependencia nula
        /// </summary>
        [Fact]
        public void OrderService_Constructor_StoresInventoryService()
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();

            // Act
            var orderService = new OrderService(mockInventoryService.Object);

            // Assert
            Assert.NotNull(orderService);
            mockInventoryService.Verify(
                x => x.IsProductAvailable(It.IsAny<string>(), It.IsAny<int>()),
                Times.Never);
        }

        /// <summary>
        /// Prueba 12: Prueba de edgecase con cantidad cero
        /// </summary>
        [Fact]
        public void PlaceOrder_WithZeroQuantity_CallsInventoryService()
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();
            mockInventoryService
                .Setup(x => x.IsProductAvailable("P1", 0))
                .Returns(false);

            var orderService = new OrderService(mockInventoryService.Object);
            var order = new Order { ProductId = "P1", Quantity = 0 };

            // Act
            bool result = orderService.PlaceOrder(order);

            // Assert
            Assert.False(result);
            mockInventoryService.Verify(
                x => x.IsProductAvailable("P1", 0),
                Times.Once);
        }

        /// <summary>
        /// Prueba 13: Prueba con valores negativos (edge case)
        /// </summary>
        [Fact]
        public void PlaceOrder_WithNegativeQuantity_CallsInventoryService()
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();
            mockInventoryService
                .Setup(x => x.IsProductAvailable("P1", -10))
                .Returns(false);

            var orderService = new OrderService(mockInventoryService.Object);
            var order = new Order { ProductId = "P1", Quantity = -10 };

            // Act
            bool result = orderService.PlaceOrder(order);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Prueba 14: Prueba con ProductId vacío
        /// </summary>
        [Fact]
        public void PlaceOrder_WithEmptyProductId_CallsInventoryService()
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();
            mockInventoryService
                .Setup(x => x.IsProductAvailable("", It.IsAny<int>()))
                .Returns(false);

            var orderService = new OrderService(mockInventoryService.Object);
            var order = new Order { ProductId = "", Quantity = 100 };

            // Act
            bool result = orderService.PlaceOrder(order);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Prueba 15: Integración de múltiples órdenes
        /// </summary>
        [Fact]
        public void PlaceOrder_ProcessMultipleOrders_CorrectSequence()
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();
            var callSequence = new Queue<bool>(new[] { true, false, true });
            
            mockInventoryService
                .Setup(x => x.IsProductAvailable(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(() => callSequence.Dequeue());

            var orderService = new OrderService(mockInventoryService.Object);

            // Act
            var order1 = new Order { ProductId = "P1", Quantity = 100 };
            var order2 = new Order { ProductId = "P2", Quantity = 200 };
            var order3 = new Order { ProductId = "P3", Quantity = 300 };

            bool result1 = orderService.PlaceOrder(order1);
            bool result2 = orderService.PlaceOrder(order2);
            bool result3 = orderService.PlaceOrder(order3);

            // Assert
            Assert.True(result1);
            Assert.False(result2);
            Assert.True(result3);
        }
    }
}
