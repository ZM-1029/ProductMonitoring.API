using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProductMonitoring.API.DTO;
using ProductMonitoring.API.Models;
using ProductMonitoring.API.SignalRsetup;
using static System.Net.WebRequestMethods;

namespace ProductMonitoring.API.Repository
{
    public class MasterRepo : IMasterRepo
    {
        private readonly ProductMonitoringDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;
        private readonly IHubContext<SolutionNotificationHub> _hub;

        public MasterRepo(ProductMonitoringDbContext dbContext, IWebHostEnvironment environment, IHubContext<SolutionNotificationHub> hub)
        {
            _dbContext = dbContext;
            _environment = environment;
            _hub = hub;
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

        public async Task<BitAddressMaster?> GetBitAddressByKeyAsync(string key,int categoryId)
        {

            /* return await _dbContext.BitAddressMasters
               .Where(x => EF.Functions.Like(x.Code, $"%{key}%"))
               .ToListAsync();*/

            return await _dbContext.BitAddressMasters
                .FirstOrDefaultAsync(x =>x.BitCategoryId==categoryId && x.Code.Trim().ToUpper().Contains(key.Trim().ToUpper()));
                
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
           
        public async Task<List<BitAddressRemedy>?> GetBitAddressRemedyWithManualAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            var normalizedKey = key.Trim().ToUpper();

            var bitAddress = await _dbContext.BitAddressMasters
                .FirstOrDefaultAsync(x => x.Code.Trim().ToUpper().Contains(normalizedKey));

            if (bitAddress == null)
                return null;

            var manual = await _dbContext.BitAddressErrorManuals
                .FirstOrDefaultAsync(x => x.BitAddressId == bitAddress.Id);

            var remedies = await _dbContext.BitAddressRemedies
                .Where(x => x.BitAddressId == bitAddress.Id)
                .ToListAsync();

            // Add manual link as a remedy if manual exists
            if (manual != null && !string.IsNullOrWhiteSpace(manual.ManualUrl))
            {
                var manualLink = $"https://zoumapushpak.com:442{manual.ManualUrl}";

                remedies.Add(new BitAddressRemedy
                {
                    BitAddressId = bitAddress.Id,
                    Remedy = $"Error Manual: {manualLink}"
                });
            }

            return remedies;
        }
        public async Task<List<BitAddressRemedy>?> GetBitAddressRemedyWithManualAsync(string key, int categoryId)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            var normalizedKey = key.Trim().ToUpper();

            var bitAddress = await _dbContext.BitAddressMasters
                .FirstOrDefaultAsync(x =>x.BitCategoryId==  categoryId &&  x.Code.Trim().ToUpper().Contains(normalizedKey));

            if (bitAddress == null)
                return null;

            var manual = await _dbContext.BitAddressErrorManuals
                .FirstOrDefaultAsync(x => x.BitAddressId == bitAddress.Id);

            var remedies = await _dbContext.BitAddressRemedies
                .Where(x => x.BitAddressId == bitAddress.Id)
                .ToListAsync();

            // Add manual link as a remedy if manual exists
            if (manual != null && !string.IsNullOrWhiteSpace(manual.ManualUrl))
            {
                var manualLink = $"https://zoumapushpak.com:442{manual.ManualUrl}";

                remedies.Add(new BitAddressRemedy
                {
                    BitAddressId = bitAddress.Id,
                    Remedy = $"Error Manual: {manualLink}"
                });
            }

            return remedies;
        }


        public async Task<List<BitCategory>> GetAllCategoriesAsync(List<int> categoryIds)
        {
            return await _dbContext.BitCategories.Where(x=> categoryIds.Contains((int)x.Id)).ToListAsync();
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

        public async Task<bool> UpdateErrorLog(string key, string? remedy, bool IsExistingSolution, IFormFile? file)
        {
            var existingBitAddress = await _dbContext.BitAddressMasters
               .FirstOrDefaultAsync(x => x.Code.Trim().ToUpper().Contains(key.Trim().ToUpper()));

            if (existingBitAddress == null) { return false; }

            var existingErrorLog = _dbContext.SolutionHistories.FirstOrDefault(x =>x.IsOpen==true && x.BitAddress.Trim().ToUpper().Contains(key.Trim().ToUpper()));
            if (existingErrorLog != null)
            {
                existingErrorLog.IsExistingSolution = IsExistingSolution;
                existingErrorLog.IsOpen = false;
                existingErrorLog.UpdatedOn = DateTime.UtcNow;
                if (file != null)
                {
                    var fileURL=await UploadErrorManual(file, key);
                    existingErrorLog.File=fileURL;
        
                    BitAddressErrorManual newErrorManual = new BitAddressErrorManual
                    {
                        BitAddressId = existingBitAddress.Id,
                        ManualUrl = fileURL,
                        IsAdditionalManual = true
                    };
                    await _dbContext.BitAddressErrorManuals.AddAsync(newErrorManual);
                                      
                }
                _dbContext.Update(existingErrorLog);
                _dbContext.SaveChanges();
                return true;
            }
            
            return false;
        }

        public async Task<List<dynamic>> ErrorLogData(int? count, DateTime? from , DateTime? to,string? code)
        {
            var solutions = await _dbContext.SolutionHistories.OrderByDescending(y => y.CreatedOn).ToListAsync();
            // .Where(x => x.Code.Trim().ToUpper().Contains(key.Trim().ToUpper()));

            var bitAddressList = await _dbContext.BitAddressMasters.ToListAsync();
            var categoryList = await _dbContext.BitCategories.ToListAsync();
             //.Where(x => x.Code.Trim().ToUpper().Contains(key.Trim().ToUpper()));

         

            if (from != null && from.HasValue && to != null && to.HasValue)
            {
                solutions = solutions.Where(x => x.CreatedOn!=null &&  x.CreatedOn.Value.Date >= from && x.CreatedOn.Value.Date <= to).ToList();
            }
            if (code != null)
            {
                solutions = solutions
                          .Where(x => x.BitAddress.Trim().ToUpper().Contains(code.Trim().ToUpper())).ToList();
            }

            if (count != null && count > 0)
            {
                solutions = solutions.Take(count ?? 0).ToList();
            }

            var response= solutions.Select(  x=> (dynamic)new 
            {
                
                BitAddress=x.BitAddress,
                x.CategoryId,
                Category= categoryList.FirstOrDefault(y=>y.Id==x.CategoryId)?.Name,
                Description = bitAddressList.FirstOrDefault(d=>d.Code.Trim().ToUpper().Contains(x.BitAddress.Trim().ToUpper()))?.Message,
               // x.IsExistingSolution,
                x.CreatedOn,
                x.IsOpen,
                x.UpdatedOn,
                x.File
            }).OrderByDescending(y=>y.CreatedOn).ToList();

           return response;
        }

        public async Task AddSolutionHistoryAsync(SolutionHistory model)
        {
            model.CreatedOn = DateTime.UtcNow;
            model.IsOpen = true;

            _dbContext.SolutionHistories.Add(model);
            await _dbContext.SaveChangesAsync();

            // 🔔 SEND REAL-TIME NOTIFICATION
            await _hub.Clients.All.SendAsync("NewErrorLog", new
            {
                id = model.Id,
                bitAddress = model.BitAddress,
                description = model.Description,
                createdOn = model.CreatedOn,
                model.CategoryId,               
            });
        }
    }
}
