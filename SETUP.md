# Hướng dẫn thiết lập và xử lý lỗi build

## Yêu cầu

- .NET SDK 9.0.301.
- Visual Studio 2026 hoặc VS Code.
- SQL Server 2019 trở lên.
- EF Core CLI: `dotnet tool install --global dotnet-ef`.

## Connection string

Windows Authentication:

```json
"DefaultConnection": "Server=localhost;Database=ContractManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

SQL Login:

```json
"DefaultConnection": "Server=SERVER;Database=ContractManagementDb;User Id=USER;Password=PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

Không commit mật khẩu production vào Git. Dùng environment variable hoặc user-secrets.

## User secrets

```bash
dotnet user-secrets init --project src/ContractManagement.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..." --project src/ContractManagement.Web
```

## Migration và chạy

```bash
dotnet restore
dotnet ef migrations add InitialCreate --project src/ContractManagement.Infrastructure --startup-project src/ContractManagement.Web
dotnet ef database update --project src/ContractManagement.Infrastructure --startup-project src/ContractManagement.Web
dotnet run --project src/ContractManagement.Web
```

## Production

- Reverse proxy IIS/Nginx.
- WebSocket phải được bật cho Blazor Interactive Server.
- Sticky session hoặc Azure SignalR nếu scale nhiều instance.
- Lưu file ngoài `wwwroot` hoặc object storage.
- Thiết lập backup DB và retention audit log.


## Khóa phiên bản SDK

File `global.json` tại thư mục gốc khóa SDK ở phiên bản `9.0.301`. Nếu máy chưa có đúng phiên bản này, cài .NET SDK 9.0.301 trước khi chạy `dotnet restore`.

## 1. Yêu cầu môi trường

- Windows 10/11 hoặc Linux server.
- .NET SDK `9.0.301`.
- SQL Server 2019 trở lên.
- Visual Studio hỗ trợ .NET 9 hoặc VS Code.
- `dotnet-ef` phiên bản 9.x.

Kiểm tra SDK:

```powershell
dotnet --info
dotnet --version
```

Kết quả `dotnet --version` phải là:

```text
9.0.301
```

Cài EF CLI:

```powershell
dotnet tool install --global dotnet-ef --version 9.*
```

Nếu đã cài:

```powershell
dotnet tool update --global dotnet-ef --version 9.*
```

## 2. Cấu hình project

### global.json

```json
{
  "sdk": {
    "version": "9.0.301",
    "rollForward": "latestPatch",
    "allowPrerelease": false
  }
}
```

### Directory.Build.props

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>
</Project>
```

### Domain project

`ContractManagement.Domain.csproj` cần:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
</Project>
```

Lý do: `ApplicationUser` kế thừa `IdentityUser`.

### Infrastructure project

Các package chính:

```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.6" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.6" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.6" />
```

### Web project

```xml
<PackageReference Include="MudBlazor" Version="9.5.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.6" />
```

## 3. Connection string

### Windows Authentication

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ContractManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### SQL Login

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SERVER;Database=ContractManagementDb;User Id=USER;Password=PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

Không commit mật khẩu production vào Git.

Dùng user-secrets:

```powershell
dotnet user-secrets init --project src/ContractManagement.Web

dotnet user-secrets set `
  "ConnectionStrings:DefaultConnection" `
  "Server=..." `
  --project src/ContractManagement.Web
```

## 4. Restore và build

```powershell
dotnet restore
dotnet build
```

## 5. Migration

Tạo migration:

```powershell
dotnet ef migrations add InitialCreate `
  --project src/ContractManagement.Infrastructure `
  --startup-project src/ContractManagement.Web
```

Xem SQL script:

```powershell
dotnet ef migrations script `
  --project src/ContractManagement.Infrastructure `
  --startup-project src/ContractManagement.Web
```

Cập nhật database:

```powershell
dotnet ef database update `
  --project src/ContractManagement.Infrastructure `
  --startup-project src/ContractManagement.Web
```

## 6. Chạy ứng dụng

```powershell
dotnet run --project src/ContractManagement.Web
```

## 7. Cấu hình Interactive Server

Trong `Program.cs` cần:

```csharp
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();
```

và:

```csharp
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
```

Trong `_Imports.razor` nên có:

```razor
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.JSInterop
@using MudBlazor
@using static Microsoft.AspNetCore.Components.Web.RenderMode
```

Trong `App.razor`:

```razor
<HeadOutlet @rendermode="InteractiveServer" />
<Routes @rendermode="InteractiveServer" />
```

Nếu Razor không nhận diện `InteractiveServer`, dùng tên đầy đủ:

```razor
<Routes @rendermode="Microsoft.AspNetCore.Components.Web.RenderMode.InteractiveServer" />
```

## 8. Sửa lỗi DatabaseSeeder.CreateScope

Lỗi:

```text
IServiceProvider does not contain a definition for CreateScope
```

Thêm:

```csharp
using Microsoft.Extensions.DependencyInjection;
```

Có thể dùng:

```csharp
await using var scope = serviceProvider.CreateAsyncScope();
```

## 9. Sửa lỗi Razor `@code`

Lỗi thường gặp:

```text
RZ2005: The 'code' directive must appear at the start of the line
CS0106: public is not valid for this item
CS0103: get/set does not exist
```

Nguyên nhân: file `.razor` bị dồn một dòng.

Sửa bằng cách để `@code` ở dòng riêng:

```razor
<MudPaper>
    Nội dung
</MudPaper>

@code {
    [Parameter]
    public string Title { get; set; } = string.Empty;
}
```

Các file cần đặc biệt kiểm tra:

- `Components/Shared/PageHeader.razor`.
- `Components/Shared/StatCard.razor`.
- `Components/Shared/SignaturePad.razor`.
- `Components/Pages/Account/Login.razor`.

## 10. Sửa lỗi MudTextField không suy luận được kiểu

Lỗi:

```text
RZ10001: The type of component MudTextField cannot be inferred
```

Thêm kiểu:

```razor
<MudTextField T="string"
              Label="Họ tên"
              @bind-Value="_model.CustomerName" />
```

Với số tiền:

```razor
<MudNumericField T="decimal?"
                 Label="Giá trị hợp đồng"
                 @bind-Value="_model.ContractValue" />
```

## 11. Sửa lỗi component không tồn tại

Lỗi:

```text
Found markup element with unexpected name 'ContractTypeStep'
```

Chỉ dùng component khi đã có file:

```text
ContractTypeStep.razor
```

và namespace đã được import.

Nếu chưa tạo component, thay bằng markup trực tiếp trong `CreateContract.razor`.

Các tên từng gây lỗi:

- ContractTypeStep.
- CustomerInformationStep.
- TripInformationStep.
- ContractContentStep.
- ContractReview.
- RedirectToLogin.

## 12. Sửa lỗi Routes context trùng

Lỗi:

```text
RZ9999: child content uses the same parameter name 'context'
```

Không lồng `AuthorizeView` mà không đổi Context.

Cách ổn định:

```razor
<AuthorizeRouteView RouteData="@routeData"
                    DefaultLayout="@typeof(MainLayout)">
    <NotAuthorized Context="routeAuthContext">
        @if (routeAuthContext.User.Identity?.IsAuthenticated == true)
        {
            <p>Không có quyền.</p>
        }
        else
        {
            <RedirectToLogin />
        }
    </NotAuthorized>
</AuthorizeRouteView>
```

Nếu dùng `RedirectToLogin`, phải tạo file component tương ứng.

## 13. Sửa lỗi Login ReturnUrl

Nếu markup có:

```razor
<input type="hidden" name="ReturnUrl" value="@ReturnUrl" />
```

thì `@code` phải có:

```razor
[SupplyParameterFromQuery(Name = "returnUrl")]
public string? ReturnUrl { get; set; }
```

Không dùng `Name` trên `MudTextField`/`MudCheckBox` để submit form HTML truyền thống. Dùng `<input>` thật hoặc binding vào model rồi gọi endpoint/service.

## 14. Bottom navigation

Nếu `MudBottomNavigation` hoặc `MudBottomNavigationItem` không được nhận diện, không cố thêm namespace không tồn tại.

Dùng:

- `MudPaper`.
- `MudStack`.
- `MudIconButton`.
- `MudButton` chứa `MudIcon` cho nút tạo mới.

Nếu `MudFab Icon="..."` bị MUD0002, thay bằng:

```razor
<MudButton Color="Color.Primary"
           Variant="Variant.Filled"
           Class="mobile-create-button">
    <MudIcon Icon="@Icons.Material.Filled.Add" />
</MudButton>
```

## 15. Stepper

Nếu MudBlazor analyzer báo `Linear` hoặc `Label` không hợp lệ, thay `MudStepper` bằng stepper thủ công:

- `_currentStep`.
- Mảng `_steps`.
- `@switch (_currentStep)`.
- Nút Quay lại/Tiếp tục.

Cách này ít phụ thuộc phiên bản MudBlazor.

## 16. Xóa cache build

Khi sửa nhiều file Razor, đóng Visual Studio rồi chạy:

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

## 17. Production

- Bật HTTPS.
- Reverse proxy IIS/Nginx phải hỗ trợ WebSocket.
- Nếu chạy nhiều instance, cần sticky session hoặc Azure SignalR.
- Không lưu file nhạy cảm trực tiếp trong `wwwroot`.
- Backup SQL Server định kỳ và kiểm tra restore.
- Thay mật khẩu seed.
- Bật antiforgery cho login/logout.
- Cấu hình giới hạn upload.
- Ghi log lỗi tập trung.
