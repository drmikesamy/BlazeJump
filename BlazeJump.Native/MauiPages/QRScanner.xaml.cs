using ZXing.Net.Maui;
using BlazeJump.Common.Services.Notification;
using Newtonsoft.Json;
using BlazeJump.Common.Services.Identity;

namespace BlazeJump.Native.MauiPages;

public partial class QRScanner : ContentPage
{
	private IIdentityService _identityService;
	public QRScanner(IIdentityService identityService)
	{
		InitializeComponent();

		barcodeView.Options = new BarcodeReaderOptions
		{
			Formats = BarcodeFormats.TwoDimensional,
			AutoRotate = true,
			Multiple = false
		};

		_identityService = identityService;
	}

	protected void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
	{
		if (!barcodeView.IsDetecting)
		{
			return;
		}
		var qr = e.Results?.FirstOrDefault();
		if (qr is not null)
		{
			var payload = JsonConvert.DeserializeObject<QrConnectEventArgs>(qr.Value);
			MainThread.BeginInvokeOnMainThread(() =>
			{
				_identityService.OnQrConnectReceived(payload);
				barcodeView.IsDetecting = false;
			});
		}
	}



	void RescanClicked(object sender, EventArgs e)
	{
		barcodeView.IsDetecting = true;
	}

	void TorchButton_Clicked(object sender, EventArgs e)
	{
		barcodeView.IsTorchOn = !barcodeView.IsTorchOn;
	}
}