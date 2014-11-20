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
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		UIWindow window;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds) {
				RootViewController = new MainController ()
			};
			window.MakeKeyAndVisible ();
			return true;
		}
	}
}

