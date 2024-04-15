using Android.App;
using Android.Content.PM;
using Android.OS;
using Java.Lang;

namespace BlazeJump.Native.TestRunner
{
	[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
	public class MainActivity : MauiAppCompatActivity
	{
		public MainActivity()
		{
		}
	}
}