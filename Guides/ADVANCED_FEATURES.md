# Advanced Orders & Kitchen Features Documentation

## 🚀 New Advanced Features

This document describes the advanced features added to the Orders and Kitchen modules to make the system more powerful and production-ready.

---

## 📊 Orders Module - Advanced Features

### 1. **Order History with Date Filtering**

Get orders filtered by date range for reporting and analytics.

**Endpoint:** `GET /orders/history`

**Query Parameters:**
- `startDate` (optional): Filter orders created after this date (ISO 8601 format)
- `endDate` (optional): Filter orders created before this date (ISO 8601 format)
- `userId` (optional): Filter by specific user

**Example Request:**
```http
GET http://localhost:5168/orders/history?startDate=2025-11-01&endDate=2025-11-10
```

**Use Cases:**
- Daily/weekly/monthly sales reports
- User-specific order history
- Financial reconciliation

---

### 2. **Order Statistics Dashboard**

Get comprehensive statistics about all orders in the system.

**Endpoint:** `GET /orders/statistics`

**Response Example:**
```json
{
  "totalOrders": 150,
  "totalRevenue": 2845.50,
  "averageOrderValue": 18.97,
  "ordersByStatus": {
    "Pending": 5,
    "Preparing": 3,
    "Ready": 2,
    "Completed": 135,
    "Cancelled": 5
  },
  "todayOrders": 12,
  "todayRevenue": 247.85
}
```

**Use Cases:**
- Business intelligence
- Performance monitoring
- Revenue tracking

---

### 3. **Order Search**

Search orders by user ID, menu item names, or notes.

**Endpoint:** `GET /orders/search`

**Query Parameters:**
- `query` (optional): Search term (searches in userId, notes, and menu item names)
- `status` (optional): Filter by order status

**Example Requests:**
```http
GET http://localhost:5168/orders/search?query=burger
GET http://localhost:5168/orders/search?status=Pending
GET http://localhost:5168/orders/search?query=customer001&status=Completed
```

**Use Cases:**
- Customer service - finding specific orders
- Searching for orders containing specific items
- Tracking orders by status

---

### 4. **Estimated Wait Time**

Calculate the estimated wait time for an order based on queue position and historical data.

**Endpoint:** `GET /orders/{id}/wait-time`

**Response Example:**
```json
{
  "orderId": "123e4567-e89b-12d3-a456-426614174000",
  "currentStatus": "Pending",
  "estimatedWaitMinutes": 23,
  "estimatedCompletionTime": "2025-11-10T15:45:00Z",
  "message": "Approximately 23 minutes. 2 order(s) ahead of you."
}
```

**Algorithm:**
- Counts orders ahead in queue
- Calculates average preparation time from completed orders
- Adds complexity factor based on number of items
- Returns estimated completion time

**Use Cases:**
- Customer notifications
- Setting realistic expectations
- Queue management

---

## 👨‍🍳 Kitchen Module - Advanced Features

### 5. **Kitchen Dashboard**

Get a comprehensive overview of kitchen operations.

**Endpoint:** `GET /kitchen/dashboard`

**Response Example:**
```json
{
  "pendingOrdersCount": 5,
  "preparingOrdersCount": 3,
  "readyOrdersCount": 2,
  "oldestPendingOrderId": "123e4567-e89b-12d3-a456-426614174000",
  "oldestPendingOrderTime": "2025-11-10T14:30:00Z",
  "averagePreparationTimeMinutes": 18.5,
  "estimatedCompletionTime": "2025-11-10T15:00:00Z"
}
```

**Use Cases:**
- Kitchen staff overview
- Performance monitoring
- Capacity planning

---

### 6. **Bulk Order Status Update**

Update the status of multiple orders at once.

**Endpoint:** `PUT /kitchen/orders/bulk-update`

**Request Body:**
```json
{
  "orderIds": [
    "123e4567-e89b-12d3-a456-426614174000",
    "223e4567-e89b-12d3-a456-426614174001",
    "323e4567-e89b-12d3-a456-426614174002"
  ],
  "status": "Ready"
}
```

**Response Example:**
```json
{
  "successCount": 2,
  "failureCount": 1,
  "updatedOrderIds": [
    "123e4567-e89b-12d3-a456-426614174000",
    "223e4567-e89b-12d3-a456-426614174001"
  ],
  "failures": [
    {
      "orderId": "323e4567-e89b-12d3-a456-426614174002",
      "reason": "Cannot transition from Completed to Ready"
    }
  ]
}
```

**Features:**
- Validates each transition
- Returns detailed success/failure information
- Atomic operation per order
- Continues processing even if some fail

**Use Cases:**
- Batch processing during busy hours
- Marking multiple orders ready at once
- Bulk cancellations

---

### 7. **Popular Items Report**

Get the most frequently ordered items with sales data.

**Endpoint:** `GET /kitchen/popular-items`

**Query Parameters:**
- `topN` (optional, default: 10): Number of top items to return

**Example Request:**
```http
GET http://localhost:5168/kitchen/popular-items?topN=5
```

**Response Example:**
```json
[
  {
    "menuItemId": "123e4567-e89b-12d3-a456-426614174000",
    "menuItemName": "Classic Burger",
    "totalQuantitySold": 245,
    "timesOrdered": 180,
    "totalRevenue": 2205.55
  },
  {
    "menuItemId": "223e4567-e89b-12d3-a456-426614174001",
    "menuItemName": "Margherita Pizza",
    "totalQuantitySold": 198,
    "timesOrdered": 165,
    "totalRevenue": 2475.00
  }
]
```

**Use Cases:**
- Inventory planning
- Menu optimization
- Promotion planning
- Revenue analysis

---

## 🎯 Complete API Reference

### Orders Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/orders` | Create new order |
| GET | `/orders` | Get all orders (with optional userId filter) |
| GET | `/orders/{id}` | Get order by ID |
| GET | `/orders/history` | Get orders with date range filter |
| GET | `/orders/statistics` | Get order statistics |
| GET | `/orders/search` | Search orders |
| GET | `/orders/{id}/wait-time` | Get estimated wait time |
| PUT | `/orders/{id}/cancel` | Cancel order |

### Kitchen Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/kitchen/orders` | Get pending/active orders |
| GET | `/kitchen/dashboard` | Get kitchen dashboard summary |
| GET | `/kitchen/popular-items` | Get popular items report |
| PUT | `/kitchen/orders/{id}/status` | Update single order status |
| PUT | `/kitchen/orders/bulk-update` | Bulk update order statuses |

---

## 💡 Real-World Usage Scenarios

### Scenario 1: Morning Rush Management

**Kitchen Dashboard Check:**
```http
GET /kitchen/dashboard
```
Shows 15 pending orders, average prep time of 12 minutes.

**Bulk Update:**
Kitchen marks 5 ready orders as complete:
```http
PUT /kitchen/orders/bulk-update
{
  "orderIds": ["id1", "id2", "id3", "id4", "id5"],
  "status": "Completed"
}
```

---

### Scenario 2: Customer Inquiry

**Customer asks:** "Where's my order?"

**Check wait time:**
```http
GET /orders/{orderId}/wait-time
```
Response: "Approximately 8 minutes. 1 order(s) ahead of you."

---

### Scenario 3: End of Day Reporting

**Get today's statistics:**
```http
GET /orders/statistics
```

**Get popular items for inventory planning:**
```http
GET /kitchen/popular-items?topN=20
```

**Get completed orders for the day:**
```http
GET /orders/search?status=Completed
```

---

### Scenario 4: Menu Planning

**Find what customers ordered with "burger":**
```http
GET /orders/search?query=burger
```

**Get popular items for the week:**
```http
GET /orders/history?startDate=2025-11-03&endDate=2025-11-10
```

---

## 📈 Performance Considerations

### Indexed Fields
- `Order.CreatedAt` - for date range queries
- `Order.Status` - for status filtering
- `Order.UserId` - for user filtering
- `OrderItem.MenuItemId` - for popular items aggregation

### Optimization Tips
1. Use date range filters to limit result sets
2. Cache statistics endpoint (low change frequency)
3. Implement pagination for large result sets
4. Consider database views for complex aggregations

---

## 🔒 Security & Validation

### Input Validation
- Date ranges are validated
- Status values are enum-validated
- Bulk operations have size limits (consider adding max batch size)

### Authorization (Future Enhancement)
- Customer can only see their own orders
- Kitchen staff can see all orders
- Admins can access statistics

---

## 🧪 Testing Examples

### Test Order History
```http
# Get last week's orders
GET /orders/history?startDate=2025-11-03&endDate=2025-11-10

# Get specific user's completed orders
GET /orders/search?userId=customer001&status=Completed
```

### Test Bulk Update
```http
# Mark multiple orders as Ready
PUT /kitchen/orders/bulk-update
{
  "orderIds": ["id1", "id2", "id3"],
  "status": "Ready"
}
```

### Test Wait Time
```http
# Check wait time for pending order
GET /orders/{pendingOrderId}/wait-time
```

---

## 🎨 Future Enhancements

Potential features to add:
1. **Real-time notifications** - WebSocket updates for order status
2. **Order modification** - Add/remove items before preparation
3. **Priority orders** - Express/VIP order handling
4. **Staff performance** - Track individual staff preparation times
5. **Revenue forecasting** - Predict future sales based on trends
6. **Customer ratings** - Order feedback and ratings
7. **Loyalty integration** - Points earning and redemption

---

## 📊 Business Intelligence Queries

### Daily Sales Report
```http
GET /orders/history?startDate=2025-11-10T00:00:00Z&endDate=2025-11-10T23:59:59Z
GET /orders/statistics
```

### Best Selling Items This Week
```http
GET /kitchen/popular-items?topN=10
```

### Peak Hours Analysis
Combine `/orders/history` with custom grouping by hour

### Customer Retention
Filter `/orders/history` by userId and analyze frequency

---

All new endpoints are available in **Swagger UI** at `http://localhost:5168` 🚀
