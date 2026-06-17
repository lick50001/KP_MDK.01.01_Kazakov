using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Kazakov_KP_01._01.Automation
{
    /// <summary>
    /// Предобработка изображения для повышения точности OCR.
    /// </summary>
    public static class ImagePreprocessor
    {
        /// <summary>
        /// Увеличивает изображение в scale раз. Tesseract значительно лучше
        /// распознаёт текст высотой от ~30px, поэтому мелкий UI-текст стоит увеличивать.
        /// </summary>
        public static Bitmap Scale(Bitmap source, float scale)
        {
            int newW = (int)(source.Width * scale);
            int newH = (int)(source.Height * scale);

            var result = new Bitmap(newW, newH, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(source, 0, 0, newW, newH);
            }
            return result;
        }

        /// <summary>
        /// Конвертирует в grayscale. Помогает Tesseract игнорировать цветовой шум фона.
        /// </summary>
        public static Bitmap ToGrayscale(Bitmap source)
        {
            var result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(result))
            {
                var colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] {0.299f, 0.299f, 0.299f, 0, 0},
                    new float[] {0.587f, 0.587f, 0.587f, 0, 0},
                    new float[] {0.114f, 0.114f, 0.114f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

                using (var attrs = new ImageAttributes())
                {
                    attrs.SetColorMatrix(colorMatrix);
                    g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
                        0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attrs);
                }
            }
            return result;
        }

        /// <summary>
        /// Бинаризация по порогу — превращает изображение в чёрно-белое.
        /// Отлично работает для текста с контрастным фоном.
        /// </summary>
        /// <param name="threshold">0-255, порог яркости</param>
        /// <param name="invert">true — если текст светлый на тёмном фоне</param>
        public static Bitmap Binarize(Bitmap source, byte threshold = 128, bool invert = false)
        {
            var gray = ToGrayscale(source);
            var result = new Bitmap(gray.Width, gray.Height, PixelFormat.Format32bppArgb);

            for (int y = 0; y < gray.Height; y++)
            {
                for (int x = 0; x < gray.Width; x++)
                {
                    var px = gray.GetPixel(x, y);
                    bool isLight = px.R >= threshold;

                    if (invert) isLight = !isLight;

                    var color = isLight ? Color.White : Color.Black;
                    result.SetPixel(x, y, color);
                }
            }

            gray.Dispose();
            return result;
        }

        /// <summary>
        /// Готовый пайплайн: масштабирование + grayscale + бинаризация.
        /// </summary>
        public static Bitmap PrepareForOcr(Bitmap source, float scale = 3f, byte threshold = 128, bool invert = false)
        {
            var scaled = Scale(source, scale);
            var binarized = Binarize(scaled, threshold, invert);
            scaled.Dispose();
            return binarized;
        }
    }
}