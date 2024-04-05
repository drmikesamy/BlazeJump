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
		var qr = e.Results?.FirstOrDefault();
		if (qr is not null)
		{
			var payload = JsonConvert.DeserializeObject<QrConnectEventArgs>(qr.Value);
			_identityService.OnQrConnectReceived(payload);
		}
	}

	void TorchButton_Clicked(object sender, EventArgs e)
	{
		barcodeView.IsTorchOn = !barcodeView.IsTorchOn;
	}
}