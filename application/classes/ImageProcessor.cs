using System.Drawing.Imaging;
using System.Diagnostics;
using System.Drawing;

namespace CharacterFinder;

public class ImageProcessor
{
    public static void ProcessImage(Bitmap image)
    {
        // Inicie o cronômetro
        Stopwatch stopwatch = new();
        stopwatch.Start();

        // image = new("tests/Sem título.png");
        Bitmap thresholdedImage = ApplyThreshold(image);

        Color backgroundColor = BorderAnalyzer.GetBorderColor(thresholdedImage);

        List<Rectangle> letterRectangles = LetterSegmenter.SegmentLetters(thresholdedImage, backgroundColor);

        // Desenhe retângulos ao redor de cada letra segmentada
        using (Graphics g = Graphics.FromImage(image))
        {
            foreach (Rectangle rect in letterRectangles)
                g.DrawRectangle(Pens.Red, rect);
        }

        string outputImagePath = "tests/output.png";
        byte[,] arr = ImageToGrayscaleMatrix(image);
        Console.WriteLine(arr[0, 0]);

        // Save the processed image
        image.Save(outputImagePath, ImageFormat.Png);

        // Pare o cronômetro
        stopwatch.Stop();

        // Exiba o tempo decorrido
        Console.WriteLine($"Tempo decorrido: {stopwatch.ElapsedMilliseconds} ms");
    }

    public static byte[,] ImageToGrayscaleMatrix(Bitmap image)
    {
        int width = image.Width;
        int height = image.Height;

        byte[,] grayscaleMatrix = new byte[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixelColor = image.GetPixel(x, y);
                byte grayscaleValue = (byte)(0.2989 * pixelColor.R + 0.5870 * pixelColor.G + 0.1140 * pixelColor.B);
                grayscaleMatrix[x, y] = grayscaleValue;
            }
        }

        return grayscaleMatrix;
    }

    public static Bitmap ApplyThreshold(Bitmap image, int threshold = 128)
    {
        Bitmap thresholdedImage = new(image.Width, image.Height, PixelFormat.Format32bppArgb);

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Color pixelColor = image.GetPixel(x, y);
                byte pixelValue = pixelColor.R;
                byte thresholdedValue = pixelValue >= threshold ? (byte)255 : (byte)0;

                Color newColor = Color.FromArgb(thresholdedValue, thresholdedValue, thresholdedValue);
                thresholdedImage.SetPixel(x, y, newColor);
            }
        }

        return thresholdedImage;
    }
}