# Template Service

## Tổng quan
Template Service là một microservice trong hệ thống RoboChemist, chịu trách nhiệm quản lý templates và các chức năng liên quan đến templates.

## Cấu trúc dự án

```
TemplateService/
├── RoboChemist.TemplateService.API/          # Web API Layer
│   ├── Controllers/                          # API Controllers
│   ├── Properties/
│   │   └── launchSettings.json               # Launch configurations
│   ├── Program.cs                            # Application entry point
│   ├── appsettings.json                      # Configuration settings
│   └── RoboChemist.TemplateService.API.csproj
│
├── RoboChemist.TemplateService.Model/        # Data Models & DbContext
│   ├── Data/
│   │   └── AppDbContext.cs                   # Entity Framework DbContext
│   ├── Models/                               # Entity models
│   └── RoboChemist.TemplateService.Model.csproj
│
├── RoboChemist.TemplateService.Repository/   # Data Access Layer
│   └── RoboChemist.TemplateService.Repository.csproj
│
└── RoboChemist.TemplateService.Service/      # Business Logic Layer
    └── RoboChemist.TemplateService.Service.csproj
```

## Các Layer trong dự án

### 1. API Layer
- Xử lý HTTP requests/responses
- Chứa các Controllers
- Cấu hình Swagger/OpenAPI
- Port mặc định: 5191 (HTTP), 7206 (HTTPS)

### 2. Model Layer
- Định nghĩa các Entity models
- Database context (Entity Framework)
- Cấu hình database connections (PostgreSQL)

### 3. Repository Layer
- Triển khai Generic Repository pattern
- Data access logic
- Database operations (CRUD)

### 4. Service Layer
- Business logic
- Xử lý các quy tắc nghiệp vụ
- Gọi Repository để thao tác dữ liệu

## Yêu cầu hệ thống

- .NET 8.0 SDK
- PostgreSQL database
- File `.env` tại thư mục solution root với biến `TEMPLATE_DB` (connection string)

## Cài đặt và chạy

### 1. Cấu hình Database
Thêm connection string vào file `.env` tại thư mục solution root:
```
TEMPLATE_DB=Host=localhost;Port=5432;Database=template_db;Username=your_username;Password=your_password
```

### 2. Restore dependencies
```bash
dotnet restore
```

### 3. Chạy API
```bash
cd RoboChemist.TemplateService.API
dotnet run
```

### 4. Truy cập Swagger UI
Mở trình duyệt và truy cập: `http://localhost:5191/swagger`

## Database Migrations

### Tạo migration mới
```bash
cd RoboChemist.TemplateService.Model
dotnet ef migrations add MigrationName --startup-project ../RoboChemist.TemplateService.API
```

### Cập nhật database
```bash
dotnet ef database update --startup-project ../RoboChemist.TemplateService.API
```

## Phát triển

### Thêm Model mới
1. Tạo class trong `RoboChemist.TemplateService.Model/Models/`
2. Thêm DbSet vào `AppDbContext.cs`
3. Cấu hình entity trong `OnModelCreating()`
4. Tạo và apply migration

### Thêm API Endpoint
1. Tạo Controller trong `RoboChemist.TemplateService.API/Controllers/`
2. Thêm Service interface và implementation
3. Đăng ký service trong `Program.cs`
4. Test qua Swagger UI

## Ghi chú
- Service này được scaffold dựa trên cấu trúc của SlidesService
- TODO: Thêm các models, repositories, services cụ thể theo yêu cầu nghiệp vụ
- TODO: Cấu hình CORS, Authentication/Authorization nếu cần

