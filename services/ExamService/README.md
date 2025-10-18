# RoboChemist - Exam Service

## Mô tả
ExamService là microservice quản lý việc tạo và quản lý đề thi hóa học trong hệ thống RoboChemist.

## Cấu trúc dự án

```
ExamService/
├── RoboChemist.ExamService.API/          # API Layer - RESTful endpoints
│   ├── Controllers/                       # API Controllers
│   ├── Properties/
│   │   └── launchSettings.json           # Launch configuration
│   ├── appsettings.json                  # Application settings
│   ├── appsettings.Development.json      # Development settings
│   ├── Program.cs                        # Application entry point
│   └── RoboChemist.ExamService.API.csproj
│
├── RoboChemist.ExamService.Model/        # Data Layer
│   ├── Data/
│   │   └── AppDbContext.cs              # Entity Framework DbContext
│   ├── Models/                           # Domain models/entities
│   └── RoboChemist.ExamService.Model.csproj
│
├── RoboChemist.ExamService.Repository/   # Repository Layer
│   ├── Repositories/                     # Repository implementations
│   └── RoboChemist.ExamService.Repository.csproj
│
└── RoboChemist.ExamService.Service/      # Business Logic Layer
    ├── Services/                         # Service implementations
    └── RoboChemist.ExamService.Service.csproj
```

## Công nghệ sử dụng

- **Framework**: .NET 8.0
- **Database**: PostgreSQL (với Entity Framework Core)
- **API Documentation**: Swagger/OpenAPI
- **Package Manager**: NuGet

## Dependencies

- Microsoft.EntityFrameworkCore (8.0.11)
- Npgsql.EntityFrameworkCore.PostgreSQL (8.0.11)
- Swashbuckle.AspNetCore (6.6.2)
- DotNetEnv (3.1.1)

## Cấu hình

### Database Connection
Service sử dụng biến môi trường `EXAM_DB` để kết nối database. Đảm bảo file `.env` ở thư mục root của solution có cấu hình:

```env
EXAM_DB=Host=localhost;Port=5432;Database=robochemist_exam;Username=your_user;Password=your_password
```

### Launch Settings
- HTTP: http://localhost:5002
- HTTPS: https://localhost:7002

## Chạy ứng dụng

### Từ Visual Studio
1. Mở solution `RoboChemist.sln`
2. Set `RoboChemist.ExamService.API` làm startup project
3. Press F5 hoặc Click Run

### Từ Command Line
```bash
cd src/dotnet/RoboChemist/services/ExamService/RoboChemist.ExamService.API
dotnet run
```

### Swagger UI
Khi chạy ở Development mode, truy cập Swagger tại:
- http://localhost:5002/swagger
- https://localhost:7002/swagger

## Database Migrations

### Tạo migration mới
```bash
cd src/dotnet/RoboChemist/services/ExamService/RoboChemist.ExamService.Model
dotnet ef migrations add InitialCreate --startup-project ../RoboChemist.ExamService.API
```

### Apply migrations
```bash
dotnet ef database update --startup-project ../RoboChemist.ExamService.API
```

## Kiến trúc

### API Layer (`RoboChemist.ExamService.API`)
- Xử lý HTTP requests/responses
- Input validation
- API documentation (Swagger)
- Dependency injection configuration

### Service Layer (`RoboChemist.ExamService.Service`)
- Business logic
- Data validation
- Business rules
- Orchestration

### Repository Layer (`RoboChemist.ExamService.Repository`)
- Data access
- Database operations
- Query implementations

### Model Layer (`RoboChemist.ExamService.Model`)
- Entity models
- DTOs
- Database context
- Configurations

## API Endpoints

_Coming soon - Sẽ được update sau khi implement các controllers_

## Development Guidelines

### Commit Message Format
Sử dụng Conventional Commits:
```
feat(exam-service): Add exam generation endpoint
fix(exam-service): Fix matrix calculation bug
docs(exam-service): Update API documentation
```

### Branch Naming
```
feature/exam-service/<feature-name>
bugfix/exam-service/<bug-description>
```

## Testing

_Coming soon - Sẽ được update sau khi implement tests_

## Liên hệ

**Dự án**: RoboChemist  
**Team**: PRN232 Development Team  
**Năm**: 2025
