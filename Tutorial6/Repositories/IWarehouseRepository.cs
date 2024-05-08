using System.Threading.Tasks;
using Tutorial6.Models.DTOs;

namespace Tutorial6.Repositories
{
    public interface IWarehouseRepository
    {
        Task<int?> AddProductAsync(ProductWarehouseRequest request);
        Task<int?> AddProductUsingProcedureAsync(ProductWarehouseRequest request);
    }
}