using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Tesseract;

namespace Kazakov_KP_01._01.Automation
{
    public class OcrResult
    {
        public string Text { get; set; }
        public float Confidence { get; set; }
        public Rectangle SourceRegion { get; set; }

        public OcrResult()
        {
            Text = "";
        }

        public override string ToString() => "\"" + Text + "\" (conf: " + Confidence.ToString("F1") + "%)";
    }

    public class OcrReader : IDisposable
    {
        private readonly TesseractEngine _engine;
        private bool _disposed = false;

        public OcrReader(string tessDataPath = "./tessdata", string language = "eng", EngineMode engineMode = EngineMode.Default)
        {
            if (!Directory.Exists(tessDataPath))
                throw new DirectoryNotFoundException(
                    "Папка с данными Tesseract не найдена: '" + Path.GetFullPath(tessDataPath) + "'. " +
                    "Скачай *.traineddata с https://github.com/tesseract-ocr/tessdata и положи в эту папку.");

            _engine = new TesseractEngine(tessDataPath, language, engineMode);

            SetCharWhitelist("0123456789");
            SetPageSegmentationMode(PageSegMode.SingleLine);
        }

        public void SetCharWhitelist(string allowedChars) => _engine.SetVariable("tessedit_char_whitelist", allowedChars);
        public void ClearCharWhitelist() => _engine.SetVariable("tessedit_char_whitelist", "");
        public void SetPageSegmentationMode(PageSegMode mode) => _engine.DefaultPageSegMode = mode;

        public OcrResult ReadRegion(Rectangle screenRegion, bool invert = false)
        {
            using (Bitmap screenshot = ScreenCapture.CaptureRegion(screenRegion))
            using (Bitmap prepared = ImagePreprocessor.PrepareForOcr(screenshot, 3f, 128, invert))
            {
                return ReadBitmap(prepared, screenRegion);
            }
        }

        public decimal? ReadNumber(Rectangle screenRegion, bool invert = false)
        {
            OcrResult result = ReadRegion(screenRegion, invert);
            return ParseNumberStrict(result.Text);
        }

        public OcrResult ReadRegionCustom(Rectangle screenRegion, Func<Bitmap, Bitmap> preprocess)
        {
            using (Bitmap screenshot = ScreenCapture.CaptureRegion(screenRegion))
            using (Bitmap prepared = preprocess(screenshot))
            {
                return ReadBitmap(prepared, screenRegion);
            }
        }

        public decimal? ReadNumberInWindow(MarketWindow market, Rectangle relativeRegion, bool invert = false)
        {
            Rectangle screenRegion = market.ToScreen(relativeRegion);
            return ReadNumber(screenRegion, invert);
        }

        public OcrResult ReadRegionInWindow(MarketWindow market, Rectangle relativeRegion, bool invert = false)
        {
            Rectangle screenRegion = market.ToScreen(relativeRegion);
            return ReadRegion(screenRegion, invert);
        }

        public decimal? ReadNumberAdaptive(Rectangle screenRegion)
        {
            var normal = ReadNumberWithConfidence(screenRegion, false);
            var inverted = ReadNumberWithConfidence(screenRegion, true);

            var best = inverted.Confidence >= normal.Confidence ? inverted : normal;
            return best.Value;
        }

        public decimal? ReadNumberAdaptiveInWindow(MarketWindow market, Rectangle relativeRegion)
        {
            Rectangle screenRegion = market.ToScreen(relativeRegion);
            return ReadNumberAdaptive(screenRegion);
        }

        private OcrNumberResult ReadNumberWithConfidence(Rectangle screenRegion, bool invert)
        {
            using (Bitmap screenshot = ScreenCapture.CaptureRegion(screenRegion))
            using (Bitmap prepared = ImagePreprocessor.PrepareForOcrAdaptive(screenshot, 6f, invert))
            {
                OcrResult result = ReadBitmap(prepared, screenRegion);
                decimal? parsed = ParseNumberStrict(result.Text);

                OcrNumberResult numResult = new OcrNumberResult();
                numResult.Value = parsed;
                numResult.Confidence = parsed.HasValue ? result.Confidence : 0f;
                return numResult;
            }
        }

        private struct OcrNumberResult
        {
            public decimal? Value;
            public float Confidence;
        }

        public OcrResult ReadBitmap(Bitmap bitmap, Rectangle? sourceRegion = null)
        {
            using (Pix pix = BitmapToPix(bitmap))
            using (Page page = _engine.Process(pix))
            {
                string text = page.GetText();
                if (text != null) text = text.Trim();
                else text = "";

                float confidence = page.GetMeanConfidence() * 100f;

                OcrResult result = new OcrResult();
                result.Text = text;
                result.Confidence = confidence;
                result.SourceRegion = sourceRegion.HasValue ? sourceRegion.Value : new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                return result;
            }
        }

        private decimal? ParseNumberStrict(string rawText)
        {
            if (string.IsNullOrEmpty(rawText)) return null;

            string cleaned = rawText
                .Replace(" ", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace(",", "")
                .Replace(".", "")
                .Trim();

            if (string.IsNullOrEmpty(cleaned)) return null;

            foreach (char c in cleaned)
            {
                if (!char.IsDigit(c)) return null;
            }

            decimal value;
            if (decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out value))
            {
                return value;
            }

            return null;
        }

        private Pix BitmapToPix(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);
                return Pix.LoadFromMemory(stream.ToArray());
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            if (_engine != null) _engine.Dispose();
            _disposed = true;
        }
    }
}