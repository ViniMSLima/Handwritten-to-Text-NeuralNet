using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        // Inicie o cronômetro
        Stopwatch stopwatch = new();
        stopwatch.Start();

        string imagePath = "inqui.png";
        Bitmap image = new(imagePath);
        Bitmap thresholdedImage = ApplyThreshold(image);

        Color backgroundColor = GetBorderColor(thresholdedImage);

        List<Rectangle> letterRectangles = SegmentLetters(thresholdedImage, backgroundColor);

        // Desenhe retângulos ao redor de cada letra segmentada
        using (Graphics g = Graphics.FromImage(image))
        {
            foreach (Rectangle rect in letterRectangles)
                g.DrawRectangle(Pens.Red, rect);
        }

        string outputImagePath = "inqui2.png";
        image.Save(outputImagePath, ImageFormat.Png);

        // Pare o cronômetro
        stopwatch.Stop();

        // Exiba o tempo decorrido
        Console.WriteLine($"Tempo decorrido: {stopwatch.ElapsedMilliseconds} ms");
    }

    static Bitmap ApplyThreshold(Bitmap image, int threshold = 128)
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


    static Color GetBorderColor(Bitmap image)
    {
        List<Color> borderColors = new();

        for (int y = 0; y < image.Height; y++)
        {
            borderColors.Add(image.GetPixel(0, y));
            borderColors.Add(image.GetPixel(image.Width - 1, y));
        }

        for (int x = 0; x < image.Width; x++)
        {
            borderColors.Add(image.GetPixel(x, 0));
            borderColors.Add(image.GetPixel(x, image.Height - 1));
        }

        //cor dominante da borda
        Color backgroundColor = GetMostCommonColor(borderColors);

        return backgroundColor;
    }

    static Color GetMostCommonColor(List<Color> colors)
    {
        Dictionary<Color, int> colorCounts = new();

        foreach (Color color in colors)
        {
            if (colorCounts.ContainsKey(color))
                colorCounts[color]++;
            else
                colorCounts[color] = 1;
        }

        return colorCounts.OrderByDescending(kv => kv.Value).First().Key;
    }

    static List<Rectangle> SegmentLetters(Bitmap image, Color backgroundColor)
    {
        List<Rectangle> letterRectangles = new();

        BitmapData imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
        byte[] pixels = new byte[Math.Abs(imageData.Stride) * image.Height];
        System.Runtime.InteropServices.Marshal.Copy(imageData.Scan0, pixels, 0, pixels.Length);
        image.UnlockBits(imageData);

        bool[,] visited = new bool[image.Width, image.Height];

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                if (!visited[x, y] && pixels[y * imageData.Stride + 4 * x] != backgroundColor.R)
                {
                    int left = x, right = x, top = y, bottom = y;
                    FloodFill(image, ref pixels, ref visited, imageData.Stride, x, y, backgroundColor, ref left, ref right, ref top, ref bottom);

                    Rectangle letterRect = new(left, top, right - left + 1, bottom - top + 1);
                    letterRectangles.Add(letterRect);
                }
            }
        }

        return letterRectangles;
    }

    static void FloodFill(Bitmap image, ref byte[] pixels, ref bool[,] visited, int stride, int startX, int startY, Color targetColor, ref int left, ref int right, ref int top, ref int bottom)
    {
        Stack<Point> stack = new();
        stack.Push(new Point(startX, startY));

        while (stack.Count > 0)
        {
            Point point = stack.Pop();
            int x = point.X;
            int y = point.Y;

            if (x < 0 || x >= image.Width || y < 0 || y >= image.Height || visited[x, y] || pixels[y * stride + 4 * x] == targetColor.R)
                continue;

            pixels[y * stride + 4 * x] = targetColor.R;
            visited[x, y] = true;

            if (x < left) left = x;
            if (x > right) right = x;
            if (y < top) top = y;
            if (y > bottom) bottom = y;

            stack.Push(new Point(x + 1, y));
            stack.Push(new Point(x - 1, y));
            stack.Push(new Point(x, y + 1));
            stack.Push(new Point(x, y - 1));
        }
    }
}
