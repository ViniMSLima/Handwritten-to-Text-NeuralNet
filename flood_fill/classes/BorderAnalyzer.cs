namespace CharacterFinder;

using System.Drawing;

public static class BorderAnalyzer
{
    public static Color GetBorderColor(Bitmap image)
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

    public static Color GetMostCommonColor(List<Color> colors)
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
}
