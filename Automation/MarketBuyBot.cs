using Kazakov_KP_01._01.Models;
using Kazakov_KP_01._01.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Kazakov_KP_01._01.Automation
{
    /// <summary>
    /// Бот авто-скупки предметов на MarketAO.
    /// Алгоритм на один предмет:
    /// 1) кликает в поле поиска и вводит название предмета,
    /// 2) распознаёт цену OCR в заданной области,
    /// 3) если цена меньше MaxBuyPrice — кликает по кнопке покупки,
    /// 4) логирует каждый шаг: перемещения/OCR — в Logs (Главная),
    ///    покупку/продажу — в Finance (Финансы).
    /// </summary>
    public class MarketBuyBot
    {
        private readonly AutomationContext _ctx;
        private readonly ApiService _api;

        // Поле ввода названия предмета (относительно окна MarketAO)
        private const int SearchFieldX = 232;
        private const int SearchFieldY = 172;

        public const int SearchFieldXPublic = SearchFieldX;
        public const int SearchFieldYPublic = SearchFieldY;

        // Область, где отображается цена предмета после поиска (относительно окна MarketAO)
        private static readonly Rectangle PriceRegion = new Rectangle(535, 247, 642 - 535, 280 - 247);

        // Точка кнопки "Купить" (относительно окна MarketAO)
        private const int BuyButtonX = 735;
        private const int BuyButtonY = 260;

        public MarketBuyBot(AutomationContext ctx)
        {
            _ctx = ctx;
            _api = new ApiService();
        }

        /// <summary>
        /// Обрабатывает один предмет: вводит название в поле поиска, читает цену,
        /// покупает если цена выгодная. Возвращает true, если покупка состоялась.
        /// </summary>
        public async Task<bool> ProcessItemAsync(Items item)
        {
            if (!_ctx.Market.IsAlive)
            {
                await LogAsync("error", "Окно MarketAO закрыто, обработка остановлена");
                return false;
            }

            // Шаг 1 — кликаем в поле поиска и вводим название предмета
            await LogAsync("info", $"Ищу предмет: {item.ItemName}");
            await MouseController.ClickInWindowAsync(_ctx.Market, SearchFieldX, SearchFieldY);
            await Task.Delay(150, _ctx.Token);
            await ClearAndTypeAsync(item.ItemName);

            // Пауза, чтобы маркет успел отрисовать результат поиска
            await Task.Delay(800, _ctx.Token);

            // Шаг 2 — распознаём цену через OCR
            decimal? price = _ctx.Ocr.ReadNumberAdaptiveInWindow(_ctx.Market, PriceRegion);

            if (!price.HasValue)
            {
                await LogAsync("warning", $"Не удалось распознать цену для '{item.ItemName}'");
                return false;
            }

            await LogAsync("info", $"Цена '{item.ItemName}': {price.Value:N0} (макс: {item.MaxBuyPrice:N0})");

            // Шаг 3 — сравниваем с MaxBuyPrice
            if (price.Value >= item.MaxBuyPrice)
            {
                await LogAsync("info", $"Цена не подходит для '{item.ItemName}', пропускаю");
                return false;
            }

            // Шаг 4 — цена выгодная, кликаем "Купить"
            await LogAsync("info", $"Цена выгодная, покупаю '{item.ItemName}'");
            await MouseController.ClickInWindowAsync(_ctx.Market, BuyButtonX, BuyButtonY);

            // Покупка идёт в финансовые логи как расход (отрицательная сумма)
            await _api.AddFinanceLogAsync("Покупка", $"Куплен предмет: {item.ItemName}", -price.Value);

            await LogAsync("success", $"Куплен '{item.ItemName}' за {price.Value:N0}");

            return true;
        }

        /// <summary>
        /// Проходит по всем активным предметам из списка и пытается купить каждый,
        /// если цена окажется выгодной.
        /// </summary>
        public async Task RunFullCycleAsync(List<Items> items)
        {
            foreach (var item in items)
            {
                if (_ctx.Token.IsCancellationRequested) break;
                if (!_ctx.Market.IsAlive)
                {
                    await LogAsync("error", "Окно MarketAO закрыто во время цикла, остановка");
                    break;
                }

                if (!item.IsActive) continue;

                await ProcessItemAsync(item);

                await Task.Delay(1200, _ctx.Token);
            }

            await LogAsync("success", "Цикл авто-скупки завершён");
        }

        /// <summary>
        /// Очищает поле ввода (Ctrl+A, Delete) и вводит текст посимвольно.
        /// </summary>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // В ClearAndTypeAsync, для теста:
        private async Task ClearAndTypeAsync(string text)
        {
            _ctx.Market.Activate();
            await Task.Delay(200, _ctx.Token);

            var screenPoint = _ctx.Market.ToScreen(SearchFieldX, SearchFieldY);

            // Кликаем мышью, чтобы поставить курсор/выделение в поле
            await MouseController.ClickAtAsync(screenPoint.X, screenPoint.Y);
            await Task.Delay(200, _ctx.Token);

            // Находим хендл конкретного контрола под курсором (не всего окна)
            IntPtr controlHandle = KeyboardController.GetControlAtScreenPoint(screenPoint.X, screenPoint.Y);

            if (controlHandle == IntPtr.Zero)
                controlHandle = _ctx.Market.Handle; // fallback на хендл всего окна

            // Очищаем поле прямым вводом в найденный контрол
            KeyboardController.SelectAllAndDeleteDirect(controlHandle);
            await Task.Delay(150, _ctx.Token);

            // Печатаем напрямую в контрол через WM_CHAR — обходит проблему с фокусом
            await KeyboardController.TypeTextDirectAsync(controlHandle, text, delayBetweenCharsMs: 50);
        }

        private async Task LogAsync(string type, string message)
        {
            await _api.AddLogAsync(type, message);
        }
    }
}