using System;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.CoreAnimation;

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
		UIView mainView;
		public UISearchBar SearchBar;
		public Action SelectedIndexChanged {get;set;}
		const int menuWidth = 250;
		
		public FlyOutNavigationController ()
		{
			navigation = new DialogViewController(UITableViewStyle.Plain,null);
			navigation.OnSelection = NavigationItemSelected;
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
			//Console.WriteLine(indexPath);
			var index =  GetIndex(indexPath);
			//if(SelectedIndex == index)
			//	return;
			SelectedIndex = index;
			//Console.WriteLine(SelectedIndex);
			if(mainView != null)
				mainView.RemoveFromSuperview();
			mainView = ViewControllers[SelectedIndex].View;
			mainView.Frame = View.Bounds;
			
			mainView.Layer.ShadowOffset = new System.Drawing.SizeF(-5,-1);
			mainView.Layer.ShadowColor = UIColor.Black.CGColor;
			mainView.Layer.ShadowOpacity = .75f;
			
			this.View.AddSubview(mainView);
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
			UIView.BeginAnimations("slideMenu");
			UIView.SetAnimationCurve(UIViewAnimationCurve.EaseIn);
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
			//UIView.SetAnimationDuration(.5);
			UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut);
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

