## Endpoints

### GET /api/ice
Returns a success message indicating the API is running.

### POST /api/ice/authenticate
Authenticates the client and returns an authentication token.

**Request Body:**
```json
{ "ClientId": "your-client-id", "Secret": "your-secret" }
```

**Response:**
```json
{ "success": true, "message": "Auth Success!", "token": "your-auth-token" }
```

### POST /api/ice/loan-field-reader
Reads loan field values.

**Request Body:**
```json
{ "Guid": "loan-guid", "LoanFields": ["field1", "field2"] }
```

**Response:**
```json
{ "loanFieldsResponse": { "success": true, "message": "Loan Fields Retrieved Successfully!", "loanFields": { "field1": "value1", "field2": "value2" }, "headers": { "X-Concurrency-Limit-Limit": "30", "X-Concurrency-Limit-Remaining": "30", "X-Rate-Limit-Limit": "500000", "X-Rate-Limit-Remaining": "500000", "X-Rate-Limit-Reset": "1729209600" }, "retryCount": 0 }, "elapsedMilliseconds": 547, "elapsedSeconds": 0.55 }
```

### POST /api/ice/loan-field-writer
Writes loan field values.

**Request Body:**

```json
{ "Guid": "loan-guid", "LoanFields": { "field1": "value1", "field2": "value2" } }
```
**Response:**
```json
{ "loanFieldsResponse": { "success": true, "message": "Loan Fields Written Successfully!", "loanFields": {}, "headers": { "X-Concurrency-Limit-Limit": "30", "X-Concurrency-Limit-Remaining": "30", "X-Rate-Limit-Limit": "500000", "X-Rate-Limit-Remaining": "500000", "X-Rate-Limit-Reset": "1729209600" }, "retryCount": 0 }, "elapsedMilliseconds": 39882, "elapsedSeconds": 39.88 }
```

