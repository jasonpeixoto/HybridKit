﻿using System;

using UIKit;
using Foundation;

using HybridKit;

namespace KitchenSink.iOS {

	public class WebViewController : UIViewController, IUIWebViewDelegate {

		public new UIWebView View {
			get { return (UIWebView)base.View; }
			set { base.View = value; }
		}

		public override void LoadView ()
		{
			View = new UIWebView {
				Delegate = this
			};
			View.LoadFromBundle ("www/index.html");
		}

		[Export ("webView:didFailLoadWithError:")]
		public void LoadFailed (UIWebView webView, NSError error)
		{
			Console.Error.WriteLine ("Web View loading failed: {0}", error.Description);
		}
		

		[Export ("webViewDidFinishLoad:")]
		public async void LoadingFinished (UIWebView webView)
		{
			await webView.RunScriptAsync (KitchenSink.CallJavaScript);
			Console.WriteLine ("Script finished!");
		}

		[Export ("webView:shouldStartLoadWithRequest:navigationType:")]
		public bool ShouldStartLoad (UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
		{
			return true;
		}
		
	}
}

