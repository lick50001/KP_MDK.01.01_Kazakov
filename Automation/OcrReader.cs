using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Tesseract;

namespace Kazakov_KP_01._01.Automation
{
    public class OcrResult
    {
        public string Text { get; set; } = "";
        public float Confidence { get; set; }
        public Rectangle SourceRegion { get; set; }

        public override string ToString() => $"\"{Text}\" (conf: {Confidence:F1}%)";
    }

    public class OcrReader : IDisposable
    {
        private readonly TesseractEngine _engine;
        private bool _disposed = false;

        public OcrReader(string tessDataPath = "./tessdata", string language = "eng", EngineMode engineMode = EngineMode.Default)
        {
            if (!Directory.Exists(tessDataPath))
                throw new DirectoryNotFoundException(
                    $"Папка с данными Tesseract не найдена: '{Path.GetFullPath(tessDataPath)}'. " +
                    $"Скачай *.traineddata с https://github.com/tesseract-ocr/tessdata и положи в эту папку.");

            _engine = new TesseractEngine(tessDataPath, language, engineMode);

            SetCharWhitelist("0123456789.,");
            SetPageSegmentationMode(PageSegMode.SingleLine);
        }

        public void SetCharWhitelist(string allowedChars) => _engine.SetVariable("tessedit_char_whitelist", allowedChars);
        public void ClearCharWhitelist() => _engine.SetVariable("tessedit_char_whitelist", "");
        public void SetPageSegmentationMode(PageSegMode mode) => _engine.DefaultPageSegMode = mode;

        // ===== По абсолютным экранным координатам =====

        public OcrResult ReadRegion(Rectangle screenRegion, bool invert = false)
        {
            using (var screenshot = ScreenCapture.CaptureRegion(screenRegion))
            {
                using (var prepared = ImagePreprocessor.PrepareForOcr(screenshot, scale: 3f, threshold: 128, invert: invert))
                {
                    return ReadBitmap(prepared, screenRegion);
                }
            }
        }

        public decimal? ReadNumber(Rectangle screenRegion, bool invert = false)
        {
            var result = ReadRegion(screenRegion, invert);
            return ParseNumber(result.Text);
        }

        public OcrResult ReadRegionCustom(Rectangle screenRegion, Func<Bitmap, Bitmap> preprocess)
        {
            using (var screenshot = ScreenCapture.CaptureRegion(screenRegion))
            {
                using (var prepared = preprocess(screenshot))
                {
                    return ReadBitmap(prepared, screenRegion);
                }
            }
        }

        // ===== Относительно окна MarketAO =====

        public decimal? ReadNumberInWindow(MarketWindow market, Rectangle relativeRegion, bool invert = false)
        {
            var screenRegion = market.ToScreen(relativeRegion);
            return ReadNumber(screenRegion, invert);
        }

        public OcrResult ReadRegionInWindow(MarketWindow market, Rectangle relativeRegion, bool invert = false)
        {
            var screenRegion = market.ToScreen(relativeRegion);
            return ReadRegion(screenRegion, invert);
        }

        // ===== Внутреннее =====

        public OcrResult ReadBitmap(Bitmap bitmap, Rectangle? sourceRegion = null)
        {
            using (var pix = BitmapToPix(bitmap))
            {
                using (var page = _engine.Process(pix))
                {
                    string text = page.GetText()?.Trim() ?? "";
                    float confidence = page.GetMeanConfidence() * 100f;

                    return new OcrResult
                    {
                        Text = text,
                        Confidence = confidence,
                        SourceRegion = sourceRegion ?? new Rectangle(0, 0, bitmap.Width, bitmap.Height)
                    };
                }
            }
        }

        private decimal? ParseNumber(string rawText)
        {
            string cleaned = rawText
                .Replace(" ", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace(",", "");

            if (decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal value))
            {
                return value;
            }

            return null;
        }

        private Pix BitmapToPix(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);
                return Pix.LoadFromMemory(stream.ToArray());
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _engine?.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// Распознаёт число, автоматически пробуя оба варианта инверсии
        /// (светлый текст на тёмном / тёмный на светлом) и выбирая результат
        /// с более высокой уверенностью распознавания.
        /// </summary>
        public decimal? ReadNumberAdaptive(Rectangle screenRegion)
        {
            var normal = ReadNumberWithConfidence(screenRegion, invert: false);
            var inverted = ReadNumberWithConfidence(screenRegion, invert: true);

            var best = inverted.Confidence >= normal.Confidence ? inverted : normal;
            return best.Value;
        }

        public decimal? ReadNumberAdaptiveInWindow(MarketWindow market, Rectangle relativeRegion)
        {
            var screenRegion = market.ToScreen(relativeRegion);
            return ReadNumberAdaptive(screenRegion);
        }

        private (decimal? Value, float Confidence) ReadNumberWithConfidence(Rectangle screenRegion, bool invert)
        {
            // ИСПРАВЛЕНО: Заменили "using var" на классические вложенные блоки using под C# 7.3
            using (var screenshot = ScreenCapture.CaptureRegion(screenRegion))
            {
                using (var prepared = ImagePreprocessor.PrepareForOcrAdaptive(screenshot, scale: 4f, invert: invert))
                {
                    var result = ReadBitmap(prepared, screenRegion);
                    decimal? parsed = ParseNumberStrict(result.Text);
                    return (parsed, parsed.HasValue ? result.Confidence : 0f);
                }
            }
        }

        /// <summary>
        /// Строгий парсинг числа — отбрасывает результат, если в строке
        /// остался хоть один не-цифровой символ после очистки разделителей.
        /// </summary>
        private decimal? ParseNumberStrict(string rawText)
        {
            string cleaned = rawText
                .Replace(" ", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace(",", "")
                .Trim();

            if (string.IsNullOrEmpty(cleaned)) return null;

            foreach (char c in cleaned)
            {
                if (!char.IsDigit(c) && c != '.') return null;
            }

            if (decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal value))
            {
                return value;
            }

            return null;
        }
    }
}