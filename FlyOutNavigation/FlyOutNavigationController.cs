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
		const int menuWidth = 250;
		private UIView shadowView;
		private UIButton closeButton;
		public bool AlwaysShowLandscapeMenuOnIpad {get;set;}
		public bool ForceMenuOpen {get;set;}
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
			AlwaysShowLandscapeMenuOnIpad = true;
			
			this.View.AddGestureRecognizer(new OpenMenuGestureRecognizer(this,new Selector("swiperight")));
		}
				
		[Export("swiperight")]
		public void Swipped(UISwipeGestureRecognizer sender)
		{
			ShowMenu();
		}
		public override void ViewWillAppear (bool animated)
		{			
			var navFrame = navigation.View.Frame;
			navFrame.Width = menuWidth;
			navigation.View.Frame = navFrame;
			base.ViewWillAppear (animated);
		}
		
		public RootElement NavigationRoot {
			get{return navigation.Root;}
			set{navigation.Root = value;}
		}
		public UITableView NavigationTableView {
			get{return navigation.TableView;}
		}
		UIViewController[] viewControllers;
		public UIViewController[] ViewControllers {
			get{return viewControllers;}
			set{
				viewControllers = value;
				NavigationItemSelected(GetIndexPath(SelectedIndex));
			}
		}
		
		private void NavigationItemSelected(NSIndexPath indexPath){
			var index =  GetIndex(indexPath);
			NavigationItemSelected(index);
			
		}		
		private void NavigationItemSelected(int index){
			selectedIndex = index;			
			if(viewControllers == null || viewControllers.Length < index || index < 0)
				return;
			if (ViewControllers[index] == null)
				return;
			
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
			isOpen = true;
			closeButton.Frame = mainView.Frame;
			shadowView.Frame = mainView.Frame;
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
		}
		bool ShouldStayOpen
		{
			get{
				if(ForceMenuOpen || (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad && 
				AlwaysShowLandscapeMenuOnIpad && 
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
			isOpen = false;
			navigation.FinishSearch();
			closeButton.RemoveFromSuperview();
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
		}
		[Export("animationEnded")]
		private void hideComplete()
		{
			shadowView.RemoveFromSuperview();
		}
		
		public void ToggleMenu()
		{
			if(isOpen)
				HideMenu();
			else
				ShowMenu();
		}
		private int selectedIndex;
		public int SelectedIndex
		{
			get{return selectedIndex;}
			set{
				if(selectedIndex == value)
					return;
				selectedIndex = value;
				NavigationItemSelected(value);
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
			return !DisableRotation;
		}
		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
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
		public override void ViewWillDisappear (bool animated)
		{
			if(!IsIos5 && CurrentViewController != null)
				CurrentViewController.ViewWillAppear(animated);
		}
		public static bool IsIos5 {
			get{ return new System.Version(UIDevice.CurrentDevice.SystemVersion).Major >= 5 ;}
		}
	}
}

