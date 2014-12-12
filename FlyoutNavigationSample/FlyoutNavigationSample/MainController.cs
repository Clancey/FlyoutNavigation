using System;
using System.Linq;
using System.Collections.Generic;

#if __UNIFIED__
using UIKit;
using Foundation;
#else
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif
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
			navigation.Position = FlyOutNavigationPosition.Left;
			navigation.View.Frame = UIScreen.MainScreen.Bounds;
			View.AddSubview (navigation.View);
			this.AddChildViewController (navigation);
			
			// Create the menu:
			navigation.NavigationRoot = new RootElement ("Task List") {
				new Section ("Task List") {
					from page in Tasks
						select new StringElement (page) as Element
				},
				new Section ("Extras")
				{
					new StringElement("Swipable Table"),
					new StringElement("Storyboard"),
				}
			};
			
			// Create an array of UINavigationControllers that correspond to your
			// menu items:
			var viewControllers = Tasks.Select (x => new UINavigationController (new TaskPageController (navigation, x))).ToList ();
			viewControllers.Add (new UINavigationController(new SwipableTableView ()));			
			//Load from Storyboard
			var storyboardVc = CreateViewController<MyStoryboardViewController> ("MyStoryBoard","MyStoryboardViewController");
			viewControllers.Add (new UINavigationController(storyboardVc));
			navigation.ViewControllers = viewControllers.ToArray ();
		}
		static T CreateViewController<T>(string storyboardName, string viewControllerStoryBoardId = "") where T : UIViewController
		{
			var storyboard = UIStoryboard.FromName (storyboardName,null);
			return string.IsNullOrEmpty(viewControllerStoryBoardId) ? (T)storyboard.InstantiateInitialViewController () : (T) storyboard.InstantiateViewController(viewControllerStoryBoardId);
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

