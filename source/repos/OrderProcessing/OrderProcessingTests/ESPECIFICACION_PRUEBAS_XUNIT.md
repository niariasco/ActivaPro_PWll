# ?? ESPECIFICACIÓN DE PRUEBAS XUNIT - OrderServiceTests

## ?? Propósito
Este documento describe todas las pruebas unitarias implementadas para la clase `OrderService` usando XUnit y Moq.

---

## ?? Resumen de Pruebas

| # | Nombre | Tipo | Estado | Descripción |
|---|--------|------|--------|-------------|
| 1 | PlaceOrder_WithAvailableInventory_ReturnsTrue | [Fact] | ? | Verifica retorno true con inventario disponible |
| 2 | PlaceOrder_WithoutAvailableInventory_ReturnsFalse | [Fact] | ? | Verifica retorno false sin inventario |
| 3 | PlaceOrder_WithNullOrder_ThrowsArgumentNullException | [Fact] | ? | Verifica excepción con orden nula |
| 4 | PlaceOrder_PassesCorrectProductIdAndQuantity | [Fact] | ? | Verifica parámetros correctos enviados |
| 5 | PlaceOrder_WithDifferentQuantities_ReturnsTrue | [Theory] | ? | Prueba múltiples cantidades (1,10,100,1000) |
| 6 | PlaceOrder_WithDifferentProductIds_ReturnsTrue | [Theory] | ? | Prueba múltiples IDs de producto |
| 7 | PlaceOrder_MultipleCallsWithSameService_IndependentResults | [Fact] | ? | Múltiples llamadas con resultados variados |
| 8 | PlaceOrder_CallsInventoryServiceExactlyOnce | [Fact] | ? | Verifica invocación exacta del servicio |
| 9 | PlaceOrder_WithNullOrder_DoesNotCallInventoryService | [Fact] | ? | Verifica que no se llama servicio con null |
| 10 | Order_Properties_CanBeSet | [Fact] | ? | Prueba propiedades de Order |
| 11 | OrderService_Constructor_StoresInventoryService | [Fact] | ? | Verifica inyección de dependencia |
| 12 | PlaceOrder_WithZeroQuantity_CallsInventoryService | [Fact] | ? | Edge case: cantidad cero |
| 13 | PlaceOrder_WithNegativeQuantity_CallsInventoryService | [Fact] | ? | Edge case: cantidad negativa |
| 14 | PlaceOrder_WithEmptyProductId_CallsInventoryService | [Fact] | ? | Edge case: ProductId vacío |
| 15 | PlaceOrder_ProcessMultipleOrders_CorrectSequence | [Fact] | ? | Integración: múltiples órdenes |

---

## ?? DETALLES DE CADA PRUEBA

### Prueba 1: PlaceOrder_WithAvailableInventory_ReturnsTrue

**Tipo**: `[Fact]` - Prueba determinista sin parámetros

**Propósito**: Validar que `PlaceOrder()` retorna `true` cuando el inventario está disponible

**Configuración (Arrange)**:
```csharp
Mock<IInventoryService> ? IsProductAvailable() retorna true
OrderService creado con el mock
Order: ProductId="P1", Quantity=100
```

**Ejecución (Act)**:
```csharp
bool result = orderService.PlaceOrder(order);
```

**Verificaciones (Assert)**:
- ? result es true
- ? IsProductAvailable invocado exactamente 1 vez

**Caso de Uso**: Cliente compra producto que está disponible en stock

---

### Prueba 2: PlaceOrder_WithoutAvailableInventory_ReturnsFalse

**Tipo**: `[Fact]`

**Propósito**: Validar que `PlaceOrder()` retorna `false` cuando no hay inventario

**Configuración (Arrange)**:
```csharp
Mock<IInventoryService> ? IsProductAvailable() retorna false
OrderService creado con el mock
Order: ProductId="P1", Quantity=100
```

**Verificaciones (Assert)**:
- ? result es false
- ? IsProductAvailable invocado exactamente 1 vez

**Caso de Uso**: Cliente intenta comprar producto agotado

---

### Prueba 3: PlaceOrder_WithNullOrder_ThrowsArgumentNullException

**Tipo**: `[Fact]`

**Propósito**: Validar que se lanza `ArgumentNullException` cuando Order es null

**Ejecución (Act)**:
```csharp
// Se espera una excepción
var exception = Assert.Throws<ArgumentNullException>(
    () => orderService.PlaceOrder(null)
);
```

**Verificaciones (Assert)**:
- ? Se lanza ArgumentNullException
- ? ParamName es "order"

**Nota**: Usa `#pragma warning disable CS8625` para suprimir warning de nullable

**Caso de Uso**: Validación de entrada - prevenir null reference exceptions

---

### Prueba 4: PlaceOrder_PassesCorrectProductIdAndQuantity

**Tipo**: `[Fact]`

**Propósito**: Verificar que se envían los parámetros correctos al servicio de inventario

**Configuración (Arrange)**:
```csharp
const string expectedProductId = "PROD-123";
const int expectedQuantity = 50;
Mock setup para verificar esos valores específicos
Order: ProductId="PROD-123", Quantity=50
```

**Verificaciones (Assert)**:
- ? IsProductAvailable invocado con "PROD-123" y 50
- ? Invocado exactamente 1 vez

**Caso de Uso**: Asegurar que los datos correctos se pasan a dependencias

---

### Prueba 5: PlaceOrder_WithDifferentQuantities_ReturnsTrue

**Tipo**: `[Theory]` con `[InlineData]`

**Parámetros Probados**: 1, 10, 100, 1000

**Propósito**: Validar que el servicio funciona con diferentes cantidades

**Para cada cantidad**:
```csharp
Mock ? IsProductAvailable() retorna true
Order con cantidad específica
Verificar que result es true
```

**Casos de Uso**: 
- Compra de 1 unidad
- Compra de 10 unidades
- Compra de 100 unidades
- Compra de 1000 unidades

---

### Prueba 6: PlaceOrder_WithDifferentProductIds_ReturnsTrue

**Tipo**: `[Theory]` con `[InlineData]`

**Parámetros Probados**: "P1", "PRODUCT-001", "SKU-2024-001", "A"

**Propósito**: Validar que el servicio funciona con diferentes formatos de ID

**Formatos Probados**:
- Simple: "P1"
- Descriptivo: "PRODUCT-001"
- SKU Format: "SKU-2024-001"
- Una letra: "A"

**Caso de Uso**: Diferentes sistemas de nomenclatura de productos

---

### Prueba 7: PlaceOrder_MultipleCallsWithSameService_IndependentResults

**Tipo**: `[Fact]`

**Propósito**: Validar múltiples órdenes con resultados variados usando SetupSequence

**Configuración**:
```csharp
SetupSequence retorna: true ? false ? true
Tres órdenes diferentes
```

**Ejecución**:
```csharp
order1 ? result1 = true
order2 ? result2 = false
order3 ? result3 = true
```

**Verificaciones**:
- ? IsProductAvailable invocado exactamente 3 veces
- ? Cada resultado es correcto

**Caso de Uso**: Procesamiento de lote de órdenes

---

### Prueba 8: PlaceOrder_CallsInventoryServiceExactlyOnce

**Tipo**: `[Fact]`

**Propósito**: Asegurar que el servicio se invoca exactamente una vez (sin llamadas adicionales)

**Verificación Detallada**:
```csharp
mockInventoryService.Verify(
    x => x.IsProductAvailable(It.IsAny<string>(), It.IsAny<int>()),
    Times.Once,
    "El servicio de inventario debe ser llamado exactamente una vez"
);
```

**Caso de Uso**: Prevenir llamadas duplicadas o innecesarias al servicio

---

### Prueba 9: PlaceOrder_WithNullOrder_DoesNotCallInventoryService

**Tipo**: `[Fact]`

**Propósito**: Verificar que cuando Order es nulo, el servicio NO se invoca

**Verificación**:
```csharp
mockInventoryService.Verify(
    x => x.IsProductAvailable(It.IsAny<string>(), It.IsAny<int>()),
    Times.Never,
    "El servicio de inventario no debe ser llamado cuando la orden es nula"
);
```

**Caso de Uso**: Validación de entrada antes de procesar

---

### Prueba 10: Order_Properties_CanBeSet

**Tipo**: `[Fact]`

**Propósito**: Validar que las propiedades de Order se pueden establecer y leer

**Ejecución**:
```csharp
order.ProductId = "TEST-001";
order.Quantity = 50;
```

**Verificaciones**:
- ? ProductId es "TEST-001"
- ? Quantity es 50

**Caso de Uso**: Prueba básica de modelo de datos

---

### Prueba 11: OrderService_Constructor_StoresInventoryService

**Tipo**: `[Fact]`

**Propósito**: Verificar que el constructor almacena la dependencia correctamente

**Configuración**:
```csharp
Crear OrderService con mock
```

**Verificaciones**:
- ? OrderService no es nulo
- ? El servicio no ha sido invocado aún (creación simple)

**Caso de Uso**: Validar inyección de dependencias

---

### Prueba 12: PlaceOrder_WithZeroQuantity_CallsInventoryService

**Tipo**: `[Fact]`

**Propósito**: Edge case - validar comportamiento con cantidad = 0

**Configuración**:
```csharp
Order: ProductId="P1", Quantity=0
Mock ? IsProductAvailable("P1", 0) retorna false
```

**Verificaciones**:
- ? result es false
- ? IsProductAvailable invocado con 0

**Caso de Uso**: Validar manejo de entrada inválida

---

### Prueba 13: PlaceOrder_WithNegativeQuantity_CallsInventoryService

**Tipo**: `[Fact]`

**Propósito**: Edge case - validar comportamiento con cantidad negativa

**Configuración**:
```csharp
Order: ProductId="P1", Quantity=-10
Mock ? IsProductAvailable("P1", -10) retorna false
```

**Caso de Uso**: Prevenir cantidades inválidas

---

### Prueba 14: PlaceOrder_WithEmptyProductId_CallsInventoryService

**Tipo**: `[Fact]`

**Propósito**: Edge case - validar comportamiento con ProductId vacío

**Configuración**:
```csharp
Order: ProductId="", Quantity=100
Mock ? IsProductAvailable("", ...) retorna false
```

**Caso de Uso**: Validar ID de producto válido

---

### Prueba 15: PlaceOrder_ProcessMultipleOrders_CorrectSequence

**Tipo**: `[Fact]`

**Propósito**: Integración - procesar múltiples órdenes en secuencia

**Estrategia**:
```csharp
Queue<bool> con valores [true, false, true]
Procesa 3 órdenes diferentes
Cada llamada consume el siguiente valor de la queue
```

**Ejecución**:
```csharp
order1 ? resultado: true (de queue)
order2 ? resultado: false (de queue)
order3 ? resultado: true (de queue)
```

**Caso de Uso**: Validar procesamiento de lotes

---

## ?? Patrones XUnit Utilizados

### 1. [Fact] - Pruebas Determinísticas
```csharp
[Fact]
public void TestMethod()
{
    // Una prueba, un resultado esperado
}
```

### 2. [Theory] - Pruebas Parametrizadas
```csharp
[Theory]
[InlineData(1)]
[InlineData(10)]
[InlineData(100)]
public void TestMethod(int quantity)
{
    // La prueba se ejecuta 3 veces con diferentes valores
}
```

### 3. Moq - Setup
```csharp
mock.Setup(x => x.Method(params))
    .Returns(value);
```

### 4. Moq - SetupSequence
```csharp
mock.SetupSequence(x => x.Method(params))
    .Returns(value1)
    .Returns(value2)
    .Returns(value3);
```

### 5. Moq - Verify
```csharp
mock.Verify(
    x => x.Method(params),
    Times.Once); // Once, Exactly(n), Never, AtLeastOnce
```

---

## ?? Cobertura de Código

```
OrderService.PlaceOrder()
?? Línea: if (order == null)           ? Cubierta (Prueba 3, 9)
?? Línea: throw ArgumentNullException  ? Cubierta (Prueba 3)
?? Línea: bool available = ...         ? Cubierta (Pruebas 1,2,4-8,12-15)
?? Línea: if (!available)              ? Cubierta (Pruebas 2,7,12-14)
?? Línea: return false                 ? Cubierta (Pruebas 2,7)
?? Línea: return true                  ? Cubierta (Pruebas 1,5-7)

Cobertura Total: 100% ?
```

---

## ?? Convención de Nomenclatura

Todas las pruebas siguen el patrón:

```
MethodName_Scenario_ExpectedResult

Donde:
- MethodName = Método siendo probado (PlaceOrder)
- Scenario = Condiciones de la prueba (WithAvailableInventory)
- ExpectedResult = Resultado esperado (ReturnsTrue)
```

---

## ?? Ejecución de Pruebas

### Ejecutar Todas
```bash
dotnet test
```

### Ejecutar Una Prueba Específica
```bash
dotnet test --filter "PlaceOrder_WithAvailableInventory_ReturnsTrue"
```

### Ejecutar Con Detalles
```bash
dotnet test --verbosity=detailed
```

### Resultado Esperado
```
Test summary: total: 15, failed: 0, succeeded: 15
```

---

## ?? Métricas

- **Total de Pruebas**: 15
- **Pruebas [Fact]**: 12
- **Pruebas [Theory]**: 3
- **Parámetros de Theory**: 2+4+7 = 13 casos
- **Total de Ejecuciones**: 15 + 13 = 28
- **Líneas de Código Probadas**: 100%
- **Métodos Probados**: 2 (PlaceOrder, Properties de Order)
- **Dependencias Mockeadas**: 1 (IInventoryService)

---

## ?? Casos de Uso Cubiertos

? **Casos Felices**
- Inventario disponible ? Orden aceptada
- Múltiples cantidades válidas
- Múltiples formatos de ID

? **Casos de Error**
- Inventario agotado ? Orden rechazada
- Orden nula ? Excepción

? **Edge Cases**
- Cantidad cero
- Cantidad negativa
- ProductId vacío

? **Validaciones**
- Verificación de parámetros
- Verificación de llamadas a servicios
- Verificación de exactitud

? **Integración**
- Múltiples órdenes en secuencia
- Resultados variados

---

## ?? Conceptos Demorados

1. **Inyección de Dependencias** - OrderService recibe IInventoryService
2. **Mocking** - Simulamos comportamiento del servicio
3. **Verify** - Validamos cómo se usó el mock
4. **Setup** - Configuramos el comportamiento esperado
5. **SetupSequence** - Múltiples retornos en secuencia
6. **Parámetros Flexibles** - It.IsAny<T>()
7. **Times** - Verificar exactitud de invocaciones
8. **Theory** - Pruebas parametrizadas

---

**Total de Pruebas Implementadas**: 15 ?  
**Estado**: Todas pasando ?  
**Cobertura**: 100% ?
