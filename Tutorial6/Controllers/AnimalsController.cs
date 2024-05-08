using System.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial6.Repositories;

namespace Tutorial6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnimalsController : ControllerBase
    {
        private readonly IAnimalsRepository _animalsRepository;
        public AnimalsController(IAnimalsRepository animalsRepository)
        {
            _animalsRepository = animalsRepository;
        }

        [HttpGet]
        [Route("{animalID}")]
        public async Task<IActionResult> GetAnimal(int animalID)
        {
            if (!await _animalsRepository.DoesAnimalExist(animalID))
                return NotFound();

            var animal = await _animalsRepository.GetAnimalByID(animalID);
            
            return Ok(animal);
        }
        
        [HttpDelete]
        public async Task<IActionResult> RemoveAnimalWithProcedures(int animalID)
        {
            if (!await _animalsRepository.DoesAnimalExist(animalID))
                return NotFound();

            await _animalsRepository.RemoveAnimalWithProcedures(animalID);
            
            return Ok();
        }
        
        [HttpDelete]
        [Route("scope")]
        public async Task<IActionResult> RemoveAnimalWithProceduresUsingScope(int animalID)
        {
            if (!await _animalsRepository.DoesAnimalExist(animalID))
                return NotFound();

            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _animalsRepository.RemoveAnimalProcedures(animalID);
                    await _animalsRepository.RemoveAnimal(animalID);
                
                    scope.Complete();
                }
            }
            catch (TransactionAbortedException ex)
            {
                Console.WriteLine("TransactionAbortedException Message: {0}", ex.Message);
                throw;
            }
            

            return Ok();
        }
        
    }
}
