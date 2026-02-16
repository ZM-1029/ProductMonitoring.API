using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ProductMonitoring.API.DTO;
using ProductMonitoring.API.Models;
using ProductMonitoring.API.SignalRsetup;
using System.Drawing;
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

        // Bulk Upload Implementation
        /*public string GeneratePartMasterTemplate()
        {
            try
            {
                var templatesFolder = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
                if (!Directory.Exists(templatesFolder))
                    Directory.CreateDirectory(templatesFolder);

                var templatePath = Path.Combine(templatesFolder, "PartMaster_Template.xlsx");

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("PartMaster");

                    // Header styling
                    worksheet.Cells["A1:C1"].Style.Font.Bold = true;
                    worksheet.Cells["A1:C1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells["A1:C1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(68, 114, 196));
                    worksheet.Cells["A1:C1"].Style.Font.Color.SetColor(Color.White);
                    worksheet.Cells["A1:C1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Headers
                    worksheet.Cells["A1"].Value = "Part Number*";
                    worksheet.Cells["B1"].Value = "Description";
                    worksheet.Cells["C1"].Value = "Quantity";

                    // Sample data
                    worksheet.Cells["A2"].Value = "PART-001";
                    worksheet.Cells["B2"].Value = "Sample Part 1";
                    worksheet.Cells["C2"].Value = 100;

                    worksheet.Cells["A3"].Value = "PART-002";
                    worksheet.Cells["B3"].Value = "Sample Part 2";
                    worksheet.Cells["C3"].Value = 50;

                    // Column widths
                    worksheet.Column(1).Width = 20;
                    worksheet.Column(2).Width = 35;
                    worksheet.Column(3).Width = 15;

                    // Instructions sheet
                    var instructionsSheet = package.Workbook.Worksheets.Add("Instructions");
                    instructionsSheet.Cells["A1"].Value = "INSTRUCTIONS FOR PART MASTER BULK UPLOAD";
                    instructionsSheet.Cells["A1"].Style.Font.Bold = true;
                    instructionsSheet.Cells["A1"].Style.Font.Size = 14;

                    instructionsSheet.Cells["A3"].Value = "1. Part Number is mandatory (marked with *)";
                    instructionsSheet.Cells["A4"].Value = "2. Description is optional";
                    instructionsSheet.Cells["A5"].Value = "3. Quantity is optional (numeric values only)";
                    instructionsSheet.Cells["A6"].Value = "4. Do not modify the header row";
                    instructionsSheet.Cells["A7"].Value = "5. Delete the sample data before uploading";
                    instructionsSheet.Cells["A8"].Value = "6. Save the file and upload it using the Bulk Upload API";

                    instructionsSheet.Column(1).Width = 60;

                    package.SaveAs(new FileInfo(templatePath));
                }

                return templatePath;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }*/

        public async Task<BulkUploadResultDTO> BulkUploadPartMaster(IFormFile file)
        {
            var result = new BulkUploadResultDTO();
            var partsToAdd = new List<PartMaster>();

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                   // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            result.Errors.Add("No worksheet found in the file");
                            return result;
                        }

                        var rowCount = worksheet.Dimension?.Rows ?? 0;
                        if (rowCount < 2)
                        {
                            result.Errors.Add("File contains no data rows");
                            return result;
                        }

                        result.TotalRows = rowCount - 1; // Excluding header

                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var partNumber = worksheet.Cells[row, 1].Value?.ToString()?.Trim();

                                if (string.IsNullOrWhiteSpace(partNumber))
                                {
                                    result.Errors.Add($"Row {row}: Part Number is required");
                                    result.FailureCount++;
                                    continue;
                                }

                                // Check for duplicate in database
                                var existingPart = await _dbContext.PartMaster
                                    .FirstOrDefaultAsync(p => p.Number.ToUpper() == partNumber.ToUpper());

                                if (existingPart != null)
                                {
                                    result.Errors.Add($"Row {row}: Part Number '{partNumber}' already exists");
                                    result.FailureCount++;
                                    continue;
                                }

                                var description = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                var quantityValue = worksheet.Cells[row, 3].Value;
                                int? quantity = null;

                                if (quantityValue != null)
                                {
                                    if (int.TryParse(quantityValue.ToString(), out int qty))
                                    {
                                        quantity = qty;
                                    }
                                    else
                                    {
                                        result.Errors.Add($"Row {row}: Invalid quantity value");
                                        result.FailureCount++;
                                        continue;
                                    }
                                }

                                partsToAdd.Add(new PartMaster
                                {
                                    Number = partNumber,
                                    Description = description,
                                    Quantity = quantity
                                });

                                result.SuccessCount++;
                            }
                            catch (Exception ex)
                            {
                                result.Errors.Add($"Row {row}: {ex.Message}");
                                result.FailureCount++;
                            }
                        }

                        // Bulk insert
                        if (partsToAdd.Any())
                        {
                            await _dbContext.PartMaster.AddRangeAsync(partsToAdd);
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"File processing error: {ex.Message}");
            }

            return result;
        }
    }
}
