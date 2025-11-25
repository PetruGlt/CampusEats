# CampusEats Payments - Complete Test Workflow

This file contains a complete step-by-step workflow to test all Payment features with Stripe integration.

## 🚀 Quick Start

The payment system integrates with Stripe and allows:
- Creating checkout sessions for orders
- Processing payments through Stripe
- Viewing payment history
- Testing payment completion manually (without webhooks)

## ⚙️ Prerequisites

Before testing payments, ensure:
1. Your application is running on `https://localhost:5001`
2. You have Stripe API keys configured in `appsettings.json`:
   ```json
   "Stripe": {
     "SecretKey": "sk_test_...",
     "PublishableKey": "pk_test_..."
   }
   ```
3. You have at least one order created (see Orders testing guide)

---

## 📝 Step-by-Step Testing Workflow

### Step 1: View Menu Items and Create an Order

First, you need an order to create a payment for.

**Request 1 - Get Menu Items:**
```http
GET https://localhost:5001/menu
```

📋 **Copy a menu item ID** from the response.

**Request 2 - Create Order:**
```http
POST https://localhost:5001/orders
Content-Type: application/json

{
  "userId": "testuser123",
  "items": [
    {
      "menuItemId": "PASTE-MENU-ITEM-ID-HERE",
      "quantity": 2,
      "specialInstructions": "Extra cheese"
    }
  ],
  "notes": "Test order for payment"
}
```

**Expected Response:**
```json
{
  "id": "order-guid-here",
  "userId": "testuser123",
  "items": [...],
  "status": "Pending",
  "totalAmount": 25.50,
  "createdAt": "2025-11-24T...",
  "notes": "Test order for payment"
}
```

📋 **Copy the order ID** from the response - you'll need it for the next step.

---

### Step 2: Create Checkout Session

Create a Stripe checkout session for the order.

**Request:**
```http
POST https://localhost:5001/payments/create-checkout
Content-Type: application/json

{
  "orderId": "PASTE-ORDER-ID-HERE",
  "userId": "testuser123",
  "successUrl": "https://localhost:5001/payment-success",
  "cancelUrl": "https://localhost:5001/payment-cancel"
}
```

**Expected Response:**
```json
{
  "paymentId": "payment-guid-here",
  "sessionId": "cs_test_...",
  "checkoutUrl": "https://checkout.stripe.com/c/pay/cs_test_...",
  "status": "Pending",
  "amount": 2550,
  "currency": "usd"
}
```

📋 **Copy the following from the response:**
- `paymentId` - for checking payment status
- `checkoutUrl` - for completing the payment
- `sessionId` - Stripe session ID

**💡 Note:** 
- Amount is in cents (2550 = $25.50)
- The checkout URL redirects to Stripe's payment page

---

### Step 3: Complete Payment (Option A - Real Stripe Payment)

Open the `checkoutUrl` from Step 2 in your browser.

**Stripe Test Card Details:**
- **Card Number:** `4242 4242 4242 4242`
- **Expiry Date:** Any future date (e.g., `12/25`)
- **CVC:** Any 3 digits (e.g., `123`)
- **ZIP Code:** Any 5 digits (e.g., `12345`)

**What Happens:**
1. You'll see Stripe's checkout page with your order items
2. Enter the test card details above
3. Click "Pay"
4. You'll be redirected to `successUrl` if successful
5. **⚠️ Payment will stay "Pending" until webhook processes it**

---

### Step 3: Complete Payment (Option B - Manual Testing)

For testing without Stripe CLI or webhooks, use the test endpoint.

**Request:**
```http
POST https://localhost:5001/payments/PASTE-PAYMENT-ID-HERE/test-complete
```

**Expected Response:**
```json
{
  "message": "Payment marked as succeeded",
  "paymentId": "payment-guid",
  "orderId": "order-guid"
}
```

**What This Does:**
- Marks payment status as "Succeeded"
- Sets `completedAt` timestamp
- Updates order status from "Pending" to "Preparing"

**💡 Note:** This endpoint simulates what the webhook would do in production.

---

### Step 4: Check Payment Status

Verify the payment was processed successfully.

**Request:**
```http
GET https://localhost:5001/payments/PASTE-PAYMENT-ID-HERE
```

**Expected Response:**
```json
{
  "paymentId": "payment-guid",
  "orderId": "order-guid",
  "amount": 2550,
  "currency": "usd",
  "status": "Succeeded",
  "userId": "testuser123",
  "createdAt": "2025-11-24T10:00:00Z",
  "completedAt": "2025-11-24T10:01:00Z",
  "failureReason": null,
  "receiptUrl": null
}
```

**Status Values:**
- `Pending` - Checkout created, waiting for payment
- `Processing` - Payment being processed by Stripe
- `Succeeded` - Payment successful ✅
- `Failed` - Payment failed ❌
- `Cancelled` - Payment cancelled
- `Refunded` - Payment refunded

---

### Step 5: View All Payment History

See all payments in the system.

**Request:**
```http
GET https://localhost:5001/payments/history
```

**Expected Response:**
```json
[
  {
    "paymentId": "payment-guid-1",
    "orderId": "order-guid-1",
    "amount": 2550,
    "currency": "usd",
    "status": "Succeeded",
    "userId": "testuser123",
    "createdAt": "2025-11-24T10:00:00Z",
    "completedAt": "2025-11-24T10:01:00Z",
    "failureReason": null,
    "receiptUrl": null
  },
  ...
]
```

---

### Step 6: Filter Payment History by User

Get payments for a specific user.

**Request:**
```http
GET https://localhost:5001/payments/history?userId=testuser123
```

**Expected Response:**
Only payments for `testuser123`

---

### Step 7: Filter Payment History by Status

Get only successful payments.

**Request:**
```http
GET https://localhost:5001/payments/history?status=Succeeded
```

**Other Status Filters:**
- `?status=Pending` - Unpaid orders
- `?status=Failed` - Failed payments
- `?status=Cancelled` - Cancelled payments

---

### Step 8: Filter Payment History by Date Range

Get payments within a specific time period.

**Request:**
```http
GET https://localhost:5001/payments/history?startDate=2025-11-01&endDate=2025-11-30
```

**Format:** ISO 8601 date format (YYYY-MM-DD)

---

### Step 9: Complex Filter - Combine Multiple Criteria

Get successful payments for a specific user in a date range.

**Request:**
```http
GET https://localhost:5001/payments/history?userId=testuser123&status=Succeeded&startDate=2025-11-01&endDate=2025-11-30
```

---

### Step 10: Verify Order Status Changed

After successful payment, verify the order status updated.

**Request:**
```http
GET https://localhost:5001/orders/PASTE-ORDER-ID-HERE
```

**Expected Response:**
```json
{
  "id": "order-guid",
  "userId": "testuser123",
  "status": "Preparing",  // ← Changed from "Pending"
  "totalAmount": 25.50,
  "updatedAt": "2025-11-24T10:01:00Z",  // ← Updated timestamp
  ...
}
```

**Order Status After Payment:**
- Before payment: `Pending`
- After payment: `Preparing` ✅

---

## 🧪 Test Error Cases

### 1. Create Payment for Non-Existent Order

**Request:**
```http
POST https://localhost:5001/payments/create-checkout
Content-Type: application/json

{
  "orderId": "00000000-0000-0000-0000-000000000001",
  "userId": "testuser",
  "successUrl": "https://localhost:5001/success",
  "cancelUrl": "https://localhost:5001/cancel"
}
```

**Expected:** 404 error - "Order with ID 00000000-0000-0000-0000-000000000001 was not found"

---

### 2. Create Payment for Order with Duplicate Payment

Try to create a second payment for the same order.

**Request:**
```http
POST https://localhost:5001/payments/create-checkout
Content-Type: application/json

{
  "orderId": "ALREADY-PAID-ORDER-ID",
  "userId": "testuser",
  "successUrl": "https://localhost:5001/success",
  "cancelUrl": "https://localhost:5001/cancel"
}
```

**Expected:** 400 error - "Payment already exists for this order"

---

### 3. Create Payment for Order with Invalid Prices

If an order has items with price = 0 (shouldn't happen with proper menu setup):

**Expected:** 400 error - "Order contains items with invalid price or quantity: [item names]. All items must have price > 0 and quantity > 0."

---

### 4. Create Payment for Order with No Items

If an order somehow has no items:

**Expected:** 400 error - "Order has no items"

---

### 5. Get Non-Existent Payment

**Request:**
```http
GET https://localhost:5001/payments/00000000-0000-0000-0000-000000000000
```

**Expected:** 400 error - "Payment with ID 00000000-0000-0000-0000-000000000000 not found"

---

### 6. Invalid Status Filter

**Request:**
```http
GET https://localhost:5001/payments/history?status=InvalidStatus
```

**Expected:** Returns empty list (invalid status is ignored)

---

## 💳 Stripe Test Cards

### Successful Payment Cards

| Card Number | Description |
|-------------|-------------|
| `4242 4242 4242 4242` | Visa - Always succeeds |
| `5555 5555 5555 4444` | Mastercard - Always succeeds |
| `3782 822463 10005` | American Express - Always succeeds |

### Failed Payment Cards

| Card Number | Description |
|-------------|-------------|
| `4000 0000 0000 0002` | Card declined |
| `4000 0000 0000 9995` | Insufficient funds |
| `4000 0000 0000 0069` | Expired card |
| `4000 0000 0000 0119` | Processing error |

### Special Test Cases

| Card Number | Description |
|-------------|-------------|
| `4000 0027 6000 3184` | Requires 3D Secure authentication |
| `4000 0000 0000 0341` | Attaches to Customer (saved payment) |

**For all test cards:**
- **Expiry:** Any future date (e.g., 12/25)
- **CVC:** Any 3 digits (e.g., 123)
- **ZIP:** Any 5 digits (e.g., 12345)

📚 **More test cards:** https://stripe.com/docs/testing

---

## 🎯 Complete Payment Workflow Example

### Scenario: Customer Orders and Pays for Food

**1. Browse Menu**
```http
GET https://localhost:5001/menu
```
→ Copy menu item IDs

**2. Create Order**
```http
POST https://localhost:5001/orders
{
  "userId": "customer001",
  "items": [{"menuItemId": "menu-item-guid", "quantity": 2}]
}
```
→ Copy `orderId`

**3. Create Payment Checkout**
```http
POST https://localhost:5001/payments/create-checkout
{
  "orderId": "order-guid",
  "userId": "customer001",
  "successUrl": "https://localhost:5001/success",
  "cancelUrl": "https://localhost:5001/cancel"
}
```
→ Copy `paymentId` and `checkoutUrl`

**4. Complete Payment (Choose One)**

**Option A - Real Stripe Payment:**
- Open `checkoutUrl` in browser
- Use test card: `4242 4242 4242 4242`
- Complete checkout

**Option B - Manual Test:**
```http
POST https://localhost:5001/payments/{paymentId}/test-complete
```

**5. Verify Payment**
```http
GET https://localhost:5001/payments/{paymentId}
```
→ Should show `status: "Succeeded"`

**6. Verify Order Updated**
```http
GET https://localhost:5001/orders/{orderId}
```
→ Should show `status: "Preparing"`

**7. Kitchen Processes Order**
```http
PUT https://localhost:5001/kitchen/orders/{orderId}/status
{"status": "Ready"}
```

**8. Complete Order**
```http
PUT https://localhost:5001/kitchen/orders/{orderId}/status
{"status": "Completed"}
```

---

## 📊 Payment Flow Diagram

```
Customer                  Application              Stripe
   |                          |                      |
   |--1. Create Order-------->|                      |
   |<------Order Created------|                      |
   |                          |                      |
   |--2. Create Checkout----->|                      |
   |                          |--Create Session----->|
   |                          |<----Session URL------|
   |<----Checkout URL---------|                      |
   |                          |                      |
   |--3. Open Checkout URL------------------------>|
   |<----Payment Page (Stripe Hosted)---------------|
   |                          |                      |
   |--4. Enter Card & Pay---------------------------->|
   |                          |                      |
   |                          |<--Webhook: Success---|
   |                          |  (Updates payment &  |
   |                          |   order status)      |
   |                          |                      |
   |<--5. Redirect to Success-|                      |
   |    URL                   |                      |
```

**Alternative Flow (Manual Testing):**
```
Customer              Application
   |                      |
   |--1. Create Order---->|
   |--2. Create Checkout->|
   |--3. Manual Complete->|  /payments/{id}/test-complete
   |<----Success----------|  (Simulates webhook)
```

---

## 🔍 Debugging Tips

### Payment Stuck in "Pending"
**Cause:** Webhook not processed or test endpoint not called.

**Solution:**
- **Development:** Use `/payments/{id}/test-complete` endpoint
- **Production:** Ensure Stripe webhooks are configured correctly

### "No API key provided" Error
**Cause:** Stripe API key not configured.

**Solution:** Check `appsettings.json`:
```json
"Stripe": {
  "SecretKey": "sk_test_YOUR_KEY_HERE"
}
```

### Checkout URL Returns 404
**Cause:** Session expired (sessions expire after 24 hours).

**Solution:** Create a new checkout session.

### Amount Shows in Cents
**This is correct!** Stripe works with cents:
- $25.50 = 2550 cents
- $10.00 = 1000 cents

---

## 📋 Summary of Payment Endpoints

### Payment Operations
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/payments/create-checkout` | POST | Create Stripe checkout session |
| `/payments/history` | GET | Get all payments (with filters) |
| `/payments/{id}` | GET | Get specific payment details |
| `/payments/{id}/test-complete` | POST | **Test only** - Mark payment as succeeded |

### Query Parameters for `/payments/history`
| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `userId` | string | Filter by user | `?userId=customer001` |
| `status` | string | Filter by status | `?status=Succeeded` |
| `startDate` | date | From date (ISO 8601) | `?startDate=2025-11-01` |
| `endDate` | date | To date (ISO 8601) | `?endDate=2025-11-30` |

### Payment Statuses
- `Pending` - Checkout created, awaiting payment
- `Processing` - Payment being processed
- `Succeeded` - Payment successful ✅
- `Failed` - Payment failed ❌
- `Cancelled` - Payment cancelled
- `Refunded` - Payment refunded

---

## ⚠️ Important Notes

### Development vs Production

**Development (Current Setup):**
- Uses test API keys (`sk_test_...`)
- Use test cards for payments
- Manual completion via `/test-complete` endpoint
- No real money involved

**Production:**
- Requires live API keys (`sk_live_...`)
- Real credit/debit cards
- Automatic webhook processing required
- Real money transactions

### Security

⚠️ **NEVER commit real API keys to Git!**

✅ **Use environment variables in production:**
```json
"Stripe": {
  "SecretKey": "${STRIPE_SECRET_KEY}",
  "PublishableKey": "${STRIPE_PUBLISHABLE_KEY}"
}
```

### Testing Best Practices

1. **Always start with menu items** - Orders need valid menu items
2. **Create orders before payments** - Can't pay for non-existent orders
3. **Use test cards in development** - Never use real cards
4. **Test error cases** - Verify validation works correctly
5. **Check order status changes** - Ensure payment updates orders
6. **Test filters** - Verify payment history filtering works

---

## 🎓 Learning Resources

- **Stripe Documentation:** https://stripe.com/docs
- **Stripe Test Cards:** https://stripe.com/docs/testing
- **Stripe Checkout:** https://stripe.com/docs/payments/checkout
- **Stripe API Reference:** https://stripe.com/docs/api

---

## 🆘 Quick Troubleshooting

| Problem | Solution |
|---------|----------|
| "Order not found" | Create an order first with POST /orders |
| "Payment already exists" | Use a different order or check existing payment |
| "No API key provided" | Check Stripe keys in appsettings.json |
| Payment stays "Pending" | Use /test-complete endpoint (development) |
| Invalid card number | Use test cards from this guide |
| Checkout URL expired | Create new checkout session |

---

**Last Updated:** November 24, 2025
**Author:** CampusEats Development Team
**Version:** 1.0.0

