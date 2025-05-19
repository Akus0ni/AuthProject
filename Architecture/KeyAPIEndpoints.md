# Key API Endpoints

## **Auth Controller**

* `POST /api/auth/register` – Local signup
* `POST /api/auth/login` – Email/password login
* `GET /api/auth/oauth/callback` – OAuth login callback
* `POST /api/auth/refresh-token` – Refresh access token
* `POST /api/auth/logout` – Revoke refresh token

## **User Controller**

* `GET /api/users` – List all users (any role)
* `POST /api/users` – Create user (Admin only)
* `PUT /api/users/{id}` – Update user (Admin only)
* `DELETE /api/users/{id}` – Delete user (Admin only)

## **Middleware/Guards**

* `AuthorizeAttribute(Role = "Admin")` – Restrict access to Admins only
* `AuthorizeAttribute(Role = "User,Admin")` – Allow any logged-in user
