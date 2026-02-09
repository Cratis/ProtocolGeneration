// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ProtoBuf.Grpc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces.Orders
{
    [ServiceContract]
    public interface IOrdersService
    {
        [OperationContract(1)]
        Task<CommandResult> CancelOrder(CancelOrder command);
        [OperationContract(2)]
        Task<Order> GetOrder(GetOrder command);
        [OperationContract(3)]
        Task<IEnumerable<Order>> GetOrders(GetOrders command);
        [OperationContract(4)]
        IAsyncEnumerable<Order> GetOrderUpdates(GetOrderUpdates command);
        [OperationContract(5)]
        Task<CommandResult> PlaceOrder(PlaceOrder command);
    }
}