// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Interfaces.Products
{
    [ServiceContract]
    public interface IProductsService
    {
        [OperationContract(1)]
        Task<CommandResult> CreateProduct(CreateProduct command);
        [OperationContract(2)]
        Task<Product> GetProduct(GetProduct command);
        [OperationContract(3)]
        Task<IEnumerable<Product>> GetProducts(GetProducts command);
        [OperationContract(4)]
        IAsyncEnumerable<Product> GetProductUpdates(GetProductUpdates command);
        [OperationContract(5)]
        Task<CommandResult> UpdateProductPrice(UpdateProductPrice command);
    }
}