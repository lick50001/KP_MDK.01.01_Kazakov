using Kazakov_KP_01._01.Models;
using Kazakov_KP_01._01.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Kazakov_KP_01._01.Automation
{
    public class MarketBuyBot
    {
        private readonly AutomationContext _ctx;
        private readonly ApiService _api;

        private const int SearchFieldX = 232;
        private const int SearchFieldY = 172;

        private static readonly Rectangle PriceRegion = new Rectangle(535, 247, 642 - 535, 280 - 247);

        private const int BuyButtonX = 735;
        private const int BuyButtonY = 260;

        // Исправлена координата кнопки переключения на продажу
        private const int SellTabX = 898;
        private const int SellTabY = 206;

        private static readonly Rectangle BalanceRegion = new Rectangle(593, 24, 748 - 593, 53 - 24);

        private const int DealWindowWidth = 850;
        private const int DealWindowHeight = 450;

        private const int DealQuantityFieldX = 253;
        private const int DealQuantityFieldY = 223;

        private static readonly Rectangle DealPriceRegion = new Rectangle(235, 168, 325 - 235, 190 - 168);

        private const int DealConfirmX = 265;
        private const int DealConfirmY = 412;

        private const int DealCancelX = 103;
        private const int DealCancelY = 415;

        private const int ErrorOkButtonScreenX = 1090;
        private const int ErrorOkButtonScreenY = 600;

        public MarketBuyBot(AutomationContext ctx)
        {
            _ctx = ctx;
            _api = new ApiService();
        }

        public async Task RunFullCycleAsync(List<Items> items)
        {
            await LogAsync("info", "Запуск цикла авто-скупки");

            foreach (Items item in items)
            {
                if (_ctx.Token.IsCancellationRequested) break;

                if (!_ctx.Market.IsAlive)
                {
                    await LogAsync("error", "Окно MarketAO закрыто во время цикла, остановка");
                    break;
                }

                if (!item.IsActive) continue;

                bool stopCycle = await ProcessItemAsync(item);

                if (stopCycle)
                {
                    await LogAsync("info", "Сигнал остановки цикла покупки получен");
                    break;
                }

                await Task.Delay(1200, _ctx.Token);
            }

            // Всегда переключаемся на продажу по завершении цикла покупки
            await HandleSwitchToSellAsync();
        }

        private async Task<bool> ProcessItemAsync(Items item)
        {
            await LogAsync("info", "Ищу предмет: " + item.ItemName);
            bool searchOk = await ClearAndTypeInWindowAsync(_ctx.Market, SearchFieldX, SearchFieldY, item.ItemName);

            if (!searchOk)
            {
                await LogAsync("warning", "Не удалось ввести название предмета: " + item.ItemName);
                return false;
            }

            await Task.Delay(800, _ctx.Token);

            decimal? price = _ctx.Ocr.ReadNumberAdaptiveInWindow(_ctx.Market, PriceRegion);

            if (!price.HasValue)
            {
                await LogAsync("warning", "Не удалось распознать цену для '" + item.ItemName + "'");
                return false;
            }

            await LogAsync("info", "Цена '" + item.ItemName + "': " + price.Value.ToString("N0") + " (макс: " + item.MaxBuyPrice.ToString("N0") + ")");

            if (price.Value >= item.MaxBuyPrice)
            {
                await LogAsync("info", "Цена не подходит для '" + item.ItemName + "', пропускаю");
                return false;
            }

            // Проверяем баланс ДО открытия окна сделки
            decimal? balanceBefore = _ctx.Ocr.ReadNumberAdaptiveInWindow(_ctx.Market, BalanceRegion);

            if (!balanceBefore.HasValue || balanceBefore.Value <= 0)
            {
                await LogAsync("warning", "Не удалось распознать баланс перед покупкой '" + item.ItemName + "'");
                return false;
            }

            if (balanceBefore.Value < price.Value)
            {
                await LogAsync("warning", "Баланса (" + balanceBefore.Value.ToString("N0") + ") не хватает даже на 1 шт '" + item.ItemName + "' по цене " + price.Value.ToString("N0"));
                return true; // сигнал завершить цикл — денег нет
            }

            await LogAsync("info", "Цена выгодная, открываю окно покупки '" + item.ItemName + "'");
            await MouseController.ClickInWindowAsync(_ctx.Market, BuyButtonX, BuyButtonY);
            await Task.Delay(1000, _ctx.Token);

            MarketWindow dealWindow = null;
            int attempts = 0;

            while (dealWindow == null && attempts < 6)
            {
                try
                {
                    dealWindow = MarketWindow.FindBySize("MarketAO", DealWindowWidth, DealWindowHeight, 8);
                    await LogAsync("success", "Окно сделки найдено, хендл: " + dealWindow.Handle.ToString());
                }
                catch (Exception ex)
                {
                    attempts++;
                    await LogAsync("warning", "Попытка " + attempts + ": окно сделки не найдено (" + ex.Message + ")");
                    await Task.Delay(400, _ctx.Token);
                }
            }

            if (dealWindow == null)
            {
                await LogAsync("error", "Окно сделки так и не найдено для '" + item.ItemName + "'");
                return false;
            }

            OcrResult rawDealPriceResult = _ctx.Ocr.ReadRegionInWindow(dealWindow, DealPriceRegion, false);
            await LogAsync("info", "OCR сырой текст цены в сделке: '" + rawDealPriceResult.Text + "' (conf: " + rawDealPriceResult.Confidence.ToString("F1") + "%)");

            decimal? unitPrice = _ctx.Ocr.ReadNumberAdaptiveInWindow(dealWindow, DealPriceRegion);

            if (!unitPrice.HasValue || unitPrice.Value <= 0)
            {
                await LogAsync("warning", "Не удалось распознать цену в окне сделки для '" + item.ItemName + "'");
                return false;
            }

            await LogAsync("info", "Цена в окне сделки распознана: " + unitPrice.Value.ToString("N0"));

            decimal? balance = _ctx.Ocr.ReadNumberAdaptiveInWindow(_ctx.Market, BalanceRegion);

            if (!balance.HasValue || balance.Value <= 0)
            {
                await LogAsync("warning", "Не удалось распознать баланс аккаунта");
                return false;
            }

            int desiredQuantity = (int)Math.Floor(balance.Value / unitPrice.Value);

            if (desiredQuantity <= 0)
            {
                await LogAsync("warning", "Баланса не хватает даже на 1 единицу '" + item.ItemName + "'");

                // Закрываем окно сделки и завершаем цикл
                await MouseController.ClickInWindowAsync(dealWindow, DealCancelX, DealCancelY);
                await Task.Delay(400, _ctx.Token);
                return true;
            }

            if (desiredQuantity > 999)
            {
                await LogAsync("warning", "Подозрительно большое количество (" + desiredQuantity + "), возможна ошибка OCR цены. Пропускаю предмет.");

                await MouseController.ClickInWindowAsync(dealWindow, DealCancelX, DealCancelY);
                await Task.Delay(400, _ctx.Token);
                return false;
            }

            await LogAsync("info", "Баланс: " + balance.Value.ToString("N0") + ", цена/шт: " + unitPrice.Value.ToString("N0") + ", хочу купить: " + desiredQuantity);

            bool qtyOk = await ClearAndTypeInWindowAsync(dealWindow, DealQuantityFieldX, DealQuantityFieldY, desiredQuantity.ToString());

            if (!qtyOk)
            {
                await LogAsync("warning", "Не удалось ввести количество для '" + item.ItemName + "'");
                return false;
            }

            await Task.Delay(400, _ctx.Token);

            // Считаем сумму по желаемому количеству (не по OCR после ввода,
            // так как OCR поля количества давал неверные значения "41")
            decimal totalToPay = unitPrice.Value * desiredQuantity;

            await MouseController.ClickInWindowAsync(dealWindow, DealConfirmX, DealConfirmY);
            await Task.Delay(700, _ctx.Token);

            bool hasError = await CheckAndDismissErrorAsync();

            if (hasError)
            {
                await LogAsync("warning", "Ошибка при покупке '" + item.ItemName + "' (нет товара или баланса)");
                await Task.Delay(300, _ctx.Token);

                // Пробуем закрыть окно сделки если оно ещё открыто
                try
                {
                    MarketWindow dealWindowCheck = MarketWindow.FindBySize("MarketAO", DealWindowWidth, DealWindowHeight, 8);
                    await MouseController.ClickInWindowAsync(dealWindowCheck, DealCancelX, DealCancelY);
                    await Task.Delay(400, _ctx.Token);
                }
                catch { }

                return true; // сигнал завершить цикл
            }

            await _api.AddFinanceLogAsync("Покупка", "Куплен предмет: " + item.ItemName + " x" + desiredQuantity, -totalToPay);
            await LogAsync("success", "Куплен '" + item.ItemName + "' x" + desiredQuantity + " за " + totalToPay.ToString("N0"));

            // Синхронизируем баланс с API после покупки
            decimal? newBalance = _ctx.Ocr.ReadNumberAdaptiveInWindow(_ctx.Market, BalanceRegion);
            if (newBalance.HasValue)
            {
                await _api.SetBalanceAsync(newBalance.Value);
            }

            return false;
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

            await LogAsync("info", "Найдено окно ошибки, хендл: " + errorHandle.ToString());

            WindowLocator.RestoreAndFocus(errorHandle);
            await Task.Delay(200, _ctx.Token);

            await MouseController.ClickAtAsync(ErrorOkButtonScreenX, ErrorOkButtonScreenY);
            await Task.Delay(300, _ctx.Token);

            return true;
        }

        /// <summary>
        /// Переключается на вкладку продажи и запускает цикл продажи.
        /// Вызывается всегда по завершении цикла покупки.
        /// </summary>
        private async Task HandleSwitchToSellAsync()
        {
            await LogAsync("info", "Переключаюсь на вкладку продажи");
            await MouseController.ClickInWindowAsync(_ctx.Market, SellTabX, SellTabY);
            await Task.Delay(800, _ctx.Token);

            MarketSellBot sellBot = new MarketSellBot(_ctx);
            await sellBot.RunFullSellCycleAsync();
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