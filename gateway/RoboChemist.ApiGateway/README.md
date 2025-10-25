# RoboChemist API Gateway

API Gateway sử dụng Ocelot để định tuyến requests đến các microservices và **SwaggerForOcelot** để hiển thị tất cả API của các services trong một Swagger UI duy nhất.

## ✨ Tính năng chính

- 🔄 **Unified Swagger UI**: Xem tất cả API từ 4 services (Auth, Slides, Exam, Wallet) trong một giao diện
- 🔐 **JWT Authentication**: Test API với JWT token ngay trên Swagger
- 🚀 **Single Entry Point**: Tất cả requests đi qua một gateway duy nhất

## Cấu trúc Routes

### Public Routes (Không cần JWT)
- `/auth/*` → AuthService (Port 7001)
  - POST `/auth/register` - Đăng ký tài khoản
  - POST `/auth/login` - Đăng nhập
  - POST `/auth/refresh-token` - Refresh JWT token

### Protected Routes (Cần JWT Token)
- `/slides/*` → SlidesService (Port 7000)
- `/exam/*` → ExamService (Port 7002)  
- `/wallet/*` → WalletService (Port 7003)

## Cách chạy

### Bước 1: Chạy tất cả downstream services
```bash
# Terminal 1: AuthService
cd services/AuthService/RoboChemist.AuthService.API
dotnet run

# Terminal 2: SlidesService  
cd services/SlidesService/RoboChemist.SlidesService.API
dotnet run

# Terminal 3: ExamService
cd services/ExamService/RoboChemist.ExamService.API
dotnet run

# Terminal 4: WalletService (optional)
cd services/WalletService/RoboChemist.WalletService.API
dotnet run
```

### Bước 2: Chạy API Gateway
```bash
# Terminal 5: API Gateway
cd gateway/RoboChemist.ApiGateway
dotnet run
```

API Gateway chạy tại: **`https://localhost:5001`**

Swagger UI: **`https://localhost:5001/swagger`**

## 🎯 Cách sử dụng Swagger UI

### 1. Mở Swagger
Truy cập: **`https://localhost:5001/swagger`**

Bạn sẽ thấy **dropdown** ở góc trên bên phải để chọn service:
- **Auth API** - Authentication endpoints  
- **Slides API** - Slides management
- **Exam API** - Exam management
- **Wallet API** - Wallet management

### 2. Đăng ký/Đăng nhập để lấy JWT token

**Chọn "Auth API"** từ dropdown → Expand endpoint → Try it out:

```json
POST /auth/register
{
  "username": "testuser",
  "password": "password123",
  "email": "test@example.com"
}

POST /auth/login
{
  "username": "testuser",
  "password": "password123"
}

// Response - Copy token này:
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2025-10-25T11:00:00Z"
  }
}
```

### 3. Authorize với JWT token

1. Click nút **"Authorize"** 🔒 (góc trên bên phải)
2. Nhập: `Bearer {paste_token_here}` (có chữ "Bearer " + space ở đầu)
3. Click **"Authorize"**
4. Click **"Close"**

### 4. Test Protected APIs

Sau khi authorize, chọn service và test API:

**Chọn "Slides API"**:
- GET `/slides/grade` - Lấy danh sách grade
- POST `/slides/topic` - Tạo topic mới

**Chọn "Exam API"**:
- GET `/exam/question` - Lấy danh sách câu hỏi
- POST `/exam/question` - Tạo câu hỏi mới

**Chọn "Wallet API"**:
- GET `/wallet/balance` - Xem số dư

## 💡 Lưu ý quan trọng

### ✅ PHẢI chạy tất cả services trước
Trước khi chạy API Gateway, **BẮT BUỘC** phải chạy:
- ✓ AuthService (7188)
- ✓ SlidesService (7205)
- ✓ ExamService (7002)
- ✓ WalletService (7100)

Nếu service nào chưa chạy, Swagger sẽ không load được API của service đó.

### ✅ JWT Token có thời hạn
- Token hết hạn sau **60 phút**
- Khi hết hạn, login lại để lấy token mới
- Nếu thấy lỗi 401, có thể token đã hết hạn

### ✅ Swagger Endpoints
Gateway fetch Swagger JSON từ:
- `https://localhost:7188/swagger/v1/swagger.json` (Auth)
- `https://localhost:7205/swagger/v1/swagger.json` (Slides)
- `https://localhost:7002/swagger/v1/swagger.json` (Exam)
- `https://localhost:7100/swagger/v1/swagger.json` (Wallet)

## 🔧 Troubleshooting

**"Failed to load API definition"**
- Service chưa chạy → Chạy service trước
- Port sai → Kiểm tra ocelot.json
- SSL certificate → Accept certificate của service

**"401 Unauthorized"**
- Chưa authorize hoặc token hết hạn
- Click "Authorize" và nhập token mới

**Swagger không hiển thị API**
- Service chưa chạy
- Kiểm tra service có Swagger không (truy cập trực tiếp)

## Ports

| Service | Direct Port | Gateway Route |
|---------|------------|---------------|
| AuthService | 7188 | /auth/* |
| SlidesService | 7205 | /slides/* |
| ExamService | 7002 | /exam/* |
| WalletService | 7100 | /wallet/* |
| **API Gateway** | **5001** | - |

## JWT Configuration

- Issuer: `RoboChemist.AuthService`
- Audience: `RoboChemist.Client`
- Secret Key: (Cùng với AuthService)
- Token Lifetime: 60 phút

## Lưu ý

1. ✅ **Swagger aggregation**: Tất cả API từ 4 services hiển thị trong 1 Swagger UI
2. ✅ **JWT token**: Authorize một lần, dùng cho tất cả protected routes
3. ✅ **Service dropdown**: Chọn service để xem API của service đó
4. ✅ **Direct routing**: Requests tự động route đến đúng service qua Ocelot
5. ✅ **CORS**: Đã cấu hình AllowAll cho development
