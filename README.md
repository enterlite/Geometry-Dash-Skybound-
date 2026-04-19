# 🎮 Skybound Launcher - Moderní Game Launcher

Plně funkční, profesionální game launcher pro hru **Skybound** napsaný v C# s WPF a MVVM architekturou.

## 📋 Obsah Projektu

```
SkyboundLauncher/
├── Infrastructure/
│   ├── RelayCommand.cs          # MVVM commands pro UI binding
│   └── ViewModelBase.cs          # Základní ViewModel třída
├── Models/
│   └── LauncherModels.cs        # LauncherState, LauncherConfig, GameInfo
├── Data/
│   └── ConfigManager.cs         # Správa konfigurace v JSON
├── Services/
│   ├── GameManager.cs           # Stahování, extrakce, spouštění hry
│   ├── InstallationService.cs  # Instalace/odinstalace workflow
│   ├── LoggingService.cs        # Logging do souboru
│   └── UpdateCheckService.cs    # Kontrola aktualizací
├── ViewModels/
│   └── MainWindowViewModel.cs   # Hlavní logika UI
├── MainWindow.xaml              # Moderní UI s animacemi
├── MainWindow.xaml.cs           # Code-behind, tray ikona
├── App.xaml                     # Globální resources
├── App.xaml.cs                  # Startup/shutdown logika
└── SkyboundLauncher.csproj      # Projekt konfigurace
```

---

## 🏗️ Architektura

### MVVM Pattern
```
View (XAML)
   ↓
ViewModel (MainWindowViewModel)
   ↓
Model (LauncherConfig, GameInfo)
   ↓
Services (GameManager, InstallationService)
```

### Datové toky

1. **Konfigurační data**: `ConfigManager` → `%AppData%\Local\Skybound\launcher.config.json`
2. **Herní soubory**: Zůstávají v uživatelské složce (např. `Downloads/skybound-game/`)
3. **Logy**: `Services/Logs/launcher_{yyyy-MM-dd}.log`

---

## 🎮 Hlavní Funkce

### 1. Stažení a instalace
```csharp
// Workflow:
1. Stáhne ZIP z URL s progress reportingem
2. Extrahuje do dočasné složky
3. Přemístí skybound-game do cílové lokace
4. Přemístí Skybound config do %AppData%
5. Uloží cestu k Skybound.exe do konfigurace
```

### 2. Spouštění hry
```csharp
// Workflow:
1. Ověří integritu (kontrola existence exe)
2. Spustí hru na pozadí
3. Minimalizuje launcher do tray
4. Čeká na zavření hry
5. Obnoví launcher okno
```

### 3. Aktualizace
```csharp
// UpdateCheckService:
- Porovnání verzí (semver)
- Stažení nového ZIP
- Aktualizace souborů bez smazání dat
- Zachování cesty k exe
```

### 4. Oprava hry (Repair)
```csharp
// InstallationService.VerifyGameIntegrity():
- Kontrola existence Skybound.exe
- Kontrola herního adresáře
- Ověření přístupových práv
```

### 5. Odinstalace
```csharp
// Workflow:
1. Smazání %AppData%\Local\Skybound\
2. Volitelné smazání skybound-game/
3. Vymazání konfigurace
4. Návrat do stavu "Not Installed"
```

---

## 🎨 UI/UX Features

### Moderní Design
- Dark theme s modrými akcenty (#0066cc)
- Gradient pozadí
- Zaoblené tlačítka a progress bary
- Hladké animace přechodů

### Stavové obrazovky
```
NotInstalled      → "DOWNLOAD GAME" tlačítko
Downloading       → Progress bar s % stažení
Extracting        → Progress bar s % extrakce
Verifying         → "Ověřování integrity..."
ReadyToPlay       → "▶ LAUNCH GAME" + "⚙ REPAIR" + "🗑 UNINSTALL"
GameRunning       → "⏵ Hra je spuštěna..."
Error             → "⚠ Došlo k chybě. Zkuste znovu."
```

### Tray ikona
```csharp
// Features:
- Minimalizace do systémové lišty (tray)
- Obnovení kliknutím na tray ikonu
- Kontextové menu (Obnovit, Zavřít)
- Automatické skrytí z taskbaru
```

---

## 📝 Klíčové Třídy

### `MainWindowViewModel` - Orchestrace
```csharp
public class MainWindowViewModel : ViewModelBase
{
    // Properties (binding-friendly)
    public LauncherState CurrentState { get; set; }
    public double DownloadProgress { get; set; }
    public string StatusMessage { get; set; }
    
    // Commands (pro UI)
    public ICommand DownloadGameCommand { get; }
    public ICommand LaunchGameCommand { get; }
    public ICommand UninstallGameCommand { get; }
    public ICommand RepairGameCommand { get; }
}
```

### `GameManager` - Stahování & Spouštění
```csharp
public class GameManager : IGameManager
{
    public async Task<string> DownloadGameAsync(string downloadUrl);
    public async Task ExtractGameAsync(string zipPath, string targetDirectory);
    public Process? LaunchGameAsync(string executablePath);
    public bool IsGameRunning(int? processId);
    public async Task WaitForGameExitAsync(int processId);
    
    // Events
    public event EventHandler<DownloadProgressEventArgs>? DownloadProgressChanged;
    public event EventHandler<ExtractionProgressEventArgs>? ExtractionProgressChanged;
}
```

### `ConfigManager` - Konfigurace
```csharp
public class ConfigManager
{
    public Models.LauncherConfig LoadConfig();
    public void SaveConfig(Models.LauncherConfig config);
    public string GetSkyboundDataPath(); // → %AppData%\Local\Skybound\
}
```

### `InstallationService` - Workflow
```csharp
public class InstallationService
{
    public async Task<bool> InstallGameAsync(string downloadUrl);
    public async Task<bool> UninstallGameAsync(bool deleteGameFiles);
    public bool VerifyGameIntegrity(string executablePath);
    
    public event EventHandler<string>? StatusChanged;
}
```

---

## 🔧 Konfigurace

### `launcher.config.json` (AppData)
```json
{
  "gameExecutablePath": "C:\\Users\\User\\Downloads\\skybound-game\\Skybound.exe",
  "installedVersion": "1.0.0",
  "lastUpdateCheckTime": "2024-12-18T10:30:00"
}
```

### URL pro stahování (DEMO)
```csharp
// V GameManager.DownloadGameAsync:
const string downloadUrl = "https://example.com/skybound-game.zip";

// Očekávaná struktura ZIP:
// skybound-game/
//   ├── Skybound.exe
//   ├── assets/
//   └── ...
// Skybound/
//   ├── version.txt
//   └── ...
```

---

## 🚀 Spuštění

### Předpoklady
- .NET 9.0 (Windows)
- Visual Studio Code / Visual Studio 2022+

### Kompilace
```powershell
cd SkyboundLauncher
dotnet build
```

### Spuštění
```powershell
dotnet run --project SkyboundLauncher.csproj
```

### Publikace (standalone)
```powershell
dotnet publish -c Release -o ./publish
# Spustitelný soubor: ./publish/SkyboundLauncher.exe
```

---

## 📦 Rozšiřitelnost

### Přidání nové funkce

#### 1. Nový Command
```csharp
public ICommand MyNewCommand { get; }

public MainWindowViewModel()
{
    MyNewCommand = new AsyncRelayCommand(
        _ => MyNewCommandAsync(),
        _ => !IsOperationInProgress
    );
}

private async Task MyNewCommandAsync()
{
    // Implementace
}
```

#### 2. Nová UI obrazovka
```xaml
<StackPanel Visibility="{Binding CurrentState, Converter=..., ConverterParameter=MyNewState}">
    <!-- Obsah -->
</StackPanel>
```

#### 3. Nový stav
```csharp
public enum LauncherState
{
    // ... existující
    MyNewState
}
```

---

## 🐛 Debugging

### Logy
```
%AppData%\Local\Skybound\Logs\launcher_{yyyy-MM-dd}.log
```

### Konzole Output
```csharp
[INFO] Skybound Launcher spuštěn
[INFO] Stahování z: https://...
[ERROR] Chyba: ...
```

### Visual Studio
```
Debug → Windows → Call Stack / Locals / Watch
```

---

## ✨ Bonus Features

### Již implementované
- ✅ MVVM architektura
- ✅ Asyncní operace
- ✅ Progress reporting
- ✅ Tray minimalizace
- ✅ Error handling
- ✅ Logging do souboru
- ✅ Update check service
- ✅ Game integrity verification

### Možné budoucí rozšíření
- [ ] Multi-language UI (CZ, EN, DE)
- [ ] Auto-start se systémem
- [ ] Download queue (více her)
- [ ] Torrent support
- [ ] Game arguments editor
- [ ] Performance monitoring
- [ ] Theme customization
- [ ] Proxy support

---

## 📄 Licenční poznámka

Projekt je určen pro edukační účely a vývoj launcheru pro hru Skybound.

---

## 🎯 Příklad Kompletního Workflow

```
1. Spuštění launcheru
   └─> App loads, MainWindow appears, ViewModel loads config

2. Přvní spuštění (hra nenainstalována)
   ├─ CurrentState = NotInstalled
   └─ Uživatel klikne "DOWNLOAD GAME"
      ├─ CurrentState = Downloading
      ├─ DownloadProgress → 0-100%
      ├─ CurrentState = Extracting
      ├─ ExtractionProgress → 0-100%
      ├─ Soubory přemístěny
      ├─ Konfigurace uložena
      ├─ CurrentState = ReadyToPlay
      └─ StatusMessage = "Připraveno! Verze: 1.0.0"

3. Spuštění hry
   ├─ Uživatel klikne "▶ LAUNCH GAME"
   ├─ Ověření integrity
   ├─ Skybound.exe spuštěn
   ├─ Launcher minimalizován do tray
   ├─ CurrentState = GameRunning
   ├─ StatusMessage = "⏵ Hra je spuštěna..."
   ├─ Čekání na zavření hry...
   ├─ Skybound.exe zavřen
   ├─ CurrentState = ReadyToPlay
   └─ Launcher obnovený

4. Odinstalace
   ├─ Uživatel klikne "🗑 UNINSTALL"
   ├─ Potvrzovací dialog
   ├─ %AppData%\Local\Skybound\ smazán
   ├─ Volitelně skybound-game/ smazán
   ├─ Konfigurace vymazána
   ├─ CurrentState = NotInstalled
   └─ StatusMessage = "Odinstalace dokončena!"
```

---

**Hotovo! 🎉 Máte kompletní, profesionální game launcher.**
