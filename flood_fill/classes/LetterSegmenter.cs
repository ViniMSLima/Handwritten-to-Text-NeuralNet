namespace CharacterFinder;

using System.Drawing.Imaging;
using System.Drawing;

public static class LetterSegmenter
{
    public static List<Rectangle> SegmentLetters(Bitmap image, Color backgroundColor)
    {
        List<Rectangle> letterRectangles = new();

        BitmapData imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
        byte[] pixels = new byte[Math.Abs(imageData.Stride) * image.Height];
        System.Runtime.InteropServices.Marshal.Copy(imageData.Scan0, pixels, 0, pixels.Length);
        image.UnlockBits(imageData);

        bool[,] visited = new bool[image.Width, image.Height];

        FloodFill.Fill(image, ref pixels, ref visited, imageData.Stride, backgroundColor, letterRectangles);

        return letterRectangles;
    }
}