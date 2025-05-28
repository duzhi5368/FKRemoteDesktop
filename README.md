`FKRemoteDesktop` is a remote control tool for Windows that can be compiled into a binary Payload and wrapped with other Shellcode Loaders. It consists of two components: a server and a client. The client operates in stealth mode, while the server provides centralized control.

## Features

### Client Disguise and Anti-Detection

- By default, the client runs silently in the background.
- Through the server's **Settings** -> **Advanced Client Settings**, it can automatically steal all information from third-party applications, including ICONs and signature certificates, for disguise.
- The client supports automatic backups.
- The client can be set to auto-start on boot.
- If the client process is terminated, it will automatically restart.
- Shellcode includes a custom encryption mechanism.
- Shellcode Loader is customizable (*see code in Resources->Scripts*).

### Communication Security

- Two-way SSL authentication between client and server.
- Uses custom ProtoBuf message format.

### Remote System Information Viewing

- View the client's hardware and system information.
- View the client's firewall and antivirus software list.

### File Transfer Functionality

- View all files on the client.
- Download files from the client to the server locally.
- Upload files from the server to the client.

### Remote Startup Management Tool

- View and edit the client's system startup items.

### Remote Registry Management Tool

- View and perform add/delete/edit operations on the client's registry.

### Remote EXE Execution Tool

- Control the client to execute EXE files in the background or download and execute files from the network.

### Password Viewing Tool

- View cookies and cached passwords from commonly used browsers on the client.

### Remote Command Tool

- Execute command-line tools on the client from the server (*client unaware*).

### Remote Network Connection Viewing Tool

- View and control the client's network connection status.

### Keylogger Tool

- Record all mouse and keyboard activities on the client.

### Remote Desktop Viewing Tool

- Monitor the client's desktop in real-time (*client unaware*) and actively control the mouse and keyboard remotely.

### Remote Message Popup Tool

- Send a message prompt box to the client.

### Remote Webpage Opening Tool

- Control the client to open a specified webpage (*can be opened in the background, client unaware*).

### User Privilege Escalation Tool

- Elevate the client's user permissions to **run as administrator**.

### Client Update Tool

- Allow the client to perform self-updates (*suitable for client version updates*).

### Client Self-Protection Tools

- Allow remote control to disconnect the client.
- Allow remote control for the client to self-delete (*remove all traces of the client*).

### Client Machine Control Tool

- Remotely control the client machine to restart, sleep, or shut down.

### Reverse Proxy Management Tool

- Allow the client to act as a proxy relay for secondary message forwarding, supporting load balancing.

## Usage

- Use the server's **Settings** function to save configurations.
  - For testing, simply go to **Settings** -> **Basic Client Configuration** -> **Server IP/Hostname**, add the server IP (*use 127.0.0.1 for local testing*) -> **Add Server Information** -> **Save**.
- Use the server's **Generate** function to create a signed and encrypted client.
- Run the client.
- Run the server (*you can manually enable listening in Configuration -> Server Configuration*).

## Miscellaneous

### TODO:
- Need more shellcode anti-detection and packing solutions.
- The client needs to disable or remove certain features to identify which components are triggering VirusTotal detections. Initially 8/72, it has increased to 20/72, and even 32/72 in the final version—this is a serious issue.

### Known Bugs:
- RECONNECTDELAY in SettingsFromServer fails to write.
- Issues with file transfer and writing, often resulting in read failures.
- Chrome v10 password decryption errors.

### VirusTotal Detection Status:
- BasicTemplate.cs: 35/72 (no encryption)
- SgnBasicTemplate.cs: 35/72 (SGN encryption)
- FKShellcodeBasicTemplate.cs: 20/72 (custom encryption) (Kaspersky, MS, Huorong, Symantec)
- acmDriverEnum: 19/72 (custom encryption) (Symantec, McAfee)
- XorBasicTemplate: 20/72 (XOR encryption only) (Symantec, McAfee, Kaspersky, Huorong)
- FindText: 17/72 (custom encryption) (Symantec, McAfee)


`FKRemoteDesktop` 是一个 Windows 上的远控工具，本身可以被生成为二进制 Payload 之后使用其他 Shellcode Loader 进行包装。它包括俩部分，服务器和客户端，客户端为隐身状态，服务器可进行总控。

## 功能

### 客户端伪装和免杀

- 默认情况下，客户端后台静默运行。
- 通过服务器 **设置** -> **客户端进阶设置**，可自动盗取第三方应用程序全部信息，包括ICON,签名证书信息 加以伪装。
- 客户端可自动备份
- 客户端可开机自启动
- 客户端进程被杀后，会自动重启
- Shellcode有自定义加密机制
- Shellcode Loader可自定义 (*请参考 Resources->Scripts 内代码*)

### 通信安全

- 客户端和服务器有双向SSL验证
- 使用 ProtoBuf 自定义消息格式

### 查看远程系统信息

- 可查看客户端硬件系统信息
- 可查看客户端防火墙，杀毒软件列表信息

### 文件传输功能

- 可查看客户端全部文件
- 可下载客户端文件到服务器本地
- 可从服务器本地上传文件到客户端

### 远程启动项管理工具

- 可查看客户端系统自启动项并编辑修改

### 远程注册表管理工具

- 允许查看客户端注册表信息并做出增删改操作

### 远程EXE执行工具

- 允许后台控制客户端执行EXE文件或从网络下载文件并执行

### 密码查看工具

- 可查看客户端中常用浏览器中的 Cookie和缓存密码 信息

### 远程命令工具

- 可从服务器对客户端执行命令行工具 (*客户端无感知*)

### 远程网络连接查看工具

- 可查看客户端网络连接状况并做出控制

### 键盘记录工具

- 可记录客户端所有鼠标和键盘操作

### 远程桌面查看工具

- 实时监控客户端桌面情况 (*客户端无感知*)，并可主动远程进行鼠标和键盘控制操作

### 远程消息弹出工具

- 允许向客户端弹出一条消息提示框

### 远程网页打开工具

- 允许控制客户端打开一个指定网页（*可以后台打开，客户端无感知*）

### 用户提权工具

- 允许将客户端的使用权限提升到 **以管理员身份进行**

### 客户端更新工具

- 允许客户端进行自更新 (*适用于客户端版本更新场景*)

### 客户端自保类工具

- 允许远程控制客户端主动断开
- 允许远程控制客户端自我删除 (*清理客户端全部痕迹*)

### 客户端机器控制工具

- 允许远程控制客户端机器重启/休眠/关机

### 反向代理管理工具

- 允许客户端作为代理中转，进行消息二次转发，支持负载均衡


## 使用方式

- 使用服务器的 **设置** 功能，保存配置
  - 测试时仅需在 **设置** -> **客户端基本配置** -> **服务器IP/主机名** 处添加服务器IP（*本机可使用 127.0.0.1*）-> **添加服务器信息** -> **保存** 即可
- 使用服务器的 **生成** 功能，生成签名加密客户端
- 运行客户端
- 运行服务器 (*可手动在 配置 -> 服务器配置 中开启监听*)

## 其他

### TODO:
- 需要更多的shellcode免杀加壳方案
- 客户端需要关闭和移除部分功能以确定到底是哪些部分引起了VT查杀，原本8/72，逐步升级到20/72，甚至最终版的32/72，这个问题很严重。

### 已知BUG：
- SettingsFromServer 中的 RECONNECTDELAY 写入失败。
- 文件传输和写入部分有问题，总出现读取失败。
- Chrome的v10密码解密出错。

### VT查杀情况：
- BasicTemplate.cs  35/72 （无加密）
- SgnBasicTemplate.cs 35/72 （SGN加密）
- FKShellcodeBasicTemplate.cs 20/72（自定义加密）（卡巴斯基，MS,火绒，Symentec）
- acmDriverEnum 19/72 (自定义加密)（Symentec,McAfee）
- XorBasicTemplate 20/72(仅XOR加密) （Symentec，McAfee，卡巴，火绒）
- FindText 17/72(自定义加密)（Symentec，McAfee）


