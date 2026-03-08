using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using DotNetEnv;
using MiApp.Domain;

namespace MiApp.Data
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            Env.Load();

            string? host = Environment.GetEnvironmentVariable("DB_HOST");
            string? port = Environment.GetEnvironmentVariable("DB_PORT");
            string? database = Environment.GetEnvironmentVariable("DB_NAME");
            string? user = Environment.GetEnvironmentVariable("DB_USER");
            string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(port) ||
                string.IsNullOrWhiteSpace(database) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Faltan variables en el archivo .env");
            }

            _connectionString =
                $"Host={host};" +
                $"Port={port};" +
                $"Database={database};" +
                $"Username={user};" +
                $"Password={password};" +
                $"SSL Mode=Require;" +
                $"Trust Server Certificate=true;";
        }

        // ========================= GET PRODUCTS =========================

        public async Task<List<Product>> GetProductsAsync(string? search = null)
        {
            var products = new List<Product>();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT * FROM products WHERE is_active = TRUE";

            if (!string.IsNullOrWhiteSpace(search))
                query += " AND name ILIKE @search";

            query += " ORDER BY id DESC";

            await using var cmd = new NpgsqlCommand(query, conn);

            if (!string.IsNullOrWhiteSpace(search))
                cmd.Parameters.AddWithValue("search", $"%{search}%");

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Barcode = reader["barcode"] as string,
                    Price = Convert.ToInt32(reader.GetDecimal(reader.GetOrdinal("price"))),
                    EcoScore = reader.IsDBNull(reader.GetOrdinal("eco_score"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("eco_score")),
                    SocialScore = reader.IsDBNull(reader.GetOrdinal("social_score"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("social_score")),
                    Category = reader["category"] as string,
                    ImageUrl = reader["image_url"] as string,
                    StockActual = reader.GetInt32(reader.GetOrdinal("stock_actual")),
                    StockMinimo = reader.GetInt32(reader.GetOrdinal("stock_minimo")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("updated_at")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("is_active"))
                });
            }

            return products;
        }

        // ========================= ADD PRODUCT =========================

        public async Task AddProductAsync(Product p)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"INSERT INTO products 
                (name, normalized_name, barcode, price, eco_score, social_score, 
                 category, image_url, stock_actual, stock_minimo, updated_at, is_active)
                VALUES 
                (@name, @norm, @barcode, @price, @eco, @social,
                 @cat, @img, @stockActual, @stockMinimo, @updatedAt, @isActive)";

            await using var cmd = new NpgsqlCommand(query, conn);

            cmd.Parameters.AddWithValue("name", p.Name);
            cmd.Parameters.AddWithValue("norm", p.NormalizedName);
            cmd.Parameters.AddWithValue("barcode", (object?)p.Barcode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("price", p.Price);
            cmd.Parameters.AddWithValue("eco", (object?)p.EcoScore ?? DBNull.Value);
            cmd.Parameters.AddWithValue("social", (object?)p.SocialScore ?? DBNull.Value);
            cmd.Parameters.AddWithValue("cat", (object?)p.Category ?? DBNull.Value);
            cmd.Parameters.AddWithValue("img", (object?)p.ImageUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("stockActual", p.StockActual);
            cmd.Parameters.AddWithValue("stockMinimo", p.StockMinimo);
            cmd.Parameters.AddWithValue("updatedAt", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("isActive", true);

            await cmd.ExecuteNonQueryAsync();
        }

        // ========================= DELETE PRODUCT =========================

        public async Task DeleteProductAsync(int id)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"UPDATE products 
                             SET is_active = FALSE,
                                 updated_at = NOW()
                             WHERE id = @id";

            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("id", id);

            await cmd.ExecuteNonQueryAsync();
        }
        // ========================= UPDATE PRODUCT =========================

        public async Task UpdateProductAsync(Product p)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
                UPDATE products
                SET
                    name = @name,
                    normalized_name = @norm,
                    barcode = @barcode,
                    price = @price,
                    eco_score = @eco,
                    social_score = @social,
                    category = @cat,
                    stock_minimo = @stockMinimo,
                    image_url = @img,
                    updated_at = NOW()
                WHERE id = @id";

            await using var cmd = new NpgsqlCommand(query, conn);

            cmd.Parameters.AddWithValue("id", p.Id);
            cmd.Parameters.AddWithValue("name", p.Name);
            cmd.Parameters.AddWithValue("norm", p.NormalizedName);
            cmd.Parameters.AddWithValue("barcode", (object?)p.Barcode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("price", p.Price);
            cmd.Parameters.AddWithValue("eco", (object?)p.EcoScore ?? DBNull.Value);
            cmd.Parameters.AddWithValue("social", (object?)p.SocialScore ?? DBNull.Value);
            cmd.Parameters.AddWithValue("cat", (object?)p.Category ?? DBNull.Value);
            cmd.Parameters.AddWithValue("stockMinimo", p.StockMinimo);
            cmd.Parameters.AddWithValue("img", (object?)p.ImageUrl ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }
        // ========================= REGISTER STOCK MOVEMENT =========================

        public async Task RegisterStockMovementAsync(int productId, int movementType, int quantity, string userName)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                var getCmd = new NpgsqlCommand(
                    "SELECT stock_actual FROM products WHERE id = @id FOR UPDATE",
                    conn,
                    transaction);

                getCmd.Parameters.AddWithValue("id", productId);

                var result = await getCmd.ExecuteScalarAsync();

                if (result == null)
                    throw new Exception("Producto no encontrado.");

                int currentStock = Convert.ToInt32(result);
                int newStock = currentStock;

                if (movementType == 1)
                    newStock += quantity;
                else if (movementType == 2)
                {
                    if (currentStock < quantity)
                        throw new Exception("Stock insuficiente.");

                    newStock -= quantity;
                }
                else
                    throw new Exception("Tipo de movimiento inválido.");

                var updateCmd = new NpgsqlCommand(
                    "UPDATE products SET stock_actual = @stock, updated_at = NOW() WHERE id = @id",
                    conn,
                    transaction);

                updateCmd.Parameters.AddWithValue("stock", newStock);
                updateCmd.Parameters.AddWithValue("id", productId);

                await updateCmd.ExecuteNonQueryAsync();

                var insertCmd = new NpgsqlCommand(@"
                    INSERT INTO stock_movements
                    (product_id, movement_type, quantity, user_name)
                    VALUES
                    (@productId, @movementType, @quantity, @userName)",
                    conn,
                    transaction);

                insertCmd.Parameters.AddWithValue("productId", productId);
                insertCmd.Parameters.AddWithValue("movementType", movementType);
                insertCmd.Parameters.AddWithValue("quantity", quantity);
                insertCmd.Parameters.AddWithValue("userName", userName);

                await insertCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ========================= AUTH =========================

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT id, username, password_hash, role, is_active 
                             FROM users 
                             WHERE username = @username";

            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("username", username);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Username = reader.GetString(reader.GetOrdinal("username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("password_hash")),
                Role = (UserRole)reader.GetInt32(reader.GetOrdinal("role")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("is_active"))
            };
        }

        // ========================= AUDIT =========================

        public async Task InsertAuditAsync(AuditLog log)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"INSERT INTO audit_logs 
                            (user_id, action, description, created_at)
                            VALUES 
                            (@userId, @action, @description, @createdAt)";

            await using var cmd = new NpgsqlCommand(query, conn);

            cmd.Parameters.AddWithValue("userId", log.UserId);
            cmd.Parameters.AddWithValue("action", log.Action);
            cmd.Parameters.AddWithValue("description", (object?)log.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("createdAt", log.CreatedAt);

            await cmd.ExecuteNonQueryAsync();
        }

        // ========================= DASHBOARD METRICS =========================

        public async Task<(int total, double avgEco, int lowEco)> GetMetricsAsync()
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
                SELECT 
                    COUNT(*) AS total,
                    AVG(eco_score) AS avg_eco,
                    COUNT(*) FILTER (WHERE eco_score < 40) AS low_eco
                FROM products
                WHERE is_active = TRUE";

            await using var cmd = new NpgsqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                int total = reader.GetInt32(0);
                double avgEco = reader.IsDBNull(1) ? 0 : reader.GetDouble(1);
                int lowEco = reader.GetInt32(2);
                
                return (total, avgEco, lowEco);
            }

            return (0, 0, 0);
        }



        public async Task<List<(string Username, string Action, string Description, DateTime CreatedAt)>> GetAuditLogsAsync()
        {
            var list = new List<(string, string, string, DateTime)>();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
                SELECT u.username, a.action, a.description, a.created_at
                FROM audit_logs a
                JOIN users u ON a.user_id = u.id
                ORDER BY a.created_at DESC";

            await using var cmd = new NpgsqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add((
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2),
                    reader.GetDateTime(3)
                ));
            }

            return list;
        }


        public async Task<List<User>> GetUsersAsync()
        {
            var list = new List<User>();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"SELECT id, username, password_hash, role, is_active 
                            FROM users
                            ORDER BY id DESC";

            await using var cmd = new NpgsqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    Role = (UserRole)reader.GetInt32(3),
                    IsActive = reader.GetBoolean(4)
                });
            }

            return list;
        }

        public async Task CreateUserAsync(string username, string passwordHash, UserRole role)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"INSERT INTO users (username, password_hash, role, is_active)
                            VALUES (@username, @hash, @role, true)";

            await using var cmd = new NpgsqlCommand(query, conn);

            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("hash", passwordHash);
            cmd.Parameters.AddWithValue("role", (int)role);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task ToggleUserActiveAsync(int userId, bool isActive)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"UPDATE users 
                            SET is_active = @active
                            WHERE id = @id";

            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("active", isActive);
            cmd.Parameters.AddWithValue("id", userId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
    
}        