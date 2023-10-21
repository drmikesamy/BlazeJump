using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Java.Lang;
using static Java.Util.Jar.Attributes;
using BlazeJump.Common.Services.Notification;

namespace BlazeJump.Native
{
	[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density, LaunchMode = LaunchMode.SingleTop)]

	[IntentFilter(new[] { Intent.ActionView },
						DataScheme = "nostrconnect",
						Categories = new[] { Intent.ActionView, Intent.CategoryDefault, Intent.CategoryBrowsable })]
	public class MainActivity : MauiAppCompatActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			OnNewIntent(this.Intent);
		}
		protected override void OnNewIntent(Intent intent)
		{
			base.OnNewIntent(intent);
			var strLink = intent.DataString;
			if (intent.Action == Intent.ActionView && !string.IsNullOrWhiteSpace(strLink))
			{
				var notificationService = MauiApplication.Current.Services.GetRequiredService<INotificationService>();
				notificationService.OnDeepLinkReceived(strLink);
			}
		}
		public MainActivity()
		{
			try
			{
				JavaSystem.LoadLibrary("secp256k1");
				JavaSystem.LoadLibrary("tinyaes");
			}
			catch (UnsatisfiedLinkError e)
			{
				Console.WriteLine(e.Message);
			}
		}
	}
}