# PBL4

**Hệ thống Quản lý Server — Client** (Windows/WPF, .NET 8)  
Ứng dụng gồm hai project tách biệt: **Server** và **Client**. Mục tiêu: quản lý danh sách client, gửi lệnh cơ bản, stream màn hình (screen share), remote control, hiển thị Task Manager và chat giữa Server — Client.

---

## Tổng quan chức năng

### Chức năng chính
- **Quản lý danh sách client**  
  - Server lưu và hiển thị danh sách client đang kết nối (tên máy, IP, trạng thái).
- **Lệnh điều khiển cơ bản**  
  - Server có thể gửi lệnh `Shutdown`, `Restart` tới client.
- **Chat / Tin nhắn (đã hoàn thiện)**  
  - Hệ thống chat 1-1 giữa server và client: mở cửa sổ chat từ Server UI (chọn client → Tin nhắn) hoặc từ Client UI.  
  - Tin nhắn gửi qua kênh command (TCP), hiển thị thời gian gửi/nhận, hỗ trợ log history phiên làm việc.
- **Screen share (Stream màn hình)**  
  - Server gửi lệnh `StartStream` / `StopStream` tới client; client gửi khung ảnh qua **UDP**; server nhận và hiển thị luồng hình ảnh.  
  - UDP port mặc định: `9999`.
- **Remote control (chuột / bàn phím)**  
  - Hỗ trợ gửi các thao tác chuột/bàn phím (MouseMove, MouseClick, KeyDown/KeyUp) thông qua `RemoteAction`.
- **Task Manager **  
  - Server có thể yêu cầu danh sách tiến trình (process list) từ client (RequestProcessList).  
  - Client trả về ResponseProcessList (danh sách process với PID, tên, tài nguyên cơ bản).  
  - Trên UI server hiển thị Task Manager cho từng client; admin có thể chọn tiến trình và gửi lệnh kill (KillProcess) tới client.  


---

## Yêu cầu môi trường

- Hệ điều hành: **Windows** (WPF GUI).  
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).  
- Visual Studio 2022/2023 (hoặc `dotnet` CLI) với workload WPF/Desktop để build và chạy.

> Cả **Server** và **Client** target `net8.0-windows`.

---
## Cấu trúc chính của repository

- `PBL4.sln` — Solution (Client + Server)

- `Client/` — Project client (WPF)
  - `Network/`
    - `TcpCommandChannel.cs`
    - `UdpStreamSender.cs`
    - `PacketChunker.cs`
  - `ScreenShare/`
    - `ScreenSharePipeline.cs`
  - `UI/`
    - `MainWindow.xaml`
    - `ChatWindow.xaml`
    - `TaskManagerWindow.xaml`
  - `Models/`
    - `InputAction.cs`
    - `RemoteAction.cs`
    - `ProcessInfo.cs`

- `Server/` — Project server (WPF)
  - `Network/`
    - `TcpServerListener.cs`
    - `UdpStreamReceiver.cs`
  - `Core/`
    - `ServerManager.cs`
    - `ClientSession.cs`
  - `ScreenShare/`
    - `ScreenReceiverPipeline.cs`
    - `ScreenRenderer.cs`
  - `UI/`
    - `MainWindow.xaml`
    - `ClientViewModel.cs`
    - `ChatWindow.xaml`
    - `TaskManagerWindow.xaml`
  - `Models/`
    - `RemoteAction.cs`

- `.gitignore`
- `README.md`
- `CHANGELOG.md`


---

## Mô tả dữ liệu & protocol (tóm tắt)

- **RemoteAction**: dùng làm DTO/command giữa server và client, bao gồm:
  - System commands: `Shutdown`, `Restart`
  - Stream control: `StartStream`, `StopStream`
  - Remote control: `MouseMove`, `MouseLeftDown`, `MouseLeftUp`, `KeyDown`, `KeyUp`, ...
  - Process management: `RequestProcessList`, `ResponseProcessList`, `KillProcess`
  - Chat: `ChatMessage` (hoặc tương tự) để truyền nội dung tin nhắn

> Lưu ý: mức độ chi tiết (các field) có thể nằm trong `Client/Models` và `Server/Models` riêng theo kiến trúc tách biệt.

---

## Hướng dẫn build & run

### Bằng Visual Studio
1. Mở `PBL4.sln` bằng Visual Studio (phiên bản hỗ trợ .NET 8 + WPF).  
2. Chọn `Server` hoặc `Client` làm Startup Project (hoặc chạy từng project riêng).  
3. `Build -> Build Solution`.  
4. Chạy `Server` và `Client` theo nhu cầu (server trước, client sau).

### Bằng dotnet CLI (Windows)
> **Lưu ý:** WPF chỉ chạy trên Windows.  
```powershell
# Build
dotnet build PBL4.sln

# Chạy Server (từ root)
dotnet run --project Server/Server.csproj

# Chạy Client
dotnet run --project Client/Client.csproj

