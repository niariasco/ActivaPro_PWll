namespace OrderProcessing
{
    /// <summary>
    /// Interface para el servicio de inventario
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Retorna true si hay suficiente inventario del producto y la cantidad solicitada para procesar un pedido
        /// </summary>
        /// <param name="productId">Identificador del producto</param>
        /// <param name="quantity">Cantidad solicitada</param>
        /// <returns>true si hay inventario suficiente, false de lo contrario</returns>
        bool IsProductAvailable(string productId, int quantity);
    }
}
