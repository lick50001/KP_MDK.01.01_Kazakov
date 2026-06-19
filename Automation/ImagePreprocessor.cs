using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Kazakov_KP_01._01.Automation
{

    public static class ImagePreprocessor
    {
        public static Bitmap Scale(Bitmap source, float scale)
        {
            int newW = (int)(source.Width * scale);
            int newH = (int)(source.Height * scale);

            Bitmap result = new Bitmap(newW, newH, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(source, 0, 0, newW, newH);
            }
            return result;
        }

        public static Bitmap ToGrayscale(Bitmap source)
        {
            Bitmap result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(result))
            {
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] {0.299f, 0.299f, 0.299f, 0, 0},
                    new float[] {0.587f, 0.587f, 0.587f, 0, 0},
                    new float[] {0.114f, 0.114f, 0.114f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

                using (ImageAttributes attrs = new ImageAttributes())
                {
                    attrs.SetColorMatrix(colorMatrix);
                    g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
                        0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attrs);
                }
            }
            return result;
        }

        public static Bitmap Binarize(Bitmap source, byte threshold = 128, bool invert = false)
        {
            Bitmap gray = ToGrayscale(source);
            Bitmap result = new Bitmap(gray.Width, gray.Height, PixelFormat.Format32bppArgb);

            for (int y = 0; y < gray.Height; y++)
            {
                for (int x = 0; x < gray.Width; x++)
                {
                    Color px = gray.GetPixel(x, y);
                    bool isLight = px.R >= threshold;

                    if (invert) isLight = !isLight;

                    Color color = isLight ? Color.White : Color.Black;
                    result.SetPixel(x, y, color);
                }
            }

            gray.Dispose();
            return result;
        }

        public static Bitmap PrepareForOcr(Bitmap source, float scale = 3f, byte threshold = 128, bool invert = false)
        {
            Bitmap scaled = Scale(source, scale);
            Bitmap binarized = Binarize(scaled, threshold, invert);
            scaled.Dispose();
            return binarized;
        }

        public static Bitmap BinarizeOtsu(Bitmap source, bool invert = false)
        {
            Bitmap gray = ToGrayscale(source);

            int[] histogram = new int[256];
            int totalPixels = gray.Width * gray.Height;

            for (int y = 0; y < gray.Height; y++)
            {
                for (int x = 0; x < gray.Width; x++)
                {
                    histogram[gray.GetPixel(x, y).R]++;
                }
            }

            double sumAll = 0;
            for (int i = 0; i < 256; i++) sumAll += i * histogram[i];

            double sumBackground = 0;
            int countBackground = 0;
            double maxVariance = 0;
            int bestThreshold = 128;

            for (int t = 0; t < 256; t++)
            {
                countBackground += histogram[t];
                if (countBackground == 0) continue;

                int countForeground = totalPixels - countBackground;
                if (countForeground == 0) break;

                sumBackground += t * histogram[t];

                double meanBackground = sumBackground / countBackground;
                double meanForeground = (sumAll - sumBackground) / countForeground;

                double variance = (double)countBackground * countForeground *
                    (meanBackground - meanForeground) * (meanBackground - meanForeground);

                if (variance > maxVariance)
                {
                    maxVariance = variance;
                    bestThreshold = t;
                }
            }

            Bitmap result = Binarize(gray, (byte)bestThreshold, invert);
            gray.Dispose();
            return result;
        }

        public static Bitmap PrepareForOcrAdaptive(Bitmap source, float scale = 3f, bool invert = false)
        {
            Bitmap scaled = Scale(source, scale);
            Bitmap binarized = BinarizeOtsu(scaled, invert);
            scaled.Dispose();
            return binarized;
        }
    }
}