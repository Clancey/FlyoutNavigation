using System;
#if __UNIFIED__
using Foundation;
using UIKit;
#else
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif
using System.CodeDom.Compiler;

namespace Sample
{
	partial class MyStoryboardViewController : UIViewController
	{
		public MyStoryboardViewController (IntPtr handle) : base (handle)
		{
		}
	}
}
