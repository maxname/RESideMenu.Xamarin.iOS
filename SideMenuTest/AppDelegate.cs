using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using SideMenuTest.Controllers;

namespace SideMenuTest
{
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
		private UIWindow _window;
        
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
			_window = new UIWindow(UIScreen.MainScreen.Bounds) {
				RootViewController = new RootController()
			};

            _window.MakeKeyAndVisible();
			
            return true;
        }
    }
}

