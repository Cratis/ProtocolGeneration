# Protocol Interface Generator

A CLI tool that generates gRPC service interfaces from types annotated with `[Command]`, `[Query]`, and `[ObservableQuery]` attributes.

## Usage

```bash
protocol-generator --assembly <path> --output <path> [--base-namespace <name>] [--skip-segments <count>]
```

## Options

- `--assembly` (required): Path to the assembly to analyze
- `--output` (required): Output directory for generated interfaces
- `--base-namespace` (optional): Base namespace for generated interfaces (default: "Interfaces")
- `--skip-segments` (optional): Number of namespace segments to skip from source types (default: 1)

## Example

```bash
protocol-generator \
  --assembly bin/Debug/net10.0/Backend.dll \
  --output ../Interfaces \
  --base-namespace Interfaces \
  --skip-segments 1
```

## How It Works

1. **Type Discovery**: Scans the assembly for types with `[Command]`, `[Query]`, or `[ObservableQuery]` attributes
2. **Service Grouping**: Groups types by their `[BelongsTo]` attribute value
3. **Namespace Validation**: Ensures all types in a service share the same namespace
4. **Interface Generation**: Creates Protobuf.net-compatible service interfaces with:
   - `[ServiceContract]` on the interface
   - `[OperationContract]` on each method with sequential operation numbers
   - Proper type transformations for ConceptAs, OneOf, DateTimeOffset, and ISubject types

## Type Transformations

The generator automatically transforms types to be serialization-friendly:

- `ConceptAs<T>` → Unwrapped to primitive type `T`
- `OneOf<T0, T1, ...>` → `Interfaces.Primitives.OneOf<T0, T1, ...>`
- `DateTimeOffset` → `Interfaces.Primitives.SerializableDateTimeOffset`
- `ISubject<T>` → `IAsyncEnumerable<T>` (for observable queries)

## Generated Interface Structure

For a service named "Orders" with commands and queries in namespace `Backend.Orders`:

```csharp
// Interfaces/Orders/IOrdersService.cs
namespace Interfaces.Orders;

[ServiceContract]
public interface IOrdersService
{
    [OperationContract(1)]
    Task<CommandResult> PlaceOrder(PlaceOrder command);
    
    [OperationContract(2)]
    Task<IEnumerable<Order>> GetOrders(GetOrders query);
    
    [OperationContract(3)]
    IAsyncEnumerable<Order> GetOrderUpdates(GetOrderUpdates query);
}
```

## Requirements

Types must:
- Have one of: `[Command]`, `[Query]`, or `[ObservableQuery]` attribute
- Have a `[BelongsTo("ServiceName")]` attribute
- Have a `Handle()` method (for queries/observable queries)
- All types in a service must be in the same namespace

## Exit Codes

- `0`: Success
- `1`: Error (assembly not found, namespace mismatch, etc.)
