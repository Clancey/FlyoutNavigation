using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Tests
{
	//[TestFixture (Platform.Android)]
	[TestFixture (Platform.iOS)]
	public class Tests
	{
		IApp app;
		Platform platform;

		public Tests (Platform platform)
		{
			this.platform = platform;
		}

		[SetUp]
		public void BeforeEachTest ()
		{
			app = AppInitializer.StartApp (platform);
		}

		[Test]
		public void AppLaunches ()
		{
			app.Screenshot ("First screen.");
		}

		[Test]
		public void MenuButtonWorks ()
		{
			app.Screenshot ("App Launched");

			app.Tap (x => x.Class ("UINavigationButton").Marked ("Share"));

			app.Screenshot ("Menu Opened");

			app.Tap (x => x.Marked ("Learn C#"));

			app.Screenshot ("Learn C#");
		}

		[Test]
		public void SwipeWorks ()
		{
			app.Screenshot ("App Launched");

			app.SwipeLeftToRight ();

			app.Screenshot ("Menu Opened");

			app.Tap (x => x.Marked ("Learn C#"));

			app.Screenshot ("Learn C#");
		}

	}
}
