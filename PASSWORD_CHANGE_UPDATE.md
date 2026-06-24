# Cập nhật luồng bắt buộc đổi mật khẩu

## Hành vi mới

1. Tài khoản Driver vừa tạo luôn có `MustChangePassword = true`.
2. Sau khi đăng nhập, Driver vẫn được điều hướng đến dashboard nhưng một overlay toàn màn hình xuất hiện trên mọi trang.
3. Overlay không có nút đóng và chặn toàn bộ thao tác với giao diện phía sau.
4. Driver chỉ nhập:
   - Mật khẩu mới
   - Nhập lại mật khẩu mới
5. Không yêu cầu mật khẩu cũ. Hệ thống dùng password reset token nội bộ của ASP.NET Identity.
6. Sau khi đổi thành công, `MustChangePassword` được đặt về `false`, phiên cũ được đăng xuất và người dùng đăng nhập lại bằng mật khẩu mới.
7. Admin có thể:
   - Reset mật khẩu về mật khẩu tạm và buộc đổi lại.
   - Chỉ bật yêu cầu đổi mật khẩu cho tài khoản mà không cần biết mật khẩu cũ.

## File chính đã thay đổi

- `src/ContractManagement.Web/Components/Shared/ForcePasswordChangeOverlay.razor`
- `src/ContractManagement.Web/Components/Layout/MainLayout.razor`
- `src/ContractManagement.Web/Components/Pages/Account/ChangePassword.razor`
- `src/ContractManagement.Web/Components/Pages/Admin/DriverAccounts/Create.razor`
- `src/ContractManagement.Web/Program.cs`

## Lưu ý kiểm thử

Môi trường đóng gói không có .NET SDK nên chưa chạy được `dotnet build`. Hãy chạy tại máy phát triển:

```bash
dotnet restore
dotnet build ContractManagement.slnx
```
