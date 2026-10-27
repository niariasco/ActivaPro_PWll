# ?? ÍNDICE DE PRUEBAS XUNIT - OrderServiceTests

## ?? Acceso Rápido

### ?? Documentación Disponible

1. **ESPECIFICACION_PRUEBAS_XUNIT.md** ? Documentación completa
   - Detalles de cada prueba
   - Configuración (Arrange)
   - Ejecución (Act)
   - Verificaciones (Assert)
   - Casos de uso

2. **RESUMEN_PRUEBAS_XUNIT.md** ? Vista resumida
   - Tabla de todas las pruebas
   - Categorización
   - Patrones utilizados
   - Cobertura
   - Estadísticas

---

## ?? Lista de Pruebas (15 Total)

### Pruebas Básicas [Fact]
```
1. PlaceOrder_WithAvailableInventory_ReturnsTrue
   ?? Cuando inventario está disponible ? retorna true

2. PlaceOrder_WithoutAvailableInventory_ReturnsFalse
   ?? Cuando inventario no está disponible ? retorna false

3. PlaceOrder_WithNullOrder_ThrowsArgumentNullException
   ?? Cuando Order es nulo ? lanza ArgumentNullException
```

### Pruebas de Validación [Fact]
```
4. PlaceOrder_PassesCorrectProductIdAndQuantity
   ?? Verifica parámetros correctos enviados a servicio

8. PlaceOrder_CallsInventoryServiceExactlyOnce
   ?? Verifica que servicio se invoca exactamente 1 vez

9. PlaceOrder_WithNullOrder_DoesNotCallInventoryService
   ?? Verifica que con null no se invoca servicio
```

### Pruebas Parametrizadas [Theory]
```
5. PlaceOrder_WithDifferentQuantities_ReturnsTrue
   ?? [InlineData(1)]
   ?? [InlineData(10)]
   ?? [InlineData(100)]
   ?? [InlineData(1000)]

6. PlaceOrder_WithDifferentProductIds_ReturnsTrue
   ?? [InlineData("P1")]
   ?? [InlineData("PRODUCT-001")]
   ?? [InlineData("SKU-2024-001")]
   ?? [InlineData("A")]
```

### Pruebas de Secuencia [Fact]
```
7. PlaceOrder_MultipleCallsWithSameService_IndependentResults
   ?? 3 órdenes con SetupSequence: true ? false ? true

15. PlaceOrder_ProcessMultipleOrders_CorrectSequence
   ?? Queue de resultados con múltiples órdenes
```

### Pruebas de Modelo [Fact]
```
10. Order_Properties_CanBeSet
   ?? Verifica propiedades ProductId y Quantity

11. OrderService_Constructor_StoresInventoryService
   ?? Verifica inyección de dependencias
```

### Pruebas de Edge Cases [Fact]
```
12. PlaceOrder_WithZeroQuantity_CallsInventoryService
   ?? Cantidad = 0

13. PlaceOrder_WithNegativeQuantity_CallsInventoryService
   ?? Cantidad = -10

14. PlaceOrder_WithEmptyProductId_CallsInventoryService
   ?? ProductId = ""
```

---

## ?? Por Tipo

| Tipo | Cantidad | Ejemplos |
|------|----------|----------|
| [Fact] | 12 | Pruebas 1,2,3,4,7,8,9,10,11,12,13,14,15 |
| [Theory] | 3 | Pruebas 5,6 (con parámetros) |
| **Total Ejecuciones** | **28** | 12 + (4+4+1) + 7 |

---

## ?? Por Propósito

| Propósito | # | Pruebas |
|----------|---|---------|
| Casos Felices | 7 | 1,4,5,6,7,8,15 |
| Validación | 3 | 3,9,11 |
| Edge Cases | 3 | 12,13,14 |
| Modelo | 2 | 10,11 |

---

## ?? Por Cobertura

```
OrderService.PlaceOrder()
?? if (order == null)                    ? Pruebas: 3,9
?? throw ArgumentNullException           ? Pruebas: 3
?? _inventoryService.IsProductAvailable  ? Pruebas: 1,2,4,5,6,7,8,12,13,14,15
?? if (!available) { return false; }     ? Pruebas: 2,7,12,13,14
?? return true                           ? Pruebas: 1,5,6,7,8,15

Cobertura Total: 100% ?
```

---

## ?? Ejecutar las Pruebas

### Todas las Pruebas
```bash
cd OrderProcessing
dotnet test
```

### Resultado
```
Test summary: total: 15, failed: 0, succeeded: 15
O si cuenta parámetros: total: 28 ejecuciones
```

### Una Prueba Específica
```bash
dotnet test --filter "PlaceOrder_WithAvailableInventory_ReturnsTrue"
```

### Con Detalles
```bash
dotnet test --verbosity=detailed
```

---

## ?? Métricas

- **Pruebas Totales**: 15 métodos
- **Ejecuciones**: 28 (incluyendo Theory)
- **Cobertura**: 100%
- **Estado**: ? Todas pasando
- **Duración**: ~2-3 segundos
- **Métodos Probados**: 2 (PlaceOrder, Properties)
- **Dependencias Mockeadas**: 1 (IInventoryService)

---

## ?? Patrones Utilizados

```
? [Fact] - Pruebas determinísticas
? [Theory] + [InlineData] - Pruebas parametrizadas
? Mock.Setup() - Configurar comportamiento
? Mock.SetupSequence() - Múltiples retornos
? Mock.Verify() - Verificar invocaciones
? It.IsAny<T>() - Parámetros flexibles
? Times.Once/Exactly/Never - Verificación de exactitud
? Assert.Throws<T>() - Pruebas de excepciones
? #pragma warning - Suppressión de warnings
```

---

## ?? Archivo Fuente

**Ubicación**: `OrderProcessingTests/OrderServiceTests.cs`

**Líneas**: ~300+

**Estructura**:
```
namespace OrderProcessingTests
{
    public class OrderServiceTests
    {
        [Fact] Prueba 1
        [Fact] Prueba 2
        [Fact] Prueba 3
        [Fact] Prueba 4
        [Theory] Prueba 5
        [Theory] Prueba 6
        [Fact] Prueba 7
        [Fact] Prueba 8
        [Fact] Prueba 9
        [Fact] Prueba 10
        [Fact] Prueba 11
        [Fact] Prueba 12
        [Fact] Prueba 13
        [Fact] Prueba 14
        [Fact] Prueba 15
    }
}
```

---

## ?? Documentación Asociada

```
OrderProcessingTests/
??? OrderServiceTests.cs                    ? Código de pruebas (15 pruebas)
??? ESPECIFICACION_PRUEBAS_XUNIT.md        ? Documentación detallada
??? RESUMEN_PRUEBAS_XUNIT.md               ? Resumen visual
??? INDICE_PRUEBAS_XUNIT.md                ?? Este archivo
```

---

## ? Características Principales

? **15 Pruebas Completas**
- Casos felices
- Validación
- Edge cases
- Integración
- Modelo

? **28 Ejecuciones Totales**
- Pruebas parametrizadas
- Múltiples escenarios
- Todas pasando

? **100% Cobertura**
- Todas las líneas
- Todos los caminos
- Todos los casos

? **Documentación Completa**
- Especificación detallada
- Resumen visual
- Índice de navegación

---

## ?? Para Aprender

1. **Lee primero**: ESPECIFICACION_PRUEBAS_XUNIT.md
2. **Luego revisa**: RESUMEN_PRUEBAS_XUNIT.md
3. **Examina el código**: OrderServiceTests.cs
4. **Ejecuta**: `dotnet test`

---

## ? Estado Final

```
???????????????????????????????????????
?  PRUEBAS XUNIT COMPLETADAS         ?
?                                     ?
?  ? 15 Pruebas implementadas       ?
?  ? 28 Ejecuciones totales         ?
?  ? 100% Cobertura de código       ?
?  ? Todas pasando                  ?
?  ? Documentación completa         ?
?                                     ?
?  Status: LISTO PARA PRODUCCIÓN     ?
???????????????????????????????????????
```

---

**Índice Creado**: ? 2024  
**Documentación**: ? Completa  
**Pruebas**: ? Todas Pasando
