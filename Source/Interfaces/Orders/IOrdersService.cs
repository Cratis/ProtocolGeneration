// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Interfaces.Orders;
[ServiceContract]
public interface IOrdersService
{
    [OperationContract]
    Task<CommandResult> CancelOrder(CancelOrder command);
    [OperationContract]
    Task<Order> GetOrder(GetOrder command);
    [OperationContract]
    Task<IEnumerable<Order>> GetOrders(GetOrders command);
    [OperationContract]
    IAsyncEnumerable<Order> GetOrderUpdates(GetOrderUpdates command);
    [OperationContract]
    Task<CommandResult> PlaceOrder(PlaceOrder command);
}