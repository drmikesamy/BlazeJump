using QRCoder;

namespace BlazeJump.Common.Helpers
{
	public static class QRCode
	{
		public static string? GenerateQRCode(string inputData)
		{
			QRCodeGenerator qrGenerator = new QRCodeGenerator();
			QRCodeData qrCodeData = qrGenerator.CreateQrCode(inputData, QRCodeGenerator.ECCLevel.Q);
			BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
			byte[] qrCodeAsBitmapByteArr = qrCode.GetGraphic(20);
			return Convert.ToBase64String(qrCodeAsBitmapByteArr);
		}
	}
}
