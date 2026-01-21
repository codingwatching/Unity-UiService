# Custom Features Sample

This sample demonstrates creating custom presenter features by extending `PresenterFeatureBase`.

## Design Philosophy

The UI Service uses feature composition instead of inheritance. Custom features allow:
1. **Reusable behaviors**: Create once, attach to any presenter.
2. **Lifecycle hooks**: Hook into presenter open/close events.
3. **Transition support**: Implement `ITransitionFeature` for features with timing requirements.

## Sample Content

### Custom Features
- **FadeFeature**: Fades the UI in/out using `CanvasGroup.alpha`.
- **ScaleFeature**: Scales the UI in/out with animation curves.
- **SoundFeature**: Plays sounds on open/close.

### Presenters
- **FadingPresenter**: Uses `FadeFeature` only.
- **ScalingPresenter**: Uses `ScaleFeature` only.
- **FullFeaturedPresenter**: Combines all three features.

## How to Use

1. **Import the sample** and open the `CustomFeatures.unity` scene.
2. **Enter Play Mode** to see custom features in action.
3. **Interact with the buttons**:
   - **Open Fade UI**: Opens presenter with fade effect.
   - **Open Scale UI**: Opens presenter with scale effect.
   - **Open All Features**: Opens presenter with fade, scale, and sound.
   - **Close All**: Closes all open presenters.

## Implementation Details

### Feature Base
All features extend `PresenterFeatureBase` and override lifecycle hooks:
- `OnPresenterOpening()`: Before the presenter becomes visible.
- `OnPresenterOpened()`: After the presenter is visible.
- `OnPresenterClosing()`: Before the presenter is hidden.
- `OnPresenterClosed()`: After the presenter is hidden.

### Transition Feature
`FadeFeature.cs` and `ScaleFeature.cs` implement `ITransitionFeature`:
- `OpenTransitionTask`: Awaited by the presenter during open.
- `CloseTransitionTask`: Awaited by the presenter during close.

### Feature Composition
Attach multiple features via `[RequireComponent]` attributes.
