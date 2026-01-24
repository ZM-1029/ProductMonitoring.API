using ProductMonitoring.API.DTO;
using ProductMonitoring.API.Models;

namespace ProductMonitoring.API.Repository
{
    public interface IMasterRepo
    {
        public  Task<bool> AddNewRemedy(string remedy, string key);
        public  Task<bool> AddTicketSolution(string key, string? remedy, bool IsExistingSolution);
        public  Task<List<SolutionHistory>?> GetTicketSolution(int? count);
        public Task<bool> PostErrorManual(RequestBody data);
        public Task<List<BitAddressMaster>> GetAllBitAddressByKeyAsync(string key);
        public Task<BitAddressMaster?> GetBitAddressByKeyAsync(string key);
        public Task<List<BitAddressCause>?> GetBitAddressCauseAsync(string key);
        public Task<List<BitAddressRemedy>?> GetBitAddressRemedyAsync(string key);
        public Task<List<BitAddressErrorManual>?> GetBitAddressManualAsync(string key);
    }
}
