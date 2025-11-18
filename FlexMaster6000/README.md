# FlexMaster 6000

## Enhanced Slice Control for Flex 6000 Series Radios
### 🎯 SmartSDR 4.0 Compatible

FlexMaster 6000 is a modern, open-source Windows application that provides enhanced control capabilities for FlexRadio 6000 series software-defined radios using SmartSDR 4.0.

---

## ✨ Features

### Core Functionality
- ✅ **Full SmartSDR 4.0 Compatibility** - Built with FlexLib API v3
- 🎚️ **Audio Mixer Panel** - Solo, mute, AGC, and level controls per slice
- 📡 **Multi-Slice Management** - Create, configure, and control multiple slices
- 🔄 **Real-time Updates** - Live frequency, mode, and status updates

### Integration & Connectivity
- 🌐 **HRD TCP Listener** - Per-slice and TX-following Ham Radio Deluxe TCP connections
- 🔌 **CAT over TCP** - Compatible with hamlib and other CAT clients
- 🔗 **Third-Party Program Support** - Works with logging programs and digital mode software

### Modern Interface
- 💻 **WPF Modern UI** - Clean, intuitive interface
- 📊 **Real-time Monitoring** - Live slice status and radio information
- ⚙️ **Easy Configuration** - Simple setup and management

---

## 📋 System Requirements

### Operating System
- Windows 10 or Windows 11 (64-bit)
- .NET 6.0 Runtime or later

### Radio Requirements
- FlexRadio 6000 or 8000 series radio
- SmartSDR 4.0 or later installed and running
- Network connection to radio (local or SmartLink)

### Development Requirements (for building from source)
- Visual Studio 2022 or later
- .NET 6.0 SDK
- FlexLib API v3.x (see installation instructions below)

---

## 🚀 Installation

### Option 1: Pre-compiled Binary (Recommended)

1. Download the latest release from the Releases page
2. Extract the ZIP file to a folder of your choice
3. **Download FlexLib API**:
   - Visit https://www.flexradio.com/software/flexlib_api_v3/
   - Download the FlexLib API ZIP file
   - Extract and copy `Flex.Smoothlake.FlexLib.dll` to the `Libs` folder in your FlexMaster installation

4. Run `FlexMaster6000.exe`

### Option 2: Build from Source

#### Step 1: Clone the Repository

```bash
git clone https://github.com/yourusername/FlexMaster6000.git
cd FlexMaster6000
```

#### Step 2: Download FlexLib API

1. Visit https://www.flexradio.com/software/flexlib_api_v3/
2. Download the latest FlexLib API package (e.g., `FlexLib_API_v3.8.19.34216.zip`)
3. Extract the ZIP file
4. Create a `Libs` folder in the project root if it doesn't exist:
   ```bash
   mkdir Libs
   ```
5. Copy `Flex.Smoothlake.FlexLib.dll` to the `Libs` folder

#### Step 3: Build the Project

**Using Visual Studio:**
1. Open `FlexMaster6000.sln` in Visual Studio 2022
2. Restore NuGet packages (right-click solution → Restore NuGet Packages)
3. Build the solution (F6 or Build → Build Solution)
4. Run the application (F5)

**Using Command Line:**
```bash
dotnet restore
dotnet build
dotnet run
```

---

## 🎮 Getting Started

### First Run

1. **Start SmartSDR** - Ensure SmartSDR is running and connected to your radio
2. **Launch FlexMaster 6000**
3. **Connect to Radio**:
   - Click **Radio → Connect** from the menu
   - FlexMaster will discover available radios
   - Select your radio and click Connect

### Basic Operations

#### Managing Slices
- **Add Slice**: Click **Slices → Add Slice** or use the Overview tab quick action
- **Remove Slice**: Select a slice in the Slices tab and click Remove
- **View Slices**: Navigate to the **Slices** tab to see all active slices

#### Audio Mixer
1. Navigate to the **Audio Mixer** tab
2. Use the vertical sliders to adjust audio gain for each slice
3. Click **M** to mute a slice
4. Click **S** to solo a slice (mutes all others)

#### HRD TCP Server
1. Navigate to the **Settings** tab
2. Configure the port (default: 7809)
3. Click **Start Server**
4. Configure your logging software to connect to `localhost:7809`

---

## 🔧 Configuration

### HRD TCP Listener Settings

**Default Port**: 7809

To configure third-party programs to use FlexMaster's HRD TCP listener:

1. In your logging program (e.g., N1MM+, DXLab, Logger32):
2. Set rig control to "Ham Radio Deluxe"
3. Set server: `localhost`
4. Set port: `7809` (or your custom port)

### Per-Slice vs TX-Following

- **Per-Slice Mode**: Each slice has its own TCP port (7809, 7810, 7811, etc.)
- **TX-Following Mode**: Server follows the active TX slice (useful for loggers)

---

## 🏗️ Architecture

### Project Structure

```
FlexMaster6000/
├── FlexMaster6000.csproj      # Main project file
├── App.xaml                   # Application entry point
├── App.xaml.cs               # Application logic
├── Models/
│   └── SliceInfo.cs          # Slice data model
├── Services/
│   ├── RadioManager.cs       # FlexRadio connection & management
│   ├── AudioMixerService.cs  # Audio control service
│   └── HrdTcpServer.cs       # HRD TCP protocol server
├── Views/
│   ├── MainWindow.xaml       # Main UI
│   └── MainWindow.xaml.cs    # Main UI code-behind
├── Libs/
│   └── Flex.Smoothlake.FlexLib.dll  # FlexLib API (not included)
└── README.md
```

### Technology Stack

- **Framework**: .NET 6.0 (Windows)
- **UI**: WPF (Windows Presentation Foundation)
- **Radio API**: FlexLib API v3
- **Logging**: Microsoft.Extensions.Logging
- **Data Binding**: MVVM pattern with INotifyPropertyChanged

---

## 📚 FlexLib API Documentation

FlexMaster 6000 uses the official FlexRadio FlexLib API v3 for all radio communication.

### Key FlexLib Classes Used

```csharp
// Initialize API
API.Init();

// Discover radios
API.RadioAdded += (radio) => { /* handle radio discovered */ };
API.StartRadioList();

// Connect to radio
radio.Connect();

// Manage slices
var slice = radio.CreateSlice();
slice.Freq = 14.200;
slice.DemodMode = "USB";
slice.Active = true;

// Audio control
slice.AudioGain = 50;
slice.Mute = false;

// DAX control
slice.DAXChannel = 1;
```

### FlexLib Resources

- **Official Documentation**: https://www.flexradio.com/api
- **Download FlexLib**: https://www.flexradio.com/software/flexlib_api_v3/
- **FlexRadio Community**: https://community.flexradio.com/
- **API Reference**: Included with FlexLib download (HTML docs)

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues.

### Development Guidelines

1. Follow C# coding conventions
2. Use async/await for I/O operations
3. Add XML documentation to public methods
4. Test with SmartSDR 4.0 before submitting PR
5. Update README with new features

---

## 🐛 Troubleshooting

### FlexLib DLL Not Found

**Error**: "Could not load file or assembly 'Flex.Smoothlake.FlexLib'"

**Solution**:
1. Download FlexLib from https://www.flexradio.com/software/flexlib_api_v3/
2. Extract and copy `Flex.Smoothlake.FlexLib.dll` to the `Libs` folder
3. Rebuild the project

### No Radios Discovered

**Possible Causes**:
- SmartSDR not running
- Radio not powered on
- Network connectivity issues
- Firewall blocking FlexMaster

**Solutions**:
1. Ensure SmartSDR is running and connected to radio
2. Check Windows Firewall settings
3. Verify radio is on same network or SmartLink is configured

### HRD TCP Connection Fails

**Possible Causes**:
- Port already in use
- Firewall blocking port
- Incorrect port in client program

**Solutions**:
1. Try a different port (e.g., 7810)
2. Add firewall exception for FlexMaster
3. Verify client program is configured with correct port

---

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## 🙏 Acknowledgments

- **FlexRadio Systems** - for the FlexLib API and excellent SDR hardware
- **K1DBO (Donald Beaudry)** - for the original Slice Master 6000 inspiration
- **FlexRadio Community** - for support and feedback

---

## 📧 Support

For questions, issues, or feature requests:

- **Issues**: https://github.com/yourusername/FlexMaster6000/issues
- **FlexRadio Community**: https://community.flexradio.com/

---

## 🔄 Version History

### v1.0.0 (2025)
- ✨ Initial release
- ✅ SmartSDR 4.0 compatibility
- 🎚️ Audio mixer implementation
- 🌐 HRD TCP server
- 📡 Multi-slice management
- 💻 Modern WPF interface

---

## 🎯 Roadmap

Future enhancements planned:

- [ ] CAT over TCP listener
- [ ] Bandmap overlay
- [ ] Spot aggregation
- [ ] N1MM Logger+ integration
- [ ] Panadapter synchronization
- [ ] Third-party app launcher (CW Skimmer, WSJT-X, etc.)
- [ ] SmartLink support
- [ ] Multi-radio support
- [ ] Audio recording
- [ ] Macro support

---

**FlexMaster 6000** - Because your Flex radio deserves modern software! 🚀📡
