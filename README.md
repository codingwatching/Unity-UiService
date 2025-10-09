# GameLovers UI Service

[![Unity Version](https://img.shields.io/badge/Unity-6000.0%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Version](https://img.shields.io/badge/version-0.13.0-green.svg)](CHANGELOG.md)

A powerful and flexible UI management system for Unity that provides a robust abstraction layer for handling game UI with support for layers, async loading, and UI sets. This service streamlines UI development by managing the complete lifecycle of UI presenters, from loading and initialization to display and cleanup.

## Table of Contents

- [Key Features](#key-features)
- [System Requirements](#system-requirements)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
  - [UI Presenter](#ui-presenter)
  - [UI Layers](#ui-layers)
  - [UI Sets](#ui-sets)
  - [UI Configuration](#ui-configuration)
- [API Documentation](#api-documentation)
  - [Creating UI Presenters](#creating-ui-presenters)
  - [Managing UI Lifecycle](#managing-ui-lifecycle)
  - [Working with UI Sets](#working-with-ui-sets)
  - [Async Operations](#async-operations)
- [Advanced Features](#advanced-features)
  - [Delayed UI Presenters](#delayed-ui-presenters)
  - [UI Toolkit Integration](#ui-toolkit-integration)
  - [Helper Views](#helper-views)
- [Package Structure](#package-structure)
- [Dependencies](#dependencies)
- [Migration Guide](#migration-guide)
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
        // Create and initialize the UI service
        _uiService = new UiService();
        _uiService.Init(_uiConfigs);
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

### Creating UI Presenters

#### Simple Presenter

```csharp
public class NotificationPresenter : UiPresenter
{
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private float _displayDuration = 3f;
    
    public void SetMessage(string message)
    {
        _messageText.text = message;
    }
    
    protected override void OnOpened()
    {
        // Auto-close after duration
        StartCoroutine(AutoClose());
    }
    
    private IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(_displayDuration);
        Close(destroy: false);
    }
}
```

#### Delayed UI Presenter

For UI with opening/closing animations:

```csharp
public class AnimatedPopup : DelayUiPresenter
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float _openDuration = 0.5f;
    [SerializeField] private float _closeDuration = 0.3f;
    
    protected override void ConfigureDelayers()
    {
        // Use animation-based delays
        OpeningDelayer = new AnimationDelayer(_animator, "Open");
        ClosingDelayer = new AnimationDelayer(_animator, "Close");
        
        // Or use time-based delays
        // OpeningDelayer = new TimeDelayer(_openDuration);
        // ClosingDelayer = new TimeDelayer(_closeDuration);
    }
    
    protected override void OnOpened()
    {
        base.OnOpened();
        // UI is fully open and animation complete
    }
}
```

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

### Delayed UI Presenters

The `DelayUiPresenter` class provides built-in support for opening/closing animations:

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

### UI Toolkit Integration

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

---

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

#### 5. Optimize Delayed Presenters

```csharp
public class FastPopup : DelayUiPresenter
{
    protected override void ConfigureDelayers()
    {
        // Use short delays for snappy UX
        OpeningDelayer = new TimeDelayer(0.2f);  // 200ms
        ClosingDelayer = new TimeDelayer(0.15f); // 150ms
    }
}
```

**Animation Performance**:
- Keep open/close animations under 0.5 seconds
- Use `TimeDelayer` for simple fades (lighter than `AnimationDelayer`)
- Disable `Animator` component when UI is closed

---

### WebGL Specific Considerations

When building for WebGL, be extra mindful of memory:

```csharp
// WebGL has limited memory - be aggressive with unloading
#if UNITY_WEBGL
    const bool DESTROY_ON_CLOSE = true;
#else
    const bool DESTROY_ON_CLOSE = false;
#endif

_uiService.CloseUi<Shop>(destroy: DESTROY_ON_CLOSE);
```

---

## Troubleshooting

### Need Help Migrating?

If you encounter issues during migration:
1. Check the [CHANGELOG.md](CHANGELOG.md) for detailed version changes
2. Review the [Troubleshooting](#troubleshooting) section below
3. Open an [issue](https://github.com/CoderGamester/com.gamelovers.uiservice/issues) with your migration question

### Common Issues and Solutions

#### Issue: UI Doesn't Appear After Opening

**Symptoms**: `OpenUiAsync` completes but UI is not visible

**Possible Causes & Solutions**:

1. **UI Layer is Behind Other UI**
   ```csharp
   // Check layer numbers - higher numbers appear on top
   // In UiConfigs, ensure critical UI has high layer numbers
   // Layer 0: Background
   // Layer 5: Popups
   ```

2. **Canvas is Disabled**
   ```csharp
   // Check if parent canvas is active
   var canvas = presenter.GetComponentInParent<Canvas>();
   if (!canvas.gameObject.activeInHierarchy)
       Debug.LogError("Parent canvas is disabled!");
   ```

3. **Already Opened**
   ```csharp
   // Service prevents opening already-visible UI
   if (_uiService.IsVisible<MyPresenter>())
   {
       Debug.Log("UI is already open");
   }
   ```

---

#### Issue: Null Reference Exception on GetUi<T>()

**Error**: `KeyNotFoundException` or `NullReferenceException`

**Cause**: Trying to get UI that hasn't been loaded yet

**Solution**:
```csharp
// ❌ BAD - Assumes UI is loaded
var shop = _uiService.GetUi<Shop>();

// ✅ GOOD - Check first
if (_uiService.LoadedPresenters.ContainsKey(typeof(Shop)))
{
    var shop = _uiService.GetUi<Shop>();
}

// ✅ BETTER - Load if needed
var shop = await _uiService.LoadUiAsync<Shop>();
```

---

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

3. **OnOpening/OnClosing Not Called**
   ```csharp
   protected override void OnOpening()
   {
       base.OnOpening(); // ⚠️ Important - calls delayer
       // Your animation code here
   }
   ```

---

#### Issue: Memory Leak - UI Not Unloading

**Symptoms**: Memory usage grows over time, UI count increases

**Diagnosis**:
```csharp
// Add this to a debug UI
void OnGUI()
{
    GUILayout.Label($"Loaded UI: {_uiService.LoadedPresenters.Count}");
    foreach (var ui in _uiService.LoadedPresenters)
    {
        GUILayout.Label($"  - {ui.Key.Name}");
    }
}
```

**Solutions**:

1. **Close With Destroy Flag**
   ```csharp
   // This closes but keeps in memory
   _uiService.CloseUi<Shop>(destroy: false);
   
   // This properly unloads
   _uiService.CloseUi<Shop>(destroy: true);
   ```

2. **Explicitly Unload**
   ```csharp
   _uiService.UnloadUi<Shop>();
   ```

3. **Unload UI Sets on Scene Change**
   ```csharp
   void OnDestroy()
   {
       _uiService.UnloadUiSet(levelSpecificSetId);
   }
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

#### Issue: Loading Spinner Not Appearing

**Symptoms**: Long loads show no feedback to user

**Solutions**:

1. **Set Loading Spinner in UiConfigs**
   ```csharp
   // In UiConfigs ScriptableObject
   // Set "Loading Spinner Type" field to your spinner presenter type
   ```

2. **Ensure Spinner is Preloaded**
   ```csharp
   // Loading spinner is auto-loaded on Init
   // Make sure Init is called before any UI operations
   _uiService.Init(_uiConfigs);
   ```

3. **Check Layer Order**
   ```csharp
   // Loading spinner should be on highest layer
   // e.g., Layer 999 so it appears above everything
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

---

### Debugging Tips

#### Enable Detailed Logging

```csharp
public class DebugUiService : UiService
{
    public override async UniTask<UiPresenter> LoadUiAsync(Type type, bool openAfter = false, CancellationToken ct = default)
    {
        Debug.Log($"[UiService] Loading {type.Name}...");
        var result = await base.LoadUiAsync(type, openAfter, ct);
        Debug.Log($"[UiService] Loaded {type.Name} successfully");
        return result;
    }
}
```

#### Inspect UI State at Runtime

```csharp
// Create a debug window
public class UiDebugWindow : MonoBehaviour
{
    private IUiService _uiService;
    
    void OnGUI()
    {
        GUILayout.Label("=== UI Service Debug ===");
        GUILayout.Label($"Loaded: {_uiService.LoadedPresenters.Count}");
        GUILayout.Label($"Visible: {_uiService.VisiblePresenters.Count}");
        GUILayout.Label($"Layers: {_uiService.Layers.Count}");
        
        foreach (var type in _uiService.VisiblePresenters)
        {
            GUILayout.Label($"  [VISIBLE] {type.Name}");
        }
    }
}
```

---

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
