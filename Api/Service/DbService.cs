using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using Api.Models;

namespace Api.Service
{
    public class DbService
    {
        private readonly IConfiguration _configuration;
        private readonly IDbConnection _dbConnection;
        private readonly string _connectionString;
        public DbService(IConfiguration configuration, IDbConnection dbConnection)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("Zintegrujemy");
        }
        public void GetInventory(string csvFilePath)
        {
            using var connection = new SqliteConnection(_connectionString);
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                IgnoreBlankLines = true, // Ignoruj puste linie
                HeaderValidated = null, // Wyłącz walidację nagłówka
                HasHeaderRecord = false // Brak nagłówków
            }))
            {
                csv.Context.RegisterClassMap<PriceMap>();
                //pomijamy pierwszą linijkę
                csv.Read();

                var allInventory = csv.GetRecords<Inventory>()
                 .Where(inv => inv.Shipping=="24h")
                 .ToList();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string query = @"
                    INSERT INTO Inventory (ProductID, SKU, Unit, Quantity, ShippingCost)
                    VALUES (@ProductID, @SKU, @Unit, @Quantity, @ShippingCost)";

                        connection.Execute(query, allInventory, transaction: transaction);

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
     

        public void GetPrice(String csvFilePath)
        {
            using var connection = new SqliteConnection(_connectionString);
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }           
            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.Context.RegisterClassMap<PriceMap>();
                var prices = csv.GetRecords<Price>().ToList();

                BulkInsertPrices(prices);
            }
        }

        // Tak Moim staniem powinno wyglądać GetProducts, niestety przez małą ilość czasu wolałem to zrobić w prostym kodem,
        // nie jest on najoptymalniejszą wersją jednakże sprawną a nie mogę poświęcić więcej czasu na debbugowanie w celu znalezienia błędu
        // Problem tworzy tu wers 52574 :"__empty_line__ " :D 
        //   public void GetProducts(string csvFilePath)
        //   {
        //       var connectionString = _configuration.GetConnectionString("Zintegrujemy");
        //       using var connection = new SqliteConnection(connectionString);
        //       if (connection.State == ConnectionState.Closed)
        //       {
        //           connection.Open();
        //       }
        //       using (var reader = new StreamReader(csvFilePath))
        //       using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        //       {
        //           Delimiter = ";"
        //       }))
        //       {
        //           csv.Context.RegisterClassMap<ProductsMap>();
        //           var allProducts = csv.GetRecords<Products>()
        //                  .Where(p => p.ID != 0 && !p.ID.Equals("'__empty_line__'") && int.TryParse(p.ID.ToString(), out _))
        //               .ToList();
        //           var productsToProcess = allProducts.Where(product => product.IsWire == true).ToList();
        //           using (var transaction = connection.BeginTransaction())
        //           {
        //               try
        //               {
        //                   string query = @"
        //           INSERT INTO Products (ID, SKU, name, EAN, producer_name, category, is_wire, available, is_vendor, default_image)
        //           VALUES (@ID, @SKU, @name, @EAN, @producer_name, @category, @is_wire, @available, @is_vendor, @default_image)";
        //                   var gridReader = connection.QueryMultiple(query, productsToProcess, transaction: transaction);
        //                   transaction.Commit();
        //               }
        //               catch (Exception)
        //               {transaction.Rollback();
        //                   throw;}}}}
        public void GetProducts(string csvFilePath)
        {
            using (StreamReader reader = new StreamReader(csvFilePath))
            {
                reader.ReadLine(); // Przeczytaj linię z headerem i zignoruj
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (CheckProductValidation(line))
                    {                   
                        // Zapisz do bazy danych SQLite używając Dappera.
         
                        using var connection = new SqliteConnection(_connectionString);
                        if (connection.State == ConnectionState.Closed)
                        {
                            connection.Open();
                        }
                        var rowParts = line.Split(';');

                        // Utwórz anonimowy obiekt, który zawiera tylko te właściwości, które chcesz zapisać do bazy danych.
                        var parameters = new Products
                        {
                            ID = int.Parse(rowParts[0].Trim('\"')),
                            SKU = rowParts[1].Trim('\"'),
                            name = rowParts[2].Trim('\"'),
                            EAN = rowParts[4],
                            ProducerName = rowParts[6].Trim('\"'),
                            Category = rowParts[7].Trim('\"'),
                            IsWire = bool.TryParse(rowParts[8].Trim('\"'), out bool isWire) ? isWire : false,
                            Available = bool.TryParse(rowParts[11].Trim('\"'), out bool available) ? available : false,
                            DefaultImage = rowParts[14],
                        };
                        var existingProduct = connection.QueryFirstOrDefault<Products>("SELECT * FROM Products WHERE ID = @ID", new { ID = parameters.ID });
                        if (existingProduct == null)
                            connection.Execute("INSERT INTO Products (ID,SKU, Name, EAN, ProducerName, Category, IsWire, Available, IsVendor, DefaultImage) VALUES (@ID,@SKU, @name, @EAN, @ProducerName, @Category, @IsWire, @Available, @IsVendor, @DefaultImage)", parameters);
                    }
                }
            }
        }
        private bool CheckProductValidation(string line)
        {
            var rowParts = line.Split(';');
            // Sprawdź czy 'is_wire' (indeks 6) to 1
            return rowParts.Length > 9 && rowParts[8].Trim() == "\"1\"" && rowParts[9] == "\"24h\"";
        }        

        public void CreateSQLiteDatabase()
        {
            Console.WriteLine("CreateSQLiteDatabase method is called.");

            var connectionString = _configuration.GetConnectionString("Zintegrujemy");
            using var connection = new SqliteConnection(connectionString);
            connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Products (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                SKU TEXT,
                Name TEXT,
                EAN TEXT,
                ProducerName TEXT,
                Category TEXT,
                IsWire INTEGER,
                Available INTEGER,
                IsVendor INTEGER,
                DefaultImage TEXT
            );

            CREATE TABLE IF NOT EXISTS Inventory (
                ProductID INTEGER,
                SKU TEXT,
                Unit TEXT,
                Quantity TEXT,
                ShippingCost TEXT
            );

            CREATE TABLE IF NOT EXISTS Prices (
                InternalID text PRIMARY KEY,
                SKU TEXT,
                NetPrice DECIMAL,
                NetPriceAfterDiscount DECIMAL,
                VATRate DECIMAL,
                NetPriceAfterDiscountForLogisticUnit DECIMAL
            );
        ");
        }

        //Prosta metoda do pobierania plików, url - link do pobrania - localPath ,ścieżka do zapisu
        public void Downloadfile(string url, string localPath)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, localPath);
            }
        }
        public void BulkInsertPrices(List<Price> prices)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        connection.Execute("INSERT INTO Prices (InternalID, SKU, NetPrice, NetPriceAfterDiscount, VATRate, NetPriceAfterDiscountForLogisticUnit) VALUES (@InternalID, @SKU, @NetPrice, @NetPriceAfterDiscount, @VATRate, @NetPriceAfterDiscountForLogisticUnit)", prices, transaction);

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

       //Metoda wykorzystywana w 2 endpoincie 
        public ReturnerdInfo GetProductInfo(string sku)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var productInfo = connection.QueryFirstOrDefault<ReturnerdInfo>(
        @"SELECT 
        Products.Name, 
        Products.EAN, 
        Products.ProducerName, 
        Products.Category, 
        Products.DefaultImage, 
        Inventory.Quantity, 
        Inventory.Unit, 
        Prices.NetPrice, 
        Prices.NetPriceAfterDiscount, 
        Prices.VATRate, 
        Prices.NetPriceAfterDiscountForLogisticUnit AS ShippingCost
      FROM 
        Products
      INNER JOIN 
        Inventory ON Products.SKU = Inventory.SKU
      INNER JOIN 
        Prices ON Products.SKU = Prices.SKU
      WHERE 
        Products.SKU = @SKU", new { SKU = sku });
                return productInfo;
            }
        }
    }
}