namespace CharacterFinder;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Diagnostics;


public static class FloodFill
{
    public static void Fill(Bitmap image, ref byte[] pixels, ref bool[,] visited, int stride, Color backgroundColor, List<Rectangle> letterRectangles)
    {
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                if (!visited[x, y] && pixels[y * stride + 4 * x] != backgroundColor.R)
                {
                    int left = x, right = x, top = y, bottom = y;
                    Flood(image, ref pixels, ref visited, stride, x, y, backgroundColor, ref left, ref right, ref top, ref bottom);

                    Rectangle letterRect = new(left, top, right - left + 1, bottom - top + 1);
                    letterRectangles.Add(letterRect);
                }
            }
        }
    }

    private static void Flood(Bitmap image, ref byte[] pixels, ref bool[,] visited, int stride, int startX, int startY, Color targetColor, ref int left, ref int right, ref int top, ref int bottom)
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