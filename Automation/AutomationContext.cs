using System;
using System.Threading;

namespace Kazakov_KP_01._01.Automation
{
    public class AutomationContext : IDisposable
    {
        public MarketWindow Market { get; }
        public OcrReader Ocr { get; }
        public CancellationTokenSource CancellationSource { get; }
        public CancellationToken Token => CancellationSource.Token;

        public AutomationContext(string marketProcessName = "MarketAO", string tessDataPath = "./tessdata", string language = "eng")
        {
            Market = MarketWindow.Find(marketProcessName);
            Market.Activate();

            Ocr = new OcrReader(tessDataPath, language);
            CancellationSource = new CancellationTokenSource();
        }

        public void Stop() => CancellationSource.Cancel();

        public void Dispose()
        {
            CancellationSource.Cancel();
            CancellationSource.Dispose();
            Ocr.Dispose();
        }
    }
}