# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html)

## [0.3.0] - 2020-02-11

- Added new *UiPresenterData* class for the case where the *UiPresenter* needs to be initialized with a default data value
- Added new *OnInitialize* method that is invoked after the *UiPresenter* is initialized

## [0.2.0] - 2020-01-19

## [0.2.1] - 2020-02-09

- Added the possibility to open the ui after adding or loading it to the *UiService*
- Added the possibility to get the canvas reference object based on the given layer
- Added the possibility to remove and unload the *UiPresenter* by only passing it's reference

**Fixed**:
- Fixed bug that prevented the *UiService* to properly unload an *UiPresenter*

## [0.2.0] - 2020-01-19

- Added easy selection of the UiConfigs.asset file. Just go to Tools > Select UiConfigs.asset. If the UiConfigs.asset does not exist, it will create a new one in the Assets folder
- Added the protected Close() method to directly allow to close the UiPresenter from the UiPresenter object file without needing to call the UiService. Also now is possible to close an Ui in the service by referencing the object directly without needing to reference the object type by calling CloseUi<T>(T presenter)
- Now the UnloadUi & UnloadUiSet properly unloads the ui from memory and removes it from the service
- Added RemoveUi & RemoveUiPresentersFromSet to allow the ui to be removed from the service without being unloaded from memory
- Improved documentation

**Changed**
- Now the Refresh method on the UiPresenter is public and can be called from any object that has asset to it. The UiService will not call this method anymore if trying to Open the same UiPresenter twice without closing
- In the UiService, IsUiLoaded changed to HasUiPresenter
- In the UiService IsAllUiLoadedInSet changed to HasAllUiPresentersInSet
- Unified AddUi methods

**Fixed**:
- Fixed bug that sometimes don't save the UiConfigs state correctly

## [0.1.3] - 2020-01-09

**Fixed**:
- Bug that sometimes was not save+ing the UiConfigs state correctly

## [0.1.2] - 2020-01-09

**Fixed**:
- Compiler Errors

## [0.1.1] - 2020-01-09

**Fixed**:
- The state of a the UiPresenter when loaded. Now the UiPresenters are always disabled when loaded

## [0.1.0] - 2020-01-05

- Initial submission for package distribution
