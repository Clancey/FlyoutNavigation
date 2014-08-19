//  Copyright 2011  Clancey
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

namespace FlyoutNavigation
{
	public enum FlyOutNavigationPosition {
		Left = 0, // default
		Right
	};

	[Register("FlyoutNavigationController")]
	public class FlyoutNavigationController : UIViewController
	{
		const float sidebarFlickVelocity = 1000.0f;
		public const int menuWidth = 280;
		//public UISearchBar SearchBar;
		UIButton closeButton;
		bool firstLaunch = true;
		FlyOutNavigationPosition position;
		DialogViewController navigation;
		int selectedIndex;
		UIView shadowView;
		float startX;
		UIColor tintColor;
		UIView statusImage;
		protected UIViewController[] viewControllers;
		bool hideShadow;

		public FlyoutNavigationController(IntPtr handle) : base(handle)
		{
			Initialize();
		}

		public FlyoutNavigationController(UITableViewStyle navigationStyle = UITableViewStyle.Plain)
		{
			Initialize(navigationStyle);
		}

		public UIColor TintColor
		{
			get { return tintColor; }
			set
			{
				if (tintColor == value)
					return;
				//SearchBar.TintColor = value;
			}
		}

		public FlyOutNavigationPosition Position {
			get {
				return position;
			}
			set {
				position = value;
				shadowView.Layer.ShadowOffset = new SizeF(Position == FlyOutNavigationPosition.Left ? -5 : 5, -1);
			}
		}

		public Action SelectedIndexChanged { get; set; }

		public bool AlwaysShowLandscapeMenu { get; set; }

		public bool ForceMenuOpen { get; set; }

		public bool HideShadow
		{
			get { return hideShadow; }
			set
			{
				if (value == hideShadow)
					return;
				hideShadow = value;
				if (hideShadow) {
					if (mainView != null)
						View.InsertSubviewBelow (shadowView, mainView);
				} else {
					shadowView.RemoveFromSuperview ();
				}

			}
		}

		public UIColor ShadowViewColor
		{
			get { return shadowView.BackgroundColor; }
			set { shadowView.BackgroundColor = value; }
		}

		public UIViewController CurrentViewController { get; private set; }

		UIView mainView
		{
			get
			{
				if (CurrentViewController == null)
					return null;
				return CurrentViewController.View;
			}
		}

		public RootElement NavigationRoot
		{
			get { return navigation.Root; }
			set { EnsureInvokedOnMainThread(delegate { navigation.Root = value; }); }
		}

		public UITableView NavigationTableView
		{
			get { return navigation.TableView; }
		}

		public UIViewController[] ViewControllers
		{
			get { return viewControllers; }
			set
			{
				EnsureInvokedOnMainThread(delegate
					{
						viewControllers = value;
						NavigationItemSelected(GetIndexPath(SelectedIndex));
					});
			}
		}

		public bool IsOpen
		{
			get {
				if (Position == FlyOutNavigationPosition.Left) {
					return mainView.Frame.X == menuWidth;
				} else {
					return mainView.Frame.X == -menuWidth;
				}
			}
			set
			{
				if (value)
					HideMenu();
				else
					ShowMenu();
			}
		}

		bool ShouldStayOpen
		{
			get
			{
				if (ForceMenuOpen || (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad &&
					AlwaysShowLandscapeMenu &&
					(InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft
						|| InterfaceOrientation == UIInterfaceOrientation.LandscapeRight)))
					return true;
				return false;
			}
		}

		public int SelectedIndex
		{
			get { return selectedIndex; }
			set
			{
				if (selectedIndex == value)
					return;
				selectedIndex = value;
				EnsureInvokedOnMainThread(delegate { NavigationItemSelected(value); });
			}
		}

		public bool DisableRotation { get; set; }

		public override bool ShouldAutomaticallyForwardRotationMethods
		{
			get { return true; }
		}

		bool isIos7 = false;
		bool isIos8 = false;
		class UAUIView : UIView
		{
			[Export ("accessibilityIdentifier")]
			public string AccessibilityId {get;set;}
		}

		void Initialize(UITableViewStyle navigationStyle = UITableViewStyle.Plain)
		{
			DisableStatusBarMoving = true;
			statusImage = new UAUIView{ ClipsToBounds = true, AccessibilityId = "statusbar" };//.SetAccessibilityId( "statusbar");
			navigation = new DialogViewController(navigationStyle, null);
			navigation.OnSelection += NavigationItemSelected;
			RectangleF navFrame = navigation.View.Frame;
			navFrame.Width = menuWidth;
			if (Position == FlyOutNavigationPosition.Right)
				navFrame.X = mainView.Frame.Width - menuWidth;
			navigation.View.Frame = navFrame;
			View.AddSubview(navigation.View);
			//SearchBar = new UISearchBar(new RectangleF(0, 0, navigation.TableView.Bounds.Width, 44))
			//	{
			//		//Delegate = new SearchDelegate (this),
			//		TintColor = TintColor
			//	};

			TintColor = UIColor.Black;
			var version = new System.Version(UIDevice.CurrentDevice.SystemVersion);
			isIos8 = version.Major >= 8;
			isIos7 = version.Major >= 7;
			if(isIos7)
				navigation.TableView.TableHeaderView = new UIView(new RectangleF(0, 0, 320, 22))
			{
				BackgroundColor = UIColor.Clear
			};
			navigation.TableView.TableFooterView = new UIView(new RectangleF(0, 0, 100, 100)) {BackgroundColor = UIColor.Clear};
			navigation.TableView.ScrollsToTop = false;
			shadowView = new UIView(){AccessibilityLabel = "flyOutShadowLayeLabel" , IsAccessibilityElement = true}.SetAccessibilityId("flyOutShadowLayer");
			shadowView.BackgroundColor = UIColor.White;
			shadowView.Layer.ShadowOffset = new SizeF(Position == FlyOutNavigationPosition.Left ? -5 : 5, -1);
			shadowView.Layer.ShadowColor = UIColor.Black.CGColor;
			shadowView.Layer.ShadowOpacity = .75f;
			closeButton = new UIButton ();
			closeButton.AccessibilityLabel = "Close Menu";
			closeButton.TouchUpInside += delegate { HideMenu(); };
			AlwaysShowLandscapeMenu = true;

			View.AddGestureRecognizer (new OpenMenuGestureRecognizer (DragContentView, shouldReceiveTouch));
		}

		public event UITouchEventArgs ShouldReceiveTouch;

		internal bool shouldReceiveTouch(UIGestureRecognizer gesture, UITouch touch)
		{
			if (ShouldReceiveTouch != null)
				return ShouldReceiveTouch(gesture, touch);
			return true;
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			RectangleF navFrame = View.Bounds;
			//			navFrame.Y += UIApplication.SharedApplication.StatusBarFrame.Height;
			//			navFrame.Height -= navFrame.Y;
			//this.statusbar
			navFrame.Width = menuWidth;
			if (Position == FlyOutNavigationPosition.Right)
				navFrame.X = mainView.Frame.Width - menuWidth;
			if (navigation.View.Frame != navFrame)
				navigation.View.Frame = navFrame;
		}

		public void DragContentView(UIPanGestureRecognizer panGesture)
		{
			if (ShouldStayOpen || mainView == null)
				return;
			if (!HideShadow)
				View.InsertSubviewBelow(shadowView, mainView);
			navigation.View.Hidden = false;
			RectangleF frame = mainView.Frame;
			shadowView.Frame = frame;
			float translation = panGesture.TranslationInView(View).X;
			if (panGesture.State == UIGestureRecognizerState.Began)
			{
				startX = frame.X;
			}
			else if (panGesture.State == UIGestureRecognizerState.Changed)
			{
				frame.X = translation + startX;
				if (Position == FlyOutNavigationPosition.Left)
				{
					if (frame.X < 0)
						frame.X = 0;
					else if (frame.X > menuWidth)
						frame.X = menuWidth;
				}
				else
				{
					if (frame.X > 0)
						frame.X = 0;
					else if (frame.X < -menuWidth)
						frame.X = -menuWidth;
				}
				SetLocation(frame);
			}
			else if (panGesture.State == UIGestureRecognizerState.Ended)
			{
				float velocity = panGesture.VelocityInView(View).X;
				float newX = translation + startX;
				bool show = Math.Abs (velocity) > sidebarFlickVelocity ? velocity > 0 : newX > (menuWidth / 2);
				if (Position == FlyOutNavigationPosition.Right) {
					show = Math.Abs(velocity) > sidebarFlickVelocity ? velocity < 0 : newX < -(menuWidth / 2);
				}
				if (show) {
					ShowMenu ();
				} else {
					HideMenu ();
				}
			}
		}

		public override void ViewWillAppear(bool animated)
		{
			RectangleF navFrame = navigation.View.Frame;
			navFrame.Width = menuWidth;
			if (Position == FlyOutNavigationPosition.Right)
				navFrame.X = mainView.Frame.Width - menuWidth;
			navFrame.Location = PointF.Empty;
			navigation.View.Frame = navFrame;
			View.BackgroundColor = NavigationTableView.BackgroundColor;
			var frame = mainView.Frame;
			setViewSize ();
			SetLocation (frame);
			base.ViewWillAppear(animated);
		}

		protected void NavigationItemSelected(NSIndexPath indexPath)
		{
			int index = GetIndex(indexPath);
			NavigationItemSelected(index);
		}

		protected void NavigationItemSelected(int index)
		{
			selectedIndex = index;
			if (viewControllers == null || viewControllers.Length <= index || index < 0)
			{
				if (SelectedIndexChanged != null)
					SelectedIndexChanged();
				return;
			}
			if (ViewControllers[index] == null)
			{
				if (SelectedIndexChanged != null)
					SelectedIndexChanged();
				return;
			}
			if(!DisableStatusBarMoving && !ShouldStayOpen)
				UIApplication.SharedApplication.SetStatusBarHidden(false,UIStatusBarAnimation.Fade);

			bool isOpen = false;
			if (mainView != null)
			{
				mainView.RemoveFromSuperview();
				isOpen = IsOpen;
			}
			CurrentViewController = ViewControllers[SelectedIndex];
			RectangleF frame = View.Bounds;
			if (isOpen || ShouldStayOpen)
				frame.X = Position == FlyOutNavigationPosition.Left ? menuWidth : -menuWidth;

			setViewSize();
			SetLocation(frame);
			View.AddSubview(mainView);
			AddChildViewController(CurrentViewController);
			if (!ShouldStayOpen)
				HideMenu();
			if (SelectedIndexChanged != null)
				SelectedIndexChanged();
		}

		//bool isOpen {get{ return mainView.Frame.X == menuWidth; }}

		public void ShowMenu()
		{
			if (mainView == null)
				return;
			EnsureInvokedOnMainThread(delegate
				{
					//navigation.ReloadData ();
					//isOpen = true;
					navigation.View.Hidden = false;
					closeButton.Frame = mainView.Frame;
					shadowView.Frame = mainView.Frame;
					var statusFrame = UIApplication.SharedApplication.StatusBarFrame;
					statusFrame.X = mainView.Frame.X;
					statusImage.Frame = statusFrame;
					if (!ShouldStayOpen)
						View.AddSubview(closeButton);
					if (!HideShadow)
						View.InsertSubviewBelow (shadowView, mainView);
					UIView.BeginAnimations("slideMenu");
					UIView.SetAnimationCurve(UIViewAnimationCurve.EaseIn);
					//UIView.SetAnimationDuration(2);
					setViewSize();
					RectangleF frame = mainView.Frame;
					frame.X = Position == FlyOutNavigationPosition.Left ? menuWidth : -menuWidth;
					SetLocation(frame);
					setViewSize();
					frame = mainView.Frame;
					shadowView.Frame = frame;
					closeButton.Frame = frame;
					statusFrame.X = mainView.Frame.X;
					statusImage.Frame = statusFrame;
					UIView.CommitAnimations();
				});
		}

		void setViewSize()
		{
			RectangleF frame = View.Bounds;
			//frame.Location = PointF.Empty;
			if (ShouldStayOpen)
				frame.Width -= menuWidth;
			if (mainView.Bounds == frame)
				return;
			mainView.Bounds = frame;
		}

		void SetLocation(RectangleF frame)
		{
			mainView.Layer.AnchorPoint = new PointF(.5f, .5f);
			frame.Y = 0;
			if (mainView.Frame.Location == frame.Location)
				return;
			frame.Size = mainView.Frame.Size;
			var center = new PointF(frame.Left + frame.Width/2,
				frame.Top + frame.Height/2);
			mainView.Center = center;
			shadowView.Center = center;

			if (Math.Abs(frame.X - 0) > float.Epsilon)
			{
				getStatus();
				var statusFrame = statusImage.Frame;
				statusFrame.X = mainView.Frame.X;
				statusImage.Frame = statusFrame;
			}
		}
		bool disableStatusBarMoving;
		public bool DisableStatusBarMoving {
			get {
				if (isIos8)
					return true;
				return disableStatusBarMoving;
			}
			set {
				disableStatusBarMoving = value;
			}
		}
		void getStatus()
		{
			if (DisableStatusBarMoving || !isIos7 || statusImage.Superview != null || ShouldStayOpen)
				return;
			var image = captureStatusBarImage ();
			if (image == null)
				return;
			this.View.AddSubview(statusImage);
			foreach (var view in statusImage.Subviews)
				view.RemoveFromSuperview ();
			statusImage.AddSubview (image);
			statusImage.Frame = UIApplication.SharedApplication.StatusBarFrame;
			UIApplication.SharedApplication.StatusBarHidden = true;

		}
		UIView captureStatusBarImage()
		{
			try{
				UIView screenShot = UIScreen.MainScreen.SnapshotView(false);
				return screenShot;
			}
			catch(Exception ex) {
				return null;
			}
		}
		void hideStatus()
		{
			if (!isIos7)
				return;
			statusImage.RemoveFromSuperview();
			UIApplication.SharedApplication.StatusBarHidden = false;
		}

		public void HideMenu()
		{
			if (mainView == null || mainView.Frame.X == 0 || ShouldStayOpen)
				return;

			EnsureInvokedOnMainThread(delegate
				{
					//isOpen = false;
					navigation.FinishSearch();
					closeButton.RemoveFromSuperview();
					shadowView.Frame = mainView.Frame;
					var statusFrame = statusImage.Frame;
					statusFrame.X = mainView.Frame.X;
					statusImage.Frame = statusFrame;
					//UIView.AnimationWillEnd += hideComplete;
					UIView.Animate(.2,	() =>
						{
							UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut);
							RectangleF frame = View.Bounds;
							frame.X = 0;
							setViewSize();
							SetLocation(frame);
							shadowView.Frame = frame;
							statusFrame.X = 0;
							statusImage.Frame = statusFrame;
						}, hideComplete);
				});
		}

		[Export("animationEnded")]
		void hideComplete()
		{
			hideStatus();
			shadowView.RemoveFromSuperview();
			navigation.View.Hidden = true;
		}

		public void ResignFirstResponders(UIView view)
		{
			if (view.Subviews == null)
				return;
			foreach (UIView subview in view.Subviews)
			{
				if (subview.IsFirstResponder)
					subview.ResignFirstResponder();
				ResignFirstResponders(subview);
			}
		}

		public void ToggleMenu()
		{
			EnsureInvokedOnMainThread(delegate
				{
					if (!IsOpen && CurrentViewController != null && CurrentViewController.IsViewLoaded)
						ResignFirstResponders(CurrentViewController.View);
					if (IsOpen)
						HideMenu();
					else
						ShowMenu();
				});
		}

		int GetIndex(NSIndexPath indexPath)
		{
			int section = 0;
			int rowCount = 0;
			while (section < indexPath.Section)
			{
				rowCount += navigation.Root[section].Count;
				section ++;
			}
			return rowCount + indexPath.Row;
		}

		protected NSIndexPath GetIndexPath(int index)
		{
			if (navigation.Root == null)
				return NSIndexPath.FromRowSection(0, 0);
			int currentCount = 0;
			int section = 0;
			foreach (Section element in navigation.Root)
			{
				if (element.Count + currentCount > index)
					break;
				currentCount += element.Count;
				section ++;
			}

			int row = index - currentCount;
			return NSIndexPath.FromRowSection(row, section);
		}

		public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
		{
			if (DisableRotation)
				return toInterfaceOrientation == InterfaceOrientation;

			bool theReturn = CurrentViewController == null
				? true
				: CurrentViewController.ShouldAutorotateToInterfaceOrientation(toInterfaceOrientation);
			return theReturn;
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
		{
			if (CurrentViewController != null)
				return CurrentViewController.GetSupportedInterfaceOrientations();
			return UIInterfaceOrientationMask.All;
		}

		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate(toInterfaceOrientation, duration);
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);

			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
				return;
			switch (InterfaceOrientation)
			{
			case UIInterfaceOrientation.LandscapeLeft:
			case UIInterfaceOrientation.LandscapeRight:
				ShowMenu ();
				return;
			default:
				HideMenu ();
				return;
			}
		}

		public override void WillAnimateRotation(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillAnimateRotation(toInterfaceOrientation, duration);
		}

		protected void EnsureInvokedOnMainThread(Action action)
		{
			if (IsMainThread())
			{
				action();
				return;
			}
			BeginInvokeOnMainThread(() =>
				action()
			);
		}

		static bool IsMainThread()
		{
			return NSThread.Current.IsMainThread;
			//return Messaging.bool_objc_msgSend(GetClassHandle("NSThread"), new Selector("isMainThread").Handle);
		}
	}

	internal static class Helpers
	{
		static readonly IntPtr setAccessibilityIdentifier_Handle = Selector.GetHandle ("setAccessibilityIdentifier:");
		public static T SetAccessibilityId<T>(this T view, string id) where T : NSObject
		{
			var nsId = NSString.CreateNative (id);
			Messaging.void_objc_msgSend_IntPtr (view.Handle, setAccessibilityIdentifier_Handle, nsId);
			return view;
		}
	}
}