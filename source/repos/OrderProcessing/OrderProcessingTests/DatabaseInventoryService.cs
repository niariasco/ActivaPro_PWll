using OrderProcessing;

namespace OrderProcessingTests
{
    /// <summary>
    /// Implementación real de IInventoryService que usa base de datos
    /// </summary>
    public class DatabaseInventoryService : IInventoryService
    {
        private readonly OrderProcessingContext _dbContext;

        public DatabaseInventoryService(OrderProcessingContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Verifica si hay inventario disponible en la base de datos
        /// </summary>
        public bool IsProductAvailable(string productId, int quantity)
        {
            if (string.IsNullOrEmpty(productId) || quantity <= 0)
            {
                return false;
            }

            var product = _dbContext.Products.FirstOrDefault(p => p.ProductId == productId);
            
            if (product == null)
            {
                return false;
            }

            return product.AvailableQuantity >= quantity;
        }

        /// <summary>
        /// Retira una cantidad del inventario si está disponible
        /// Retorna true si la operación fue exitosa
        /// </summary>
        public bool ReserveInventory(string productId, int quantity)
        {
            if (!IsProductAvailable(productId, quantity))
            {
                return false;
            }

            var product = _dbContext.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                product.AvailableQuantity -= quantity;
                _dbContext.SaveChanges();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Obtiene la cantidad disponible de un producto
        /// </summary>
        public int GetAvailableQuantity(string productId)
        {
            var product = _dbContext.Products.FirstOrDefault(p => p.ProductId == productId);
            return product?.AvailableQuantity ?? 0;
        }
    }
}
