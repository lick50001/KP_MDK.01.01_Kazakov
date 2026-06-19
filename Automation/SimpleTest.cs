using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;

namespace Kazakov_KP_01._01.Automation
{
    public static class SimpleTest
    {
        public static async Task RunAsync(
            string marketProcessName,
            int moveToX, int moveToY,
            Rectangle ocrRegion,
            bool invertColors = false)
        {
            MarketWindow market;

            try
            {
                market = MarketWindow.Find(marketProcessName);
                market.Activate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось найти окно '{marketProcessName}': {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await MouseController.MoveInWindowAsync(market, moveToX, moveToY, durationMs: 500);

            using (var ocr = new OcrReader(tessDataPath: "./tessdata", language: "eng"))
            {
                ocr.ClearCharWhitelist();
                var result = ocr.ReadRegionInWindow(market, ocrRegion, invertColors);

                MessageBox.Show(
                    $"Распознанный текст: \"{result.Text}\"\n" +
                    $"Уверенность: {result.Confidence:F1}%\n" +
                    $"Область (относительно окна): {ocrRegion}",
                    "Результат OCR",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }
}