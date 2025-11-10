# Week 3 Implementation: Orders + Kitchen Features

This document describes the implementation of the **Orders** and **Kitchen** features for the CampusEats application, following the Vertical Slice Architecture pattern.

---

## 📋 Overview

The Orders and Kitchen features enable customers to place food orders and allow kitchen staff to manage order preparation workflow. The implementation includes:

- **Orders Feature**: Create, view, and cancel orders
- **Kitchen Feature**: View pending orders and update order status through the preparation workflow

---

## 🏗️ Architecture

Following the Vertical Slice Architecture, each feature is self-contained with:
- Request/Response DTOs
- Handler (business logic)
- Validators (FluentValidation)
- Entity models
- API endpoints

### Entities

#### **Order**
```csharp
public class Order
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public List<OrderItem> Items { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Notes { get; set; }
}
```

#### **OrderItem**
```csharp
public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public string MenuItemName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? SpecialInstructions { get; set; }
}
```

#### **OrderStatus Enum**
```csharp
public enum OrderStatus
{
    Pending,    // Order placed, waiting for kitchen
    Preparing,  // Kitchen is preparing the order
    Ready,      // Order is ready for pickup
    Completed,  // Order has been picked up
    Cancelled   // Order was cancelled
}
```

---

## 🔌 API Endpoints

### Orders Endpoints

#### **Create Order**
```http
POST /orders
Content-Type: application/json

{
  "userId": "user123",
  "items": [
    {
      "menuItemId": "guid-here",
      "quantity": 2,
      "specialInstructions": "No onions"
    }
  ],
  "notes": "Deliver to Room 301"
}

Response: 201 Created
{
  "id": "order-guid",
  "userId": "user123",
  "items": [...],
  "status": "Pending",
  "totalAmount": 17.98,
  "createdAt": "2025-11-10T10:00:00Z",
  "updatedAt": null,
  "notes": "Deliver to Room 301"
}
```

#### **Get All Orders**
```http
GET /orders
GET /orders?userId=user123  # Filter by user

Response: 200 OK
[
  {
    "id": "order-guid",
    "userId": "user123",
    "items": [...],
    "status": "Pending",
    "totalAmount": 17.98,
    ...
  }
]
```

#### **Get Order By ID**
```http
GET /orders/{id}

Response: 200 OK
{
  "id": "order-guid",
  "userId": "user123",
  ...
}

Response: 404 Not Found (if order doesn't exist)
{
  "message": "Order with ID {id} not found",
  "statusCode": 404,
  "errorCode": "ORDER_NOT_FOUND"
}
```

#### **Cancel Order**
```http
PUT /orders/{id}/cancel

Response: 200 OK
{
  "id": "order-guid",
  "status": "Cancelled",
  ...
}

Response: 400 Bad Request (if order is already Completed/Cancelled)
{
  "message": "Cannot cancel order with status Completed",
  "statusCode": 400,
  "errorCode": "INVALID_ORDER_STATUS"
}
```

### Kitchen Endpoints

#### **Get Pending Orders**
```http
GET /kitchen/orders

Response: 200 OK
[
  {
    "id": "order-guid",
    "userId": "user123",
    "items": [
      {
        "menuItemName": "Burger",
        "quantity": 2,
        "specialInstructions": "No onions"
      }
    ],
    "status": "Pending",
    "totalAmount": 17.98,
    "createdAt": "2025-11-10T10:00:00Z"
  }
]
```
Returns all orders with status: Pending, Preparing, or Ready.

#### **Update Order Status**
```http
PUT /kitchen/orders/{id}/status
Content-Type: application/json

{
  "status": "Preparing"
}

Response: 200 OK
{
  "id": "order-guid",
  "status": "Preparing",
  "updatedAt": "2025-11-10T10:05:00Z",
  ...
}
```

---

## 🔄 Order Workflow

The system enforces valid status transitions:

```
Pending → Preparing → Ready → Completed
   ↓
Cancelled
```

### Valid Transitions
- **Pending** → Preparing, Cancelled
- **Preparing** → Ready, Cancelled
- **Ready** → Completed
- **Completed** → (terminal state)
- **Cancelled** → (terminal state)

### Example Workflow
1. Customer creates order → Status: **Pending**
2. Kitchen starts preparing → Status: **Preparing**
3. Food is ready → Status: **Ready**
4. Customer picks up → Status: **Completed**

Alternative: Customer cancels before kitchen starts → Status: **Cancelled**

---

## ✅ Validation Rules

### Create Order Validation
- `UserId` is required
- `Items` list must not be empty
- Each item must have:
  - Valid `MenuItemId`
  - `Quantity` > 0

### Update Status Validation
- `Status` must be a valid OrderStatus value
- Status transition must be valid (enforced by business logic)

---

## 🛠️ Implementation Details

### Files Created

**Orders Feature** (`Features/Orders/`):
- `Order.cs` - Entity model
- `OrderItem.cs` - Entity model
- `CreateOrderRequest.cs` & `CreateOrderHandler.cs`
- `GetAllOrdersRequest.cs` & `GetAllOrdersHandler.cs`
- `GetOrderByIdRequest.cs` & `GetOrderByIdHandler.cs`
- `CancelOrderRequest.cs` & `CancelOrderHandler.cs`

**Kitchen Feature** (`Features/Kitchen/`):
- `GetPendingOrdersRequest.cs` & `GetPendingOrdersHandler.cs`
- `UpdateOrderStatusRequest.cs` & `UpdateOrderStatusHandler.cs`

**Validators** (`Validators/`):
- `CreateOrderValidator.cs`
- `UpdateOrderStatusValidator.cs`

**Exceptions** (`Exceptions/`):
- `OrderNotFoundException.cs`
- `InvalidOrderStatusException.cs`

### Database Schema

The `CampusEatsContext` was updated to include:
```csharp
public DbSet<Order> Orders { get; set; }
public DbSet<OrderItem> OrderItems { get; set; }
```

With proper Entity Framework Core configuration:
- One-to-Many relationship between Order and OrderItems
- Cascade delete for order items
- OrderStatus stored as string
- Decimal precision for prices

---

## 🧪 Testing

Use the provided `CampusEats_Orders_Kitchen.http` file to test all endpoints.

### Test Scenario
1. Create menu items (burger, pizza, salad)
2. Create an order with multiple items
3. View all orders
4. Kitchen views pending orders
5. Update order status through workflow
6. Test error cases (invalid transitions, missing data)

---

## 🎯 Features Implemented

✅ **Order Management**
- Create orders with multiple menu items
- Calculate total amount automatically
- Filter orders by user
- Cancel orders (with validation)

✅ **Kitchen Operations**
- View all pending/preparing/ready orders
- Update order status with workflow validation
- Prevent invalid status transitions

✅ **Validation & Error Handling**
- FluentValidation for request validation
- Custom exceptions with proper HTTP status codes
- Global exception middleware for consistent error responses

✅ **Database Integration**
- EF Core entities with relationships
- Proper foreign key constraints
- Automatic database creation

✅ **Swagger Documentation**
- All endpoints organized by tags (Orders, Kitchen)
- Clear operation names and descriptions

---

## 📝 Next Steps (Week 4+)

- Implement Payments integration with actual payment processing
- Add Loyalty program features
- Build Blazor UI for customer and kitchen views
- Add unit tests with XUnit
- Add integration tests with NSubstitute

---

## 🚀 Running the Application

```bash
cd CampusEats
dotnet run
```

Navigate to `http://localhost:5168` to access Swagger UI.

---

## 📖 Developer Notes

- All handlers follow the same pattern for consistency
- DTOs are used for API contracts, entities for database
- Status transitions are validated to maintain data integrity
- Timestamps are UTC for consistency
- Foreign keys ensure referential integrity

---

**Developer:** Week 3 Implementation  
**Date:** November 10, 2025  
**Status:** ✅ Complete
