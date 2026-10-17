# MollyPS

<p align="center">
  <a href="https://dotnet.microsoft.com/download/dotnet/10.0"><img src="https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10" /></a>
  <a href="https://learn.microsoft.com/dotnet/csharp/"><img src="https://img.shields.io/badge/C%23-14-239120?style=for-the-badge&logo=csharp" alt="C# 14" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-PolyForm%20Noncommercial%201.0.0-blue?style=for-the-badge" alt="PolyForm Noncommercial 1.0.0" /></a>
  <img src="https://img.shields.io/badge/Client-1.6.3%20Compatible-orange?style=for-the-badge" alt="Client 1.6.3" />
</p>

## 💡 Features

- [x] **Scenes**
- [x] **Teleportation**
- [x] **Team management**

## 🍗 Quick start

### Requirements

1. [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
2. A matching **`OS_WIN_REL_1.6.3` Windows x64 client**
3. The [client patch](Patch/gfsdk.dll) included with this project

### Build

```powershell
git clone https://github.com/xibei1204/m0llyps.git
cd m0llyps
dotnet build
```

### Run the server

Start the server from the build output directory:

```powershell
Set-Location Program/bin/Debug/net10.0
.\MollyPS.exe
```

## 🔌 Install the patch and play

1. Close the game and back up the client's original `gfsdk.dll`.
2. Copy this project's `Patch/gfsdk.dll` to the original DLL's location in the client directory, replacing it.
3. Start the server, then launch the game through the launcher.

## License

This project is licensed under the [PolyForm Noncommercial License 1.0.0](LICENSE).s