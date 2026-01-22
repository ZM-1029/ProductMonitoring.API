using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductMonitoring.API.Repository;

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
            var bitAddressList=await _masterRepo.GetAllBitAddressByKeyAsync(key);
            if(!bitAddressList.Any()) return Ok(new { Status = false, Message = "No data found!" });

            return Ok(new {Status=true, Data= bitAddressList, Message="Data retrieved successfully!"});
        }

        [HttpGet]
        public async Task<IActionResult> GetBitAddress(string key)
        {
            var bitAddress = await _masterRepo.GetBitAddressByKeyAsync(key);
            if (bitAddress == null) return Ok(new { Status = false, Message = "No data found!" });

            return Ok(new { Status = true, Data = bitAddress, Message = "Data retrieved successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetBitAddressCauseList(string key)
        {
            var bitAddressCauseList = await _masterRepo.GetBitAddressCauseAsync(key);
            if (bitAddressCauseList==null || !bitAddressCauseList.Any()) return Ok(new { Status = false, Message = "No data found!" });

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
        public async Task<IActionResult> GetBitAddressErrorManuals(string key)
        {
            var bitAddressErrorManualList = await _masterRepo.GetBitAddressManualAsync(key);
            if (bitAddressErrorManualList == null || !bitAddressErrorManualList.Any()) return Ok(new { Status = false, Message = "No data found!" });

            return Ok(new { Status = true, Data = bitAddressErrorManualList, Message = "Data retrieved successfully!" });
        }
    }
}
