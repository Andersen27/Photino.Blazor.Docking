# ![ ](docking.png) Photino.Blazor.Docking
![ ](preview.gif)

## About
Cross-platform docking system for Photino.Blazor applications with split, tabs and floating panels support.\
Implements panel arrangement with a horizontal/vertical splitter, grouping into a tabbed panel and detaching panels into a separate floating windows.
Supports hiding/showing panels by its identificators and saving/loading docking layout in JSON format.

## How to use
1. Include **Photino.Blazor.Docking** as PackageReference to your project.
2. Copy files from [Photino.Blazor.Docking/wwwroot](Photino.Blazor.Docking/wwwroot) and [Photino.Blazor.CustomWindow/wwwroot](https://github.com/Andersen27/Photino.Blazor.CustomWindow/tree/master/Photino.Blazor.CustomWindow/wwwroot) folders to your wwwroot.
3. Configure and add DockingService to your Photino.Blazor application service collection with `AddPhotinoBlazorDocking()` extension.\
   You must declare services initialization function and dock panels configuration set (refer to the extension summary and [example](Photino.Blazor.Docking.Sample/Program.cs)).
4. Use `DockPanelHost` component as docking host component. Next, there are two options for the panels initial placement:
   - Load docking layout from JSON via DockingService's `LoadLayout()` method before the DockPanelHost component is initialized.
   - Define DockPanelHost's `DefaultLayout` RenderFragment (refer to the [example](Photino.Blazor.Docking.Sample/Pages/DockingDemoPage.razor)).
     > You can paste here DockPanelSplit, DockPanelTabs or DockPanel component.\
     DockPanelSplit should contain two components of the types listed above.\
     DockPanelTabs can contain only DockPanel components.\
     DockPanel must be used strictly with the identificator or blazor component, specified in the configuration.

## Next steps
- At the moment the project works correctly only for screens with a 100% zoom factor. See Photino.Blazor.CustomWindow [next steps](https://github.com/Andersen27/Photino.Blazor.CustomWindow?tab=readme-ov-file#next-steps).
- Optimize the code to avoid crashes.
- Get away from Radzen dependencies (own splitter implementation).
