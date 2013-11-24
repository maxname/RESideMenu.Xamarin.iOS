using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.ObjCRuntime;
using System.Drawing;
using System.Linq;
using MonoTouch.Foundation;

namespace SideMenuTest
{
	public class RESideMenuViewController : UIViewController
    {
		private UIImage _backgroundImage;
		private UIViewController _contentViewController;
		private UIViewController _menuViewController;
		private UIImageView _backgroundImageView;
		private UIButton _contentButton;
		private bool _visible;
		private PointF _originalPoint;

		public event Action<UIViewController> WillShowMenuViewController;
		public event Action<UIViewController> DidShowMenuViewController;
		public event Action<UIViewController> WillHideMenuViewController;
		public event Action<UIViewController> DidHideMenuViewController;
		public event Action<UIPanGestureRecognizer> DidRecognizePanGesture;


		public float AnimationDuration { get; set; }
		public bool IsPanGestureEnabled { get; set; }
		public bool ScaleContentView { get; set; }
		public bool ScaleBackgroundImageView { get; set; }
		public float ContentViewScaleValue { get; set; }
		public float ContentViewInLandscapeOffsetCenterX { get; set; }
		public float ContentViewInPortraitOffsetCenterX { get; set; }
		public int ParallaxMenuMinimumRelativeValue { get; set; }
		public int ParallaxMenuMaximumRelativeValue { get; set; }
		public int ParallaxContentMinimumRelativeValue { get; set; }
		public int ParallaxContentMaximumRelativeValue { get; set; }
		public bool IsParallaxEnabled { get; set; }


		public UIImage BackgroundImage
		{
			get { return _backgroundImage; }
			set 
			{ 
				_backgroundImage = value;
				if (_backgroundImageView != null)
					_backgroundImageView.Image = _backgroundImage; 
			}
		}

		public UIViewController ContentViewController
		{
			get { return _contentViewController; }
			set
			{
				if (_contentViewController == null) 
				{
					_contentViewController = value;
					return;
				}

				var frame = _contentViewController.View.Frame;
				var transform = _contentViewController.View.Transform;
				RehideController(_contentViewController);
				_contentViewController = value;
				RedisplayController(_contentViewController, View.Frame);
				_contentViewController.View.Transform = transform;
				_contentViewController.View.Frame = frame;
				AddContentViewControllerMotionEffects();
			}
		}

		public UIViewController MenuViewController
		{
			get { return _menuViewController; }
			set
			{
				if (_menuViewController == null)
				{
					_menuViewController = value;
					return;
				}
				RehideController(_menuViewController);
				_menuViewController = value;
				RedisplayController(_menuViewController, View.Frame);

				AddMenuViewControllerMotionEffects();
				View.BringSubviewToFront(_contentViewController.View);
			}
		}

		public void SetContentViewController(UIViewController contentViewController, bool animated)
		{
			if (!animated) 
			{
				ContentViewController = contentViewController;
			}
			else
			{
				contentViewController.View.Alpha = 0;
				_contentViewController.View.AddSubview(contentViewController.View);
				UIView.Animate(AnimationDuration, () => {
					contentViewController.View.Alpha = 1;
				},
				() => {
					contentViewController.View.RemoveFromSuperview();
					ContentViewController = contentViewController;
				});
			}
		}


		public RESideMenuViewController()
        {
			WantsFullScreenLayout = true;
			AnimationDuration = 0.35f;
			IsPanGestureEnabled = true;
			ScaleContentView = true;
			ContentViewScaleValue = 0.7f;
			ScaleBackgroundImageView = true;
			IsParallaxEnabled = true;
			ParallaxMenuMinimumRelativeValue = -15;
			ParallaxMenuMaximumRelativeValue = 15;
			ParallaxContentMinimumRelativeValue = -25;
			ParallaxContentMaximumRelativeValue = 25;
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			if (ContentViewInLandscapeOffsetCenterX == 0.0f)
				ContentViewInLandscapeOffsetCenterX = View.Frame.Height + 30f;

			if (ContentViewInPortraitOffsetCenterX == 0.0f)
				ContentViewInPortraitOffsetCenterX  = View.Frame.Width + 30f;

			View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

			_backgroundImageView =  new UIImageView(View.Bounds)
			{
				Image = _backgroundImage,
				ContentMode = UIViewContentMode.ScaleAspectFill,
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight
			};

			_contentButton = UIButton.FromType(UIButtonType.Custom);
			_contentButton.TouchUpInside += (sender, e) => {
				HideMenuViewController();
			};

			View.AddSubview(_backgroundImageView);

			RedisplayController(_menuViewController, View.Frame);
			RedisplayController(_contentViewController, View.Frame);

			_menuViewController.View.Alpha = 0;

			if (ScaleBackgroundImageView)
				_backgroundImageView.Transform = CGAffineTransform.MakeScale(1.7f, 1.7f);

			AddMenuViewControllerMotionEffects();

			if (IsPanGestureEnabled) 
			{
				View.AddGestureRecognizer(new UIPanGestureRecognizer(PanGestureRecognized));
			}
		}

		private void RedisplayController(UIViewController controller, RectangleF frame)
		{
			AddChildViewController(controller);
			controller.View.Frame = frame;
			View.AddSubview(controller.View);
			controller.DidMoveToParentViewController(this);
		}

		private void RehideController(UIViewController controller)
		{
			controller.WillMoveToParentViewController(null);
			controller.View.RemoveFromSuperview();
			controller.RemoveFromParentViewController();
		}

		public void PresentMenuViewController()
		{
			_menuViewController.View.Transform = CGAffineTransform.MakeIdentity();
			if (ScaleBackgroundImageView)
			{
				_backgroundImageView.Transform = CGAffineTransform.MakeIdentity();
				_backgroundImageView.Frame = View.Bounds;
			}

			_menuViewController.View.Frame = View.Bounds;
			_menuViewController.View.Transform = CGAffineTransform.MakeScale(1.5f, 1.5f);
			_menuViewController.View.Alpha = 0;

			if (ScaleBackgroundImageView)
			{
				_backgroundImageView.Transform = CGAffineTransform.MakeScale(1.7f, 1.7f);
			}

			if (WillShowMenuViewController != null)
			{
				WillShowMenuViewController(_menuViewController);
			}

			ShowMenuViewController();
		}

		private void ShowMenuViewController()
		{
			View.Window.EndEditing(true);
			AddContentButton();

			UIView.Animate(AnimationDuration, () => {
				if (ScaleContentView)
				{
					_contentViewController.View.Transform = CGAffineTransform.MakeScale(ContentViewScaleValue, ContentViewScaleValue);
				}

				var landscape = UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeLeft 
				                || UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeRight;
				var x = landscape ? ContentViewInLandscapeOffsetCenterX : ContentViewInPortraitOffsetCenterX;
				var y = _contentViewController.View.Center.Y;
				_contentViewController.View.Center = new PointF(x, y);

				_menuViewController.View.Alpha = 1.0f;
				_menuViewController.View.Transform = CGAffineTransform.MakeIdentity();
				if (ScaleBackgroundImageView)
				{
					_backgroundImageView.Transform = CGAffineTransform.MakeIdentity();
				}
			}, 
			() => {
				AddContentViewControllerMotionEffects();

				if (!_visible && DidShowMenuViewController != null)
				{
						DidShowMenuViewController(_menuViewController);
				}

				_visible = true;
			});

			UpdateStatusBar();
		}

		private void HideMenuViewController()
		{
			if (WillHideMenuViewController != null) 
			{
				WillHideMenuViewController(_menuViewController);
			}

			_contentButton.RemoveFromSuperview();

			UIApplication.SharedApplication.BeginIgnoringInteractionEvents();

			UIView.Animate(AnimationDuration, () => {

				_contentViewController.View.Transform = CGAffineTransform.MakeIdentity();
				_contentViewController.View.Frame = View.Bounds;
				_menuViewController.View.Transform = CGAffineTransform.MakeScale(1.5f, 1.5f);
				_menuViewController.View.Alpha = 0;

				if (ScaleBackgroundImageView) {
					_backgroundImageView.Transform = CGAffineTransform.MakeScale(1.7f, 1.7f);
				}

				if (IsParallaxEnabled && IsIOS7) 
				{
					if (_contentViewController.View.MotionEffects != null)
					{
						foreach (var me in _contentViewController.View.MotionEffects) {
							_contentViewController.View.RemoveMotionEffect(me);
	                    }
					}
				}
			}, 
			() => {
					UIApplication.SharedApplication.EndIgnoringInteractionEvents();

					if (!_visible && DidHideMenuViewController != null) 
					{
						DidHideMenuViewController(_menuViewController);
					}
			});

			_visible = false;
			UpdateStatusBar();
		}

		private void AddContentButton()
		{
			if (_contentButton.Superview != null)
				return;
			_contentButton.AutoresizingMask = UIViewAutoresizing.None;
			_contentButton.Frame = _contentViewController.View.Bounds;
			_contentButton.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			_contentViewController.View.AddSubview(_contentButton);
		}

		private void AddMenuViewControllerMotionEffects()
		{
			if (IsParallaxEnabled && IsIOS7) 
			{
				if (_menuViewController.View.MotionEffects != null)
				{
					foreach (var me in _menuViewController.View.MotionEffects)
					{
						_menuViewController.View.RemoveMotionEffect(me);
					}
				}
				var interpolationHorizontal = new UIInterpolatingMotionEffect("center.x", UIInterpolatingMotionEffectType.TiltAlongHorizontalAxis);
				interpolationHorizontal.MinimumRelativeValue = new NSNumber(ParallaxMenuMinimumRelativeValue);
				interpolationHorizontal.MaximumRelativeValue = new NSNumber(ParallaxMenuMaximumRelativeValue);

				var interpolationVertical = new UIInterpolatingMotionEffect("center.y", UIInterpolatingMotionEffectType.TiltAlongVerticalAxis);
				interpolationVertical.MinimumRelativeValue = new NSNumber(ParallaxMenuMinimumRelativeValue);
				interpolationVertical.MaximumRelativeValue = new NSNumber(ParallaxMenuMaximumRelativeValue);

				_menuViewController.View.AddMotionEffect(interpolationHorizontal);
				_menuViewController.View.AddMotionEffect(interpolationVertical);
			}
		}

		private void AddContentViewControllerMotionEffects()
		{
			if (IsParallaxEnabled && IsIOS7)
			{
				if (_contentViewController.View.MotionEffects != null)
				{
					foreach (var me in _contentViewController.View.MotionEffects)
					{
						_contentViewController.View.RemoveMotionEffect(me);
					}
				}
				UIView.Animate(0.2, () => {
					var interpolationHorizontal = new UIInterpolatingMotionEffect("center.x", UIInterpolatingMotionEffectType.TiltAlongHorizontalAxis);
					interpolationHorizontal.MinimumRelativeValue = new NSNumber(ParallaxContentMinimumRelativeValue);
					interpolationHorizontal.MaximumRelativeValue = new NSNumber(ParallaxContentMaximumRelativeValue);

					var interpolationVertical = new UIInterpolatingMotionEffect("center.y", UIInterpolatingMotionEffectType.TiltAlongVerticalAxis);
					interpolationVertical.MinimumRelativeValue = new NSNumber(ParallaxContentMinimumRelativeValue);
					interpolationVertical.MaximumRelativeValue = new NSNumber(ParallaxContentMaximumRelativeValue);

					_contentViewController.View.AddMotionEffect(interpolationHorizontal);
					_contentViewController.View.AddMotionEffect(interpolationVertical);
				});
			}
		}

		private void PanGestureRecognized(UIPanGestureRecognizer recognizer)
		{
			if (DidRecognizePanGesture != null) {
				DidRecognizePanGesture(recognizer);
			}

			if (!IsPanGestureEnabled) {
				return;
			}

			var point = recognizer.TranslationInView(View);

			if (recognizer.State == UIGestureRecognizerState.Began) 
			{
				if (!_visible && WillShowMenuViewController != null) {
					WillShowMenuViewController(_menuViewController);
				}
				_originalPoint = _contentViewController.View.Frame.Location;
				_menuViewController.View.Transform = CGAffineTransform.MakeIdentity();
				if (ScaleBackgroundImageView) 
				{
					_backgroundImageView.Transform = CGAffineTransform.MakeIdentity();
					_backgroundImageView.Frame = View.Bounds;
				}

				_menuViewController.View.Frame = View.Bounds;
				AddContentButton();
				View.Window.EndEditing(true);
			}

			if (recognizer.State == UIGestureRecognizerState.Began || recognizer.State == UIGestureRecognizerState.Changed)
			{
				var delta = _visible ? (point.X + _originalPoint.X) / _originalPoint.X : point.X / View.Frame.Size.Width;

				var contentViewScale = ScaleContentView ? 1 - ((1 - ContentViewScaleValue) * delta) : 1;
				var backgroundViewScale = 1.7f - (0.7f * delta);
				var menuViewScale = 1.5f - (0.5f * delta);

				_menuViewController.View.Alpha = delta;
				if (ScaleBackgroundImageView)
				{
					_backgroundImageView.Transform = CGAffineTransform.MakeScale(backgroundViewScale, backgroundViewScale);
				}

				_menuViewController.View.Transform = CGAffineTransform.MakeScale(menuViewScale, menuViewScale);

				if (ScaleBackgroundImageView && backgroundViewScale < 1)
				{
					_backgroundImageView.Transform = CGAffineTransform.MakeIdentity();
				}

				if (contentViewScale > 1)
				{
					if (!_visible) {
						_contentViewController.View.Transform = CGAffineTransform.MakeIdentity();
					}
					_contentViewController.View.Frame = View.Bounds;
				} 
				else 
				{
					_contentViewController.View.Transform = CGAffineTransform.MakeScale(contentViewScale, contentViewScale);
					_contentViewController.View.Transform.Translate(_visible ? point.X * 0.8f : point.X, 0);
				}

				UpdateStatusBar();
			}

			if (recognizer.State == UIGestureRecognizerState.Ended)
			{
				if (recognizer.VelocityInView(View).X > 0) 
				{
					ShowMenuViewController();
				}
				else 
				{
					HideMenuViewController();
				}
			}
		}

		public override bool ShouldAutorotate()
		{
			if (_contentViewController != null)
				return _contentViewController.ShouldAutorotate();
			return base.ShouldAutorotate();
		}

		public override void WillAnimateRotation(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			if (_visible)
			{
				_contentViewController.View.Transform = CGAffineTransform.MakeIdentity();
				_contentViewController.View.Frame = View.Bounds;
				_contentViewController.View.Transform = CGAffineTransform.MakeScale(ContentViewScaleValue, ContentViewScaleValue);
				var landscape = UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeLeft 
				                || UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeRight;
				var x = landscape ? ContentViewInLandscapeOffsetCenterX : ContentViewInPortraitOffsetCenterX;
				var y = _contentViewController.View.Center.Y;
				_contentViewController.View.Center = new PointF(x, y);
			}
		}

		private void UpdateStatusBar()
		{
			UIView.Animate(0.3f, () => {
				SetNeedsStatusBarAppearanceUpdate();
			});
		}

		public override UIStatusBarStyle PreferredStatusBarStyle()
		{
			var statusBarStyle = UIStatusBarStyle.Default;

			if (IsIOS7)
			{
				statusBarStyle = _visible ? _menuViewController.PreferredStatusBarStyle() : _contentViewController.PreferredStatusBarStyle();
				if (_contentViewController.View.Frame.Y > 10)
				{
					statusBarStyle = _menuViewController.PreferredStatusBarStyle();
				}
				else
				{
					statusBarStyle = _contentViewController.PreferredStatusBarStyle();
				}
			}

			return statusBarStyle;
		}

		public override bool PrefersStatusBarHidden()
		{
			var statusBarHidden = false;
			if (IsIOS7)
			{
				statusBarHidden = _visible ? _menuViewController.PrefersStatusBarHidden() : _contentViewController.PrefersStatusBarHidden();
				if (_contentViewController.View.Frame.Y > 10)
				{
					statusBarHidden = _menuViewController.PrefersStatusBarHidden();
				}
				else
				{
					statusBarHidden = _contentViewController.PrefersStatusBarHidden();
				}
			}
			return statusBarHidden;
		}

		public override UIStatusBarAnimation PreferredStatusBarUpdateAnimation
		{
			get
			{
				var statusBarAnimation = UIStatusBarAnimation.None;
				if (IsIOS7)
				{
					statusBarAnimation = _visible ? _menuViewController.PreferredStatusBarUpdateAnimation : _contentViewController.PreferredStatusBarUpdateAnimation;
					if (_contentViewController.View.Frame.Y > 10)
					{
						statusBarAnimation = _menuViewController.PreferredStatusBarUpdateAnimation;
					}
					else
					{
						statusBarAnimation = _contentViewController.PreferredStatusBarUpdateAnimation;
					}
				}
				return statusBarAnimation;
			}
		}

		private bool? _ios7;
		private bool IsIOS7 {
			get {
				if (_ios7 == null)
				{
					int version = Convert.ToInt16(UIDevice.CurrentDevice.SystemVersion.Split('.')[0]);
					_ios7 = version >= 7;
				}
				return _ios7.Value;
			}
		}
    }
}

