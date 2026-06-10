using MarketAO.Models;
using MySqlConnector;
using System;
using System.Collections.ObjectModel;

namespace MarketAO.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString = "Server=localhost;Database=AlbionMarket;User ID=root;Password=;";

        public ObservableCollection<MarketItem> GetItems(string cityName, string orderType)
        {
            var items = new ObservableCollection<MarketItem>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    // ДОБАВЛЕНО: достаем m.item_id
                    string sql = @"SELECT m.id, m.item_id, i.name, i.tier, m.price, m.amount 
                                   FROM market_orders m 
                                   JOIN items i ON m.item_id = i.id 
                                   WHERE m.order_type = @type AND m.city = @city";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@type", orderType);
                        command.Parameters.AddWithValue("@city", cityName);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int tier = reader.GetInt32("tier");
                                items.Add(new MarketItem
                                {
                                    Id = reader.GetInt32("id"),
                                    ItemId = reader.GetInt32("item_id"), // Сохраняем системный ID предмета
                                    ItemName = reader.GetString("name"),
                                    TierInt = tier,
                                    Tier = "T" + tier,
                                    Price = reader.GetInt64("price"),
                                    Quantity = reader.GetInt32("amount")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Error: " + ex.Message); }
            return items;
        }

        public void BuyItem(MarketItem item, int quantityToBuy, string buyerCity)
        {
            // Списываем серебро
            long totalCost = item.Price * quantityToBuy;
            BalanceService.Instance.Subtract(totalCost);

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. УБИРАЕМ КУПЛЕННОЕ С РЫНКА
                        if (quantityToBuy >= item.Quantity)
                        {
                            // Купили весь лот — удаляем строку рынка
                            string sqlDelete = "DELETE FROM market_orders WHERE id = @id";
                            using (var cmd = new MySqlCommand(sqlDelete, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@id", item.Id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Купили часть — отнимаем количество
                            string sqlUpdate = "UPDATE market_orders SET amount = amount - @bought WHERE id = @id";
                            using (var cmd = new MySqlCommand(sqlUpdate, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@bought", quantityToBuy);
                                cmd.Parameters.AddWithValue("@id", item.Id);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 2. ДОБАВЛЯЕМ В ИНВЕНТАРЬ (СТАКАЕМ)
                        // Проверяем, есть ли уже такой предмет в инвентаре в этом городе
                        string checkInvSql = "SELECT id FROM market_orders WHERE item_id = @itemId AND city = @city AND order_type = 'sell' LIMIT 1";
                        object existingInvId = null;
                        using (var cmd = new MySqlCommand(checkInvSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@itemId", item.ItemId);
                            cmd.Parameters.AddWithValue("@city", buyerCity);
                            existingInvId = cmd.ExecuteScalar();
                        }

                        if (existingInvId != null)
                        {
                            // ПРЕДМЕТ ЕСТЬ: Просто увеличиваем количество (Стакаем)
                            string updateInvSql = "UPDATE market_orders SET amount = amount + @qty WHERE id = @invId";
                            using (var cmd = new MySqlCommand(updateInvSql, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@qty", quantityToBuy);
                                cmd.Parameters.AddWithValue("@invId", Convert.ToInt32(existingInvId));
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // ПРЕДМЕТА НЕТ: Создаем новую запись в инвентаре
                            string insertInvSql = @"INSERT INTO market_orders (item_id, city, price, amount, order_type) 
                                            VALUES (@itemId, @city, @price, @qty, 'sell')";
                            using (var cmd = new MySqlCommand(insertInvSql, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@itemId", item.ItemId);
                                cmd.Parameters.AddWithValue("@city", buyerCity);
                                cmd.Parameters.AddWithValue("@price", item.Price);
                                cmd.Parameters.AddWithValue("@qty", quantityToBuy);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine("Transaction Error: " + ex.Message);
                    }
                }
            }
        }


        public void SellItem(MarketItem item, int quantity)
        {
            long profit = (long)((item.Price * quantity) * 0.90); // Сразу чистая прибыль (минус 10% налог)
            BalanceService.Instance.Add(profit);

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string sql = quantity >= item.Quantity
                    ? "DELETE FROM market_orders WHERE id = @id"
                    : "UPDATE market_orders SET amount = amount - @qty WHERE id = @id";

                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@qty", quantity);
                    cmd.Parameters.AddWithValue("@id", item.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public ObservableCollection<MarketItem> GetBuyItems(string city) => GetItems(city, "buy");
        public ObservableCollection<MarketItem> GetInventoryItems(string city) => GetItems(city, "sell");
    }
}
