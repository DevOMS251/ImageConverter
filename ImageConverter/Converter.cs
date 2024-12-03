using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageConverter
{
    public static class Converter
    {
        public static long ConvertToIco(byte iconSize, string inputFilePath, string outputFilePath)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            using (var inputImage = new Bitmap(inputFilePath))
            using (var resizedImage = new Bitmap(inputImage, new Size(iconSize, iconSize)))
            using (var memoryStream = new MemoryStream())
            {
                // 이미지를 PNG 형식으로 메모리 스트림에 저장
                resizedImage.Save(memoryStream, ImageFormat.Png);

                // ICO 파일 생성
                memoryStream.Seek(0, SeekOrigin.Begin);
                CreateIcoFile(memoryStream, outputFilePath, iconSize);
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        private static void CreateIcoFile(Stream imageStream, string outputFilePath, byte iconSize)
        {
            using (var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fileStream))
            {
                // ICO 헤더 작성
                WriteIcoHeader(writer, iconSize, (int)imageStream.Length);

                // 이미지 데이터를 파일에 복사
                imageStream.CopyTo(fileStream);
            }
        }

        private static void WriteIcoHeader(BinaryWriter writer, byte iconSize, int imageSize)
        {
            writer.Write((short)0);  // Reserved (2 bytes)
            writer.Write((short)1);  // Image type (1 for icons)
            writer.Write((short)1);  // Number of images

            // Image entry (16 bytes)
            writer.Write(iconSize);       // Width
            writer.Write(iconSize);       // Height
            writer.Write((byte)0);        // Color palette
            writer.Write((byte)0);        // Reserved
            writer.Write((short)1);       // Color planes
            writer.Write((short)32);      // Bits per pixel
            writer.Write(imageSize);      // Image data size
            writer.Write(22);             // Data offset (header size in bytes)
        }
    }
}
