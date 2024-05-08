using Tutorial6.Models.DTOs;

namespace Tutorial6.Repositories;

public interface IAnimalsRepository
{
    Task<bool> DoesAnimalExist(int id);
    Task<AnimalDTO> GetAnimalByID(int id);
    Task RemoveAnimalWithProcedures(int id);
    Task RemoveAnimal(int id);
    Task RemoveAnimalProcedures(int id);
}