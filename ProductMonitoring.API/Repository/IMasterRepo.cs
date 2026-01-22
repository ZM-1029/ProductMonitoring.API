using ProductMonitoring.API.Models;

namespace ProductMonitoring.API.Repository
{
    public interface IMasterRepo
    {
        public Task<List<BitAddressMaster>> GetAllBitAddressByKeyAsync(string key);
        public Task<BitAddressMaster?> GetBitAddressByKeyAsync(string key);
        public Task<List<BitAddressCause>?> GetBitAddressCauseAsync(string key);
        public Task<List<BitAddressRemedy>?> GetBitAddressRemedyAsync(string key);
        public Task<List<BitAddressErrorManual>?> GetBitAddressManualAsync(string key);
    }
}
