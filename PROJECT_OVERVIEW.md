# Tổng quan dự án ContractManagement

## 1. Mục tiêu

Xây dựng hệ thống quản lý hợp đồng điện tử sử dụng được trên điện thoại, tablet và máy tính thông qua trình duyệt.

Tài xế là người bắt đầu và kết thúc quy trình:

1. Đăng nhập tài khoản.
2. Chọn loại hợp đồng.
3. Nhập thông tin khách hàng.
4. Nhập nội dung hợp đồng/chuyến đi.
5. Cho khách hàng kiểm tra nội dung.
6. Khách hàng ký trực tiếp trên điện thoại.
7. Hệ thống lưu thời gian server và vị trí trình duyệt.
8. Tài xế xác nhận bằng chữ ký cá nhân đã lưu.
9. Hợp đồng được khóa, sinh PDF và lưu audit log.

Khách hàng không cần tài khoản.

## 2. Kiến trúc

```text
Safari/Chrome trên mobile
        |
        | HTTPS + SignalR
        v
Blazor Web App Interactive Server
        |
        ├── ASP.NET Core Identity
        ├── MudBlazor UI
        ├── Application Services
        ├── EF Core
        ├── SQL Server
        ├── File/Object Storage
        └── PDF Service
```

### Domain

Chứa mô hình dữ liệu và enum, không chứa code UI.

### Application

Chứa use case, DTO, interface và validation nghiệp vụ.

### Infrastructure

Chứa EF Core, SQL Server, Identity persistence, storage, PDF và external services.

### Web

Chứa Razor component, layout, endpoint login/logout, theme MudBlazor và JavaScript Interop.

## 3. Phiên bản kỹ thuật

| Thành phần | Phiên bản |
|---|---:|
| .NET SDK | 9.0.301 |
| Target framework | net9.0 |
| EF Core | 9.0.6 |
| Identity EF Core | 9.0.6 |
| MudBlazor | 9.5.0 |
| Database | SQL Server |

`global.json` khóa SDK `9.0.301` với `rollForward=latestPatch`.

## 4. Vai trò

### Admin

- CRUD tài xế.
- Khóa/mở tài khoản.
- Gán role.
- Quản lý loại hợp đồng.
- Quản lý mẫu hợp đồng.
- Xem toàn bộ hợp đồng.
- Xem vị trí/thời gian ký.
- Xem audit log.
- Hủy hoặc vô hiệu hóa hợp đồng theo quyền.

### Driver

- Đăng nhập bằng Identity.
- Quản lý hồ sơ cá nhân.
- Tạo và cập nhật chữ ký cá nhân.
- Tạo khách hàng.
- Tạo hợp đồng.
- Thu chữ ký khách hàng.
- Xác nhận hợp đồng bằng chữ ký tài xế.
- Xem hợp đồng do mình tạo.

### Customer

- Không có tài khoản.
- Chỉ xem hợp đồng đang được tài xế mở.
- Tick đồng ý nội dung.
- Ký trực tiếp trên canvas.
- Có thể xác thực OTP trong giai đoạn mở rộng.

## 5. Module dữ liệu

### Identity và Driver Profile

- `ApplicationUser` kế thừa `IdentityUser`.
- `DriverProfile` quan hệ 1-1 với user.
- `EmployeeCode` và `DriverCode` dùng tra cứu nhanh.
- Tài khoản có `IsActive`, `MustChangePassword`, `AreaCode`.

### Driver Signature

- Lưu nhiều version.
- Mỗi tài xế chỉ có một chữ ký `IsCurrent=true`.
- Không ghi đè chữ ký cũ.
- Khi ký hợp đồng phải snapshot chữ ký vào hợp đồng.

### Customer

- Tên, số điện thoại, CCCD, địa chỉ.
- Có thể trùng số điện thoại trong một số trường hợp thực tế.
- Không tự động gộp khách chỉ theo họ tên.

### Contract Type

Cấu hình yêu cầu theo loại hợp đồng:

- Bắt buộc chữ ký khách.
- Bắt buộc chữ ký tài xế.
- Bắt buộc GPS.
- Bắt buộc OTP.
- Bắt buộc giấy tờ khách hàng.

### Contract Template

- Lưu HTML/CSS và version.
- Có thời gian hiệu lực.
- Hợp đồng đã tạo phải giữ snapshot, không phụ thuộc template hiện tại.

### Contract

Lưu:

- Mã hợp đồng.
- Loại và mẫu hợp đồng.
- Khách hàng.
- Tài xế.
- Khu vực.
- Xe.
- Điểm đón/đến.
- Thời gian.
- Giá trị hợp đồng.
- Snapshot nội dung.
- JSON dữ liệu.
- Hash.
- PDF URL.
- Các mốc trạng thái.

### Contract Signature

Lưu:

- Bên ký: Customer hoặc Driver.
- Ảnh/vector chữ ký.
- Hash chữ ký.
- Hash hợp đồng tại thời điểm ký.
- `DeviceSignedAt`.
- `ServerSignedAt`.
- Latitude/Longitude/Accuracy.
- IP, device, OS, browser, app version.
- Nội dung đồng ý.

### Attachment

- CCCD mặt trước/sau.
- Ảnh hợp đồng.
- Biên nhận.
- File khác.

### Audit Log

Ghi nhận:

- CREATE_CONTRACT.
- UPDATE_CONTRACT.
- SUBMIT_FOR_SIGNATURE.
- CUSTOMER_SIGNED.
- DRIVER_SIGNED.
- COMPLETE_CONTRACT.
- GENERATE_PDF.
- CANCEL_CONTRACT.
- INVALIDATE_CONTRACT.
- VIEW_CONTRACT.
- DOWNLOAD_CONTRACT.

## 6. Trạng thái hợp đồng

```text
Draft
  -> WaitingCustomerSignature
  -> CustomerSigned
  -> WaitingDriverConfirmation
  -> Completed
```

Nhánh ngoại lệ:

```text
Cancelled
Expired
Invalidated
```

Sau khi `Completed`, hợp đồng không được sửa trực tiếp. Nếu cần thay đổi, tạo phiên bản mới hoặc vô hiệu hóa hợp đồng cũ theo nghiệp vụ.

## 7. Fluent API và index

### DriverProfiles

- `UserId` unique.
- `DriverCode` unique filtered theo `IsDeleted=0`.
- `VehiclePlate` index filtered.

### DriverSignatures

- `DriverId + Version` unique.
- `DriverId + IsCurrent` unique filtered.
- `SignatureHash` index.

### Customers

- `PhoneNumber`.
- `CitizenId` filtered.
- `FullName + PhoneNumber`.
- `CreatedByDriverId + CreatedAt DESC`.

### ContractTypes

- `Code` unique filtered.
- `IsActive + Name`.

### ContractTemplates

- `ContractTypeId + Version` unique.
- `ContractTypeId + IsActive + EffectiveFrom DESC`.

### Contracts

- `ContractNumber` unique.
- `DriverId + Status + CreatedAt DESC`.
- `AreaCode + CreatedAt DESC`.
- `CustomerId + CreatedAt DESC`.
- `Status + CreatedAt DESC`.
- `Status + CompletedAt` filtered.
- `ContractTypeId + CreatedAt DESC`.
- `ContractHash` filtered.

### ContractSignatures

- `ContractId + Party` unique filtered.
- `SignatureHash`.
- `ContractHashAtSigning`.
- `ServerSignedAt DESC`.

### ContractAttachments

- `ContractId + AttachmentType`.
- `FileHash` filtered.

### ContractAuditLogs

- `ContractId + CreatedAt DESC`.
- `UserId + CreatedAt DESC`.
- `Action + CreatedAt DESC`.

## 8. Quy tắc bảo mật

- Chỉ kết nối SQL Server từ server, không từ trình duyệt.
- Dùng HTTPS.
- Cookie HttpOnly, Secure và SameSite phù hợp.
- Logout dùng POST.
- Login/logout production phải có antiforgery.
- Tài xế chỉ được truy vấn `DriverId == currentUserId`.
- Không chỉ dựa vào `AuthorizeView`; service và query vẫn phải kiểm tra quyền.
- Không lưu OTP plaintext.
- Không lưu mật khẩu hoặc token trong localStorage.
- Không tin tuyệt đối thời gian thiết bị.
- Không xóa cứng hợp đồng đã phát sinh.

## 9. UI và layout

### Desktop

- MudAppBar.
- Responsive Drawer.
- Menu theo role.
- Bảng hoặc card tùy màn hình.

### Mobile

- AppBar nhỏ.
- Nội dung toàn chiều rộng.
- Bottom navigation tự dựng bằng MudPaper/MudStack.
- Form một cột.
- Danh sách hợp đồng dạng card.
- Vùng ký có `touch-action: none`.

### Layout

- `MainLayout`: Admin và Driver sau đăng nhập.
- `AuthLayout`: trang đăng nhập.
- Signing screen nên dùng layout riêng, không hiển thị menu tài xế.

## 10. Trạng thái scaffold hiện tại

Đã có:

- Solution 4 tầng.
- Entity và enum.
- DbContext và Fluent API.
- Index và check constraint.
- Identity roles Admin/Driver.
- Seed admin.
- Theme MudBlazor.
- Layout responsive.
- Các trang mẫu Admin/Driver.
- Canvas chữ ký và JS Geolocation.

Cần tiếp tục:

- Hoàn thiện command/query/service.
- Hoàn thiện form create contract kết nối DB.
- Tạo flow ký khách hàng hoàn chỉnh.
- Lưu file chữ ký ra storage.
- Snapshot và hash hợp đồng.
- Sinh PDF.
- Audit service.
- OTP nếu cần.
- Unit/integration test.
- PWA.

## 11. Quy ước Razor/MudBlazor đã thống nhất

- Mỗi `@code` phải bắt đầu ở đầu dòng.
- Không dồn file `.razor` thành một dòng.
- `MudTextField` phải có `T="string"` hoặc kiểu phù hợp.
- Không gọi component chưa có file `.razor`.
- `_Imports.razor` phải import Routing, Authorization, JSInterop, MudBlazor và Shared namespace.
- `Routes.razor` không được lồng child content có cùng tên `context`.
- Nếu lồng `AuthorizeView`, đặt `Context="authContext"` hoặc đơn giản hóa bằng `NotAuthorized Context="routeAuthContext"`.
- `RedirectToLogin` chỉ dùng khi file component đã được tạo.
- Login HTTP form không dùng `Name` trên MudTextField; dùng input HTML thật hoặc Identity endpoint chuẩn.
- Không phụ thuộc `MudBottomNavigation` nếu package không hỗ trợ.
- Không phụ thuộc `MudStepper` API cũ; có thể dùng stepper thủ công để ổn định.
