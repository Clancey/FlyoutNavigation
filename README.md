The `FlyOutNavigationController` presents a simple navigation view that
appears to slide out from underneath a main view, allowing you to
navigate among a list of view controllers. The same view also functions
as a `UISplitViewController` on the iPad. 

```csharp
using FlyOutNavigation;
using MonoTouch.Dialog;
...
public override void ViewDidLoad ()
{
	base.ViewDidLoad ();
	var navigation = new FlyOutNavigationController {
		// Create the navigation menu
		NavigationRoot = new RootElement ("Navigation") {
			new Section ("Pages") {
				new StringElement ("Animals"),
				new StringElement ("Vegetables"),
				new StringElement ("Minerals"),
			}
		},
		// Supply view controllers corresponding to menu items:
		ViewControllers = new [] {
			new UIViewController { View = new UILabel { Text = "Animals (drag right)" } },
			new UIViewController { View = new UILabel { Text = "Vegetables (drag right)" } },
			new UIViewController { View = new UILabel { Text = "Minerals (drag right)" } },
		},
	};
	// Show the navigation view
	navigation.ToggleMenu ();
	View.AddSubview (navigation.View);
}
```
