using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductMonitoring.API.DTO;
using ProductMonitoring.API.Models;
using ProductMonitoring.API.Repository;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace ProductMonitoring.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly IMasterRepo _masterRepo;
        public MasterController(IMasterRepo masterRepo)
        {
            _masterRepo = masterRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetBitAddressList(string key)
        {
            var bitAddressList = await _masterRepo.GetAllBitAddressByKeyAsync(key);
            if (!bitAddressList.Any()) return Ok(new { Status = false, Message = "No data found!" });

            return Ok(new { Status = true, Data = bitAddressList, Message = "Data retrieved successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetBitAddress(string key,int categoryId)
        {
            var bitAddress = await _masterRepo.GetBitAddressByKeyAsync(key,categoryId);
            if (bitAddress == null) return Ok(new { Status = false, Message = "No data found!" });

            return Ok(new { Status = true, Data = bitAddress, Message = "Data retrieved successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetBitAddressCauseList(string key)
        {
            var bitAddressCauseList = await _masterRepo.GetBitAddressCauseAsync(key);
            if (bitAddressCauseList == null || !bitAddressCauseList.Any()) return Ok(new { Status = false, Message = "No data found!" });

            return Ok(new { Status = true, Data = bitAddressCauseList, Message = "Data retrieved successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetBitAddressRemedyList(string key)
        {
            var bitAddressRemedyList = await _masterRepo.GetBitAddressRemedyAsync(key);
            if (bitAddressRemedyList == null || !bitAddressRemedyList.Any()) return Ok(new { Status = false, Message = "No data found!" });

            return Ok(new { Status = true, Data = bitAddressRemedyList, Message = "Data retrieved successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetBitAddressRemedyData(string key)
        {
            var bitAddressRemedyList = await _masterRepo.GetBitAddressRemedyWithManualAsync(key);
            if (bitAddressRemedyList == null || !bitAddressRemedyList.Any()) return Ok(new { Status = false, Message = "No data found!" });

            return Ok(new { Status = true, Data = bitAddressRemedyList, Message = "Data retrieved successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetBitAddressErrorManuals(string key)
        {
            var bitAddressErrorManualList = await _masterRepo.GetBitAddressManualAsync(key);
            if (bitAddressErrorManualList == null || !bitAddressErrorManualList.Any()) return Ok(new { Status = false, Message = "No data found!" });

            return Ok(new { Status = true, Data = bitAddressErrorManualList, Message = "Data retrieved successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetBitAddressWithDetails(string key,int categoryId)
        {
            var bitAddress = await _masterRepo.GetBitAddressByKeyAsync(key, categoryId);
            var bitAddressCauseList = await _masterRepo.GetBitAddressCauseAsync(key);
            var bitAddressRemedyList = await _masterRepo.GetBitAddressRemedyAsync(key);
            var bitAddressErrorManualList = await _masterRepo.GetBitAddressManualAsync(key);
            if (bitAddress == null) return Ok(new { Status = false, Message = "No data found!" });

            var response = new
            {
                BitAddress = bitAddress,
                Causes = bitAddressCauseList,
                Remedies = bitAddressRemedyList,
                ErrorManuals = bitAddressErrorManualList
            };
            return Ok(new { Status = true, Data = response, Message = "Data retrieved successfully!" });
        } // Main API

        [HttpPost]
        public async Task<IActionResult> UploadErrorManual([FromForm] RequestBody data)
        {
            var isUploaded=await _masterRepo.PostErrorManual(data);
            if(isUploaded) 
            return Ok(new { Status = true, Data = data, Message = "Data received successfully!" });
            else
            return Ok(new { Status = false, Message = "Data upload failed!" });
        }

        [HttpPost]
        public async Task<IActionResult> CloseTicket([FromForm] TicketRequest data)
        {
            if (!data.IsExistingSolution && data.Remedy!=null)
            {
                var isAdded = await _masterRepo.AddNewRemedy(data.Remedy,data.key);         
            }

            await _masterRepo.UpdateErrorLog(data.key, data.Remedy, data.IsExistingSolution,data.File);
            return Ok(new { Status = true, Message = "Ticket closed successfully" } );
        }

        [HttpGet]
        public async Task<IActionResult> ErrorLogList(int? count, DateTime? from, DateTime? to,string? code)
        { 
           var data=await _masterRepo.ErrorLogData(count,from,to,code);

            return Ok(new {Status=true, Data= data, Message="Data retrieved successfully"});
        }
                
        [HttpGet]
        // CHat not API
        [HttpGet]
        public async Task<IActionResult> SolveError(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Ok(new { Status = false, Data = new[] { "Please enter your question or error code." } });

            // 🔹 STEP 1: Normal chat handling
            var basicReply = GetBasicChatResponse(input);
            if (basicReply != null)
            {
                return Ok(new
                {
                    Status = true,
                    Message = "Data retrieved successfully",
                    Data = new[] { basicReply }
                });
            }

            // ============================================================
            // 🔹 NEW: PartNumber handling (ADDED – existing logic unchanged)
            // ============================================================
            if (input.IndexOf("part", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // Remove words "part" and "number"
                var cleanedInput = Regex.Replace(input,
                    @"\bpartnumber\b|\bpart\b|\bnumber\b",
                    "",
                    RegexOptions.IgnoreCase).Trim();

                // Extract possible part number token
                var partMatch = Regex.Match(cleanedInput,
                    @"[\[\]A-Za-z0-9\-]+");

                if (partMatch.Success)
                {
                    string partNumber = partMatch.Value.Trim();

                    var partDetails = await _masterRepo.GetPartMasterByPartNumberAsync(partNumber);

                    if (partDetails == null)
                    {
                        return Ok(new
                        {
                            Status = false,
                            Data = new[] { $"I couldn’t find any records for Part Number '{partNumber}'." }
                        });
                    }

                    return Ok(new
                    {
                        Status = true,
                        IsChat = false,
                        Message = $"Here are the details for Part Number '{partNumber}':",
                        Data = new[]
                            {
                            $"Part Number: {partDetails.Number}",
                            $"Part Location: {partDetails.Location}",
                            $"Section: {partDetails.Section}",
                            $"Quantity: {partDetails.Quantity??0}"
                        }
                    });
                }
            }

            // ============================================================

            // 🔹 STEP 2A: Check if user provided BOTH error code AND category
            var keyCategoryMatch = Regex.Match(input,
                @"(?<key>\[[A-Za-z0-9]+\]M?\d+|M\d{3,6}|\b\d{3}\b)\s*[-,:]?\s*(?<category>[A-Za-z ]{3,})",
                RegexOptions.IgnoreCase);

            if (keyCategoryMatch.Success)
            {
                string keyFromUser = keyCategoryMatch.Groups["key"].Value.Trim();
                string categoryFromUser = keyCategoryMatch.Groups["category"].Value.Trim();

                var bitAddresses = await _masterRepo.GetAllBitAddressByKeyAsync(keyFromUser);

                if (bitAddresses == null || !bitAddresses.Any())
                {
                    return Ok(new
                    {
                        Status = false,
                        Data = new[] { $"I couldn’t find any records for error code '{keyFromUser}'." }
                    });
                }

                var categories = await _masterRepo.GetAllCategoriesAsync(
                    bitAddresses.Select(x => (int)x.BitCategoryId).Distinct().ToList());

                var selectedCategory = categories
                    .FirstOrDefault(c => c.Name.Equals(categoryFromUser, StringComparison.OrdinalIgnoreCase));

                if (selectedCategory == null)
                {
                    return Ok(new
                    {
                        Status = false,
                        Data = new[]
                        {
                            $"I found error code '{keyFromUser}', but category '{categoryFromUser}' did not match.",
                            $"Please reply in this format: {keyFromUser} - {string.Join(" / ", categories.Select(c => c.Name))}"
                        }
                    });
                }

                // 🔹 FINAL FILTER using key + category
                var finalResult = await _masterRepo.GetBitAddressRemedyWithManualAsync(keyFromUser, (int)selectedCategory.Id);

                if (finalResult == null || !finalResult.Any())
                {
                    return Ok(new
                    {
                        Status = false,
                        Data = new[] { $"No remedy found for '{keyFromUser}' under category '{selectedCategory.Name}'." }
                    });
                }

                return Ok(new
                {
                    Status = true,
                    IsChat = false,
                    DetectedCode = keyFromUser,
                    Message = $"Here’s the solution under category '{selectedCategory.Name}':",
                    Data = finalResult.Select(x => x.Remedy)
                });
            }

            // 🔹 STEP 2B: Only error code detection (your original logic)
            var match = Regex.Match(input, @"(\[[A-Za-z0-9]+\]M?\d+|M\d{3,6}|\b\d{3}\b)");

            if (!match.Success)
            {
                return Ok(new
                {
                    Status = false,
                    Message = "",
                    Data = new[] { "I couldn’t find a valid error code. Please share the exact error code like M1234 or [ABC]M456." }
                });
            }

            string key = match.Value;

            // 🔹 STEP 3: Fetch BitAddress records
            var BitAddressList = (await _masterRepo.GetAllBitAddressByKeyAsync(key));
            var matchingCodeCount = BitAddressList.Count();

            if (matchingCodeCount <= 0)
            {
                return Ok(new
                {
                    Status = false,
                    Message = "",
                    Data = new[] { $"I found the error code '{key}', but no remedy is available yet. Please contact support." }
                });
            }
            else if (matchingCodeCount > 1)
            {
                // ask user to choose category
                var categoryList = await _masterRepo.GetAllCategoriesAsync(
                    BitAddressList.Select(x => (int)x.BitCategoryId).Distinct().ToList());

                return Ok(new
                {
                    Status = false,
                    IsChat = false,
                    DetectedCode = key,
                    Message = "I found multiple categories for your error code. Please select the relevant category to proceed:",
                    Data = new[] {$"I found multiple categories for error code '{key}'. " +
                           $"Please choose specific Category from provided options and reply in this format:\n👉 {key} - {string.Join(" / ", categoryList.Select(x => x.Name))}" }
                });
            }
            else
            {
                var result = await _masterRepo.GetBitAddressRemedyWithManualAsync(key);

                if (result == null || !result.Any())
                {
                    return Ok(new
                    {
                        Status = false,
                        Message = "",
                        Data = new[] { $"I found the error code '{key}', but no remedy is available yet. Please contact support." }
                    });
                }
                else
                {
                    return Ok(new
                    {
                        Status = true,
                        IsChat = false,
                        DetectedCode = key,
                        Message = "Here’s the solution for your error:",
                        Data = result.Select(x => x.Remedy)
                    });
                }
            }
        }

        /*        public async Task<IActionResult> SolveError2(string input)
                {
                    if (string.IsNullOrWhiteSpace(input))
                        return Ok(new { Status = false, Data = new[] { "Please enter your question or error code." } });

                    // 🔹 STEP 1: Check for normal chat messages
                    var basicReply = GetBasicChatResponse(input);
                    if (basicReply != null)
                    {
                        return Ok(new
                        {
                            Status = true,
                            Message = "Data retrieved successfully",
                            Data =new[] { basicReply }
                        });
                    }

                    // 🔹 STEP 2: Try extracting error code using regex
                    var match = Regex.Match(input, @"(\[[A-Za-z0-9]+\]M?\d+|M\d{3,6}|\b\d{3}\b)");

                    if (!match.Success)
                    {
                        return Ok(new
                        {
                            Status = false,
                            Message = "",
                            Data = new[] { "I couldn’t find a valid error code. Please share the exact error code like M1234 or [ABC]M456." }
                        });
                    }

                    string key = match.Value;

                    // 🔹 STEP 3: Fetch remedy from DB
                    var BitAddressList = (await _masterRepo.GetAllBitAddressByKeyAsync(key));

                    var matchingCodeCount = BitAddressList.Count();

                    if (matchingCodeCount <= 0)
                    {
                        return Ok(new
                        {
                            Status = false,
                            Message = "",
                            Data = new[] { $"I found the error code '{key}', but no remedy is available yet. Please contact support." }
                        });
                    }
                    else if (matchingCodeCount > 1)
                    {
                        // ask for category
                        var categoryList=await _masterRepo.GetAllCategoriesAsync(BitAddressList.Select(x=>(int)x.BitCategoryId).Distinct().ToList());
                        return Ok(new
                        {
                            Status = false,
                            IsChat = false,
                            DetectedCode = key,
                            Message = "I found multiple categories for your error code. Please select the relevant category to proceed:",
                            Data = $"I found multiple categories for your error code: `{key}`. Please type the relevant category from the provided list to proceed: {string.Join(", ", categoryList.Select(x => x.Name))}"
                        });
                    }
                    else
                    {
                        var result = await _masterRepo.GetBitAddressRemedyWithManualAsync(key);
                        if (result == null || !result.Any())
                        {
                            return Ok(new
                            {
                                Status = false,
                                Message = "",
                                Data = new[] { $"I found the error code '{key}', but no remedy is available yet. Please contact support." }
                            });
                        }
                        else
                        {
                            return Ok(new
                            {
                                Status = true,
                                IsChat = false,
                                DetectedCode = key,
                                Message = "Here’s the solution for your error:",
                                Data = result?.Select(x => x.Remedy)
                            });
                        }
                    }

                    */
        
        /* var result = await _masterRepo.GetBitAddressRemedyWithManualAsync(key);

                     if (result == null || !result.Any())
                     {
                         return Ok(new
                         {
                             Status = false,
                             Message = "",
                             Data = new[] { $"I found the error code '{key}', but no remedy is available yet. Please contact support." }
                         });
                     }

                     return Ok(new
                     {
                         Status = true,
                         IsChat = false,
                         DetectedCode = key,
                         Message = "Here’s the solution for your error:",
                         Data = result.Select(x => x.Remedy)
                     });*/
        
        /*
                }
        */
        private string? GetBasicChatResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            input = input.ToLower().Trim();

            var greetings = new[] { "hi", "hello", "hey", "hii", "good morning", "good evening" };
            var howAreYou = new[] { "how are you", "how r u", "how are you doing" };
            var thanks = new[] { "thanks", "thank you", "thx" };
            var bye = new[] { "bye", "goodbye", "see you" };

            if (greetings.Any(g => input.Contains(g)))
                return "Hello 👋 I'm your support assistant. Please share the error code and I’ll help you fix it.";

            if (howAreYou.Any(q => input.Contains(q)))
                return "I'm doing great 😊 Ready to help you solve any system errors!";

            if (thanks.Any(t => input.Contains(t)))
                return "You're welcome! Let me know if you face any other issues.";

            if (bye.Any(b => input.Contains(b)))
                return "Goodbye 👋 If you get any errors later, I’m here to help!";

            return null;
        }

        /* public async Task<IActionResult> SolveError(string input)
         {
             // simple extraction using regex
             var match = Regex.Match(input, @"(\[[A-Za-z0-9]+\]M?\d+|M\d{3,6}|\b\d{3}\b)");

             if (!match.Success)
             return Ok(new { Status = false, Message = "No error code found in the text." });


             string key = match.Value;


             // Call your existing API logic
             var result = await _masterRepo.GetBitAddressRemedyWithManualAsync(key);

             if (result == null || !result.Any())
                 return Ok("No remedy found");

             return Ok(new { Status = true, Data = result.Select(x => x.Remedy),Message="Data retrieved successfully"});
         }*/

        [HttpPost]
        public async Task<IActionResult> AddErrorLog(ErrorLogDTO data)
        {
            //  data.CreatedOn = DateTime.UtcNow;
            var modeldata = new SolutionHistory()
            { 
              BitAddress = data.BitAddress,
              CategoryId = data.CategoryId,
              IsExistingSolution=true,
              CreatedOn = DateTime.UtcNow,
              Description = data.Description,
              IsOpen=true

            };
             await _masterRepo.AddSolutionHistoryAsync(modeldata);
            return Ok(new {Status=true, Data=data, Message="Data added successfully"});
        }

        /* [HttpGet]
         public IActionResult DownloadPartMasterTemplate()
         {
             var templatePath = _masterRepo.GeneratePartMasterTemplate();
             if (string.IsNullOrEmpty(templatePath) || !System.IO.File.Exists(templatePath))
                 return Ok(new { Status = false, Message = "Failed to generate template" });

             var fileBytes = System.IO.File.ReadAllBytes(templatePath);
             var fileName = "PartMaster_Template.xlsx";

             return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
         }*/
        public record BulkUploadPartMasterDto(IFormFile File);

        [HttpPost]
        public async Task<IActionResult> BulkUploadPartMaster([FromForm]BulkUploadPartMasterDto file)
        {
            if (file.File == null || file.File.Length == 0)
                return Ok(new { Status = false, Message = "No file uploaded" });

            if (!file.File.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return Ok(new { Status = false, Message = "Only .xlsx files are supported" });

            try
            {
                var result = await _masterRepo.BulkUploadPartMaster(file.File);

                if (result.FailureCount > 0)
                {
                    return Ok(new
                    {
                        Status = result.SuccessCount > 0,
                        Data = result,
                        Message = $"Upload completed with {result.SuccessCount} successful and {result.FailureCount} failed records"
                    });
                }

                return Ok(new
                {
                    Status = true,
                    Data = result,
                    Message = $"Successfully uploaded {result.SuccessCount} parts"
                });
            }
            catch (Exception ex)
            {
                return Ok(new { Status = false, Message = $"Upload failed: {ex.Message}" });
            }
        }
    }
}
    