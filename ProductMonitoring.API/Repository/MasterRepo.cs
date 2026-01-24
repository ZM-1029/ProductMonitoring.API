using Microsoft.EntityFrameworkCore;
using ProductMonitoring.API.DTO;
using ProductMonitoring.API.Models;

namespace ProductMonitoring.API.Repository
{
    public class MasterRepo : IMasterRepo
    {
        private readonly ProductMonitoringDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public MasterRepo(ProductMonitoringDbContext dbContext, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _environment = environment;
        }
        private async Task<string?> UploadErrorManual(IFormFile? manual,string key)
        {
            if (manual == null || manual.Length == 0)
                return null;
            var CurrDir = Directory.GetCurrentDirectory();
            var uploadsFolder = Path.Combine(CurrDir, "ErrorManual");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{key}_{Path.GetFileName(manual.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await manual.CopyToAsync(fileStream);
            }

            return $"/errorManual/{uniqueFileName}";
        }

        public async Task<bool> PostErrorManual(RequestBody data)
        { 
            var bitAddress = await _dbContext.BitAddressMasters
                           .FirstOrDefaultAsync(x =>x.BitCategoryId==data.CategoryId && x.Code.Trim().ToUpper().Contains(data.Key.Trim().ToUpper()));

            if (bitAddress == null) return false;
            // file operation
            var url = data.File!=null?await UploadErrorManual(data.File, data.Key):null;

            var existingManual = await _dbContext.BitAddressErrorManuals
                                        .FirstOrDefaultAsync(x => x.BitAddressId == bitAddress.Id);
            if (existingManual == null)
            {
                BitAddressErrorManual newErrorManual = new BitAddressErrorManual
                {
                    BitAddressId = bitAddress.Id,
                    ManualUrl = url
                };

               await  _dbContext.BitAddressErrorManuals.AddAsync(newErrorManual);
            }
            else
            {
                existingManual.ManualUrl = url;
                //_dbContext.Update(existingManual);

            }

            _dbContext.SaveChanges();
            return true;
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

        public async Task<bool> AddNewRemedy(string remedy, string key)
        {
            var existingBitAddress =  await _dbContext.BitAddressMasters
                .FirstOrDefaultAsync(x => x.Code.Trim().ToUpper().Contains(key.Trim().ToUpper()));

            if (existingBitAddress == null) { return false; }

            var data = new BitAddressRemedy { 
            Remedy = remedy,
            IsAdditionRemedy=true,
            BitAddressId=existingBitAddress.Id,

            };

            await _dbContext.BitAddressRemedies.AddAsync(data);
            _dbContext.SaveChanges();
            return true;
        }

        public async Task<bool> AddTicketSolution(string key, string? remedy, bool IsExistingSolution)
        {
            var existingBitAddress = await _dbContext.BitAddressMasters
               .FirstOrDefaultAsync(x => x.Code.Trim().ToUpper().Contains(key.Trim().ToUpper()));

            if (existingBitAddress == null) { return false; }

            var data = new SolutionHistory
            {
                Description = remedy,
                IsExistingSolution = IsExistingSolution,
                BitAddressId = existingBitAddress.Id,

            };

            await _dbContext.SolutionHistories.AddAsync(data);
            _dbContext.SaveChanges();
            return true;
        }

        public async Task<List<SolutionHistory>?> GetTicketSolution(int? count)
        {
            var solutions = await _dbContext.SolutionHistories.ToListAsync();
              // .Where(x => x.Code.Trim().ToUpper().Contains(key.Trim().ToUpper()));

            if (count != null && count>0) 
            {
                solutions = solutions.Take(count??0).ToList();
            }

           return solutions;
        }
    }
}
