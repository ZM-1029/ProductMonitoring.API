using Microsoft.EntityFrameworkCore;
using ProductMonitoring.API.Models;

namespace ProductMonitoring.API.Repository
{
    public class MasterRepo : IMasterRepo
    {
        private readonly ProductMonitoringDbContext _dbContext;
        public MasterRepo(ProductMonitoringDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<BitAddressMaster>> GetAllBitAddressByKeyAsync(string key)
        {
            return await _dbContext.BitAddressMasters
            .Where(x => EF.Functions.Like(x.Code, $"%{key}%"))
            .ToListAsync();
        }

        public async Task<BitAddressMaster?> GetBitAddressByKeyAsync(string key)
        {

            /* return await _dbContext.BitAddressMasters
               .Where(x => EF.Functions.Like(x.Code, $"%{key}%"))
               .ToListAsync();*/

            return await _dbContext.BitAddressMasters
                .FirstOrDefaultAsync(x => x.Code.Trim().ToUpper().Contains(key.Trim().ToUpper()));
                
        }

        public async Task<List<BitAddressCause>?> GetBitAddressCauseAsync(string key)
        {
            var bitAddress= await _dbContext.BitAddressMasters
                           .FirstOrDefaultAsync(x => x.Code.Trim().ToUpper().Contains(key.Trim().ToUpper()));

            if(bitAddress==null) return null;

            return await _dbContext.BitAddressCauses.Where(x => x.BitAddressId == bitAddress.Id).ToListAsync();
        }

        public async Task<List<BitAddressErrorManual>?> GetBitAddressManualAsync(string key)
        {
            var bitAddress = await _dbContext.BitAddressMasters
                           .FirstOrDefaultAsync(x => x.Code.Trim().ToUpper().Contains(key.Trim().ToUpper()));

            if (bitAddress == null) return null;

            return await _dbContext.BitAddressErrorManuals.Where(x => x.BitAddressId == bitAddress.Id).ToListAsync();
        }

        public async Task<List<BitAddressRemedy>?> GetBitAddressRemedyAsync(string key)
        {
            var bitAddress = await _dbContext.BitAddressMasters
                           .FirstOrDefaultAsync(x => x.Code.Trim().ToUpper().Contains(key.Trim().ToUpper()));

            if (bitAddress == null) return null;

            return await _dbContext.BitAddressRemedies.Where(x => x.BitAddressId == bitAddress.Id).ToListAsync();
        }
    }
}
