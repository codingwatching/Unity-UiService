# GameLovers UI Service

[![Unity Version](https://img.shields.io/badge/Unity-6000.0%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Version](https://img.shields.io/badge/version-1.0.0-green.svg)](CHANGELOG.md)

A powerful and flexible UI management system for Unity that provides a robust abstraction layer for handling game UI with support for layers, async loading, and UI sets. This service streamlines UI development by managing the complete lifecycle of UI presenters, from loading and initialization to display and cleanup.

## Table of Contents

- [Key Features](#key-features)
- [System Requirements](#system-requirements)
- [Installation](#installation)
- [Package Structure](#package-structure)
- [Dependencies](#dependencies)
- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
  - [Editor Windows](#editor-windows)
  - [UI Presenter](#ui-presenter)
  - [UI Layers](#ui-layers)
  - [UI Sets](#ui-sets)
  - [UI Configuration](#ui-configuration)
- [API Documentation](#api-documentation)
  - [Managing UI Lifecycle](#managing-ui-lifecycle)
  - [Working with UI Sets](#working-with-ui-sets)
  - [Async Operations](#async-operations)
- [Advanced Features](#advanced-features)
  - [UI Analytics and Performance Tracking](#ui-analytics-and-performance-tracking)
  - [Helper Views](#helper-views)
- [Performance Optimization](#performance-optimization)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [Support](#support)
- [License](#license)

## Key Features

- **🎭 UI Presenter Pattern** - Clean separation of UI logic with lifecycle management
- **📚 Layer-based Organization** - Organize UI elements by depth layers
- **🔄 Async Loading** - Load UI assets asynchronously with UniTask support
- **📦 UI Sets** - Group related UI elements for batch operations
- **💾 Memory Management** - Efficient loading/unloading of UI assets
- **🎯 Type-safe API** - Generic methods for compile-time safety
- **📊 Analytics & Performance Tracking** - Optional analytics system with dependency injection
- **📱 Responsive Design** - Built-in support for safe areas and screen size adjustments
- **🔧 Addressables Integration** - Seamless integration with Unity's Addressables system
- **🎨 UI Toolkit Support** - Compatible with both uGUI and UI Toolkit

## System Requirements

- **Unity** 6000.0 or higher
- **Addressables** 1.22.0 or higher
- **UniTask** 2.5.10 or higher
- **TextMeshPro** 3.0.9 or higher
- **Git** (for installation via Package Manager)

## Installation

### Via Unity Package Manager (Recommended)

1. Open Unity Package Manager (`Window` → `Package Manager`)
2. Click the `+` button and select `Add package from git URL`
3. Enter the following URL:
   ```
   https://github.com/CoderGamester/com.gamelovers.uiservice.git
   ```

### Via manifest.json

Add the following line to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.gamelovers.uiservice": "https://github.com/CoderGamester/com.gamelovers.uiservice.git"
  }
}
```

## Package Structure

```
<root>
  ├── package.json          # Package manifest with dependencies and metadata
  ├── README.md             # This documentation file
  ├── CHANGELOG.md          # Version history and release notes
  ├── LICENSE.md            # MIT license terms
  ├── Runtime/              # Core runtime scripts for the UI service
  │   ├── *.asmdef          # Assembly definition for runtime code
  │   ├── Core Services     # Main UI service implementation and interfaces
  │   ├── Presenters        # Base classes for UI presenters and UI Toolkit support
  │   ├── Configuration     # ScriptableObject configs for UI setup
  │   ├── Asset Loading     # Addressables integration and loading utilities
  │   ├── Delayers/         # Animation and time-based delay implementations
  │   └── Views/            # Helper components for responsive and optimized UI
  └── Editor/               # Unity Editor extensions and custom inspectors
      ├── *.asmdef          # Assembly definition for editor code
      ├── Config Editors    # Custom inspectors for UI configuration assets
      └── View Editors      # Custom editors for specialized UI components
```

## Dependencies

This package requires:

- **[Unity Addressables](https://docs.unity3d.com/Packages/com.unity.addressables@latest)** (v2.6.0+) - For async asset loading
- **[UniTask](https://github.com/Cysharp/UniTask)** (v2.5.10+) - For efficient async operations

Dependencies are automatically resolved when installing via Unity Package Manager.

## Quick Start

### 1. Create UI Configuration

First, create a UI configuration asset:

1. Right-click in Project View
2. Navigate to `Create` → `ScriptableObjects` → `Configs` → `UiConfigs`
3. Configure your UI presenters in the created asset

### 2. Initialize the UI Service

```csharp
using UnityEngine;
using GameLovers.UiService;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private UiConfigs _uiConfigs;
    private IUiServiceInit _uiService;
    
    void Start()
    {
        // Create and initialize the UI service (without analytics)
        _uiService = new UiService();
        _uiService.Init(_uiConfigs);
        
        // Or with analytics (opt-in)
        // var analytics = new UiAnalytics();
        // _uiService = new UiService(new UiAssetLoader(), analytics);
        // _uiService.Init(_uiConfigs);
    }
}
```

### 3. Create Your First UI Presenter

```csharp
using UnityEngine;
using GameLovers.UiService;

public class MainMenuPresenter : UiPresenter
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _settingsButton;
    
    protected override void OnInitialized()
    {
        _playButton.onClick.AddListener(OnPlayClicked);
        _settingsButton.onClick.AddListener(OnSettingsClicked);
    }
    
    protected override void OnOpened()
    {
        Debug.Log("Main menu opened!");
        // Perform opening animations or setup
    }
    
    protected override void OnClosed()
    {
        Debug.Log("Main menu closed!");
        // Cleanup or save state
    }
    
    private void OnPlayClicked()
    {
        Close(destroy: false);
        _uiService.OpenUiAsync<GameplayHudPresenter>();
    }
    
    private async void OnSettingsClicked()
    {
        var settings = await _uiService.OpenUiAsync<SettingsPresenter>();
        // Settings is now open and ready
    }
}
```

### 4. Open and Manage UI

```csharp
public class GameManager : MonoBehaviour
{
    private IUiService _uiService;
    
    async void Start()
    {
        // Open main menu
        var mainMenu = await _uiService.OpenUiAsync<MainMenuPresenter>();
        
        // Check if a UI is visible
        if (_uiService.IsVisible<MainMenuPresenter>())
        {
            Debug.Log("Main menu is currently visible");
        }
        
        // Close specific UI
        _uiService.CloseUi<MainMenuPresenter>();
        
        // Close all UI
        _uiService.CloseAllUi();
    }
}
```

## Core Concepts

### Editor Windows

The package includes powerful editor windows for managing and monitoring your UI system:

#### Analytics Window

Monitor UI performance metrics and events in real-time during play mode.

**Opening the Window:**
- Navigate to **Tools → UI Service → Analytics**

**Features:**
- **Real-time Performance Metrics** - View load, open, and close durations for each UI
- **Usage Statistics** - Track open/close counts and total lifetime for each presenter
- **Color-coded Performance** - Visual indicators for slow operations (green/yellow/red)
- **Timeline Information** - See when UIs were first opened and last closed
- **Auto-refresh** - Automatically updates metrics while playing
- **Clear Data** - Reset all collected analytics
- **Log Summary** - Export performance summary to console

**Usage:**
1. Open **Tools → UI Service → Analytics**
2. Enter Play Mode
3. Use your UI system normally
4. View metrics updating in real-time

**Performance Thresholds:**
- **Load Time**: <0.1s (green), <0.5s (yellow), ≥0.5s (red)
- **Open/Close Time**: <0.05s (green), <0.2s (yellow), ≥0.2s (red)

#### Hierarchy Window

View and control all active UI presenters in the scene during play mode.

**Features:**
- **Live UI Hierarchy** - See all loaded presenters grouped by layer
- **Status Indicators** - Visual open (🟢) / closed (🔴) status for each UI
- **Quick Controls** - Open/close any UI with one click
- **Detailed Inspector** - Expand presenters to see full details
- **GameObject Navigation** - Select and ping presenters in the hierarchy
- **Batch Operations** - Close all UIs at once
- **Auto-refresh** - Updates every 0.5 seconds automatically

**Usage:**
1. Open **Tools → UI Service → Hierarchy Window**
2. Enter Play Mode
3. See all active presenters organized by layer
4. Click on any presenter to expand details
5. Use quick controls to open/close/destroy UIs

#### Layer Visualizer

Visualize your UI configuration and layer organization before entering play mode.

**Features:**
- **Layer Organization** - See all UIs grouped by layer number
- **Color-coded Layers** - Each layer has a unique color for easy identification
- **Configuration Overview** - View UiConfigs structure without playing
- **Search & Filter** - Find specific UIs quickly
- **Statistics** - Total UIs, layer counts, sync vs async loading
- **Synchronous Loading Indicators** - Clearly marks UIs that load synchronously

**Usage:**
1. Open **Tools → UI Service → Layer Visualizer**
2. Select or auto-find your UiConfigs asset
3. Browse layer hierarchy and configuration
4. Use search to filter specific UIs
5. Review statistics for optimization insights

### UI Presenter

The `UiPresenter` is the base class for all UI elements in the system. It provides:

- **Lifecycle callbacks** - `OnInitialized()`, `OnOpened()`, `OnClosed()`
- **State management** - Track open/closed state
- **Service integration** - Direct access to UI service

#### Basic UI Presenter

```csharp
public class BasicPopup : UiPresenter
{
    protected override void OnInitialized()
    {
        // Called once when the presenter is first loaded
        // Set up UI elements, subscribe to events
    }
    
    protected override void OnOpened()
    {
        // Called every time the UI is shown
        // Start animations, refresh data
    }
    
    protected override void OnClosed()
    {
        // Called when the UI is hidden
        // Stop animations, save state
    }
}
```

#### UI Presenter with Data

For UI that needs initialization data:

```csharp
public struct PlayerProfileData
{
    public string PlayerName;
    public int Level;
    public Sprite Avatar;
}

public class PlayerProfilePresenter : UiPresenter<PlayerProfileData>
{
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _levelText;
    [SerializeField] private Image _avatarImage;
    
    protected override void OnSetData()
    {
        // Called when data is set
        _nameText.text = Data.PlayerName;
        _levelText.text = $"Level {Data.Level}";
        _avatarImage.sprite = Data.Avatar;
    }
}

// Usage
var profileData = new PlayerProfileData 
{ 
    PlayerName = "Hero", 
    Level = 42,
    Avatar = avatarSprite
};

await _uiService.OpenUiAsync<PlayerProfilePresenter, PlayerProfileData>(profileData);
```

#### UI Toolkit Integration

For UI Toolkit (UI Elements) support:

```csharp
public class UIToolkitMenu : UiToolkitPresenter
{
    [SerializeField] private UIDocument _document;
    
    private Button _playButton;
    private Label _titleLabel;
    
    protected override void OnInitialized()
    {
        var root = _document.rootVisualElement;
        
        _playButton = root.Q<Button>("play-button");
        _titleLabel = root.Q<Label>("title-label");
        
        _playButton.clicked += OnPlayClicked;
    }
    
    private void OnPlayClicked()
    {
        Close(destroy: false);
    }
}
```

#### Delayed UI Presenter

For UI with opening/closing animations:

```csharp
public class SlideInPanel : DelayUiPresenter
{
    [SerializeField] private RectTransform _panel;
    [SerializeField] private float _slideDuration = 0.5f;
    
    protected override void ConfigureDelayers()
    {
        OpeningDelayer = new TimeDelayer(_slideDuration);
        ClosingDelayer = new TimeDelayer(_slideDuration);
    }
    
    protected override void OnPreOpen()
    {
        // Position panel off-screen
        _panel.anchoredPosition = new Vector2(-1000, 0);
    }
    
    protected override void OnOpening()
    {
        // Animate panel sliding in
        _panel.DOAnchorPosX(0, _slideDuration);
    }
    
    protected override void OnClosing()
    {
        // Animate panel sliding out
        _panel.DOAnchorPosX(-1000, _slideDuration);
    }
}
```

### UI Layers

UI elements are organized into layers, where higher layer numbers appear on top:

```csharp
// Configure layers in your UiConfigs asset
// Layer 0: Background UI
// Layer 1: Game HUD
// Layer 2: Menus
// Layer 3: Popups
// Layer 4: System messages

// Close all UI in a specific layer
_uiService.CloseAllUi(layer: 2);
```

### UI Sets

Group related UI elements for batch operations:

```csharp
// Define UI sets in your UiConfigs
// Set 1: Main Menu Set (logo, menu, background)
// Set 2: Gameplay Set (HUD, minimap, controls)
// Set 3: Shop Set (shop window, inventory, currency display)

// Load entire UI set
var loadTasks = _uiService.LoadUiSetAsync(setId: 2);
await UniTask.WhenAll(loadTasks);

// Close entire UI set
_uiService.CloseAllUiSet(setId: 2);

// Unload UI set from memory
_uiService.UnloadUiSet(setId: 2);
```

### UI Configuration

Configure your UI in the `UiConfigs` ScriptableObject:

1. **Type** - The presenter class type
2. **Prefab Reference** - Addressable reference to the UI prefab
3. **Layer** - Which layer the UI belongs to
4. **Load Synchronously** - Whether to load synchronously (use sparingly)
5. **UI Set ID** - Optional grouping ID

## API Documentation

### Managing UI Lifecycle

#### Loading and Unloading

```csharp
// Load UI into memory without opening
var ui = await _uiService.LoadUiAsync<InventoryPresenter>();

// Load and immediately open
var ui = await _uiService.LoadUiAsync<InventoryPresenter>(openAfter: true);

// Check if loaded
if (_uiService.LoadedPresenters.ContainsKey(typeof(InventoryPresenter)))
{
    // UI is loaded in memory
}

// Unload from memory
_uiService.UnloadUi<InventoryPresenter>();
```

#### Opening and Closing

```csharp
// Open UI (loads if necessary)
var shop = await _uiService.OpenUiAsync<ShopPresenter>();

// Open with data
var questData = new QuestData { QuestId = 101, Title = "Dragon Slayer" };
await _uiService.OpenUiAsync<QuestPresenter, QuestData>(questData);

// Close UI (keeps in memory)
_uiService.CloseUi<ShopPresenter>();

// Close and destroy
_uiService.CloseUi<ShopPresenter>(destroy: true);

// Get UI if loaded
var hud = _uiService.GetUi<GameHudPresenter>();
```

### Working with UI Sets

```csharp
// Load all UI in a set
var loadTasks = _uiService.LoadUiSetAsync(setId: 1);
var presenters = await UniTask.WhenAll(loadTasks);

// Add runtime UI to service
var dynamicUi = Instantiate(uiPrefab);
_uiService.AddUi(dynamicUi, layer: 3, openAfter: true);

// Remove UI set and get removed presenters
var removedPresenters = _uiService.RemoveUiSet(setId: 2);
foreach (var presenter in removedPresenters)
{
    Destroy(presenter.gameObject);
}
```

### Async Operations

All async operations use UniTask for better performance and WebGL support:

```csharp
// Sequential loading
var menu = await _uiService.OpenUiAsync<MainMenuPresenter>();
var settings = await _uiService.OpenUiAsync<SettingsPresenter>();

// Parallel loading
var menuTask = _uiService.OpenUiAsync<MainMenuPresenter>();
var hudTask = _uiService.OpenUiAsync<GameHudPresenter>();
await UniTask.WhenAll(menuTask, hudTask);

// With cancellation
var cts = new CancellationTokenSource();
try
{
    await _uiService.OpenUiAsync<LoadingPresenter>()
        .AttachExternalCancellation(cts.Token);
}
catch (OperationCanceledException)
{
    Debug.Log("UI loading was cancelled");
}
```

## Advanced Features

### UI Analytics and Performance Tracking

The UI Service includes an optional analytics system for tracking UI events and performance metrics. Analytics is **opt-in** via dependency injection.

#### Enabling Analytics

```csharp
using GameLovers.UiService;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private UiConfigs _uiConfigs;
    
    void Start()
    {
        // Create analytics instance
        var analytics = new UiAnalytics();
        
        // Inject analytics into UI service
        var uiService = new UiService(new UiAssetLoader(), analytics);
        uiService.Init(_uiConfigs);
        
        // Analytics is now tracking all UI events
    }
}
```

#### Running Without Analytics

By default, the UI Service runs without analytics overhead:

```csharp
// No analytics - uses NullAnalytics internally (zero overhead)
var uiService = new UiService();
uiService.Init(_uiConfigs);

// Or explicitly without analytics
var uiService = new UiService(new UiAssetLoader(), null);
```

#### What Gets Tracked

When analytics is enabled, the system automatically tracks:

- **UI Loading** - Time to load UI from Addressables
- **UI Opening** - Time to complete opening animations
- **UI Closing** - Time to complete closing animations
- **UI Lifecycle** - Total lifetime from first open to final close
- **Open/Close Counts** - How many times each UI was opened/closed
- **Timestamps** - When UI was first opened and last closed

#### Accessing Performance Metrics

```csharp
public class PerformanceMonitor : MonoBehaviour
{
    private IUiService _uiService;
    
    void LogPerformance()
    {
        var analytics = _uiService.Analytics;
        
        // Get all metrics
        foreach (var kvp in analytics.PerformanceMetrics)
        {
            var metrics = kvp.Value;
            Debug.Log($"{metrics.UiName}:");
            Debug.Log($"  Load Time: {metrics.LoadDuration:F3}s");
            Debug.Log($"  Open Time: {metrics.OpenDuration:F3}s");
            Debug.Log($"  Close Time: {metrics.CloseDuration:F3}s");
            Debug.Log($"  Opened {metrics.OpenCount} times");
            Debug.Log($"  Total Lifetime: {metrics.TotalLifetime:F1}s");
        }
        
        // Get specific UI metrics
        var shopMetrics = analytics.GetMetrics(typeof(ShopPresenter));
        if (shopMetrics.OpenCount > 0)
        {
            Debug.Log($"Shop was opened {shopMetrics.OpenCount} times");
        }
        
        // Print summary to console
        analytics.LogPerformanceSummary();
    }
}
```

#### Subscribing to Analytics Events

```csharp
public class AnalyticsListener : MonoBehaviour
{
    private IUiService _uiService;
    
    void Start()
    {
        var analytics = _uiService.Analytics;
        
        // Subscribe to Unity Events
        analytics.OnUiOpened.AddListener(OnUiOpened);
        analytics.OnUiClosed.AddListener(OnUiClosed);
        analytics.OnPerformanceMetricsUpdated.AddListener(OnMetricsUpdated);
    }
    
    private void OnUiOpened(UiEventData data)
    {
        Debug.Log($"UI Opened: {data.UiName} on layer {data.Layer}");
        // Send to your analytics backend
        SendToBackend("ui_opened", data.UiName);
    }
    
    private void OnUiClosed(UiEventData data)
    {
        Debug.Log($"UI Closed: {data.UiName} (destroyed: {data.WasDestroyed})");
    }
    
    private void OnMetricsUpdated(UiPerformanceMetrics metrics)
    {
        // Track performance issues
        if (metrics.LoadDuration > 1.0f)
        {
            Debug.LogWarning($"{metrics.UiName} took {metrics.LoadDuration:F2}s to load!");
        }
    }
}
```

#### Custom Analytics Integration

Implement `IUiAnalyticsCallback` for custom analytics backends:

```csharp
public class CustomAnalytics : IUiAnalyticsCallback
{
    public void OnUiLoaded(UiEventData data)
    {
        // Send to Firebase, Unity Analytics, or custom backend
        FirebaseAnalytics.LogEvent("ui_loaded", new Parameter("ui_name", data.UiName));
    }
    
    public void OnUiOpened(UiEventData data)
    {
        MyAnalyticsService.Track("ui_opened", new { 
            name = data.UiName, 
            layer = data.Layer,
            timestamp = data.Timestamp 
        });
    }
    
    public void OnUiClosed(UiEventData data)
    {
        MyAnalyticsService.Track("ui_closed", new { 
            name = data.UiName,
            destroyed = data.WasDestroyed
        });
    }
    
    public void OnUiUnloaded(UiEventData data)
    {
        // Track memory management
    }
    
    public void OnPerformanceMetricsUpdated(UiPerformanceMetrics metrics)
    {
        // Send performance data
        if (metrics.LoadDuration > 0.5f)
        {
            MyAnalyticsService.TrackPerformance("slow_ui_load", metrics.UiName, metrics.LoadDuration);
        }
    }
}

// Register callback
void Start()
{
    var analytics = new UiAnalytics();
    analytics.SetCallback(new CustomAnalytics());
    
    var uiService = new UiService(new UiAssetLoader(), analytics);
    uiService.Init(_uiConfigs);
}
```

#### Creating Custom Analytics Implementations

You can create custom analytics implementations for special use cases:

```csharp
public class FileLoggingAnalytics : IUiAnalytics
{
    private readonly UiAnalytics _baseAnalytics = new UiAnalytics();
    private readonly StreamWriter _logFile;
    
    public FileLoggingAnalytics(string logPath)
    {
        _logFile = new StreamWriter(logPath, append: true);
    }
    
    public void TrackOpenComplete(Type uiType, int layer)
    {
        // Log to file
        _logFile.WriteLine($"{DateTime.Now:HH:mm:ss} - Opened: {uiType.Name}");
        _logFile.Flush();
        
        // Also track normally
        _baseAnalytics.TrackOpenComplete(uiType, layer);
    }
    
    // Implement other IUiAnalytics methods...
    // Delegate to _baseAnalytics for standard tracking
}

// Usage
var analytics = new FileLoggingAnalytics("ui_events.log");
var uiService = new UiService(new UiAssetLoader(), analytics);
```

### Helper Views

The package includes several helper components:

#### Safe Area Helper

Automatically adjusts UI for device safe areas (notches, rounded corners):

```csharp
// Add SafeAreaHelperView component to UI panels that should respect safe areas
[RequireComponent(typeof(RectTransform))]
public class SafeAreaPanel : MonoBehaviour
{
    void Awake()
    {
        gameObject.AddComponent<SafeAreaHelperView>();
    }
}
```

#### Non-Drawing View

Optimize UI performance by making non-visible graphics non-rendering:

```csharp
// Add to invisible UI elements that need to block raycasts
gameObject.AddComponent<NonDrawingView>();
```

#### Screen Size Fitter

Responsive UI that adjusts to screen size:

```csharp
// Add AdjustScreenSizeFitterView for responsive layouts
var fitter = gameObject.AddComponent<AdjustScreenSizeFitterView>();
// Configure min/max sizes in the inspector
```

#### Interactable Text View

Make text elements interactive with clickable links:

```csharp
// Add InteractableTextView to TextMeshPro components
var interactableText = gameObject.AddComponent<InteractableTextView>();
// Supports <link> tags in TextMeshPro for clickable URLs
```

## Performance Optimization

### Memory Management Best Practices

#### When to Load vs. Preload UI

**Load on Demand** (Lazy Loading):
```csharp
// Best for: Infrequently used UI, large asset sizes
await _uiService.OpenUiAsync<SettingsMenu>();
// Loads and opens in one call - UI loads when needed
```

**Preload** (Eager Loading):
```csharp
// Best for: Frequently used UI, critical path, avoid loading hitches
await _uiService.LoadUiAsync<GameHud>();
// Loads into memory but keeps hidden - ready to open instantly
```

**When to Use Each Strategy**:

| Strategy | Use For | Pros | Cons |
|----------|---------|------|------|
| **On Demand** | Settings, shops, rare popups | Lower initial memory | Loading hitch when opened |
| **Preload** | HUD, frequent dialogs | Instant display | Higher memory usage |
| **Preload Sets** | Level-specific UI groups | Batch loading efficiency | Memory overhead |

#### Unloading UI from Memory

```csharp
// Close but keep in memory (fast to reopen)
_uiService.CloseUi<Shop>(destroy: false);

// Close and unload from memory (free resources)
_uiService.CloseUi<Shop>(destroy: true);
// Or explicitly:
_uiService.UnloadUi<Shop>();
```

**When to Destroy**:
- ✅ Large UI assets (>5MB)
- ✅ Level-specific UI when changing levels
- ✅ One-time tutorial/onboarding screens
- ❌ Frequently reopened UI (Settings, Pause Menu)
- ❌ Small, lightweight presenters

#### UI Sets for Efficient Batch Management

```csharp
// Define in UiConfigs: SetId 1 = Main Menu Set
// Contains: MainMenu, Background, Logo, Buttons

// Load entire menu system at once
var tasks = _uiService.LoadUiSetAsync(setId: 1);
await UniTask.WhenAll(tasks);

// When done with menu (e.g., entering gameplay):
_uiService.CloseAllUiSet(setId: 1);
_uiService.UnloadUiSet(setId: 1); // Free all menu memory
```

**Recommended Set Organization**:
- Set 0: Core/Persistent UI (always loaded)
- Set 1-10: Scene-specific UI (load per scene)
- Set 11-20: Feature-specific UI (shop, inventory, etc.)

---

### Performance Monitoring

#### Detecting Memory Issues

```csharp
// Check what's currently loaded
Debug.Log($"Loaded presenters: {_uiService.LoadedPresenters.Count}");
foreach (var kvp in _uiService.LoadedPresenters)
{
    Debug.Log($"  - {kvp.Key.Name}");
}

// Monitor visible UI
Debug.Log($"Visible presenters: {_uiService.VisiblePresenters.Count}");
```

#### Profiling UI Operations

Use Unity Profiler to monitor:
1. **Memory.Allocations** - Watch for GC spikes during `OpenUiAsync`
2. **Memory.Total** - Track memory after `LoadUiAsync` / `UnloadUi`
3. **Loading.AsyncLoad** - Identify slow-loading UI assets

**Red Flags**:
- More than 10 presenters loaded simultaneously
- Loading same UI multiple times per second
- Memory not decreasing after `UnloadUi`

---

### Optimization Tips

#### 1. Avoid Opening Same UI Repeatedly

```csharp
// ❌ BAD - Opens new instance every frame
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape))
        _uiService.OpenUiAsync<PauseMenu>().Forget();
}

// ✅ GOOD - Check if already visible
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape) && !_uiService.IsVisible<PauseMenu>())
        _uiService.OpenUiAsync<PauseMenu>().Forget();
}
```

#### 2. Use Parallel Loading for Multiple UI

```csharp
// ❌ SLOW - Sequential loading (3 seconds total if each takes 1s)
await _uiService.OpenUiAsync<Hud>();
await _uiService.OpenUiAsync<Minimap>();
await _uiService.OpenUiAsync<Chat>();

// ✅ FAST - Parallel loading (1 second total)
await UniTask.WhenAll(
    _uiService.OpenUiAsync<Hud>(),
    _uiService.OpenUiAsync<Minimap>(),
    _uiService.OpenUiAsync<Chat>()
);
```

#### 3. Preload Critical UI During Loading Screen

```csharp
public async UniTask LoadLevel()
{
    // Show loading screen
    await _uiService.OpenUiAsync<LoadingScreen>();
    
    // Preload level UI in parallel with level load
    var uiTask = UniTask.WhenAll(
        _uiService.LoadUiAsync<GameHud>(),
        _uiService.LoadUiAsync<PauseMenu>(),
        _uiService.LoadUiAsync<GameOverScreen>()
    );
    var levelTask = SceneManager.LoadSceneAsync("GameLevel").ToUniTask();
    
    await UniTask.WhenAll(uiTask, levelTask);
    
    // Hide loading screen
    _uiService.CloseUi<LoadingScreen>();
}
```

#### 4. Clean Up When Switching Scenes

```csharp
async void OnLevelComplete()
{
    // Close all gameplay UI
    _uiService.CloseAllUi(layer: 1); // Gameplay layer
    
    // Unload gameplay UI set
    _uiService.UnloadUiSet(setId: 2);
    
    // Load main menu
    await _uiService.OpenUiAsync<LevelCompleteScreen>();
}
```

**Animation Performance**:
- Keep open/close animations under 0.5 seconds
- Use `TimeDelayer` for simple fades (lighter than `AnimationDelayer`)
- Disable `Animator` component when UI is closed

## Troubleshooting

### Common Issues and Solutions

#### Issue: Data Not Showing in UiPresenter<TData>

**Symptoms**: UI opens but data fields are empty/default

**Possible Causes & Solutions**:

1. **Not Using Generic Open Method**
   ```csharp
   // ❌ WRONG - Data not passed
   await _uiService.OpenUiAsync<PlayerProfile>();
   
   // ✅ CORRECT - Use generic overload
   var data = new PlayerData { Name = "Hero", Level = 10 };
   await _uiService.OpenUiAsync<PlayerProfile, PlayerData>(data);
   ```

2. **OnSetData Not Implemented**
   ```csharp
   public class PlayerProfile : UiPresenter<PlayerData>
   {
       // ❌ MISSING - Override OnSetData
       
       // ✅ CORRECT
       protected override void OnSetData()
       {
           nameText.text = Data.Name;
           levelText.text = Data.Level.ToString();
       }
   }
   ```

3. **Wrong Base Class**
   ```csharp
   // ❌ WRONG - Missing <T> generic
   public class PlayerProfile : UiPresenter
   
   // ✅ CORRECT
   public class PlayerProfile : UiPresenter<PlayerData>
   ```

---

#### Issue: Animations Not Playing

**Symptoms**: `DelayUiPresenter` opens/closes instantly without animation

**Possible Causes & Solutions**:

1. **Delayers Not Configured**
   ```csharp
   public class AnimatedPopup : DelayUiPresenter
   {
       // ❌ MISSING - ConfigureDelayers not called
       
       // ✅ CORRECT
       protected override void ConfigureDelayers()
       {
           OpeningDelayer = new AnimationDelayer(_animator, "Open");
           ClosingDelayer = new AnimationDelayer(_animator, "Close");
       }
   }
   ```

2. **Animation Clip Not Assigned**
   ```csharp
   // Check if animator has the specified clip
   // Ensure clip names match: "Open", "Close"
   // Or use TimeDelayer if animations aren't ready:
   OpeningDelayer = new TimeDelayer(0.5f);
   ```

---

#### Issue: "UiConfig was not added to the service" Error

**Error**: `KeyNotFoundException: The UiConfig of type X was not added to the service`

**Cause**: UI type not registered in `UiConfigs` asset

**Solution**:
1. Open your `UiConfigs` ScriptableObject asset
2. Add a new entry for your presenter type
3. Set the addressable reference to your UI prefab
4. Set the layer number
5. Save the asset

```csharp
// In UiConfigs inspector:
// Type: MyNewPresenter
// Addressable: Assets/UI/MyNewPresenter.prefab
// Layer: 2
```

---

#### Issue: UI Flickers or Appears Briefly Before Opening

**Cause**: UI GameObject is active during initialization

**Solution**: UiService automatically handles this, but if creating UI manually:
```csharp
// Ensure prefab's root GameObject is DISABLED in the prefab
// The service will enable it when ready
```

---

#### Issue: Cancellation Not Working

**Symptoms**: `CancellationToken` doesn't stop UI loading

**Possible Causes**:

1. **Token Not Passed Through**
   ```csharp
   // ❌ WRONG - Token not used
   await _uiService.OpenUiAsync<Shop>();
   
   // ✅ CORRECT - Pass token
   var cts = new CancellationTokenSource();
   await _uiService.OpenUiAsync<Shop>(cancellationToken: cts.Token);
   ```

2. **Cancelling After Load Complete**
   ```csharp
   // Can only cancel during async load, not after
   var task = _uiService.OpenUiAsync<Shop>(ct);
   await task; // Already complete
   cts.Cancel(); // Too late - no effect
   ```

### Still Having Issues?

1. **Check Unity Console** - Look for warnings/errors from UiService
2. **Verify Dependencies** - Ensure UniTask and Addressables are installed
3. **Review CHANGELOG** - Check if recent version introduced breaking changes
4. **Search GitHub Issues** - Your issue may already be reported
5. **Create New Issue** - [Report bug](https://github.com/CoderGamester/com.gamelovers.uiservice/issues) with:
   - Unity version
   - Package version
   - Code sample
   - Error logs
   - Steps to reproduce

---

## Contributing

We welcome contributions from the community! Here's how you can help:

### Reporting Issues

- Use the [GitHub Issues](https://github.com/CoderGamester/com.gamelovers.uiservice/issues) page
- Include your Unity version, package version, and reproduction steps
- Attach relevant code samples, error logs, or screenshots

### Development Setup

1. Fork the repository on GitHub
2. Clone your fork: `git clone https://github.com/yourusername/com.gamelovers.uiservice.git`
3. Create a feature branch: `git checkout -b feature/amazing-feature`
4. Make your changes with tests
5. Commit: `git commit -m 'Add amazing feature'`
6. Push: `git push origin feature/amazing-feature`
7. Create a Pull Request

### Code Guidelines

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Add XML documentation to all public APIs
- Include unit tests for new features
- Maintain backward compatibility when possible
- Update CHANGELOG.md for notable changes

### Pull Request Process

1. Ensure all tests pass
2. Update documentation if needed
3. Add changelog entry if applicable
4. Request review from maintainers

## Support

### Documentation

- **API Reference**: See inline XML documentation in code
- **Examples**: Check sample implementations in this README
- **Changelog**: See [CHANGELOG.md](CHANGELOG.md) for version history

### Getting Help

- **Issues**: [Report bugs or request features](https://github.com/CoderGamester/com.gamelovers.uiservice/issues)
- **Discussions**: [Ask questions and share ideas](https://github.com/CoderGamester/com.gamelovers.uiservice/discussions)

### Community

- Follow [@CoderGamester](https://github.com/CoderGamester) for updates
- Star the repository if you find it useful
- Share your projects using this package

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

---

**Made with ❤️ for the Unity community**

*If this package helps your project, please consider giving it a ⭐ on GitHub!*
