using construkto3._0.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace construkto3._0.Services
{
    public static class DatabaseService
    {
        private static readonly string _connString =
            App.Configuration.GetConnectionString("OfferConstructorDb");

        public static List<Item> LoadItems()
        {
            var items = new List<Item>();

            using var conn = new SqlConnection(_connString);
            conn.Open();

            // ===== 1) Товары =====
            using (var cmd = new SqlCommand("SELECT GoodID, Description, Cost FROM dbo.Goods", conn))
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    items.Add(new Item
                    {
                        Id = rdr.GetInt32(0),
                        Name = rdr.GetString(1), // Название может быть тем же, что и Description
                        Description = rdr.GetString(1), // Используем Description как основное описание
                        UnitPrice = rdr.GetDecimal(2),
                        Source = "нет",
                        Category = "Товары"
                    });
                }
            }

            // ===== 2) Услуги =====
            using (var cmd = new SqlCommand("SELECT ServiceID, Name, Cost FROM dbo.Services", conn))
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    items.Add(new Item
                    {
                        Id = rdr.GetInt32(0),
                        Name = rdr.GetString(1), // Название услуги
                        Description = rdr.GetString(1), // Для унификации используем Name как Description
                        UnitPrice = rdr.GetDecimal(2),
                        Source = "нет",
                        Category = "Услуги"
                    });
                }
            }

            // ===== 3) Доп. товары =====
            using (var cmd = new SqlCommand("SELECT AdditionalGoodID, Description, Cost FROM dbo.AdditionalGoods", conn))
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    items.Add(new Item
                    {
                        Id = rdr.GetInt32(0),
                        Name = rdr.GetString(1), // Название может быть тем же, что и Description
                        Description = rdr.GetString(1), // Используем Description как основное описание
                        UnitPrice = rdr.GetDecimal(2),
                        Source = "нет",
                        Category = "Доп. товары"
                    });
                }
            }

            return items;
        }

        public static List<Counterparty> LoadCounterparties()
        {
            var list = new List<Counterparty>();
            using var conn = new SqlConnection(_connString);
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT CounterpartyID, Name, Address, ContactInfo, Email FROM dbo.Counterparties", conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                list.Add(new Counterparty
                {
                    Id = rdr.GetInt32(0),
                    Name = rdr.GetString(1),
                    Address = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                    Contact = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                    Email = rdr.IsDBNull(4) ? null : rdr.GetString(4)
                });
            }
            return list;
        }

        public static void AddCounterparty(Counterparty cp)
        {
            using var conn = new SqlConnection(_connString);
            conn.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO dbo.Counterparties (Name, Address, ContactInfo, Email)
                VALUES (@Name, @Address, @ContactInfo, @Email);
                SELECT CAST(SCOPE_IDENTITY() AS int);", conn);
            cmd.Parameters.AddWithValue("@Name", cp.Name);
            cmd.Parameters.AddWithValue("@Address", cp.Address ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ContactInfo", cp.Contact ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", cp.Contact ?? (object)DBNull.Value);

            cp.Id = (int)cmd.ExecuteScalar();
        }

        public static void UpdateCounterparty(Counterparty cp)
        {
            using var conn = new SqlConnection(_connString);
            conn.Open();
            using var cmd = new SqlCommand(@"
                UPDATE dbo.Counterparties
                SET Name = @Name, Address = @Address, ContactInfo = @ContactInfo, Email = @Email
                WHERE CounterpartyID = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", cp.Id);
            cmd.Parameters.AddWithValue("@Name", cp.Name);
            cmd.Parameters.AddWithValue("@Address", cp.Address ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ContactInfo", cp.Contact ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", cp.Contact ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteCounterparty(int id)
        {
            using var conn = new SqlConnection(_connString);
            conn.Open();
            using var cmd = new SqlCommand("DELETE FROM dbo.Counterparties WHERE CounterpartyID = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        public static void AddItem(Item item)
        {
            string tableName;
            string idColumn;
            string nameColumn;

            switch (item.Category)
            {
                case "Товары":
                    tableName = "Goods";
                    idColumn = "GoodID";
                    nameColumn = "Description";
                    break;
                case "Услуги":
                    tableName = "Services";
                    idColumn = "ServiceID";
                    nameColumn = "Name";
                    break;
                case "Доп. товары":
                    tableName = "AdditionalGoods";
                    idColumn = "AdditionalGoodID";
                    nameColumn = "Description";
                    break;
                default:
                    throw new ArgumentException("Неизвестная категория: " + item.Category);
            }

            using var conn = new SqlConnection(_connString);
            conn.Open();
            using var cmd = new SqlCommand($@"
                INSERT INTO dbo.{tableName} ({nameColumn}, Cost)
                VALUES (@Name, @Cost);
                SELECT CAST(SCOPE_IDENTITY() AS int);", conn);
            cmd.Parameters.AddWithValue("@Name", item.Name);
            cmd.Parameters.AddWithValue("@Cost", item.UnitPrice);

            item.Id = (int)cmd.ExecuteScalar();
        }

        public static void UpdateItem(Item item)
        {
            string tableName;
            string idColumn;
            string nameColumn;

            switch (item.Category)
            {
                case "Товары":
                    tableName = "Goods";
                    idColumn = "GoodID";
                    nameColumn = "Description";
                    break;
                case "Услуги":
                    tableName = "Services";
                    idColumn = "ServiceID";
                    nameColumn = "Name";
                    break;
                case "Доп. товары":
                    tableName = "AdditionalGoods";
                    idColumn = "AdditionalGoodID";
                    nameColumn = "Description";
                    break;
                default:
                    throw new ArgumentException("Неизвестная категория: " + item.Category);
            }

            using var conn = new SqlConnection(_connString);
            conn.Open();
            using var cmd = new SqlCommand($@"
                UPDATE dbo.{tableName}
                SET {nameColumn} = @Name, Cost = @Cost
                WHERE {idColumn} = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", item.Id);
            cmd.Parameters.AddWithValue("@Name", item.Name);
            cmd.Parameters.AddWithValue("@Cost", item.UnitPrice);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteItem(Item item)
        {
            string tableName;
            string idColumn;

            switch (item.Category)
            {
                case "Товары":
                    tableName = "Goods";
                    idColumn = "GoodID";
                    break;
                case "Услуги":
                    tableName = "Services";
                    idColumn = "ServiceID";
                    break;
                case "Доп. товары":
                    tableName = "AdditionalGoods";
                    idColumn = "AdditionalGoodID";
                    break;
                default:
                    throw new ArgumentException("Неизвестная категория: " + item.Category);
            }

            using var conn = new SqlConnection(_connString);
            conn.Open();
            using var cmd = new SqlCommand($"DELETE FROM dbo.{tableName} WHERE {idColumn} = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", item.Id);
            cmd.ExecuteNonQuery();
        }
    }
}