using ProductMonitoring.API.DTO;
using ProductMonitoring.API.Models;

namespace ProductMonitoring.API.Repository
{
    public interface IMasterRepo
    {
/*        public string GeneratePartMasterTemplate();
*/        public  Task<BulkUploadResultDTO> BulkUploadPartMaster(IFormFile file);

        public Task AddSolutionHistoryAsync(SolutionHistory model);
        public  Task<List<BitCategory>> GetAllCategoriesAsync(List<int> categoryIds);
        public  Task<List<BitAddressRemedy>?> GetBitAddressRemedyWithManualAsync(string key, int categoryId);

        public Task<bool> AddNewRemedy(string remedy, string key);
        public  Task<bool> UpdateErrorLog(string key, string? remedy, bool IsExistingSolution, IFormFile? file);
        public  Task<List<dynamic>> ErrorLogData(int? count, DateTime? from, DateTime? to, string? code);
        public Task<bool> PostErrorManual(RequestBody data);
        public Task<List<BitAddressMaster>> GetAllBitAddressByKeyAsync(string key);
        public Task<BitAddressMaster?> GetBitAddressByKeyAsync(string key,int categoryId);
        public Task<List<BitAddressCause>?> GetBitAddressCauseAsync(string key);
        public Task<List<BitAddressRemedy>?> GetBitAddressRemedyAsync(string key);
        public Task<List<BitAddressErrorManual>?> GetBitAddressManualAsync(string key);
        public  Task<List<BitAddressRemedy>?> GetBitAddressRemedyWithManualAsync(string key);
        public  Task<PartMaster?> GetPartMasterByPartNumberAsync(string partNumber);
    }
}
