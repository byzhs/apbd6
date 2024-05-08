using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Tutorial6.Models.DTOs;

namespace Tutorial6.Repositories;

public class AnimalsRepository : IAnimalsRepository
{
    private readonly IConfiguration _configuration;
    public AnimalsRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<bool> DoesAnimalExist(int id)
    {
        var query = "SELECT 1 FROM Animal WHERE ID = @ID";
        
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();
        
        return res is not null;
    }

    public async Task<AnimalDTO> GetAnimalByID(int id)
    {
        var query = @"SELECT Animal.ID AS AnimalID, Animal.Name AS AnimalName, AdmissionDate, Owner.FirstName, Owner.LastName, Animal_Class.Name AS ClassName
                      FROM Animal
                      JOIN Owner ON Animal.Owner_ID = Owner.ID
                      JOIN Animal_Class ON Animal.Animal_Class_ID = Animal_Class.ID
                      WHERE Animal.ID = @ID;";
        
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);
        
        await connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();
        
        await reader.ReadAsync();

        if (!reader.HasRows) throw new Exception();

        var animalIDOrdinal = reader.GetOrdinal("AnimalID");
        var animalNameOrdinal = reader.GetOrdinal("AnimalName");
        var admissionDateOrdinal = reader.GetOrdinal("AdmissionDate");
        var firstNameOrdinal = reader.GetOrdinal("FirstName");
        var lastNameOrdinal = reader.GetOrdinal("LastName");
        var classNameOrdinal = reader.GetOrdinal("ClassName");

        var animalDTO = new AnimalDTO()
        {
            ID = reader.GetInt32(animalIDOrdinal),
            Name = reader.GetString(animalNameOrdinal),
            AdmissionDate = reader.GetDateTime(admissionDateOrdinal),
            Owner = new OwnerDTO()
            {
                FirstName = reader.GetString(firstNameOrdinal),
                LastName = reader.GetString(lastNameOrdinal)
            },
            Class = new ClassDTO()
            {
                Name = reader.GetString(classNameOrdinal)
            },
        };

        return animalDTO;
    }

    public async Task RemoveAnimalWithProcedures(int id)
    {
        var query = @"DELETE FROM Procedure_Animal WHERE Animal_ID = @ID;";
        var query2 = @"DELETE FROM Animal WHERE ID = @ID";

        
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;

        await connection.OpenAsync();
        
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = (SqlTransaction)transaction;

        try
        {
            // Execution of the first command
            command.Parameters.Clear();
            command.CommandText = query;
            command.Parameters.AddWithValue("@ID", id);
            
            await command.ExecuteNonQueryAsync();
            
            // Execution of the second command
            command.Parameters.Clear();
            command.CommandText = query2;
            command.Parameters.AddWithValue("@ID", id);
            
            await command.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task RemoveAnimal(int id)
    {
        var query = @"DELETE FROM Animal WHERE ID = @ID";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);
        
        await connection.OpenAsync();
        
        await command.ExecuteNonQueryAsync();
    }

    public async Task RemoveAnimalProcedures(int id)
    {
        var query = @"DELETE FROM Procedure_Animal WHERE Animal_ID = @ID;";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        await command.ExecuteNonQueryAsync();
    }
}