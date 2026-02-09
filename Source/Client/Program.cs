// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Grpc.Net.Client;
using Interfaces;
using Interfaces.Orders;
using Interfaces.Primitives;
using Interfaces.Products;
using ProtoBuf.Grpc.Client;

var serverAddress = "https://localhost:7000";
Console.WriteLine($"=== gRPC Test Client ===");
Console.WriteLine($"Connecting to: {serverAddress}");
Console.WriteLine();

using var channel = GrpcChannel.ForAddress(serverAddress);
var productsService = channel.CreateGrpcService<IProductsService>();
var ordersService = channel.CreateGrpcService<IOrdersService>();

var running = true;

while (running)
{
    DisplayMenu();
    var option = Console.ReadLine();

    try
    {
        switch (option)
        {
            case "1":
                await CreateProduct(productsService);
                break;
            case "2":
                await UpdateProductPrice(productsService);
                break;
            case "3":
                await GetProduct(productsService);
                break;
            case "4":
                await GetAllProducts(productsService);
                break;
            case "5":
                await SubscribeToProductUpdates(productsService);
                break;
            case "6":
                await PlaceOrderAsync(ordersService);
                break;
            case "7":
                await CancelOrderAsync(ordersService);
                break;
            case "8":
                await GetOrderAsync(ordersService);
                break;
            case "9":
                await GetAllOrders(ordersService);
                break;
            case "10":
                await SubscribeToOrderUpdates(ordersService);
                break;
            case "0":
                running = false;
                Console.WriteLine("Exiting...");
                break;
            default:
                Console.WriteLine("Invalid option. Please try again.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    Console.WriteLine();
}

void DisplayMenu()
{
    Console.WriteLine("=== gRPC Test Client ===");
    Console.WriteLine("1. Create Product");
    Console.WriteLine("2. Update Product Price");
    Console.WriteLine("3. Get Product");
    Console.WriteLine("4. Get All Products");
    Console.WriteLine("5. Subscribe to Product Updates (observable)");
    Console.WriteLine("6. Place Order");
    Console.WriteLine("7. Cancel Order");
    Console.WriteLine("8. Get Order");
    Console.WriteLine("9. Get All Orders");
    Console.WriteLine("10. Subscribe to Order Updates (observable)");
    Console.WriteLine("0. Exit");
    Console.WriteLine();
    Console.Write("Select an option: ");
}

async Task CreateProduct(IProductsService service)
{
    Console.WriteLine("\n--- Create Product ---");
    var name = PromptForInput("Product name");
    var priceInput = PromptForInput("Product price");

    if (!decimal.TryParse(priceInput, out var price))
    {
        Console.WriteLine("Invalid price. Please enter a valid decimal number.");
        return;
    }

    var productId = Guid.NewGuid();
    var command = new CreateProduct
    {
        Id = productId,
        Name = name,
        Price = price,
        CreatedAt = DateTimeOffset.UtcNow
    };

    var result = await service.CreateProduct(command);

    if (result.IsSuccess)
    {
        Console.WriteLine($"✓ Product created successfully with ID: {productId}");
    }
    else
    {
        Console.WriteLine($"✗ Failed to create product: {result.ErrorMessage}");
    }
}

async Task UpdateProductPrice(IProductsService service)
{
    Console.WriteLine("\n--- Update Product Price ---");
    var idInput = PromptForInput("Product ID");

    if (!Guid.TryParse(idInput, out var productId))
    {
        Console.WriteLine("Invalid product ID. Please enter a valid GUID.");
        return;
    }

    var priceInput = PromptForInput("New price");

    if (!decimal.TryParse(priceInput, out var price))
    {
        Console.WriteLine("Invalid price. Please enter a valid decimal number.");
        return;
    }

    var command = new UpdateProductPrice
    {
        Id = productId,
        Price = price
    };

    var result = await service.UpdateProductPrice(command);

    if (result.IsSuccess)
    {
        Console.WriteLine("✓ Product price updated successfully");
    }
    else
    {
        Console.WriteLine($"✗ Failed to update product price: {result.ErrorMessage}");
    }
}

async Task GetProduct(IProductsService service)
{
    Console.WriteLine("\n--- Get Product ---");
    var idInput = PromptForInput("Product ID");

    if (!Guid.TryParse(idInput, out var productId))
    {
        Console.WriteLine("Invalid product ID. Please enter a valid GUID.");
        return;
    }

    var query = new GetProduct { Id = productId };
    var product = await service.GetProduct(query);

    if (product != null)
    {
        DisplayProduct(product);
    }
    else
    {
        Console.WriteLine("Product not found.");
    }
}

async Task GetAllProducts(IProductsService service)
{
    Console.WriteLine("\n--- Get All Products ---");
    var query = new GetProducts();
    var products = await service.GetProducts(query);

    var productList = products.ToList();
    if (productList.Any())
    {
        Console.WriteLine($"Found {productList.Count} product(s):");
        foreach (var product in productList)
        {
            DisplayProduct(product);
            Console.WriteLine();
        }
    }
    else
    {
        Console.WriteLine("No products found.");
    }
}

async Task SubscribeToProductUpdates(IProductsService service)
{
    Console.WriteLine("\n--- Subscribe to Product Updates ---");
    Console.WriteLine("Listening for product updates for 30 seconds...");
    Console.WriteLine("Press Ctrl+C to stop early\n");

    var query = new GetProductUpdates();
    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

    try
    {
        await foreach (var product in service.GetProductUpdates(query).WithCancellation(cts.Token))
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Product Update:");
            DisplayProduct(product);
            Console.WriteLine();
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Subscription ended.");
    }
}

async Task PlaceOrderAsync(IOrdersService service)
{
    Console.WriteLine("\n--- Place Order ---");
    var productIdInput = PromptForInput("Product ID");

    if (!Guid.TryParse(productIdInput, out var productId))
    {
        Console.WriteLine("Invalid product ID. Please enter a valid GUID.");
        return;
    }

    var quantityInput = PromptForInput("Quantity");

    if (!int.TryParse(quantityInput, out var quantity) || quantity <= 0)
    {
        Console.WriteLine("Invalid quantity. Please enter a positive integer.");
        return;
    }

    Console.WriteLine("Delivery preference:");
    Console.WriteLine("1. Address (string)");
    Console.WriteLine("2. Pickup location (int)");
    var preferenceType = PromptForInput("Select type (1 or 2)");

    OneOf<string, int> deliveryPreference;

    if (preferenceType == "1")
    {
        var address = PromptForInput("Enter delivery address");
        deliveryPreference = new OneOf<string, int>(address);
    }
    else if (preferenceType == "2")
    {
        var locationInput = PromptForInput("Enter pickup location ID");
        if (!int.TryParse(locationInput, out var locationId))
        {
            Console.WriteLine("Invalid location ID. Please enter a valid integer.");
            return;
        }
        deliveryPreference = new OneOf<string, int>(locationId);
    }
    else
    {
        Console.WriteLine("Invalid preference type.");
        return;
    }

    var orderId = Guid.NewGuid();
    var command = new PlaceOrder
    {
        Id = orderId,
        ProductId = productId,
        Quantity = quantity,
        DeliveryPreference = deliveryPreference
    };

    var result = await service.PlaceOrder(command);

    if (result.IsSuccess)
    {
        Console.WriteLine($"✓ Order placed successfully with ID: {orderId}");
    }
    else
    {
        Console.WriteLine($"✗ Failed to place order: {result.ErrorMessage}");
    }
}

async Task CancelOrderAsync(IOrdersService service)
{
    Console.WriteLine("\n--- Cancel Order ---");
    var idInput = PromptForInput("Order ID");

    if (!Guid.TryParse(idInput, out var orderId))
    {
        Console.WriteLine("Invalid order ID. Please enter a valid GUID.");
        return;
    }

    var command = new CancelOrder { Id = orderId };
    var result = await service.CancelOrder(command);

    if (result.IsSuccess)
    {
        Console.WriteLine("✓ Order cancelled successfully");
    }
    else
    {
        Console.WriteLine($"✗ Failed to cancel order: {result.ErrorMessage}");
    }
}

async Task GetOrderAsync(IOrdersService service)
{
    Console.WriteLine("\n--- Get Order ---");
    var idInput = PromptForInput("Order ID");

    if (!Guid.TryParse(idInput, out var orderId))
    {
        Console.WriteLine("Invalid order ID. Please enter a valid GUID.");
        return;
    }

    var query = new GetOrder { Id = orderId };
    var order = await service.GetOrder(query);

    if (order != null)
    {
        DisplayOrder(order);
    }
    else
    {
        Console.WriteLine("Order not found.");
    }
}

async Task GetAllOrders(IOrdersService service)
{
    Console.WriteLine("\n--- Get All Orders ---");
    var query = new GetOrders();
    var orders = await service.GetOrders(query);

    var orderList = orders.ToList();
    if (orderList.Any())
    {
        Console.WriteLine($"Found {orderList.Count} order(s):");
        foreach (var order in orderList)
        {
            DisplayOrder(order);
            Console.WriteLine();
        }
    }
    else
    {
        Console.WriteLine("No orders found.");
    }
}

async Task SubscribeToOrderUpdates(IOrdersService service)
{
    Console.WriteLine("\n--- Subscribe to Order Updates ---");
    Console.WriteLine("Listening for order updates for 30 seconds...");
    Console.WriteLine("Press Ctrl+C to stop early\n");

    var query = new GetOrderUpdates();
    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

    try
    {
        await foreach (var order in service.GetOrderUpdates(query).WithCancellation(cts.Token))
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Order Update:");
            DisplayOrder(order);
            Console.WriteLine();
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Subscription ended.");
    }
}

void DisplayProduct(Product product)
{
    Console.WriteLine($"  Product ID: {product.Id}");
    Console.WriteLine($"  Name: {product.Name}");
    Console.WriteLine($"  Price: ${product.Price:F2}");
}

void DisplayOrder(Order order)
{
    Console.WriteLine($"  Order ID: {order.Id}");
    Console.WriteLine($"  Product ID: {order.ProductId}");
    Console.WriteLine($"  Quantity: {order.Quantity}");
}

string PromptForInput(string prompt)
{
    Console.Write($"{prompt}: ");
    return Console.ReadLine() ?? string.Empty;
}
