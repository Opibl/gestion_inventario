using QRCoder;

namespace MiApp.Business;

public class QrService
{
    public byte[] GenerateQr(string text)
    {
        QRCodeGenerator generator = new QRCodeGenerator();
        QRCodeData data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

        PngByteQRCode qr = new PngByteQRCode(data);

        return qr.GetGraphic(20);
    }
}