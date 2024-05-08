using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Tutorial6.Models.DTOs;

namespace Tutorial6.Repositories
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly IConfiguration _configuration;

        public WarehouseRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int?> AddProductAsync(ProductWarehouseRequest request)
        {
            var checkProductQuery = "SELECT 1 FROM Product WHERE IdProduct = @IdProduct";
            var checkWarehouseQuery = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
            var findOrderQuery = @"
                SELECT IdOrder, Price 
                FROM [Order] 
                WHERE IdProduct = @IdProduct 
                AND Amount = @Amount 
                AND CreatedAt < @CreatedAt
                AND IdOrder NOT IN (SELECT IdOrder FROM Product_Warehouse)";
            var insertProductWarehouseQuery = @"
                INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                OUTPUT INSERTED.IdProductWarehouse
                VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)";

            using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await connection.OpenAsync();
            
            using var transaction = await connection.BeginTransactionAsync();
            using var command = new SqlCommand { Connection = connection, Transaction = (SqlTransaction)transaction };

            try
            {
                command.CommandText = checkProductQuery;
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                var productExists = await command.ExecuteScalarAsync() != null;
                if (!productExists)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                command.CommandText = checkWarehouseQuery;
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                var warehouseExists = await command.ExecuteScalarAsync() != null;
                if (!warehouseExists)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                command.CommandText = findOrderQuery;
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                command.Parameters.AddWithValue("@Amount", request.Amount);
                command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);
                using var reader = await command.ExecuteReaderAsync();
                if (!reader.HasRows)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                await reader.ReadAsync();
                int idOrder = reader.GetInt32(reader.GetOrdinal("IdOrder"));
                decimal price = reader.GetDecimal(reader.GetOrdinal("Price"));

                command.CommandText = insertProductWarehouseQuery;
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                command.Parameters.AddWithValue("@IdOrder", idOrder);
                command.Parameters.AddWithValue("@Amount", request.Amount);
                command.Parameters.AddWithValue("@Price", request.Amount * price);
                command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

                var newProductWarehouseId = await command.ExecuteScalarAsync();
                await transaction.CommitAsync();

                return newProductWarehouseId as int?;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error in AddProductAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<int?> AddProductUsingProcedureAsync(ProductWarehouseRequest request)
        {
            var query = "AddProductToWarehouse";

            using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            using var command = new SqlCommand(query, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
            command.Parameters.AddWithValue("@Amount", request.Amount);
            command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();

            return result as int?;
        }
    }
}
