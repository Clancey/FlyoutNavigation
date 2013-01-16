using System;
using System.Linq;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

using FlyoutNavigation;

namespace Sample
{
	public class MainController : UIViewController
	{
		FlyoutNavigationController navigation;
		
		// Data we'll use to create our flyout menu and views:
		string[] Tasks = {
			"Get Xamarin",
			"Learn C#",
			"Write Killer App",
			"Add Platforms",
			"Profit",
			"Meet Obama",
		};
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Create the flyout view controller, make it large,
			// and add it as a subview:
			navigation = new FlyoutNavigationController ();
			navigation.View.Frame = UIScreen.MainScreen.Bounds;
			View.AddSubview (navigation.View);
			
			// Create the menu:
			navigation.NavigationRoot = new RootElement ("Task List") {
				new Section ("Task List") {
					from page in Tasks
						select new StringElement (page) as Element
				}
			};
			
			// Create an array of UINavigationControllers that correspond to your
			// menu items:
			navigation.ViewControllers = Array.ConvertAll (Tasks, title =>
           		new UINavigationController (new TaskPageController (navigation, title))
			);
		}
		
		class TaskPageController : DialogViewController
		{
			public TaskPageController (FlyoutNavigationController navigation, string title) : base (null)
			{
				Root = new RootElement (title) {
					new Section {
						new CheckboxElement (title)
					}
				};
				NavigationItem.LeftBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Action, delegate {
					navigation.ToggleMenu ();
				});
			}
		}
	}
}

