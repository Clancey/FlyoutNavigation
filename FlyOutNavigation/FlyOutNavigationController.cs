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
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.ObjCRuntime;
using MonoTouch.MediaPlayer;

namespace FlyOutNavigation
{
	public class FlyOutNavigationController : UIViewController
	{
		UIColor tintColor;
		public UIColor TintColor
		{
			get{return tintColor;}
			set{ 
				if(tintColor == value)
					return;
				SearchBar.TintColor = value;
			}
		}
		
		DialogViewController navigation;
		public UISearchBar SearchBar;
		public Action SelectedIndexChanged {get;set;}
		public const int menuWidth = 250;
		private UIView shadowView;
		private UIButton closeButton;
		public bool AlwaysShowLandscapeMenu {get;set;}
		public bool ForceMenuOpen {get;set;}
		public bool HideShadow{get;set;}
		public UIViewController CurrentViewController{get;private set;}
		UIView mainView {
			get{
				if(CurrentViewController == null)
					return null;
				return CurrentViewController.View;
			}
		}
		
		public FlyOutNavigationController ()
		{
			navigation = new DialogViewController(UITableViewStyle.Plain,null);
			navigation.OnSelection += NavigationItemSelected;
			var navFrame = navigation.View.Frame;
			navFrame.Width = menuWidth;
			navigation.View.Frame = navFrame;
			this.View.AddSubview(navigation.View);
			SearchBar = new UISearchBar (new RectangleF (0, 0, navigation.TableView.Bounds.Width, 44)) {
			//Delegate = new SearchDelegate (this),
			TintColor = this.TintColor
				};
			
			TintColor = UIColor.Black;
			//navigation.TableView.TableHeaderView = SearchBar;
			navigation.TableView.TableFooterView = new UIView(new RectangleF(0,0,100,100)){BackgroundColor = UIColor.Clear};
			navigation.TableView.ScrollsToTop = false;
			shadowView = new UIView();
			shadowView.BackgroundColor = UIColor.White;
			shadowView.Layer.ShadowOffset = new System.Drawing.SizeF(-5,-1);
			shadowView.Layer.ShadowColor = UIColor.Black.CGColor;
			shadowView.Layer.ShadowOpacity = .75f;
			closeButton = new UIButton();
			closeButton.TouchDown += delegate {
				HideMenu();
			};
			AlwaysShowLandscapeMenu = true;
			
			this.View.AddGestureRecognizer(new OpenMenuGestureRecognizer(this,new Selector("swiperight")));
		}
			
		public override void ViewDidLayoutSubviews ()
		{
			base.ViewDidLayoutSubviews ();
			var navFrame = navigation.View.Frame;
			navFrame.Width = menuWidth;
			if(navigation.View.Frame != navFrame)
				navigation.View.Frame = navFrame;
		}
		[Export("swiperight")]
		public void Swipped(UISwipeGestureRecognizer sender)
		{
			if(!ShouldStayOpen)
				ShowMenu();
		}
		public override void ViewWillAppear (bool animated)
		{			
			var navFrame = navigation.View.Frame;
			navFrame.Width = menuWidth;
			navFrame.Location = PointF.Empty;
			navigation.View.Frame = navFrame;
			base.ViewWillAppear (animated);
		}
		
		public RootElement NavigationRoot {
			get{return navigation.Root;}
		set{EnsureInvokedOnMainThread(delegate{navigation.Root = value;});}
		}
		public UITableView NavigationTableView {
			get{return navigation.TableView;}
		}
		UIViewController[] viewControllers;
		public UIViewController[] ViewControllers {
			get{return viewControllers;}
			set{
				EnsureInvokedOnMainThread(delegate{
					viewControllers = value;
					NavigationItemSelected(GetIndexPath(SelectedIndex));
				});
			}
		}
		
		private void NavigationItemSelected(NSIndexPath indexPath){
			var index =  GetIndex(indexPath);
			NavigationItemSelected(index);
			
		}		
		private void NavigationItemSelected(int index){
			selectedIndex = index;			
			if(viewControllers == null || viewControllers.Length <= index || index < 0)
			{
				if(SelectedIndexChanged != null)
					SelectedIndexChanged();
				return;
			}
			if (ViewControllers[index] == null)
			{
				if(SelectedIndexChanged != null)
					SelectedIndexChanged();
				return;
			}
			
			if(mainView != null)
				mainView.RemoveFromSuperview();
			CurrentViewController = ViewControllers[SelectedIndex];
			var frame = View.Bounds;
			if(isOpen)
				frame.X = menuWidth;
		
			mainView.Frame = frame;
			setViewSize();
		
			this.View.AddSubview(mainView);
			if(!ShouldStayOpen)
				HideMenu();
			if(SelectedIndexChanged != null)
				SelectedIndexChanged();
			
		}
		
		bool isOpen;
		public bool IsOpen{
			get{return isOpen;}
			set{ 
				if(isOpen == value)
					return;
				if(value)
					HideMenu();
				else
					ShowMenu();
			}
		}
		
		public void ShowMenu()
		{
			if(isOpen)
				return;
			EnsureInvokedOnMainThread(delegate{
				isOpen = true;
				closeButton.Frame = mainView.Frame;
				shadowView.Frame = mainView.Frame;
				if(!HideShadow)
					this.View.InsertSubviewBelow(shadowView,mainView);
				if(!ShouldStayOpen)
					this.View.AddSubview(closeButton);
				UIView.BeginAnimations("slideMenu");
				UIView.SetAnimationCurve(UIViewAnimationCurve.EaseIn);
				//UIView.SetAnimationDuration(2);
				setViewSize();
				var frame = mainView.Frame;
				frame.X = menuWidth;
				mainView.Frame = frame;
				setViewSize();
				frame = mainView.Frame;
				shadowView.Frame = frame;
				closeButton.Frame = frame;
				UIView.CommitAnimations();
			});
		}
		bool ShouldStayOpen
		{
			get{
				if(ForceMenuOpen || (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad && 
				AlwaysShowLandscapeMenu && 
				(this.InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft 
				|| this.InterfaceOrientation == UIInterfaceOrientation.LandscapeRight)))
					return true;
				return false;	
			}
		}
		private void setViewSize()
		{
			var frame = View.Bounds;
			frame.Location = mainView.Frame.Location;
			if(ShouldStayOpen)
				frame.Width -= menuWidth;
			mainView.Frame = frame;
			
				
		}
		
		public void HideMenu()
		{
			if(!IsOpen)
				return;
			EnsureInvokedOnMainThread(delegate{
				isOpen = false;
				navigation.FinishSearch();
				closeButton.RemoveFromSuperview();
				shadowView.Frame = mainView.Frame;
				//UIView.AnimationWillEnd += hideComplete;
				UIView.BeginAnimations("slideMenu");
				UIView.SetAnimationDidStopSelector(new Selector("animationEnded"));
				//UIView.SetAnimationDuration(.5);
				UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut);
				var frame = this.View.Bounds;
				frame.X = 0;
				mainView.Frame = frame;
				shadowView.Frame = frame;
				UIView.CommitAnimations();
			});
		}
		[Export("animationEnded")]
		private void hideComplete()
		{
			shadowView.RemoveFromSuperview();
		}

		public void ResignFirstResponders(UIView view)
		{
			foreach(var subview in view.Subviews)
			{
				if (subview.IsFirstResponder)
					subview.ResignFirstResponder();
				ResignFirstResponders(subview);
			}
		}
		
		public void ToggleMenu()
		{
			EnsureInvokedOnMainThread(delegate{
				if(!isOpen && CurrentViewController != null && CurrentViewController.IsViewLoaded)
					ResignFirstResponders(CurrentViewController.View);
				if(isOpen)
					HideMenu();
				else
					ShowMenu();
			});
		}
		private int selectedIndex;
		public int SelectedIndex
		{
			get{return selectedIndex;}
			set{
				if(selectedIndex == value)
					return;
				selectedIndex = value;
				EnsureInvokedOnMainThread(delegate{
					NavigationItemSelected(value);
				});
			}
		}
		
		private int GetIndex(NSIndexPath indexPath)
		{
			int section = 0;
			int rowCount = 0;
			while(section < indexPath.Section)
			{
				rowCount += navigation.Root[section].Count;
				section ++;
			}
			return rowCount + indexPath.Row;
		}
		private NSIndexPath GetIndexPath(int index)
		{
			int currentCount = 0;
			int section = 0;
			foreach(var element in navigation.Root)
			{
				if(element.Count + currentCount > index)
					break;
				currentCount += element.Count;
				section ++;
			}
			
			var row = index - currentCount;
			return NSIndexPath.FromRowSection(row,section);
		}
		public bool DisableRotation {get;set;}
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			if(DisableRotation)
				return false;
			
			var theReturn= CurrentViewController == null? true: CurrentViewController.ShouldAutorotateToInterfaceOrientation(toInterfaceOrientation);
			return theReturn;
		}
		public override void WillRotate (UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate (toInterfaceOrientation, duration);
			if(CurrentViewController != null)
				CurrentViewController.WillRotate(toInterfaceOrientation,duration);
		}
		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
			CurrentViewController.DidRotate(fromInterfaceOrientation);
			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone) 
				return;
			switch(InterfaceOrientation)
			{
			case UIInterfaceOrientation.LandscapeLeft:
			case UIInterfaceOrientation.LandscapeRight:
				ShowMenu();
				return;
			default:
				HideMenu();
				return;
			}
			
		}

		public override void WillAnimateRotation (UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillAnimateRotation (toInterfaceOrientation, duration);
			if(CurrentViewController != null)
				CurrentViewController.WillAnimateRotation(toInterfaceOrientation, duration);
		}

		private void EnsureInvokedOnMainThread (Action action)
		{
			if (IsMainThread ())
			{
				action ();
				return;
			}
			this.BeginInvokeOnMainThread (() => action());
		}
		private static IntPtr GetClassHandle (string clsName)
		{
			return (new Class(clsName)).Handle;
		}
		
		private static bool IsMainThread() {
			return Messaging.bool_objc_msgSend(GetClassHandle("NSThread"), new Selector("isMainThread").Handle);
		}
		public override void ViewWillDisappear (bool animated)
		{
			if(!IsIos5 && CurrentViewController != null)
				CurrentViewController.ViewWillDisappear(animated);
		}
		public static bool IsIos5 {
			get{ return new System.Version(UIDevice.CurrentDevice.SystemVersion).Major >= 5 ;}
		}
	}
}

