using System;

namespace OrderProcessing
{
    /// <summary>
    /// Servicio para procesar órdenes
    /// </summary>
    public class OrderService
    {
        private readonly IInventoryService _inventoryService;

        public OrderService(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// Procesa una orden de compra
        /// </summary>
        /// <param name="order">La orden a procesar</param>
        /// <returns>true si hay inventario suficiente, false de lo contrario</returns>
        /// <exception cref="ArgumentNullException">Se lanza si el objeto Order es nulo</exception>
        public bool PlaceOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order), "La orden no puede ser null");

            bool available = _inventoryService.IsProductAvailable(order.ProductId, order.Quantity);
            
            if (!available)
            {
                // No hay inventario
                return false;
            }

            // Sí, puede continuar con el pedido
            return true;
        }
    }
}
