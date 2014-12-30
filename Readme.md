# HybridKit

Simple C# - JavaScript bridge for building hybrid iOS and Android apps.

### Here's what it does:

- Call script functions and get/set values on script objects from C# using `dynamic`
	- Marshals simple values between script and C# by value (JSON)
	- Marshals complex script objects to C# by reference
- Catch script exceptions in C#

### Here's what it doesn't do (yet):

- Call C# methods from a script within the web view
- Attach C# event handlers to events on script objects
- Create strongly-typed wrappers for JavaScript APIs

# Getting Started

1. Add HybridKit to your app.
2. Add the following references to your project:
	+ `Microsoft.CSharp`
	+ For Android only:
		- `Mono.Android.Export`
		- `Xamarin.Forms` (currently required even if your project does not use it)

Note that for Xamarin.Forms, HybridKit currently requires Xamarin.Forms 1.3.1 or later. At this time, you need to select `Show pre-release packages` in the NuGet gallery to install this on your project.

## Create the Web View

### Xamarin.iOS

On iOS, the HybridKit API is exposed as extension methods on `UIWebView`:

```
using UIKit;
using HybridKit;
// ...
var webView = new UIWebView ();
```

### Xamarin.Android

On Android, create an instance of `HybridKit.Android.HybridWebView`. It extends `Android.WebKit.WebView` and will need to be configured in the same way (generally at least enabling JavaScript, as shown below):

```
using HybridKit.Android;
// ...
var webView = new HybridWebView ();
webView.Settings.JavaScriptEnabled = true;
```

### Xamarin.Forms

On iOS or Android when using [Xamarin.Forms](http://xamarin.com/forms), create a new `HybridKit.Forms.HybridWebView`, which extends from `Xamarin.Forms.WebView`:

```
using HybridKit.Forms;

// ... On the native app:
HybridWebViewRenderer.Init ();

// ... In the shared code:
var webView = new HybridWebView ();
```

## Load some Content

You may want to load an HTML file that is bundled with your app into the web view. To make this easier, HybridKit supplies the `LoadFromBundle` extension method on **iOS and Android**:

```
webView.LoadFromBundle ("index.html");
```

For **Xamarin.Forms**, HybridKit supplies the `BundleWebViewSource` class:

```
webView.Source = new BundleWebViewSource ("index.html");
```
The path passed into both of these APIs is the bundle-relative path on iOS, or the path relative to the Assets folder for an Android project.

## Run some Script

Call `RunScriptAsync` on the web view to execute script inside the web view.

Although the name of this method ends with "Async," it may actually run synchronously depending on the threading constraints of the current platform (see important note on threading below). Generally, you'll want to await the `Task` returned by this method, which simulates a synchronous method call in either case:

```
await webView.RunScriptAsync (window => {
	var name = window.prompt ("What is your name?") ?? "World";
	var node = window.document.createTextNode (string.Format ("Hello {0} from C#!", name));
	window.document.body.appendChild (node);
});
```
The `window` argument to the lambda is the JavaScript global object. In the above example, we are calling the JavaScript `prompt` function, which displays a dialog to the user and returns their entered text (or the string "World" if they click Cancel). Then we are creating a new text node and appending it to the DOM. You can see this code in action in the KitchenSink sample app.

**Important Note about Threading**

Within the script lambda passed to `RunScriptAsync`, HybridKit evaluates JavaScript synchronously. Unfortunately, different platforms have different requirements for this:

- On iOS, all calls into JavaScript must be done from the main UI thread.
- On newer versions of Android, we will deadlock if we try to call JavaScript synchronously from the main UI thread (as of KitKat and later).

To accommodate these different requirements, the `RunScriptAsync` method may dispatch to a different thread to run the passed lambda. Because of this, you must be careful to properly synchronize external objects used in the lambda, and take proper care of any script objects persisted outside the scope of the lambda. Script objects may be used safely in multiple `RunScriptAsync` calls. For example:

```
// DO NOT do this! It causes an implicit ToString call on the current thread, which might not work:

dynamic foo;
await webView.RunScriptAsync (window => {
	foo = window.foo;
});
// ...
Console.WriteLine (foo);
```

```
// This is OK - the RunScriptAsync method ensures the implicit ToString call happens on the correct thread:

dynamic foo;
await webView.RunScriptAsync (window => {
	foo = window.foo;
});
// ...
await webView.RunScriptAsync (_ => Console.WriteLine (foo));
```

# Current Status

This project is brand new, so expect bugs. Feedback and contributions are always welcome!