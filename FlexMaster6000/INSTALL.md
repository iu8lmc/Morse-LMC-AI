# FlexMaster 6000 - Installation Guide

## Quick Start Installation

### Prerequisites

1. **Windows 10 or 11** (64-bit)
2. **.NET 6.0 Runtime** - Download from https://dotnet.microsoft.com/download/dotnet/6.0
3. **SmartSDR 4.0 or later** - Must be installed and running
4. **FlexRadio 6000/8000 series** radio

### Step-by-Step Installation

#### 1. Download FlexMaster 6000

**Option A: Pre-compiled Release (Easiest)**
```
1. Go to Releases page
2. Download FlexMaster6000-v1.0.0.zip
3. Extract to C:\Program Files\FlexMaster6000\ (or your preferred location)
```

**Option B: Build from Source**
```bash
git clone https://github.com/yourusername/FlexMaster6000.git
cd FlexMaster6000
```

#### 2. Download FlexLib API (REQUIRED)

This is the most important step!

```
1. Visit: https://www.flexradio.com/software/flexlib_api_v3/
2. Download the latest version (e.g., FlexLib_API_v3.8.19.34216.zip)
3. Extract the ZIP file
4. Locate Flex.Smoothlake.FlexLib.dll in the extracted files
5. Copy it to:
   - Pre-compiled: C:\Program Files\FlexMaster6000\Libs\
   - Source build: [project-folder]\FlexMaster6000\Libs\
```

**Directory structure should look like:**
```
FlexMaster6000/
├── FlexMaster6000.exe
├── Libs/
│   └── Flex.Smoothlake.FlexLib.dll  ← This file is REQUIRED
└── other files...
```

#### 3. Install .NET 6.0 Runtime (if needed)

Check if .NET 6 is installed:
```bash
dotnet --list-runtimes
```

If not installed:
1. Download from https://dotnet.microsoft.com/download/dotnet/6.0
2. Choose "Windows x64" runtime
3. Run the installer

#### 4. First Run

1. **Start SmartSDR first** - Connect to your radio
2. **Run FlexMaster6000.exe**
3. Click **Radio → Connect**
4. FlexMaster will discover and connect to your radio

#### 5. Configure Firewall (if needed)

Windows may ask for firewall permission. Click "Allow access" when prompted.

If FlexMaster can't discover radios:
```
1. Open Windows Defender Firewall
2. Click "Allow an app through firewall"
3. Click "Change settings"
4. Find FlexMaster6000.exe
5. Check both Private and Public
6. Click OK
```

---

## Building from Source

### Requirements

- **Visual Studio 2022** (Community Edition is free)
- **.NET 6.0 SDK** (included with Visual Studio)
- **FlexLib API v3** (download separately)

### Build Steps

1. **Clone Repository**
   ```bash
   git clone https://github.com/yourusername/FlexMaster6000.git
   cd FlexMaster6000
   ```

2. **Create Libs folder**
   ```bash
   mkdir Libs
   ```

3. **Download and copy FlexLib DLL**
   - Download from https://www.flexradio.com/software/flexlib_api_v3/
   - Extract and copy `Flex.Smoothlake.FlexLib.dll` to `Libs\` folder

4. **Open in Visual Studio**
   ```
   - Open FlexMaster6000.sln
   - Right-click Solution → Restore NuGet Packages
   - Build → Build Solution (F6)
   ```

5. **Run**
   - Press F5 or click Start

### Command Line Build

```bash
# Restore packages
dotnet restore

# Build
dotnet build --configuration Release

# Run
dotnet run --project FlexMaster6000
```

The compiled executable will be in:
```
bin\Release\net6.0-windows\FlexMaster6000.exe
```

---

## Troubleshooting Installation

### Problem: "Could not load FlexLib DLL"

**Solution:**
```
1. Verify Flex.Smoothlake.FlexLib.dll is in the Libs folder
2. Check the DLL is not blocked:
   - Right-click the DLL
   - Properties
   - If there's an "Unblock" checkbox, check it
   - Click Apply
```

### Problem: "This application requires .NET 6.0"

**Solution:**
```
1. Download .NET 6.0 Runtime: https://dotnet.microsoft.com/download/dotnet/6.0
2. Install the Windows x64 runtime
3. Restart FlexMaster
```

### Problem: "No radios discovered"

**Solutions:**
```
1. Ensure SmartSDR is running and connected
2. Check radio is on same network
3. Try connecting via SmartSDR first to verify radio connectivity
4. Check Windows Firewall settings
5. Restart FlexMaster and try again
```

### Problem: Build errors in Visual Studio

**Solutions:**
```
1. Clean Solution (Build → Clean Solution)
2. Restore NuGet packages (right-click solution → Restore NuGet Packages)
3. Verify FlexLib DLL is in Libs folder
4. Rebuild Solution (Build → Rebuild Solution)
```

---

## Deployment

### Creating a Distribution Package

1. **Build in Release mode**
   ```
   Build → Configuration Manager → Release
   Build → Build Solution
   ```

2. **Collect files**
   ```
   bin\Release\net6.0-windows\
   ├── FlexMaster6000.exe
   ├── FlexMaster6000.dll
   ├── Newtonsoft.Json.dll
   ├── Microsoft.Extensions.*.dll
   └── ... (all DLLs except FlexLib)
   ```

3. **Create Libs folder**
   ```
   - Create empty Libs folder
   - Add .gitkeep or README explaining FlexLib download
   ```

4. **Add documentation**
   ```
   - Copy README.md
   - Copy INSTALL.md
   - Copy LICENSE
   ```

5. **Create ZIP**
   ```
   FlexMaster6000-v1.0.0.zip containing:
   ├── FlexMaster6000.exe
   ├── *.dll files
   ├── Libs/ (empty with instructions)
   ├── README.md
   ├── INSTALL.md
   └── LICENSE
   ```

### User Installation Instructions (include in ZIP)

```
INSTALLATION INSTRUCTIONS
=========================

1. Extract this ZIP to your preferred location
2. Download FlexLib from https://www.flexradio.com/software/flexlib_api_v3/
3. Copy Flex.Smoothlake.FlexLib.dll to the Libs folder
4. Ensure .NET 6.0 Runtime is installed
5. Run FlexMaster6000.exe

See INSTALL.md for detailed instructions.
```

---

## System Requirements Check

Run this PowerShell script to check your system:

```powershell
# Check Windows version
Write-Host "Windows Version:" (Get-CimInstance Win32_OperatingSystem).Caption

# Check .NET installations
Write-Host "`n.NET Runtimes:"
dotnet --list-runtimes

# Check if SmartSDR is running
Write-Host "`nSmartSDR Status:"
Get-Process | Where-Object {$_.Name -like "*SmartSDR*"} | Select-Object Name, Id

# Check network connectivity
Write-Host "`nNetwork Interfaces:"
Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.InterfaceAlias -notlike "*Loopback*"} | Select-Object InterfaceAlias, IPAddress
```

---

## Next Steps

After installation:
1. Read the [README.md](README.md) for usage instructions
2. Configure HRD TCP server if needed
3. Explore the Audio Mixer features
4. Join the FlexRadio community for support

---

## Support

- **Issues**: https://github.com/yourusername/FlexMaster6000/issues
- **FlexRadio Community**: https://community.flexradio.com/
- **FlexLib Documentation**: Included with FlexLib download

---

**Happy operating!** 73! 📡
