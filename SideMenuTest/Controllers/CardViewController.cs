using System;
using MonoTouch.UIKit;

namespace SideMenuTest.Controllers
{
	public class CardViewController : UIViewController
    {
        public CardViewController()
        {
        }

		public override void LoadView()
		{
			base.LoadView();
			View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle("Images/Balloon.png"));
		}
    }
}

