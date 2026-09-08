# Đưa demo quản lý thư viện lên Render Free

Ứng dụng ASP.NET Core .NET 10 + SQLite. Bộ này đã có Dockerfile và render.yaml.
Không tải file ZIP trực tiếp lên Render: giải nén và đưa các file lên GitHub trước.

## 1. Tạo kho mã nguồn GitHub

1. Đăng nhập https://github.com và tạo repository mới, có thể chọn Private.
2. Chọn Upload files (hoặc Add file > Upload files).
3. Tải NỘI DUNG thư mục HeThongQLTV đã giải nén lên, rồi Commit changes.
4. Kiểm tra ngay trang đầu repository có Dockerfile, render.yaml, NuGet.Config và thư mục HeThongQLTV chứa file .csproj.
5. Không đưa .env.local, khóa API hoặc cơ sở dữ liệu cá nhân lên repository.

## 2. Triển khai

1. Đăng nhập https://dashboard.render.com bằng tài khoản của bạn.
2. Chọn New > Web Service, kết nối GitHub và chọn repository vừa tạo.
3. Đặt tên dịch vụ tùy ý; chọn Language/Runtime: Docker.
4. Root Directory để trống; Dockerfile Path: ./Dockerfile nếu giao diện yêu cầu.
5. Chọn Instance Type: Free. Không chọn gói trả phí hoặc thêm disk.
6. Không cần nhập Build Command hay Start Command: Dockerfile đã xử lý.
7. Chọn Deploy Web Service. Đợi trạng thái Live rồi mở địa chỉ https://<ten-dich-vu>.onrender.com do Render cấp.

Dockerfile đã bật SeedDemo=true, tự tạo dữ liệu mẫu ở lần khởi động đầu tiên.
Có thể dùng New > Blueprint với render.yaml thay cho cấu hình thủ công ở trên.

## 3. Đăng nhập và kiểm tra

Mật khẩu mẫu của cả ba tài khoản: ThuVien@123

- admin: quản trị viên.
- thuthu: thủ thư.
- docgia: độc giả.

Thử đăng nhập, mở danh mục sách, thêm độc giả và tạo phiếu mượn.
Đây là tài khoản demo công khai; chỉ dùng dữ liệu giả để trình diễn.

## Giới hạn cần biết

- Render Free ngủ sau 15 phút không có truy cập; lần mở tiếp theo có thể mất khoảng một phút.
- SQLite nằm trên ổ tạm. Dữ liệu nhập thêm sẽ mất khi dịch vụ ngủ, khởi động lại hoặc triển khai lại; dữ liệu mẫu được tạo lại.
- Bộ triển khai không mang theo file database trong ZIP gốc; dữ liệu gốc vẫn nằm nguyên trong ZIP gốc.
- Chatbot hiện cần OPENAI_API_KEY. Bản triển khai này không cấu hình khóa, nên chat báo chưa cấu hình; các chức năng thư viện vẫn chạy. Không cần bật API để trình diễn thư viện.
- Gói miễn phí có hạn mức sử dụng: https://render.com/docs/free
- Hướng dẫn Render: https://render.com/docs/web-services

## Chạy bằng Docker trên máy (nếu đã cài Docker)

```powershell
docker build -t qltv-demo .
docker run --rm -p 10000:10000 qltv-demo
```

Mở http://localhost:10000. Dữ liệu trong container này cũng là dữ liệu demo tạm.
