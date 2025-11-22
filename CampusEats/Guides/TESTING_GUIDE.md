# CampusEats Orders & Kitchen - Complete Test Workflow

This file contains a complete step-by-step workflow to test all Orders and Kitchen features.

## 🚀 Quick Start

The database is automatically seeded with 18 menu items when you start the application:
- Burgers (Classic, Cheese, Veggie)
- Pizzas (Margherita, Pepperoni)
- Salads (Caesar, Greek)
- Sides (Fries, Onion Rings, Wings)
- Main dishes (Pasta, Sandwiches, Fish & Chips)
- Desserts (Cake, Ice Cream)
- Drinks (Coffee, Juice, Soda)

## 📝 Step-by-Step Testing Workflow

### Step 1: View All Menu Items

First, get all menu items to see what's available and copy some IDs for ordering.

**Request:**
```http
GET http://localhost:5168/menu
```

**Expected Response:**
```json
[
  {
    "id": "some-guid-here",
    "name": "Classic Burger",
    "price": 8.99
  },
  ...
]
```

📋 **Copy 2-3 menu item IDs** from the response to use in the next steps.

---

### Step 2: Create an Order (Customer)

Create an order using actual menu item IDs from Step 1.

**Request:**
```http
POST http://localhost:5168/orders
Content-Type: application/json

{
  "userId": "customer001",
  "items": [
    {
      "menuItemId": "PASTE-MENU-ITEM-ID-HERE",
      "quantity": 2,
      "specialInstructions": "No onions please"
    },
    {
      "menuItemId": "PASTE-ANOTHER-MENU-ITEM-ID-HERE",
      "quantity": 1,
      "specialInstructions": "Extra cheese"
    }
  ],
  "notes": "Deliver to Room 301"
}
```

**Expected Response:**
```json
{
  "id": "order-guid-here",
  "userId": "customer001",
  "items": [
    {
      "id": "item-guid",
      "menuItemId": "menu-item-guid",
      "menuItemName": "Classic Burger",
      "price": 8.99,
      "quantity": 2,
      "specialInstructions": "No onions please"
    }
  ],
  "status": "Pending",
  "totalAmount": 17.98,
  "createdAt": "2025-11-10T...",
  "updatedAt": null,
  "notes": "Deliver to Room 301"
}
```

📋 **Copy the order ID** from the response for the next steps.

---

### Step 3: View All Orders

See all orders in the system.

**Request:**
```http
GET http://localhost:5168/orders
```

---

### Step 4: View Orders by User

Filter orders for a specific user.

**Request:**
```http
GET http://localhost:5168/orders?userId=customer001
```

---

### Step 5: View Specific Order

Get details of a specific order.

**Request:**
```http
GET http://localhost:5168/orders/PASTE-ORDER-ID-HERE
```

---

### Step 6: Kitchen Views Pending Orders

Kitchen staff can see all active orders.

**Request:**
```http
GET http://localhost:5168/kitchen/orders
```

**Expected Response:**
```json
[
  {
    "id": "order-guid",
    "userId": "customer001",
    "status": "Pending",
    "totalAmount": 17.98,
    ...
  }
]
```

---

### Step 7: Kitchen Starts Preparing Order

Update order status to "Preparing".

**Request:**
```http
PUT http://localhost:5168/kitchen/orders/PASTE-ORDER-ID-HERE/status
Content-Type: application/json

{
  "status": "Preparing"
}
```

**Expected Response:**
Order with status updated to "Preparing"

---

### Step 8: Kitchen Marks Order as Ready

When food is ready for pickup.

**Request:**
```http
PUT http://localhost:5168/kitchen/orders/PASTE-ORDER-ID-HERE/status
Content-Type: application/json

{
  "status": "Ready"
}
```

---

### Step 9: Mark Order as Completed

When customer picks up the order.

**Request:**
```http
PUT http://localhost:5168/kitchen/orders/PASTE-ORDER-ID-HERE/status
Content-Type: application/json

{
  "status": "Completed"
}
```

---

### Alternative: Customer Cancels Order

Customer can cancel before it's completed (only works if status is Pending or Preparing).

**Request:**
```http
PUT http://localhost:5168/orders/PASTE-ORDER-ID-HERE/cancel
```

---

## ✅ Valid Status Transitions

```
Pending → Preparing → Ready → Completed
   ↓
Cancelled
```

- From **Pending**: Can go to Preparing or Cancelled
- From **Preparing**: Can go to Ready or Cancelled
- From **Ready**: Can only go to Completed
- **Completed** and **Cancelled** are final states

---

## 🧪 Test Error Cases

### 1. Empty Order Items

**Request:**
```http
POST http://localhost:5168/orders
Content-Type: application/json

{
  "userId": "user123",
  "items": []
}
```

**Expected:** Validation error - "Order must contain at least one item"

---

### 2. Missing User ID

**Request:**
```http
POST http://localhost:5168/orders
Content-Type: application/json

{
  "userId": "",
  "items": [
    {
      "menuItemId": "some-guid",
      "quantity": 1
    }
  ]
}
```

**Expected:** Validation error - "UserId is required"

---

### 3. Invalid Quantity

**Request:**
```http
POST http://localhost:5168/orders
Content-Type: application/json

{
  "userId": "user123",
  "items": [
    {
      "menuItemId": "some-guid",
      "quantity": 0
    }
  ]
}
```

**Expected:** Validation error - "Quantity must be greater than 0"

---

### 4. Non-existent Menu Item

**Request:**
```http
POST http://localhost:5168/orders
Content-Type: application/json

{
  "userId": "user123",
  "items": [
    {
      "menuItemId": "00000000-0000-0000-0000-000000000000",
      "quantity": 1
    }
  ]
}
```

**Expected:** 404 error - "Menu items not found: 00000000-0000-0000-0000-000000000000"

---

### 5. Invalid Status Transition

Try to update a completed order:

**Request:**
```http
PUT http://localhost:5168/kitchen/orders/COMPLETED-ORDER-ID/status
Content-Type: application/json

{
  "status": "Preparing"
}
```

**Expected:** 400 error - "Cannot transition from Completed to Preparing"

---

### 6. Invalid Status Value

**Request:**
```http
PUT http://localhost:5168/kitchen/orders/some-order-id/status
Content-Type: application/json

{
  "status": "InvalidStatus"
}
```

**Expected:** Validation error - "Status must be one of: Pending, Preparing, Ready, Completed, Cancelled"

---

## 🎯 Complete Workflow Example

1. **GET /menu** - Browse menu (copy IDs)
2. **POST /orders** - Customer creates order (copy order ID)
3. **GET /kitchen/orders** - Kitchen sees new order
4. **PUT /kitchen/orders/{id}/status** - Set to "Preparing"
5. **PUT /kitchen/orders/{id}/status** - Set to "Ready"
6. **GET /orders?userId=customer001** - Customer checks their order
7. **PUT /kitchen/orders/{id}/status** - Set to "Completed"

---

## 📊 Summary of Available Endpoints

### Orders (Customer-facing)
- `POST /orders` - Create new order
- `GET /orders` - Get all orders (optional ?userId filter)
- `GET /orders/{id}` - Get specific order
- `PUT /orders/{id}/cancel` - Cancel order

### Kitchen (Staff-facing)
- `GET /kitchen/orders` - Get pending/active orders
- `PUT /kitchen/orders/{id}/status` - Update order status

### Menu (Reference)
- `GET /menu` - List all menu items
- `POST /menu` - Create menu item
- `PUT /menu/{id}` - Update menu item
- `DELETE /menu/{id}` - Delete menu item

---

## 🔍 Tips

- All endpoints are available in **Swagger UI** at `http://localhost:5168`
- The database is automatically seeded with 18 menu items on first run
- Use real GUIDs from the menu items when creating orders
- Status transitions are validated - you can't skip steps
- Orders can only be cancelled if they're Pending or Preparing
- The system automatically calculates the total amount based on item prices and quantities
