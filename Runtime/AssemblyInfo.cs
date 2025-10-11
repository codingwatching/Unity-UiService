using System.Runtime.CompilerServices;

// Assembly information for GameLovers.UiService
// 
// This file exposes internal members of the runtime assembly to other assemblies within this package.
// Use the [InternalsVisibleTo] attribute to grant access to internal APIs without making them public.

// Makes internal members visible to the Editor assembly.
// This allows editor tools (like UiAnalyticsWindow) to access internal APIs such as UiService.CurrentAnalytics
// without making them public to end users.
[assembly: InternalsVisibleTo("GameLovers.UiService.Editor")]

