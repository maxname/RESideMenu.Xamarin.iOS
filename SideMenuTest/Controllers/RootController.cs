using System;
using MonoTouch.UIKit;

namespace SideMenuTest.Controllers
{
	public class RootController : RESideMenuViewController
    {
        public RootController()
        {
			ContentViewController = new UINavigationController(new CardViewController {
				NavigationItem = {
					LeftBarButtonItem = new UIBarButtonItem("Menu", UIBarButtonItemStyle.Plain, ShowMenu)
				}
			});

			MenuViewController = new MenuController();

			BackgroundImage = UIImage.FromBundle("Images/MenuBackground.png");

			IsPanGestureEnabled = false;
        }

		public void ShowMenu(object sender, EventArgs e)
		{
			base.PresentMenuViewController();
		}
    }
}

