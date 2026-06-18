using Kazakov_KP_01._01.Services;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Kazakov_KP_01._01.Automation
{
    public class MarketSellBot
    {
        private readonly AutomationContext _ctx;
        private readonly ApiService _api;

        // ===== Координаты на главном окне (вкладка продажи) =====

        private const int SellCardButtonX = 750;
        private const int SellCardButtonY = 213;

        private static readonly Rectangle StockRegion = new Rectangle(550, 190, 637 - 550, 230 - 190);

        // Кнопка возврата на вкладку покупки (BUY таб в правом меню)
        private const int BuyTabX = 901;
        private const int BuyTabY = 146;

        // ===== Координаты внутри ОКНА ПРОДАЖИ =====

        private const int SellWindowWidth = 850;
        private const int SellWindowHeight = 450;

        private static readonly Rectangle SellPriceRegion = new Rectangle(222, 167, 320 - 222, 187 - 167);

        private const int SellPriceFieldX = 313;
        private const int SellPriceFieldY = 177;

        private const int SellQuantityFieldX = 275;
        private const int SellQuantityFieldY = 223;

        private static readonly Rectangle SellTotalRegion = new Rectangle(190, 307, 323 - 190, 335 - 307);

        private const int SellConfirmX = 273;
        private const int SellConfirmY = 410;

        private const int ErrorOkButtonScreenX = 1090;
        private const int ErrorOkButtonScreenY = 600;

        private const decimal MarkupMultiplier = 1.30m;

        public MarketSellBot(AutomationContext ctx)
        {
            _ctx = ctx;
            _api = new ApiService();
        }

        public async Task RunFullSellCycleAsync(int maxItemsToProcess = 50)
        {
            await LogAsync("info", "Запуск цикла авто-продажи");

            int processed = 0;
            int emptyStreak = 0;

            while (processed < maxItemsToProcess)
            {
                if (_ctx.Token.IsCancellationRequested) break;

                if (!_ctx.Market.IsAlive)
                {
                    await LogAsync("error", "Окно MarketAO закрыто во время продажи, остановка");
                    break;
                }

                // Читаем остаток предмета на текущей карточке
                decimal? stock = _ctx.Ocr.ReadNumberAdaptiveInWindow(_ctx.Market, StockRegion);

                await LogAsync("info", "Остаток предмета: " + (stock.HasValue ? stock.Value.ToString("N0") : "не распознан"));

                if (!stock.HasValue || stock.Value <= 0)
                {
                    emptyStreak++;

                    if (emptyStreak >= 3)
                    {
                        await LogAsync("success", "Инвентарь пуст (3 раза подряд пусто), завершаю продажу");
                        break;
                    }

                    await Task.Delay(800, _ctx.Token);
                    continue;
                }

                emptyStreak = 0;

                bool sold = await SellOneItemAsync();
                processed++;

                if (!sold)
                {
                    await LogAsync("warning", "Не удалось продать текущий предмет");
                }

                await Task.Delay(1000, _ctx.Token);
            }

            // Всегда возвращаемся на вкладку покупки по завершении продажи
            await LogAsync("info", "Возвращаюсь на вкладку покупки");
            await MouseController.ClickInWindowAsync(_ctx.Market, BuyTabX, BuyTabY);
            await Task.Delay(800, _ctx.Token);

            await LogAsync("success", "Цикл авто-продажи завершён");
        }

        private async Task<bool> SellOneItemAsync()
        {
            await LogAsync("info", "Открываю окно продажи предмета");
            await MouseController.ClickInWindowAsync(_ctx.Market, SellCardButtonX, SellCardButtonY);
            await Task.Delay(1000, _ctx.Token);

            MarketWindow sellWindow = null;
            int attempts = 0;

            while (sellWindow == null && attempts < 6)
            {
                try
                {
                    sellWindow = MarketWindow.FindBySize("MarketAO", SellWindowWidth, SellWindowHeight, 8);
                    await LogAsync("success", "Окно продажи найдено, хендл: " + sellWindow.Handle.ToString());
                }
                catch (Exception ex)
                {
                    attempts++;
                    await LogAsync("warning", "Попытка " + attempts + ": окно продажи не найдено (" + ex.Message + ")");
                    await Task.Delay(400, _ctx.Token);
                }
            }

            if (sellWindow == null)
            {
                await LogAsync("error", "Окно продажи так и не найдено");
                return false;
            }

            // Читаем текущую цену за единицу
            OcrResult rawPrice = _ctx.Ocr.ReadRegionInWindow(sellWindow, SellPriceRegion, false);
            await LogAsync("info", "OCR сырой текст цены продажи: '" + rawPrice.Text + "' (conf: " + rawPrice.Confidence.ToString("F1") + "%)");

            decimal? currentPrice = _ctx.Ocr.ReadNumberAdaptiveInWindow(sellWindow, SellPriceRegion);

            if (!currentPrice.HasValue || currentPrice.Value <= 0)
            {
                await LogAsync("warning", "Не удалось распознать текущую цену в окне продажи");
                return false;
            }

            decimal newPrice = Math.Floor(currentPrice.Value * MarkupMultiplier);

            await LogAsync("info", "Цена: " + currentPrice.Value.ToString("N0") + " → +30% → " + newPrice.ToString("N0"));

            // Вводим новую цену
            bool priceOk = await ClearAndTypeInWindowAsync(sellWindow, SellPriceFieldX, SellPriceFieldY, ((long)newPrice).ToString());

            if (!priceOk)
            {
                await LogAsync("warning", "Не удалось ввести новую цену");
                return false;
            }

            await Task.Delay(300, _ctx.Token);

            // Вводим максимальное количество — поле само обрежет до остатка
            bool qtyOk = await ClearAndTypeInWindowAsync(sellWindow, SellQuantityFieldX, SellQuantityFieldY, "9999");

            if (!qtyOk)
            {
                await LogAsync("warning", "Не удалось ввести количество для продажи");
                return false;
            }

            await Task.Delay(400, _ctx.Token);

            // Читаем итоговую сумму продажи для финансового лога
            decimal? totalSale = _ctx.Ocr.ReadNumberAdaptiveInWindow(sellWindow, SellTotalRegion);
            await LogAsync("info", "Итоговая сумма продажи: " + (totalSale.HasValue ? totalSale.Value.ToString("N0") : "не распознана"));

            await MouseController.ClickInWindowAsync(sellWindow, SellConfirmX, SellConfirmY);
            await Task.Delay(700, _ctx.Token);

            bool hasError = await CheckAndDismissErrorAsync();

            if (hasError)
            {
                await LogAsync("warning", "Ошибка при продаже предмета");
                return false;
            }

            decimal finalAmount = totalSale.HasValue ? totalSale.Value : newPrice;

            await _api.AddFinanceLogAsync("Продажа", "Продан предмет по цене " + newPrice.ToString("N0") + "/шт", finalAmount);
            await LogAsync("success", "Предмет продан, итого: " + finalAmount.ToString("N0"));

            return true;
        }

        private async Task<bool> CheckAndDismissErrorAsync()
        {
            IntPtr errorHandle = IntPtr.Zero;

            try
            {
                errorHandle = WindowLocator.FindByTitleEnum("Внимание");
                if (errorHandle == IntPtr.Zero)
                    errorHandle = WindowLocator.FindByTitleEnum("Ошибка");
            }
            catch (Exception)
            {
                errorHandle = IntPtr.Zero;
            }

            if (errorHandle == IntPtr.Zero)
                return false;

            await LogAsync("info", "Найдено окно ошибки при продаже, хендл: " + errorHandle.ToString());

            WindowLocator.RestoreAndFocus(errorHandle);
            await Task.Delay(200, _ctx.Token);

            await MouseController.ClickAtAsync(ErrorOkButtonScreenX, ErrorOkButtonScreenY);
            await Task.Delay(300, _ctx.Token);

            return true;
        }

        private async Task<bool> ClearAndTypeInWindowAsync(MarketWindow window, int relativeX, int relativeY, string text)
        {
            try
            {
                window.Activate();
                await Task.Delay(200, _ctx.Token);

                Point screenPoint = window.ToScreen(relativeX, relativeY);

                await MouseController.ClickAtAsync(screenPoint.X, screenPoint.Y);
                await Task.Delay(100, _ctx.Token);
                await MouseController.ClickAtAsync(screenPoint.X, screenPoint.Y);
                await Task.Delay(150, _ctx.Token);

                IntPtr controlHandle = KeyboardController.GetControlAtScreenPoint(screenPoint.X, screenPoint.Y);
                if (controlHandle == IntPtr.Zero)
                    controlHandle = window.Handle;

                KeyboardController.SelectAllAndDeleteDirect(controlHandle);
                await Task.Delay(150, _ctx.Token);

                KeyboardController.BackspaceDirect(controlHandle, 10);
                await Task.Delay(150, _ctx.Token);

                await KeyboardController.TypeTextDirectAsync(controlHandle, text, 60);
                await Task.Delay(200, _ctx.Token);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task LogAsync(string type, string message)
        {
            await _api.AddLogAsync(type, message);
        }
    }
}