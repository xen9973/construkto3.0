using construkto3._0.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace construkto3._0.Services
{
    public static class DatabaseService
    {
        // Берём connection string из appsettings.json через App.Configuration
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
                        Name = rdr.GetString(1),
                        UnitPrice = rdr.GetDecimal(2),
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
                        Name = rdr.GetString(1),
                        UnitPrice = rdr.GetDecimal(2),
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
                        Name = rdr.GetString(1),
                        UnitPrice = rdr.GetDecimal(2),
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
                "SELECT CounterpartyID, Name, Address, ContactInfo FROM dbo.Counterparties", conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                list.Add(new Counterparty
                {
                    Id = rdr.GetInt32(0),
                    Name = rdr.GetString(1),
                    Address = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                    Contact = rdr.IsDBNull(3) ? null : rdr.GetString(3)
                });
            }
            return list;
        }

        public static void AddCounterparty(Counterparty cp)
        {
            using var conn = new SqlConnection(_connString);
            conn.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO dbo.Counterparties (Name, Address, ContactInfo)
                VALUES (@Name, @Address, @ContactInfo);
                SELECT CAST(SCOPE_IDENTITY() AS int);", conn);
            cmd.Parameters.AddWithValue("@Name", cp.Name);
            cmd.Parameters.AddWithValue("@Address", cp.Address ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ContactInfo", cp.Contact ?? (object)DBNull.Value);

            // Получаем созданный ID и присваиваем его объекту
            cp.Id = (int)cmd.ExecuteScalar();
        }
    }
}
