using System.Drawing;
using QRCoder;
namespace WebApplication3.Utils
{
    public class Utils
    {
        
        public static Bitmap GenerateQRCodeFromLink(string link)
        {
            
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData  = qrGenerator.CreateQrCode(link, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap image = qrCode.GetGraphic(20, Color.Brown, Color.White, false);
            return image;
        }
        
        public static string GenerateBase64QRCodeFromLink(string link)
        {
            
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData  = qrGenerator.CreateQrCode(link, QRCodeGenerator.ECCLevel.Q);
            Base64QRCode qrCode = new Base64QRCode(qrCodeData);
            string image = qrCode.GetGraphic(50);
            return image;
        }
    }
}