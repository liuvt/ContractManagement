# Cập nhật kết nối bằng DbContextFactory

Đã chuyển các phần nghiệp vụ không còn giữ `ApplicationDbContext` theo lifetime của Blazor Server circuit:

- `DriverAccountService`
- `DriverProfileService`
- `PasswordChangedHandler`
- `DatabaseSeeder`

Mỗi thao tác tạo một DbContext riêng bằng:

```csharp
await using var dbContext =
    await _dbContextFactory.CreateDbContextAsync(cancellationToken);
```

`Program.cs` tiếp tục đăng ký:

```csharp
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});
```

## Kiểm tra trên máy phát triển

```bash
dotnet clean
dotnet restore
dotnet build
dotnet ef database update \
  --project src/ContractManagement.Infrastructure \
  --startup-project src/ContractManagement.Web
```

Lưu ý: môi trường đóng gói hiện tại không có .NET SDK nên chưa chạy được `dotnet build` tại đây.
