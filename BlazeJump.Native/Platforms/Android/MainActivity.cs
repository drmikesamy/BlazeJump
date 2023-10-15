using Android.App;
using Android.Content.PM;
using Android.OS;
using Java.Lang;
using static Java.Util.Jar.Attributes;

namespace BlazeJump.Native
{
	[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
	public class MainActivity : MauiAppCompatActivity
	{
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