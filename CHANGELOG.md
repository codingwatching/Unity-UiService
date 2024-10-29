# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html)

## [0.8.0] - 2024-10-29

- Added new *PresenterDelayerBase*, *AnimationDelayer* and *TimeDelayer* to support presenters that open/close with a delay
- Added new *DelayUiPresenter* to interact with *PresenterDelayerBase* implementations and allow presenters to open/close with a delay
- Improved performance of *UiService*

***Changed**:
- Removed *AnimatedUiPresenter*. Use the new *DelayUiPresenter* with one of the *PresenterDelayerBase* implementations
- Removed *UiCloseActivePresenter*  and *UiCloseActivePresenterData*. Use the new *DelayUiPresenter* with one of the *PresenterDelayerBase* implementations
- Removed the dependency of *UiPresenter* from Canvas. Allowing different structures of UI Unity project hierarchy to work with the *UiService*
- Removed all Get and Has methods from *IUiService*. Replaced with IReadOnlyDictionaries for all the collections being requested from the service
- Changed all OpenUi methods to be async. This guarantees the expected behaviour that will always load the Ui first before opening
- Changed all CloseUi methods to be synchronous. Closing an Ui will now always be atomic. To get the close delay, you can request directly from the *DelayUiPresenter*
- Changed *IUiAssetLoader* to unify the prefab instantiation into a single call. This simplefies the method so the caller doesn't have to worry about synchronous or async behaviour
- Changed the *UiConfig* to know contain the information of the *UiPresenter* if is loaded sync or async

## [0.7.2] - 2021-05-09

**Fixed**:
- Fixed the issue where closing *UiPresenter* would be called after the game object is disabled

## [0.7.1] - 2021-05-03

- Added the possibility for *SafeAreaHelpersView* to maintain the View in the same position if not placed outside of the safe area

**Fixed**:
- Fixed the duplicated memory issue when loading the same *UiPresenter* multiple times at the same time before when of them is finished

## [0.7.0] - 2021-03-12

- Added *NonDrawingView* to have an Image without a renderer to not add additional draw calls.
- Added *SafeAreaHelperView* to add the possibility for the *RectTransform* to adjust himself to the screen notches
- Added *AnimatedUiPresenter* to play animation on enter or closing
- Added the possibility to add *Layers* externally into the *UiService*
  
**Changed**:
- Now *Canvas* are single *GameObjects* that can be controlled outside of the *UiService*

**Fixed**:
- Fixed the issue when setting data on *UiPresenterData* not being invoked

## [0.6.1] - 2020-09-24

- Updated dependency packages

## [0.6.0] - 2020-09-24

- Added the possibility for the *IUiService* to allow to open/close already opened/closed *UiPresenters*, and throw an exception if not. 
- Added the visible property to UiPresenter of its current visual status Added *IUiServiceInit* to give a new contract interface for the *UiService" initialisation

**Fixed**:
- Fixed issue that was not unloading the *UiPresenter* correctly with the asset bundles
- Fixed issue when *UiPresenter* might not have a canvas attached
- Fixed crash when trying to open a *UiPresenter* after loading it

## [0.5.0] - 2020-07-13

- Added *UiAssetLoader* to load Ui assets to memory

**Changed**:
- Removed the *UiService* dependency from the *com.gamelovers.assetLoader*

## [0.4.0] - 2020-07-13

**Changed**:
- Removed the *UiService* dependency from the *com.unity.addressables*
- Modified the *UiService* to be testable and injectable into other systems

## [0.3.2] - 2020-04-18

- Moved interface *IUiService* to a separate file to improve the code readability

## [0.3.1] - 2020-02-15

- Updated dependency packages

## [0.3.0] - 2020-02-11

- Added new *UiPresenterData* class for the case where the *UiPresenter* needs to be initialized with a default data value
- Added new *OnInitialize* method that is invoked after the *UiPresenter* is initialized

## [0.2.1] - 2020-02-09

- Added the possibility to open the ui after adding or loading it to the *UiService*
- Added the possibility to get the canvas reference object based on the given layer
- Added the possibility to remove and unload the *UiPresenter* by only passing it's reference

**Fixed**:
- Fixed bug that prevented the *UiService* to properly unload an *UiPresenter*

## [0.2.0] - 2020-01-19

- Added easy selection of the *UiConfigs.asset* file. Just go to *Tools > Select UiConfigs.asset*. If the *UiConfigs.asset* does not exist, it will create a new one in the Assets folder
- Added the protected *Close()* method to directly allow to close the *UiPresenter* from the *UiPresenter* object file without needing to call the *UiService*. Also now is possible to close an Ui in the service by referencing the object directly without needing to reference the object type by calling *CloseUi<T>(T presenter)*
- Now the *UnloadUi* & *UnloadUiSet* properly unloads the ui from memory and removes it from the service
- Added *RemoveUi* & *RemoveUiPresentersFromSet* to allow the ui to be removed from the service without being unloaded from memory
- Improved documentation

**Changed**
- Now the Refresh method on the *UiPresenter* is public and can be called from any object that has asset to it. The *UiService* will not call this method anymore if trying to open the same *UiPresenter* twice without closing
- *UiService.IsUiLoaded* changed to *HasUiPresenter*
- *UiService.IsAllUiLoadedInSet* changed to *HasAllUiPresentersInSet*
- Unified *AddUi* methods

**Fixed**:
- Fixed bug that sometimes don't save the *UiConfigs* state correctly

## [0.1.3] - 2020-01-09

**Fixed**:
- Bug that sometimes was not save+ing the *UiConfigs* state correctly

## [0.1.2] - 2020-01-09

**Fixed**:
- Compiler Errors

## [0.1.1] - 2020-01-09

**Fixed**:
- The state of a the *UiPresenter* when loaded. Now the *UiPresenters* are always disabled when loaded

## [0.1.0] - 2020-01-05

- Initial submission for package distribution
