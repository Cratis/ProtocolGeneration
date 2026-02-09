// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Interfaces.Products;
[ServiceContract]
public interface IProductsService
{
    [OperationContract]
    Task<CommandResult> CreateProduct(CreateProduct command);
    [OperationContract]
    Task<Product> GetProduct(GetProduct command);
    [OperationContract]
    Task<IEnumerable<Product>> GetProducts(GetProducts command);
    [OperationContract]
    IAsyncEnumerable<Product> GetProductUpdates(GetProductUpdates command);
    [OperationContract]
    Task<CommandResult> UpdateProductPrice(UpdateProductPrice command);
}