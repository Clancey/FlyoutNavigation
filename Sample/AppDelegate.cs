using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using FlyOutNavigation;
using MonoTouch.Dialog;

namespace Sample
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		FlyOutNavigationController viewController;

		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.BlackTranslucent;
			viewController = new FlyOutNavigationController ();
			viewController.NavigationRoot = new RootElement ("")
			{
				new Section ("Section 1"){
					new StringElement ("View 1"),
					new ImageStringElement("View 2",UIImage.FromFile("jhill.jpeg")),
					new StringElement ("View 3"),
				},
				new Section ("Section 2"){
					new StringElement ("View 1"),
					new StringElement ("View 2"),
				}
			};
			viewController.ViewControllers = new UIViewController[]{
				 new UINavigationController (new BaseDialogViewController (viewController, new RootElement ("Section 1"){new Section (){new StringElement ("View 1")}}))
				,new UINavigationController (new BaseDialogViewController (viewController, new RootElement ("Section 1"){new Section (){new StringElement ("View 2")}}))
				,new UINavigationController (new BaseDialogViewController (viewController, new RootElement ("Section 1"){new Section (){new StringElement ("View 3")}}))
				,new UINavigationController (new BaseDialogViewController (viewController, new RootElement ("Section 2"){new Section (){new StringElement ("View 1")}}))
				,new UINavigationController (new BaseDialogViewController (viewController, new RootElement ("Section 2"){new Section (){new StringElement ("View 2")}}))
			};
			window.RootViewController = viewController;
			window.MakeKeyAndVisible ();
			
			return true;
		}
		
		public class BaseDialogViewController : DialogViewController
		{
			FlyOutNavigationController navigation;

			public BaseDialogViewController (FlyOutNavigationController navigation, RootElement root) : base(root)
			{
				this.navigation = navigation;
			}
			
			public override void ViewWillAppear (bool animated)
			{
				base.ViewWillAppear (animated);
				
				this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Bookmarks, delegate {
					navigation.ToggleMenu ();
				});
			}
		}
	}
}

