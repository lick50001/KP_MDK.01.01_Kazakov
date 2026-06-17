using System;
using System.Threading;

namespace Kazakov_KP_01._01.Automation
{
    /// <summary>
    /// Контекст автоматизации — связывает вместе окно MarketAO, OCR-движок
    /// и токен отмены для одного запуска функции.
    /// Создавай новый при старте функции, Dispose при остановке.
    /// </summary>
    public class AutomationContext : IDisposable
    {
        public MarketWindow Market { get; }
        public OcrReader Ocr { get; }
        public CancellationTokenSource CancellationSource { get; }
        public CancellationToken Token => CancellationSource.Token;

        /// <summary>
        /// Создаёт контекст автоматизации, привязанный к окну MarketAO.
        /// Бросает исключение, если окно MarketAO не найдено — это нужно
        /// обработать в коде, который вызывает старт функции.
        /// </summary>
        public AutomationContext(string marketProcessName = "MarketAO", string tessDataPath = "./tessdata", string language = "eng")
        {
            Market = MarketWindow.Find(marketProcessName);
            Market.Activate();

            Ocr = new OcrReader(tessDataPath, language);
            CancellationSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Сигнал на остановку всех циклов, использующих Token.
        /// </summary>
        public void Stop() => CancellationSource.Cancel();

        public void Dispose()
        {
            CancellationSource.Cancel();
            CancellationSource.Dispose();
            Ocr.Dispose();
        }
    }
}