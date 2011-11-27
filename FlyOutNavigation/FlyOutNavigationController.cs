using System;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;

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
				navigation.SearchBarTintColor = value;
			}
		}
		DialogViewController navigation;
		UIView mainView;
		UIView currentView;
		const int menuWidth = 250;
		
		public FlyOutNavigationController ()
		{
			navigation = new DialogViewController(UITableViewStyle.Plain,null);
			navigation.EnableSearch = true;
			navigation.OnSelection = NavigationItemSelected;
			TintColor = UIColor.Black;
			var navFrame = navigation.View.Frame;
			navFrame.Width = menuWidth;
			navigation.View.Frame = navFrame;
			this.View.AddSubview(navigation.View);
			var mainFrame = View.Bounds;
			//mainFrame.X = menuWidth;
			mainView = new UIView(mainFrame);
			mainView.BackgroundColor = UIColor.Blue;
			mainView.Layer.ShadowOffset = new System.Drawing.SizeF(-5,-1);
			mainView.Layer.ShadowColor = UIColor.Black.CGColor;
			mainView.Layer.ShadowOpacity = .75f;
			this.View.AddSubview(mainView);
		}
		
		public RootElement NavigationRoot {
			get{return navigation.Root;}
			set{navigation.Root = value;}
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
			Console.WriteLine(indexPath);
			SelectedIndex = GetIndex(indexPath);
			Console.WriteLine(SelectedIndex);
			if(currentView != null)
				currentView.RemoveFromSuperview();
			currentView = ViewControllers[SelectedIndex].View;
			currentView.Frame = mainView.Bounds;
			mainView.AddSubview(currentView);
			HideMenu();
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
			UIView.BeginAnimations("slideMenu");
			//UIView.SetAnimationDuration(2);
			var frame = mainView.Frame;
			frame.X = menuWidth;
			mainView.Frame = frame;
			UIView.CommitAnimations();
		}
		
		public void HideMenu()
		{
			isOpen = false;
			navigation.FinishSearch();
			UIView.BeginAnimations("slideMenu");
			var frame = mainView.Frame;
			frame.X = 0;
			mainView.Frame = frame;
			UIView.CommitAnimations();
		}
		
		public void ToggleMenu()
		{
			if(isOpen)
				HideMenu();
			else
				ShowMenu();
		}
		
		public int SelectedIndex
		{
			get;set;
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
	}
}

