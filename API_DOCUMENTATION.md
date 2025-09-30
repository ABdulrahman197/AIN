## AIN API Documentation

### Conventions
- **Base URL**: your server origin (e.g., `https://your-host/`)
- **Auth**: send `Authorization: Bearer <JWT>` for protected endpoints
- **Content-Type**: `application/json` unless stated; uploads use `multipart/form-data`
- **Dates**: ISO 8601 (UTC)
- **Pagination**: `page` (default 1), `pageSize` (default 20, max 100)
- **Enums**: send enum names as strings

### Enums
- **ReportCategory**: `Security` | `PublicSafety` | `Traffic` | `Environment` | `Other`
- **ReportVisibility**: `Public` | `Confidential` | `Anonymous`
- **ReportStatus**: `Pending` | `InReview` | `Dispatched` | `Resolved` | `Rejected`
- **TrustBadge**: `Newcomer` | `Contributor` | `Trusted` | `Guardian` | `Vanguard`
- **UserRole**: `User` | `Admin` | `Authority`

---

### Auth

#### POST /api/auth/register
- Auth: none
- Body:
```json
{
  "email": "user@example.com",
  "displayName": "John Doe",
  "password": "StrongPass123!"
}
```
- Response 201:
```json
{ "id": "guid", "email": "user@example.com", "displayName": "John Doe" }
```

#### POST /api/auth/verify-otp
- Auth: none
- Body:
```json
{ "email": "user@example.com", "otp": "123456" }
```
- Response 200:
```json
"OTP verified!"
```

#### POST /api/auth/login
- Auth: none
- Body:
```json
{ "email": "user@example.com", "password": "StrongPass123!" }
```
- Response 200:
```json
{
  "token": "jwt-token",
  "userId": "guid",
  "refreshToken": "refresh-token",
  "expiry": "2025-09-24T12:34:56Z"
}
```

#### GET /api/auth/me
- Auth: Bearer required
- Response 200:
```json
{
  "id": "guid",
  "email": "user@example.com",
  "displayName": "John Doe",
  "trustPoints": 0,
  "badge": "Newcomer"
}
```

#### POST /api/auth/forget-password
- Auth: none
- Body:
```json
{ "email": "user@example.com" }
```
- Response 200:
```json
{ "message": "Password reset code sent to your email" }
```

#### POST /api/auth/reset-password
- Auth: none
- Body:
```json
{ "email": "user@example.com", "otp": "123456", "newPassword": "NewStrongPass123!" }
```
- Response 200:
```json
{ "message": "Password reset successfully" }
```

#### POST /api/auth/refresh-token
- Auth: none
- Body:
```json
{ "refreshToken": "refresh-token" }
```
- Response 200:
```json
{ "token": "new-jwt", "refreshToken": "new-refresh-token", "expiry": "2025-09-24T12:34:56Z" }
```

---

### Users & Authorities

#### GET /api/authorities
- Auth: none
- Response 200:
```json
[
  { "id": "guid", "name": "Police", "department": "Security" }
]
```

#### POST /api/users/{id}/trustpoints?delta={int}
- Auth: none (internal/admin usage)
- Response 200:
```json
{ "userId": "guid", "total": 42 }
```

---

### Reports

#### POST /api/reports
- Auth: none
- Body:
```json
{
  "title": "Accident on 5th Ave",
  "description": "Two cars collided...",
  "category": "Traffic",
  "visibility": "Public",
  "latitude": 24.7136,
  "longitude": 46.6753,
  "reporterId": "guid-or-null"
}
```
- Response 201:
```json
{ "id": "guid" }
```

#### GET /api/reports/{id}
- Auth: none
- Response 200:
```json
{
  "id": "guid",
  "title": "Accident on 5th Ave",
  "description": "Two cars collided...",
  "category": "Traffic",
  "visibility": "Public",
  "status": "Pending",
  "latitude": 24.7136,
  "longitude": 46.6753,
  "createdAt": "2025-09-24T11:22:33Z",
  "reporterId": "guid-or-null",
  "routedAuthorityId": "guid-or-null",
  "attachments": [
    { "id": "guid", "fileName": "photo.jpg", "contentType": "image/jpeg", "sizeBytes": 123456, "url": "/uploads/uuid_photo.jpg" }
  ]
}
```

#### GET /api/feed?page={int}&pageSize={int}
- Auth: none
- Response 200:
```json
[
  {
    "id": "guid",
    "title": "...",
    "description": "...",
    "category": "Traffic",
    "visibility": "Public",
    "status": "Pending",
    "latitude": 0.0,
    "longitude": 0.0,
    "createdAt": "2025-09-24T11:22:33Z",
    "reporterId": "guid-or-null",
    "routedAuthorityId": "guid-or-null",
    "attachments": []
  }
]
```

#### PATCH /api/reports/{id}/status
- Auth: none (intended for privileged flows)
- Body (enum name as JSON string):
```json
"InReview"
```
- Response: 204 No Content

#### GET /api/reports/{id}/interactions
- Auth: optional (uses current user if present)
- Response 200:
```json
{
  "id": "guid",
  "reporterId": "guid-or-null",
  "reporterDisplayName": "John Doe",
  "visibility": "Public",
  "category": "Traffic",
  "status": "Pending",
  "title": "Accident on 5th Ave",
  "description": "...",
  "latitude": 0.0,
  "longitude": 0.0,
  "createdAt": "2025-09-24T11:22:33Z",
  "routedAuthorityId": "guid-or-null",
  "authorityName": "Traffic Department",
  "attachments": [],
  "likeCount": 10,
  "commentCount": 2,
  "isLikedByCurrentUser": true,
  "recentComments": [
    { "id": "guid", "reportId": "guid", "userId": "guid", "userDisplayName": "Alice", "content": "Stay safe!", "createdAt": "2025-09-24T11:30:00Z", "updatedAt": null }
  ]
}
```

#### PUT /api/reports/{id}
- Auth: Bearer required
- Body:
```json
{
  "title": "Updated title",
  "description": "Updated description",
  "category": "Traffic",
  "visibility": "Public",
  "latitude": 24.7136,
  "longitude": 46.6753
}
```
- Response 200:
```json
{ "message": "Report updated successfully" }
```

#### DELETE /api/reports/{id}
- Auth: Bearer required
- Response: 204 No Content

#### POST /api/reports/{id}/like
- Auth: Bearer required
- Body: none
- Response 200:
```json
{ "isLiked": true, "message": "Report liked" }
```

#### POST /api/reports/{id}/comments
- Auth: Bearer required
- Body:
```json
{ "reportId": "ignored", "content": "Great report!" }
```
- Response 201:
```json
{ "id": "guid", "reportId": "guid", "userId": "guid", "userDisplayName": "John Doe", "content": "Great report!", "createdAt": "2025-09-24T11:45:00Z", "updatedAt": null }
```

#### GET /api/reports/{id}/comments?page={int}&pageSize={int}
- Auth: none
- Response 200:
```json
[
  { "id": "guid", "reportId": "guid", "userId": "guid", "userDisplayName": "John Doe", "content": "Great report!", "createdAt": "2025-09-24T11:45:00Z", "updatedAt": null }
]
```

#### PUT /api/comments/{commentId}
- Auth: Bearer required
- Body:
```json
{ "content": "Edited comment" }
```
- Response 200:
```json
{ "id": "guid", "reportId": "guid", "userId": "guid", "userDisplayName": "John Doe", "content": "Edited comment", "createdAt": "2025-09-24T11:45:00Z", "updatedAt": "2025-09-24T12:00:00Z" }
```

#### DELETE /api/comments/{commentId}
- Auth: Bearer required
- Response: 204 No Content

---

### Attachments

#### POST /api/reports/{id}/attachments
- Auth: none
- Content-Type: `multipart/form-data`
- Form fields:
  - `file`: binary
- Response 201:
```json
{ "id": "guid", "url": "/uploads/uuid_filename.ext" }
```

---

### Admin (Require Admin role)

#### GET /api/admin/users?page={int}&pageSize={int}
- Auth: Bearer (Admin)
- Response 200:
```json
{
  "users": [
    {
      "id": "guid",
      "email": "user@example.com",
      "displayName": "John Doe",
      "trustPoints": 0,
      "badge": "Newcomer",
      "role": "User",
      "isEmailConfirmed": true,
      "lastLogin": "2025-09-24T10:00:00Z",
      "reportsCount": 3,
      "createdAt": "2025-09-20T09:00:00Z"
    }
  ],
  "totalCount": 123,
  "page": 1,
  "pageSize": 20,
  "totalPages": 7
}
```

#### GET /api/admin/users/{userId}
- Auth: Bearer (Admin)
- Response 200: single user object (same shape as above item)

#### PUT /api/admin/users/{userId}
- Auth: Bearer (Admin)
- Body:
```json
{ "email": "user@example.com", "displayName": "Updated Name", "password": "optional-or-null" }
```
- Response 200:
```json
{ "message": "User updated successfully" }
```

#### DELETE /api/admin/users/{userId}
- Auth: Bearer (Admin)
- Response: 204 No Content

---

### Notes for Flutter
- Add `Authorization` header for protected endpoints.
- For status update endpoint, send enum as a JSON string (e.g., `"InReview"`).
- Uploaded file `url` is relative; compose with base URL to load.


