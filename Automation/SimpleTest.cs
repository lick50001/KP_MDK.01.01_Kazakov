using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;

namespace Kazakov_KP_01._01.Automation
{
    /// <summary>
    /// Простой тестовый сценарий: один раз перемещает мышку в точку
    /// относительно окна MarketAO и распознаёт текст в заданной области.
    /// Используй для проверки, что OCR и движение мыши вообще работают,
    /// прежде чем писать полноценную логику автоматизации.
    /// </summary>
    public static class SimpleTest
    {
        /// <summary>
        /// Перемещает курсор в указанную точку (относительно окна MarketAO)
        /// и распознаёт текст в указанной области (тоже относительно окна).
        /// Показывает результат в MessageBox.
        /// </summary>
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

            // Двигаем мышь в указанную точку относительно окна
            await MouseController.MoveInWindowAsync(market, moveToX, moveToY, durationMs: 500);

            // Распознаём текст в области относительно того же окна
            using (var ocr = new OcrReader(tessDataPath: "./tessdata", language: "eng"))
            {
                ocr.ClearCharWhitelist(); // снимаем ограничение на цифры для теста
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