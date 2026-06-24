# ContractManagement

Ứng dụng quản lý hợp đồng điện tử dành cho tài xế, chạy trên web responsive bằng Blazor Web App Interactive Server. Tài xế có thể đăng nhập trên điện thoại, nhập thông tin khách hàng, tạo hợp đồng, ký tên, cho khách hàng ký trực tiếp trên màn hình và lưu thông tin thời gian/vị trí phục vụ xác thực.

## Công nghệ hiện tại

- .NET SDK `9.0.301`.
- Target framework `net9.0`.
- Blazor Web App với Interactive Server.
- MudBlazor `9.5.0`.
- ASP.NET Core Identity và cookie authentication.
- Entity Framework Core `9.0.6`.
- SQL Server.
- JavaScript Interop cho canvas chữ ký và Geolocation API.

Project được khóa SDK bằng file `global.json`:

```json
{
  "sdk": {
    "version": "9.0.301",
    "rollForward": "latestPatch",
    "allowPrerelease": false
  }
}
```

Kiểm tra SDK:

```powershell
dotnet --version
```

Kết quả mong đợi:

```text
9.0.301
```

## Cấu trúc solution

```text
ContractManagement
├── ContractManagement.slnx
├── global.json
├── Directory.Build.props
├── README.md
├── PROJECT_OVERVIEW.md
├── SETUP.md
├── WORKFLOW.md
└── src
    ├── ContractManagement.Domain
    ├── ContractManagement.Application
    ├── ContractManagement.Infrastructure
    └── ContractManagement.Web
```

### Domain

Chứa entity, enum và kiểu dữ liệu cốt lõi:

- ApplicationUser.
- DriverProfile.
- DriverSignature.
- Customer.
- ContractType.
- ContractTemplate.
- Contract.
- ContractSignature.
- ContractAttachment.
- ContractAuditLog.

Project Domain có `FrameworkReference` tới `Microsoft.AspNetCore.App` vì `ApplicationUser` kế thừa `IdentityUser`.

### Application

Chứa interface, DTO và nghiệp vụ ứng dụng. Hiện tại đã scaffold:

- `IContractService`.
- `ContractListItemDto`.

Các command/query thực tế vẫn cần phát triển tiếp.

### Infrastructure

Chứa:

- `ApplicationDbContext`.
- Fluent API.
- Index và check constraint.
- EF Core SQL Server.
- Identity EF Core.
- Database seeder.

`DatabaseSeeder.cs` phải có:

```csharp
using Microsoft.Extensions.DependencyInjection;
```

để sử dụng `CreateScope()` hoặc `CreateAsyncScope()`.

### Web

Chứa:

- Layout desktop/mobile.
- Login/logout.
- Menu theo role.
- Dashboard Admin và Driver.
- Các trang hợp đồng, khách hàng, chữ ký.
- Theme MudBlazor.
- JavaScript chữ ký và GPS.

## Tài khoản seed

```text
Username: admin
Password: Admin@123456
Role: Admin
```

Phải thay đổi hoặc loại bỏ mật khẩu seed trước khi triển khai production.

## Chạy nhanh

### 1. Cấu hình SQL Server

Sửa connection string trong:

```text
src/ContractManagement.Web/appsettings.json
```

Ví dụ Windows Authentication:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ContractManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### 2. Restore và build

```powershell
dotnet restore
dotnet build
```

### 3. Tạo migration

```powershell
dotnet ef migrations add InitialCreate `
  --project src/ContractManagement.Infrastructure `
  --startup-project src/ContractManagement.Web
```

### 4. Cập nhật database

```powershell
dotnet ef database update `
  --project src/ContractManagement.Infrastructure `
  --startup-project src/ContractManagement.Web
```

### 5. Chạy web

```powershell
dotnet run --project src/ContractManagement.Web
```

## Quy ước Razor bắt buộc

Các file `.razor` phải được định dạng nhiều dòng. Không dồn toàn bộ markup và `@code` trên cùng một dòng.

Sai:

```razor
</MudPaper>@code{[Parameter]public string Title{get;set;}}
```

Đúng:

```razor
</MudPaper>

@code {
    [Parameter]
    public string Title { get; set; } = string.Empty;
}
```

Mọi `MudTextField` cần khai báo rõ kiểu:

```razor
<MudTextField T="string"
              Label="Họ tên"
              @bind-Value="_model.CustomerName" />
```

Không sử dụng component chưa tồn tại trong project như:

```text
ContractTypeStep
CustomerInformationStep
TripInformationStep
ContractContentStep
ContractReview
RedirectToLogin
```

trừ khi đã tạo file `.razor` tương ứng và import đúng namespace.

## Lưu ý MudBlazor 9.5.0

Một số API component có thể khác với các ví dụ cũ trên internet.

- Không dùng `Name` trên `MudTextField` hoặc `MudCheckBox` để submit HTML form truyền thống.
- Login bằng HTTP POST nên dùng thẻ `<input>` HTML thật hoặc endpoint Identity chuẩn.
- Không dùng `MudBottomNavigation` nếu package hiện tại không nhận diện component.
- Có thể tạo bottom navigation bằng `MudPaper`, `MudStack`, `MudIconButton` và `MudButton`.
- Nếu `MudFab Icon="..."` bị analyzer cảnh báo, thay bằng `MudButton` chứa `MudIcon`.
- Nếu `MudStepper`, `MudStep Label` hoặc `Linear` không tương thích, dùng stepper thủ công bằng biến `_currentStep`.

## Build sạch khi Razor cache lỗi

Đóng Visual Studio rồi chạy:

```powershell
dotnet clean

Get-ChildItem -Path . -Include bin,obj -Recurse -Directory |
    Remove-Item -Recurse -Force

if (Test-Path ".vs") {
    Remove-Item ".vs" -Recurse -Force
}

dotnet restore
dotnet build
```

## Tài liệu liên quan

- [PROJECT_OVERVIEW.md](PROJECT_OVERVIEW.md): kiến trúc và module.
- [SETUP.md](SETUP.md): cấu hình môi trường, build, migration và production.
- [WORKFLOW.md](WORKFLOW.md): workflow nghiệp vụ và quy trình phát triển.
