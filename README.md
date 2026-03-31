# MultiChat
💬 C# WinForms Chat App - 人機介面期中考
# 💬 MultiChat — C# WinForms Chat Application

> Dự án chat nhiều người theo thời gian thực, xây dựng bằng **C# WinForms .NET 8**
> Được phát triển từ bài kiểm tra giữa kỳ môn **Giao diện Người-Máy (人機介面)**
> Trường: **Justice University (JUST)** — Taiwan 🇹🇼

---

## 📸 Giao diện

| Dark Theme | WhatsApp Theme | Zalo Theme |
|:---:|:---:|:---:|
| 🌙 Tối | 💚 Xanh lá | 💙 Xanh dương |

---

## ✨ Tính năng đầy đủ

### 💬 Chat cơ bản
- ✅ Gửi / nhận tin nhắn văn bản theo thời gian thực (TCP)
- ✅ Tin nhắn **trái / phải / giữa** (mình = phải, người khác = trái, hệ thống = giữa)
- ✅ Hiển thị **giờ gửi** (HH:mm) cạnh mỗi tin nhắn
- ✅ Nhấn **Enter** để gửi, **Shift+Enter** để xuống dòng
- ✅ **Danh sách người dùng online** ở panel bên phải

### 😊 Media & Tệp
- ✅ **Emoji Picker** — 60 emoji chia 4 nhóm
- ✅ **Gửi hình ảnh** (JPG/PNG/GIF, hiển thị inline)
- ✅ **Gửi file** đính kèm (tối đa 2MB, tự lưu vào máy nhận)
- ✅ **Chụp màn hình** rồi gửi ngay

### 👤 Người dùng
- ✅ **Avatar tự động** — vòng tròn màu với chữ cái đầu tên
- ✅ **Đang nhập...** (typing indicator, tắt sau 3 giây)
- ✅ **Online / Offline indicator** (chấm xanh / đỏ)

### 🔵 Tương tác nâng cao
- ✅ **Đã đọc / Chưa đọc** — `✓ 已送出` → `✓✓ 已讀`
- ✅ **Thu hồi tin nhắn** — nút toolbar hoặc chuột phải
- ✅ **Tin nhắn riêng (Private Message)** — double-click tên người dùng

### 🎨 Giao diện
- ✅ **6 theme màu** đổi realtime: 🌙 Dark · ☀️ Light · 💙 Zalo · 💚 WhatsApp · 🎮 Discord · 🌅 Sunset
- ✅ **Chỉnh cỡ chữ** (8–16pt) bằng thanh kéo
- ✅ **Link bấm được** trong nội dung chat

### 🔔 Thông báo
- ✅ **Thông báo tray** (Windows balloon) khi có tin mới
- ✅ **Đếm tin chưa đọc** hiện trên tiêu đề `(3) MultiChat`
- ✅ **Âm thanh thông báo** (bật/tắt)
- ✅ **Minimize xuống System Tray**

### 🛠️ Tiện ích
- ✅ **Tìm kiếm tin nhắn** (highlight, prev/next)
- ✅ **Xuất lịch sử chat** ra `.txt` hoặc `.rtf`
- ✅ **Xoá lịch sử** chat
- ✅ **Nút Gọi thoại 📞 / Gọi video 🎥** (UI, chờ VoIP server)

---

## 🏗️ Cấu trúc dự án

```
MultiChat/
│
├── MultiChatServer/ ← Chạy TRƯỚC
│ ├── ChatServer.cs ← TCP server, broadcast tới tất cả
│ ├── ClientSession.cs ← Quản lý từng client kết nối
│ ├── FrmServer.cs ← Form hiển thị log + danh sách
│ ├── FrmServer.Designer.cs
│ ├── ProtocolHelper.cs ← Build các lệnh giao tiếp
│ └── MultiChatServer.sln
│
├── MultiChatClient/ ← Chạy SAU (nhiều cửa sổ)
│ ├── FrmClient.cs ← Logic toàn bộ UI (~700 dòng)
│ ├── FrmClient.Designer.cs ← Khai báo và layout tất cả control
│ ├── ChatClient.cs ← TCP client + tất cả events
│ ├── ProtocolHelper.cs ← Build/parse tất cả lệnh
│ ├── ChatTheme.cs ← 6 theme màu sắc
│ ├── SearchForm.cs ← Form tìm kiếm tin nhắn
│ ├── Program.cs
│ └── MultiChatClient.sln
│
└── README.md
```

---

## 🔌 Giao thức TCP (Protocol)

Mọi gói tin kết thúc bằng ký tự `\x04` để tránh TCP packet fragmentation.

| Lệnh | Chiều | Ý nghĩa |
|------|:---:|---------|
| `JOIN\|name` | C → S | Đăng nhập vào phòng |
| `MSG\|name\|text` | C ↔ S | Tin nhắn văn bản |
| `IMG\|name\|base64` | C ↔ S | Hình ảnh (Base64) |
| `FILE\|name\|filename\|base64` | C ↔ S | File đính kèm |
| `TYPING\|name` | C → S | Đang nhập... |
| `PM\|sender\|recipient\|text` | C ↔ S | Tin nhắn riêng |
| `RECALL\|sender\|msgId` | C → S | Thu hồi tin nhắn |
| `SYS\|message` | S → C | Thông báo hệ thống |

---

## ⚙️ Yêu cầu hệ thống

| Yêu cầu | Chi tiết |
|---------|---------|
| **OS** | Windows 10 / 11 (x64) |
| **IDE** | Visual Studio 2022 / 2026 |
| **SDK** | .NET 8 (Windows) |
| **Workload** | .NET desktop development |

---

## 🚀 Hướng dẫn cài đặt & chạy

### Bước 1 — Clone dự án

```bash
git clone [https://github.com/YOUR_USERNAME/MultiChat.git](https://github.com/cuongspvlk34a/MultiChat)
cd MultiChat
```

### Bước 2 — Mở và Build Server

1. Mở thư mục `MultiChatServer`
2. Double-click `MultiChatServer.sln` → Visual Studio mở lên
3. Nhấn **Ctrl+Shift+B** để Build
4. Nhấn **F5** để chạy Server

> Server mặc định lắng nghe **IP: 127.0.0.1 | Port: 5000**
> Nhấn **Start** trong cửa sổ Server để bắt đầu

### Bước 3 — Mở và Build Client

1. Mở thư mục `MultiChatClient`
2. Double-click `MultiChatClient.sln`
3. Nhấn **Ctrl+Shift+B** → Build
4. Nhấn **F5** để chạy

### Bước 4 — Kết nối và chat

1. Điền **Server IP**: `127.0.0.1` (nếu cùng máy)
2. Điền **Port**: `5000`
3. Điền **暱稱** (tên hiển thị)
4. Nhấn **連線** → bắt đầu chat!

> 💡 Mở nhiều cửa sổ Client để test chat nhiều người

---

## 🌐 Chat qua mạng LAN

Để chat giữa nhiều máy tính trong cùng mạng:

1. Trên máy **chạy Server**: mở **Command Prompt** gõ `ipconfig`
→ Tìm **IPv4 Address** (VD: `192.168.1.100`)

2. Trên máy **chạy Client**: điền IP đó vào ô **Server IP**

3. Đảm bảo **Firewall** không chặn port 5000:
```
# Mở PowerShell với quyền Admin và chạy:
netsh advfirewall firewall add rule name="MultiChat" dir=in action=allow protocol=TCP localport=5000
```

---

## 🎨 Hướng dẫn đổi theme

Trong thanh **Settings** (hàng thứ 3 từ trên):
- Dropdown **🎨 Theme** → chọn 1 trong 6 theme
- Theme đổi ngay lập tức, không cần restart

| Theme | Phong cách |
|-------|-----------|
| 🌙 Dark | Tím tối — mặc định |
| ☀️ Light | Trắng sáng |
| 💙 Zalo | Xanh dương |
| 💚 WhatsApp | Xanh lá |
| 🎮 Discord | Xám tối |
| 🌅 Sunset | Cam đỏ |

---

## 💬 Hướng dẫn các tính năng đặc biệt

### Gửi hình ảnh
1. Nhấn nút **🖼️** trên toolbar
2. Chọn file ảnh → tự động resize và gửi
3. Ảnh hiển thị inline trong chat

### Chụp màn hình
1. Nhấn nút **📷**
2. Chọn **[Có]** để chụp màn hình ngay
3. Chọn **[Không]** để chọn từ thư viện ảnh

### Tin nhắn riêng (Private Message)
1. **Double-click** vào tên người dùng trong danh sách bên phải
2. Nhập nội dung → nhấn **送出私訊**
3. PM hiển thị màu tím, chỉ người gửi/nhận thấy

### Thu hồi tin nhắn
- Nhấn nút **↩️** trên toolbar, hoặc
- **Chuột phải** vào vùng chat → **撤回最後一則訊息**

### Tìm kiếm
1. Nhấn **🔍 搜尋**
2. Gõ từ khoá → nhấn **▶ / ◀** để duyệt kết quả

### Xuất lịch sử
1. Nhấn **💾 匯出**
2. Chọn định dạng `.txt` hoặc `.rtf`
3. Lưu vào máy

---

## 📁 Thêm theme tuỳ chỉnh

Mở file `ChatTheme.cs` và thêm theme mới theo mẫu:

```csharp
public static readonly ChatTheme MyTheme = new()
{
Name = "🌸 Pink",
Background = Color.FromArgb(255, 240, 245),
Panel = Color.FromArgb(255, 250, 252),
Header = Color.FromArgb(255, 182, 193),
Accent = Color.FromArgb(255, 105, 180),
// ... các màu khác
};
```

Rồi thêm vào mảng `All`:
```csharp
public static ChatTheme[] All => new[] { Dark, Light, Zalo, WhatsApp, Discord, Sunset, MyTheme };
```

---

## 🔧 Cách mở rộng dự án

### Thêm lệnh protocol mới

**1. `ProtocolHelper.cs`** — thêm hàm Build:
```csharp
public static string BuildReaction(string sender, string msgId, string emoji)
=> $"REACT|{sender}|{msgId}|{emoji}";
```

**2. `ChatClient.cs`** — thêm event và xử lý trong `HandleMessage()`:
```csharp
public event Action<string, string, string>? ReactionReceived;

case "REACT":
// parse và invoke event
break;
```

**3. `FrmClient.cs`** — subscribe event và hiển thị UI

---

## 🐛 Xử lý lỗi thường gặp

| Lỗi | Nguyên nhân | Giải pháp |
|-----|------------|-----------|
| `連線失敗` | Server chưa chạy | Chạy Server trước |
| `Port 格式錯誤` | Port không phải số | Nhập đúng số (VD: 5000) |
| Không thấy tin nhắn | Firewall chặn | Mở port 5000 trong Firewall |
| Build lỗi `net8.0-windows` | Thiếu SDK | Cài .NET 8 SDK |
| Ảnh không hiển thị | Clipboard bị khoá | Đóng ứng dụng khác đang dùng clipboard |

---

## 🛠️ Công nghệ sử dụng

- **Ngôn ngữ:** C# 12
- **Framework:** .NET 8 / WinForms
- **Giao tiếp:** TCP Socket (System.Net.Sockets)
- **Encoding:** UTF-8
- **Ảnh:** System.Drawing
- **IDE:** Visual Studio 2026 Community

---

## 📝 Changelog

| Phiên bản | Nội dung |
|-----------|---------|
| **v1.0** | Giao diện dark, bong bóng trái/phải, emoji, giờ gửi |
| **v2.0** | Gửi ảnh, file, chụp màn hình, thông báo tray, typing indicator |
| **v3.0** | 6 theme màu, cỡ chữ, tìm kiếm, xuất chat, minimize tray |
| **v4.0 FINAL** | Avatar, thu hồi tin nhắn, đã đọc/chưa đọc, tin nhắn riêng |

---

## 👨‍💻 Tác giả

**Văn Cường** — Sinh viên Justice University (JUST), Taiwan
📧 s121170906@just.edu.tw

---

## 📜 Giấy phép

Dự án mã nguồn mở, dành cho mục đích học tập.
Vui lòng ghi nguồn khi sử dụng lại.

---

> ⭐ Nếu dự án này hữu ích, hãy **Star** để ủng hộ nhé!
