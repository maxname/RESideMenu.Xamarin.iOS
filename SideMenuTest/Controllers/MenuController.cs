using System;
using MonoTouch.UIKit;

namespace SideMenuTest.Controllers
{
	public class MenuController : UITableViewController
    {
		public override void LoadView()
		{
			base.LoadView();
			View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle("Images/Stars.png"));
			TableView.ScrollEnabled = false;
		}

		public override UIStatusBarStyle PreferredStatusBarStyle()
		{
			return UIStatusBarStyle.LightContent;
		}
    }
}

