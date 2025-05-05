// using Microsoft.AspNetCore.Mvc;
// using WisVestAPI.Models.DTOs;
// using WisVestAPI.Services.Interfaces;
// using System.Threading.Tasks;

// namespace WisVestAPI.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class AllocationController : ControllerBase
//     {
//         private readonly IAllocationService _allocationService;

//         public AllocationController(IAllocationService allocationService)
//         {
//             _allocationService = allocationService;
//         }

//         // GET: api/Allocation/compute
//         [HttpPost("compute")]
//         public async Task<ActionResult<AllocationResultDTO>> GetAllocation([FromBody] UserInputDTO input)
//         {
//             var allocation = await _allocationService.CalculateFinalAllocation(input);

//             if (allocation == null)
//             {
//                 return NotFound("Allocation could not be computed.");
//             }

//             // var result = new AllocationResultDTO
//             // {
//             //     Equity = allocation["equity"],
//             //     FixedIncome = allocation["fixedIncome"],
//             //     Commodities = allocation["commodities"],
//             //     Cash = allocation["cash"],
//             //     RealEstate = allocation["realEstate"]
//             // };

//             return Ok(allocation);
//         }
//     }
// }

// using Microsoft.AspNetCore.Mvc;
// using WisVestAPI.Models.DTOs;
// using WisVestAPI.Services.Interfaces;
// using System.Threading.Tasks;

// namespace WisVestAPI.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class AllocationController : ControllerBase
//     {
//         private readonly IAllocationService _allocationService;

//         public AllocationController(IAllocationService allocationService)
//         {
//             _allocationService = allocationService;
//         }

//         // POST: api/Allocation/compute
//         [HttpPost("compute")]
//         public async Task<ActionResult<AllocationResultDTO>> GetAllocation([FromBody] UserInputDTO input)
//         {
//             var allocation = await _allocationService.CalculateFinalAllocation(input);

//             // Validate allocation
//             if (allocation == null || allocation.Values.Sum() == 0)
//             {
//                 return NotFound("Allocation could not be computed.");
//             }

//             // Map the allocation dictionary to the DTO
//             var result = new AllocationResultDTO
//             {
//                 Equity = allocation.ContainsKey("equity") ? allocation["equity"] : 0,
//                 FixedIncome = allocation.ContainsKey("fixedIncome") ? allocation["fixedIncome"] : 0,
//                 Commodities = allocation.ContainsKey("commodities") ? allocation["commodities"] : 0,
//                 Cash = allocation.ContainsKey("cash") ? allocation["cash"] : 0,
//                 RealEstate = allocation.ContainsKey("realEstate") ? allocation["realEstate"] : 0
//             };

//             return Ok(result);
//         }
//     }
// }
// using Microsoft.AspNetCore.Mvc;
// using WisVestAPI.Models.DTOs;
// using WisVestAPI.Services.Interfaces;
// using System.Threading.Tasks;
// using System.Text.Json;
// using System.Linq;
// using System.Collections.Generic;
 
// namespace WisVestAPI.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class AllocationController : ControllerBase
//     {
//         private readonly IAllocationService _allocationService;
 
//         public AllocationController(IAllocationService allocationService)
//         {
//             _allocationService = allocationService;
//         }
 
//         // POST: api/Allocation/compute
//         [HttpPost("compute")]
//         public async Task<ActionResult<AllocationResultDTO>> GetAllocation([FromBody] UserInputDTO input)
//         {
//             var fullAllocationResult = await _allocationService.CalculateFinalAllocation(input);
 
//             // Validate allocation
//             if (fullAllocationResult == null || !fullAllocationResult.ContainsKey("assets"))
//             {
//                 return NotFound("Allocation could not be computed or formatted correctly.");
//             }
 
//             var assetsData = fullAllocationResult["assets"] as Dictionary<string, object>;
//             if (assetsData == null)
//             {
//                 return StatusCode(500, "Error: Final allocation data format is incorrect.");
//             }
 
//             var result = new AllocationResultDTO { Assets = new Dictionary<string, AssetAllocation>() };
 
//             foreach (var assetPair in assetsData)
//             {
//                 var assetName = assetPair.Key;
//                 if (assetPair.Value is Dictionary<string, object> assetDetails)
//                 {
//                     if (assetDetails.TryGetValue("percentage", out var percentageObj) &&
//                         assetDetails.TryGetValue("subAssets", out var subAssetsObj) &&
//                         percentageObj is double percentage &&
//                         subAssetsObj is Dictionary<string, double> subAssets)
//                     {
//                         result.Assets[assetName] = new AssetAllocation
//                         {
//                             Percentage = percentage,
//                             SubAssets = subAssets
//                         };
//                     }
//                 }
//             }
 
//             return Ok(result);
//         }
//     }
// }

using Microsoft.AspNetCore.Mvc;
using WisVestAPI.Models.DTOs;
using WisVestAPI.Services.Interfaces;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

namespace WisVestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AllocationController : ControllerBase
    {
        private readonly IAllocationService _allocationService;

        public AllocationController(IAllocationService allocationService)
        {
            _allocationService = allocationService;
        }

        // POST: api/Allocation/compute
        [HttpPost("compute")]
        public async Task<ActionResult<AllocationResultDTO>> GetAllocation([FromBody] UserInputDTO input)
        {
            if (input == null)
            {
                return BadRequest("User input cannot be null.");
            }

            var fullAllocationResult = await _allocationService.CalculateFinalAllocation(input);

            // Validate allocation
            if (fullAllocationResult == null || !fullAllocationResult.ContainsKey("assets"))
            {
                return BadRequest("Allocation could not be computed or formatted correctly.");
            }

            var assetsData = fullAllocationResult["assets"] as Dictionary<string, object>;
            if (assetsData == null)
            {
                return StatusCode(500, "Error: Final allocation data format is incorrect.");
            }

            var result = new AllocationResultDTO { Assets = new Dictionary<string, AssetAllocation>() };

            foreach (var assetPair in assetsData)
            {
                var assetName = assetPair.Key;
                if (assetPair.Value is Dictionary<string, object> assetDetails)
                {
                    var assetAllocation = ParseAssetDetails(assetDetails);
                    if (assetAllocation != null)
                    {
                        result.Assets[assetName] = assetAllocation;
                    }
                }
            }

            return Ok(result);
        }

        private AssetAllocation? ParseAssetDetails(Dictionary<string, object> assetDetails)
        {
            if (assetDetails.TryGetValue("percentage", out var percentageObj) &&
                assetDetails.TryGetValue("subAssets", out var subAssetsObj) &&
                percentageObj is double percentage &&
                subAssetsObj is Dictionary<string, double> subAssets)
            {
                return new AssetAllocation
                {
                    Percentage = percentage,
                    SubAssets = subAssets
                };
            }
            return null;
        }
    }
}
