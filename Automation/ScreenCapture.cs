using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Shapes;
using Rectangle = System.Drawing.Rectangle;

namespace Kazakov_KP_01._01.Automation
{
    /// <summary>
    /// Захват области экрана в Bitmap для последующего OCR.
    /// </summary>
    public static class ScreenCapture
    {
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        /// <summary>
        /// Захватывает прямоугольную область экрана по абсолютным координатам.
        /// </summary>
        public static Bitmap CaptureRegion(Rectangle region)
        {
            if (region.Width <= 0 || region.Height <= 0)
                throw new ArgumentException("Размер области захвата должен быть больше нуля.");

            var bmp = new Bitmap(region.Width, region.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(region.Left, region.Top, 0, 0, region.Size, CopyPixelOperation.SourceCopy);
            }

            return bmp;
        }

        /// <summary>
        /// Захватывает область экрана по координатам x, y, width, height.
        /// </summary>
        public static Bitmap CaptureRegion(int x, int y, int width, int height)
            => CaptureRegion(new Rectangle(x, y, width, height));

        /// <summary>
        /// Захватывает весь экран (основной монитор).
        /// </summary>
        public static Bitmap CaptureFullScreen()
        {
            int screenW = GetSystemMetrics(SM_CXSCREEN);
            int screenH = GetSystemMetrics(SM_CYSCREEN);
            return CaptureRegion(0, 0, screenW, screenH);
        }
    }
}