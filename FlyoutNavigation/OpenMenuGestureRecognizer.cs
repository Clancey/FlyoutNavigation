// 
//  Copyright 2012  Clancey
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
using UIKit;
using ObjCRuntime;
using MediaPlayer;
using Foundation;

namespace FlyoutNavigation
{
	public class OpenMenuGestureRecognizer : UIPanGestureRecognizer
	{
		public OpenMenuGestureRecognizer (Action<UIPanGestureRecognizer> callback, Func<UIGestureRecognizer, UITouch,bool>  shouldReceiveTouch) : base (callback)
		{
			this.ShouldReceiveTouch += (sender,touch)=> {
				//Ugly hack to ignore touches that are on a cell that is moving...
				bool isMovingCell = touch.View.ToString().IndexOf("UITableViewCellReorderControl",StringComparison.InvariantCultureIgnoreCase) > -1;
				if(touch.View is UISlider || touch.View is MPVolumeView || isMovingCell)
					return false;
				return shouldReceiveTouch(sender,touch);
			};
		}
	}
}

