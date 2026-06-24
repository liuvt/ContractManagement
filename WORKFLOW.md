# Workflow nghiệp vụ và quy trình cập nhật dự án

## 1. Workflow tài khoản tài xế

```text
Admin tạo user
  -> Gán role Driver
  -> Tạo DriverProfile
  -> Tài xế đăng nhập lần đầu
  -> Đổi mật khẩu
  -> Tạo chữ ký cá nhân
  -> Có thể tạo hợp đồng
```

### Quy tắc

- Mỗi user chỉ có một DriverProfile.
- Tài xế bị `IsActive=false` không được đăng nhập.
- Chữ ký tài xế có version.
- Mỗi tài xế chỉ có một chữ ký hiện hành.
- Không xóa lịch sử chữ ký cũ.

## 2. Workflow tạo hợp đồng

```text
Driver đăng nhập
  -> Chọn loại hợp đồng
  -> Chọn mẫu hợp đồng
  -> Tìm hoặc tạo khách hàng
  -> Nhập thông tin chuyến đi
  -> Nhập giá trị/nội dung
  -> Lưu Draft
  -> Tạo snapshot + hash
  -> Chuyển WaitingCustomerSignature
```

### Tự động lưu

Nên lưu Draft tại các thời điểm:

- Sau khi chọn loại hợp đồng.
- Sau khi lưu thông tin khách hàng.
- Sau khi nhập chuyến đi.
- Trước khi mở màn hình ký.

Không chờ đến bước cuối mới lưu toàn bộ dữ liệu.

## 3. Workflow khách hàng ký

```text
Hiển thị snapshot hợp đồng
  -> Khách đọc nội dung
  -> Tick đồng ý
  -> Ký trên canvas
  -> Trình duyệt xin quyền GPS
  -> Gửi chữ ký + metadata
  -> Server tạo ServerSignedAt
  -> Kiểm tra ContractHash
  -> Lưu ContractSignature Customer
  -> Chuyển CustomerSigned
```

### Dữ liệu cần lưu

- SignerName.
- SignerPhone.
- SignatureFileUrl.
- SignatureVectorJson.
- SignatureHash.
- ContractHashAtSigning.
- DeviceSignedAt.
- ServerSignedAt.
- Latitude.
- Longitude.
- LocationAccuracy.
- LocationAddress hoặc LocationError.
- IpAddress.
- DeviceName.
- OperatingSystem.
- BrowserName.
- ConsentText.

### Quy tắc GPS

- Website phải chạy HTTPS.
- Người dùng phải cấp quyền.
- Không giả lập vị trí nếu không lấy được GPS.
- Lưu rõ lỗi từ chối quyền hoặc timeout.
- `ServerSignedAt` là thời gian chính thức.

## 4. Workflow tài xế xác nhận

```text
Contract đã CustomerSigned
  -> Lấy DriverSignature IsCurrent
  -> Snapshot chữ ký vào hợp đồng
  -> Lưu ContractSignature Driver
  -> Kiểm tra hash
  -> Sinh PDF
  -> Chuyển Completed
  -> Ghi audit log
```

Toàn bộ bước hoàn tất nên chạy trong transaction.

## 5. Workflow hủy hoặc sửa hợp đồng

### Draft

Có thể sửa hoặc soft delete theo quyền.

### Đã có chữ ký

Không sửa trực tiếp nội dung đã ký.

Cách xử lý:

1. Chuyển `Invalidated` hoặc `Cancelled`.
2. Lưu lý do.
3. Ghi audit log.
4. Tạo hợp đồng mới nếu cần.
5. Liên kết hợp đồng mới với hợp đồng cũ trong giai đoạn mở rộng.

## 6. Workflow login/logout

```text
Mở trang protected
  -> Chưa đăng nhập
  -> Chuyển /account/login?returnUrl=...
  -> POST /account/login
  -> Identity kiểm tra tài khoản
  -> Kiểm tra IsActive
  -> Tạo cookie
  -> Admin về /admin/dashboard
  -> Driver về /driver/dashboard
```

Logout:

```text
Bấm Đăng xuất
  -> POST /account/logout
  -> SignInManager.SignOutAsync
  -> Xóa cookie
  -> /account/login
```

### Quy tắc

- Không logout bằng GET.
- Production phải có antiforgery.
- `ReturnUrl` chỉ chấp nhận local URL.
- Không lưu mật khẩu trên trình duyệt.
- Không dùng `Name` của MudTextField để submit HTML form.

## 7. Workflow UI mobile

```text
Driver Dashboard
  -> Tạo hợp đồng
  -> Bước 1: Loại hợp đồng
  -> Bước 2: Khách hàng
  -> Bước 3: Chuyến đi
  -> Bước 4: Nội dung
  -> Bước 5: Kiểm tra
  -> Bước 6: Khách ký
  -> Hoàn tất
```

### Quy tắc giao diện

- Mobile-first.
- Form một cột trên màn hình nhỏ.
- Không dùng bảng rộng trên mobile.
- Danh sách dùng card.
- Bottom nav không phụ thuộc component không có trong MudBlazor package.
- Vùng ký phải có `touch-action: none`.
- Nút xác nhận phải disable khi đang submit.

## 8. Workflow phát triển tính năng

1. Xác định use case và role được phép dùng.
2. Thêm/sửa entity trong Domain.
3. Cập nhật enum nếu cần.
4. Cập nhật Fluent API trong Infrastructure.
5. Kiểm tra index/check constraint/delete behavior.
6. Tạo migration.
7. Tạo DTO/interface/service trong Application.
8. Viết query có `AsNoTracking` cho màn hình đọc.
9. Viết UI trong Web.
10. Thêm authorization ở UI và service/query.
11. Thêm audit log.
12. Thêm unit/integration test.
13. Build sạch.
14. Cập nhật toàn bộ file Markdown.

## 9. Workflow thay đổi database

Tạo migration:

```powershell
dotnet ef migrations add TenMigration `
  --project src/ContractManagement.Infrastructure `
  --startup-project src/ContractManagement.Web
```

Xem script:

```powershell
dotnet ef migrations script `
  --project src/ContractManagement.Infrastructure `
  --startup-project src/ContractManagement.Web
```

Update:

```powershell
dotnet ef database update `
  --project src/ContractManagement.Infrastructure `
  --startup-project src/ContractManagement.Web
```

Không sửa migration đã chạy production. Tạo migration mới để điều chỉnh.

## 10. Workflow xử lý lỗi build Razor

### Bước 1: Đọc lỗi đầu tiên

Ưu tiên sửa lỗi đầu tiên trong log; nhiều lỗi sau thường là lỗi dây chuyền.

### Bước 2: Kiểm tra file bị dồn một dòng

Đặc biệt:

- PageHeader.razor.
- StatCard.razor.
- SignaturePad.razor.
- Login.razor.

### Bước 3: Kiểm tra `_Imports.razor`

Phải có namespace Routing, Authorization, JSInterop, MudBlazor và Shared.

### Bước 4: Kiểm tra generic MudBlazor

- `MudTextField T="string"`.
- `MudSelect T="Guid?"` hoặc kiểu đúng.
- `MudNumericField T="decimal?"`.
- `MudList T="string"` nếu version yêu cầu.

### Bước 5: Kiểm tra component có tồn tại

Không sử dụng tên component nếu chưa có file `.razor` tương ứng.

### Bước 6: Kiểm tra context lồng nhau

Đặt tên riêng:

```razor
<NotAuthorized Context="routeAuthContext">
```

### Bước 7: Kiểm tra API MudBlazor

Nếu analyzer báo `MUD0002`, không bỏ qua. Thay thuộc tính hoặc thay component bằng cấu trúc ổn định hơn.

### Bước 8: Xóa cache

```powershell
dotnet clean

Get-ChildItem -Path . -Include bin,obj -Recurse -Directory |
    Remove-Item -Recurse -Force

dotnet restore
dotnet build
```

## 11. Workflow release

### Development

- Build Debug.
- Dùng SQL Server development.
- Seed admin chỉ trong môi trường dev.

### Staging

- Dùng database riêng.
- Chạy migration script có review.
- Test trên Safari iPhone và Chrome Android.
- Test mất mạng, reconnect SignalR và GPS bị từ chối.

### Production

1. Backup database.
2. Apply migration.
3. Publish Release.
4. Deploy IIS/Nginx.
5. Kiểm tra WebSocket.
6. Kiểm tra HTTPS.
7. Test login/logout.
8. Test tạo hợp đồng.
9. Test ký trên mobile.
10. Test sinh PDF.
11. Kiểm tra log và backup.

## 12. Checklist trước khi merge

- `dotnet build` thành công.
- Không còn error Razor.
- Không còn MUD0002 liên quan component đang dùng.
- Không dồn file Razor thành một dòng.
- Không có component giả/chưa tạo.
- Không commit connection string production.
- Không commit mật khẩu thật.
- Migration đã được review.
- Authorization được kiểm tra ở service/query.
- Audit log được thêm cho thao tác quan trọng.
- README/PROJECT_OVERVIEW/SETUP/WORKFLOW đã cập nhật.

## 13. Checklist trước production

- Đã đổi mật khẩu admin seed.
- HTTPS hoạt động.
- Cookie Secure/HttpOnly.
- Antiforgery cho login/logout.
- Driver không truy cập được hợp đồng người khác.
- Upload kiểm tra size/MIME/hash.
- GPS xử lý trường hợp từ chối quyền.
- Hợp đồng Completed không sửa trực tiếp.
- SQL backup đã test restore.
- WebSocket hoạt động sau reverse proxy.
- Storage chữ ký/CCCD không public trực tiếp.
