# UI Service Samples

This folder contains example implementations demonstrating the **pure feature composition** pattern of the UI Service.

---

## Architecture Overview

All samples use the new self-contained feature pattern:
- **Base:** `UiPresenter` or `UiPresenter<T>`
- **Features:** Self-contained components (TimeDelayFeature, AnimationDelayFeature, UiToolkitPresenterFeature)
- **Composition:** Mix features as needed using `[RequireComponent]`

---

## Samples Included

### 1. BasicUiFlow

**Files:**
- `BasicUiExamplePresenter.cs` - Simple presenter without features
- `BasicUiFlowExample.cs` - Scene setup and UI service usage

**Demonstrates:**
- Basic UI presenter lifecycle
- Opening and closing UI
- Simple button interactions

**Pattern:**
```csharp
public class BasicUiExamplePresenter : UiPresenter
{
    protected override void OnOpened() { }
    protected override void OnClosed() { }
}
```

---

### 2. DataPresenter

**Files:**
- `DataUiExamplePresenter.cs` - Presenter with data
- `DataPresenterExample.cs` - Data-driven UI example

**Demonstrates:**
- Using `UiPresenter<T>` for data-driven UI
- Setting and accessing presenter data
- `OnSetData()` lifecycle hook

**Pattern:**
```csharp
public struct MyData
{
    public string Title;
}

public class DataUiExamplePresenter : UiPresenter<MyData>
{
    protected override void OnSetData()
    {
        // Use Data.Title
    }
}
```

---

### 3. DelayedPresenter

**Files:**
- `DelayedUiExamplePresenter.cs` - Time-based delays
- `AnimatedUiExamplePresenter.cs` - Animation-based delays
- `DelayedPresenterExample.cs` - Scene setup

**Demonstrates:**
- Using `TimeDelayFeature` for time-based delays
- Using `AnimationDelayFeature` for animation-synchronized delays
- Subscribing to delay completion events
- Configuring delays in inspector

**Pattern (Time Delay):**
```csharp
[RequireComponent(typeof(TimeDelayFeature))]
public class DelayedUiExamplePresenter : UiPresenter
{
    [SerializeField] private TimeDelayFeature _delayFeature;
    
    protected override void OnInitialized()
    {
        _delayFeature.OnOpenCompletedEvent += OnDelayComplete;
    }
    
    private void OnDelayComplete()
    {
        // Called after delay finishes
    }
    
    private void OnDestroy()
    {
        _delayFeature.OnOpenCompletedEvent -= OnDelayComplete;
    }
}
```

**Pattern (Animation Delay):**
```csharp
[RequireComponent(typeof(AnimationDelayFeature))]
public class AnimatedUiExamplePresenter : UiPresenter
{
    [SerializeField] private AnimationDelayFeature _animationFeature;
    
    protected override void OnInitialized()
    {
        _animationFeature.OnOpenCompletedEvent += OnAnimationComplete;
    }
    
    private void OnAnimationComplete()
    {
        // Called after animation finishes
    }
}
```

**Setup:**
1. Attach presenter to GameObject
2. Add `TimeDelayFeature` or `AnimationDelayFeature` component
3. Configure delay times (time) or animation clips (animation) in inspector
4. Feature handles all timing automatically

---

### 4. UiToolkit

**Files:**
- `UiToolkitExamplePresenter.cs` - UI Toolkit presenter
- `UiToolkitExample.cs` - Scene setup
- `UiToolkitExample.uxml` - UI Toolkit layout

**Demonstrates:**
- Using `UiToolkitPresenterFeature` for UI Toolkit integration
- Querying VisualElements from the root
- Binding UI Toolkit events
- Clean event subscription management

**Pattern:**
```csharp
[RequireComponent(typeof(UiToolkitPresenterFeature))]
public class UiToolkitExamplePresenter : UiPresenter
{
    [SerializeField] private UiToolkitPresenterFeature _toolkitFeature;
    
    protected override void OnInitialized()
    {
        var root = _toolkitFeature.Root;
        var button = root.Q<Button>("MyButton");
        button.clicked += OnButtonClicked;
    }
    
    private void OnButtonClicked()
    {
        // Handle button click
    }
}
```

**Setup:**
1. Attach presenter to GameObject
2. Add `UiToolkitPresenterFeature` component (auto-detects UIDocument)
3. Create UXML file with your UI elements
4. Assign UXML to UIDocument in inspector

---

### 5. DelayedUiToolkit

**Files:**
- `TimeDelayedUiToolkitPresenter.cs` - Time delay + UI Toolkit
- `AnimationDelayedUiToolkitPresenter.cs` - Animation delay + UI Toolkit + Data
- `DelayedUiToolkitExample.cs` - Scene setup
- `DelayedUiToolkitExample.uxml` - UI Toolkit layout

**Demonstrates:**
- **Composing multiple features together**
- Combining TimeDelayFeature/AnimationDelayFeature + UiToolkitPresenterFeature
- Using data with multiple features
- Coordinating feature interactions

**Pattern (Time Delay + UI Toolkit):**
```csharp
[RequireComponent(typeof(TimeDelayFeature))]
[RequireComponent(typeof(UiToolkitPresenterFeature))]
public class TimeDelayedUiToolkitPresenter : UiPresenter
{
    [SerializeField] private TimeDelayFeature _delayFeature;
    [SerializeField] private UiToolkitPresenterFeature _toolkitFeature;
    
    protected override void OnInitialized()
    {
        // Disable UI until delay completes
        _toolkitFeature.Root.SetEnabled(false);
        _delayFeature.OnOpenCompletedEvent += EnableUI;
    }
    
    private void EnableUI()
    {
        _toolkitFeature.Root.SetEnabled(true);
    }
}
```

**Pattern (Animation + UI Toolkit + Data):**
```csharp
[RequireComponent(typeof(AnimationDelayFeature))]
[RequireComponent(typeof(UiToolkitPresenterFeature))]
public class AnimationDelayedUiToolkitPresenter : UiPresenter<MyData>
{
    [SerializeField] private AnimationDelayFeature _animationFeature;
    [SerializeField] private UiToolkitPresenterFeature _toolkitFeature;
    
    protected override void OnSetData()
    {
        // Update UI with data
        _toolkitFeature.Root.Q<Label>("Title").text = Data.Title;
    }
    
    protected override void OnInitialized()
    {
        _animationFeature.OnOpenCompletedEvent += OnAnimationComplete;
    }
    
    private void OnAnimationComplete()
    {
        // Enable interactions after animation
        _toolkitFeature.Root.SetEnabled(true);
    }
}
```

**Setup:**
1. Attach presenter to GameObject
2. Add both feature components
3. Configure each feature in inspector
4. Features work together automatically

---

### 6. Analytics

**Files:**
- `AnalyticsCallbackExample.cs` - Analytics integration example

**Demonstrates:**
- Integrating analytics with UI events
- Tracking UI opens/closes
- Custom analytics callbacks

---

## Common Patterns

### Feature Configuration

#### TimeDelayFeature
Configure in inspector:
- **Open Delay In Seconds:** Time to wait after opening (default: 0.5s)
- **Close Delay In Seconds:** Time to wait before closing (default: 0.3s)

#### AnimationDelayFeature
Configure in inspector:
- **Animation Component:** Auto-detected or manually assigned
- **Intro Animation Clip:** Animation to play when opening
- **Outro Animation Clip:** Animation to play when closing

Delays automatically match animation clip lengths!

#### UiToolkitPresenterFeature
Configure in inspector:
- **Document:** Auto-detected UIDocument component
- Access via: `_toolkitFeature.Document` or `_toolkitFeature.Root`

---

### Event Subscription Pattern

Always subscribe in `OnInitialized()` and unsubscribe in `OnDestroy()`:

```csharp
protected override void OnInitialized()
{
    _delayFeature.OnOpenCompletedEvent += MyHandler;
}

private void OnDestroy()
{
    if (_delayFeature != null)
    {
        _delayFeature.OnOpenCompletedEvent -= MyHandler;
    }
}
```

---

### Multiple Features Pattern

```csharp
[RequireComponent(typeof(TimeDelayFeature))]
[RequireComponent(typeof(UiToolkitPresenterFeature))]
public class MyPresenter : UiPresenter
{
    [SerializeField] private TimeDelayFeature _delayFeature;
    [SerializeField] private UiToolkitPresenterFeature _toolkitFeature;
    
    // Use both features together!
}
```

---

## Creating Custom Features

Follow this pattern to create your own features:

```csharp
using UnityEngine;
using GameLovers.UiService;

/// <summary>
/// Custom feature that adds fade effect
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class FadeFeature : PresenterFeatureBase
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeDuration = 0.3f;
    
    private void OnValidate()
    {
        _canvasGroup = _canvasGroup ?? GetComponent<CanvasGroup>();
    }
    
    public override void OnPresenterOpening()
    {
        _canvasGroup.alpha = 0f;
    }
    
    public override void OnPresenterOpened()
    {
        StartCoroutine(FadeIn());
    }
    
    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            _canvasGroup.alpha = elapsed / _fadeDuration;
            elapsed += Time.deltaTime;
            yield return null;
        }
        _canvasGroup.alpha = 1f;
    }
}
```

Use it:
```csharp
[RequireComponent(typeof(FadeFeature))]
public class MyFadingPresenter : UiPresenter
{
    // Fade happens automatically!
}
```

---

## Best Practices

### 1. One Component Per Feature
Each feature is self-contained - no need for separate delayer components.

### 2. Clean Up Events
Always unsubscribe from events in `OnDestroy()`.

### 3. Use OnValidate
Let Unity auto-assign component references in `OnValidate()`.

### 4. Configure in Inspector
Use `[SerializeField]` and let designers configure values in Unity's inspector.

### 5. Compose Freely
Mix and match features without worrying about inheritance conflicts.

---

## Architecture Benefits

✅ **Self-Contained** - Each feature owns its complete logic  
✅ **Composable** - Mix any features freely  
✅ **Configurable** - All settings in Unity inspector  
✅ **Clear** - One component = one capability  
✅ **Scalable** - Add new features without modifying existing code  

---

## Getting Started

1. **Choose a sample** that matches your use case
2. **Copy the pattern** to your own presenter
3. **Add required features** via `[RequireComponent]`
4. **Configure in inspector** - delays, animations, etc.
5. **Subscribe to events** for feature completion notifications

---

## Documentation

For complete architecture documentation, see:
- **[README.md](../README.md)** - Quick start guide
- **[FINAL_SIMPLIFICATION.md](../FINAL_SIMPLIFICATION.md)** - Architecture details
- **[FINAL_ARCHITECTURE_SUMMARY.md](../FINAL_ARCHITECTURE_SUMMARY.md)** - Complete overview

---

**All samples use the pure feature composition pattern introduced in architecture v2.0**
