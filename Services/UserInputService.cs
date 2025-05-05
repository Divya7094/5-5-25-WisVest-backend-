// using WisVestAPI.Models.DTOs;
// using WisVestAPI.Services.Interfaces;
// using System.Text.Json;

// namespace WisVestAPI.Services
// {
//     public class UserInputService : IUserInputService
//     {
//         private readonly IAllocationService _allocationService;

//         public UserInputService(IAllocationService allocationService)
//         {
//             _allocationService = allocationService;
//         }

//         public async Task<AllocationResultDTO> HandleUserInput(UserInputDTO input)
//         {
//             // Validate the input
//             if (input == null)
//             {
//                 throw new ArgumentNullException(nameof(input), "User input cannot be null.");
//             }

//             if (string.IsNullOrWhiteSpace(input.RiskTolerance) || 
//                 string.IsNullOrWhiteSpace(input.InvestmentHorizon) || 
//                 string.IsNullOrWhiteSpace(input.Goal))
//             {
//                 throw new ArgumentException("RiskTolerance, InvestmentHorizon, and Goal cannot be null or empty.");
//             }

//             // Calculate the allocation based on user input
//             var allocationDictionary = await _allocationService.CalculateFinalAllocation(input);

//             if (allocationDictionary == null)
//             {
//                 throw new InvalidOperationException("Allocation calculation failed.");
//             }

//             // Map the allocation to a DTO
//             var result = new AllocationResultDTO
//             {
//                 Equity = allocationDictionary.GetValueOrDefault("equity", 0.0),
//                 FixedIncome = allocationDictionary.GetValueOrDefault("fixedIncome", 0.0),
//                 Commodities = allocationDictionary.GetValueOrDefault("commodities", 0.0),
//                 Cash = allocationDictionary.GetValueOrDefault("cash", 0.0),
//                 RealEstate = allocationDictionary.GetValueOrDefault("realEstate", 0.0)
//             };

//             // (Optional) Log the serialized result for debugging
//             var jsonResult = JsonSerializer.Serialize(result);
//             Console.WriteLine($"Serialized Result: {jsonResult}");

//             return result;
//         }
//     }
// }
// using WisVestAPI.Models.DTOs;
// using WisVestAPI.Services.Interfaces;
// using System.Text.Json;

// namespace WisVestAPI.Services
// {
//     public class UserInputService : IUserInputService
//     {
//         private readonly IAllocationService _allocationService;

//         public UserInputService(IAllocationService allocationService)
//         {
//             _allocationService = allocationService;
//         }
//         public async Task<AllocationResultDTO> HandleUserInput(UserInputDTO input)
// {
//     if (input == null)
//         throw new ArgumentNullException(nameof(input), "User input cannot be null.");

//     Console.WriteLine($"Received input: {JsonSerializer.Serialize(input)}");

//     var allocationDictionary = await _allocationService.CalculateFinalAllocation(input);

//     if (allocationDictionary == null)
//         throw new InvalidOperationException("Allocation calculation failed.");

//     var result = new AllocationResultDTO
//     {
//         Equity = allocationDictionary.GetValueOrDefault("equity", 0.0),
//         FixedIncome = allocationDictionary.GetValueOrDefault("fixedIncome", 0.0),
//         Commodities = allocationDictionary.GetValueOrDefault("commodities", 0.0),
//         Cash = allocationDictionary.GetValueOrDefault("cash", 0.0),
//         RealEstate = allocationDictionary.GetValueOrDefault("realEstate", 0.0)
//     };

//     Console.WriteLine($"Calculated allocation: {JsonSerializer.Serialize(result)}");

//     return result;
// }
//         }
//     }

// public async Task<AllocationResultDTO> HandleUserInput(UserInputDTO input)
// {
//     if (input == null)
//         throw new ArgumentNullException(nameof(input), "User input cannot be null.");

//     Console.WriteLine($"Received input: {JsonSerializer.Serialize(input)}");

//     var allocationDictionary = await _allocationService.CalculateFinalAllocation(input);

//     if (allocationDictionary == null)
//         throw new InvalidOperationException("Allocation calculation failed.");

//     var result = new AllocationResultDTO();

//     foreach (var allocation in allocationDictionary)
//     {
//         result.Assets[allocation.Key] = new AssetAllocation
//         {
//             Percentage = allocation.Value,
//             SubAssets = new Dictionary<string, double>() // Add sub-assets if applicable
//         };
//     }

//     Console.WriteLine($"Calculated allocation: {JsonSerializer.Serialize(result)}");

//     return result;
// }

// using WisVestAPI.Services.Interfaces; // For IUserInputService and IAllocationService
// using WisVestAPI.Models.DTOs;         // For UserInputDTO and AllocationResultDTO
// using System.Text.Json;               // For JsonSerializer

// namespace WisVestAPI.Services
// {
//     public class UserInputService : IUserInputService
//     {
//         private readonly IAllocationService _allocationService;

//         public UserInputService(IAllocationService allocationService)
//         {
//             _allocationService = allocationService;
//         }

//         //         public async Task<AllocationResultDTO> HandleUserInput(UserInputDTO input)
//         // {
//         //     if (input == null)
//         //         throw new ArgumentNullException(nameof(input), "User input cannot be null.");
        
//         //     Console.WriteLine($"Received input: {JsonSerializer.Serialize(input)}");
        
//         //     var allocationDictionary = await _allocationService.CalculateFinalAllocation(input);
        
//         //     if (allocationDictionary == null)
//         //         throw new InvalidOperationException("Allocation calculation failed.");
        
//         //     var result = new AllocationResultDTO();
        
//         //     foreach (var allocation in allocationDictionary)
//         //     {
//         //         // Safely convert the object to double
//         //         if (allocation.Value is double percentage)
//         //         {
//         //             result.Assets[allocation.Key] = new AssetAllocation
//         //             {
//         //                 Percentage = percentage,
//         //                 SubAssets = new Dictionary<string, double>() // Add sub-assets if applicable
//         //             };
//         //         }
//         //         else
//         //         {
//         //             throw new InvalidCastException($"Value for key '{allocation.Key}' is not a valid double.");
//         //         }
//         //     }
        
//         //     Console.WriteLine($"Calculated allocation: {JsonSerializer.Serialize(result)}");
        
//         //     return result;
//         // }
//         public async Task<AllocationResultDTO> HandleUserInput(UserInputDTO input)
// {
//     if (input == null)
//         throw new ArgumentNullException(nameof(input), "User input cannot be null.");

//     Console.WriteLine($"Received input: {JsonSerializer.Serialize(input)}");

//     var allocationDictionary = await _allocationService.CalculateFinalAllocation(input);

//     if (allocationDictionary == null)
//         throw new InvalidOperationException("Allocation calculation failed.");

//     var result = new AllocationResultDTO();

//     // foreach (var allocation in allocationDictionary)
//     // {
//     //     result.Assets[allocation.Key] = new AssetAllocation
//     //     {
//     //         Percentage = allocation.Value.Values.Sum(), // Sum of all sub-assets
//     //         SubAssets = allocation.Value // Assign sub-assets directly
//     //     };
//     // }

//     foreach (var allocation in allocationDictionary)
//     {
//         // Explicitly cast allocation.Value to Dictionary<string, double>
//         if (allocation.Value is Dictionary<string, double> subAssets)
//         {
//             result.Assets[allocation.Key] = new AssetAllocation
//             {
//                 Percentage = subAssets.Values.Sum(), // Sum of all sub-assets
//                 SubAssets = subAssets // Assign sub-assets directly
//             };
//         }
//         else
//         {
//             throw new InvalidCastException($"Value for key '{allocation.Key}' is not a valid Dictionary<string, double>.");
//         }
//     }

//     Console.WriteLine($"Calculated allocation: {JsonSerializer.Serialize(result)}");

//     return result;
// }
//     }
// }

using WisVestAPI.Services.Interfaces;
using WisVestAPI.Models.DTOs;
using System.Text.Json;

namespace WisVestAPI.Services
{
    public class UserInputService : IUserInputService
    {
        private readonly IAllocationService _allocationService;
        private readonly ILogger<UserInputService> _logger;

        public UserInputService(IAllocationService allocationService, ILogger<UserInputService> logger)
        {
            _allocationService = allocationService;
            _logger = logger;
        }

        public async Task<AllocationResultDTO> HandleUserInput(UserInputDTO input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), "User input cannot be null.");

            _logger.LogInformation($"Received input: {JsonSerializer.Serialize(input)}");

            Dictionary<string, Dictionary<string, double>> allocationDictionary;

            try
            {
                allocationDictionary = (await _allocationService.CalculateFinalAllocation(input))
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value as Dictionary<string, double> ?? throw new InvalidCastException($"Value for key '{kvp.Key}' is not a valid Dictionary<string, double>.")
                    );
                if (allocationDictionary == null)
                    throw new InvalidOperationException("Allocation calculation failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during allocation calculation: {ex.Message}");
                throw;
            }

            var result = new AllocationResultDTO();

            foreach (var allocation in allocationDictionary)
            {
                if (allocation.Value is Dictionary<string, double> subAssets)
                {
                    result.Assets[allocation.Key] = new AssetAllocation
                    {
                        Percentage = subAssets.Values.Sum(), // Sum of all sub-assets
                        SubAssets = subAssets // Assign sub-assets directly
                    };
                }
                else
                {
                    throw new InvalidCastException($"Value for key '{allocation.Key}' is not a valid Dictionary<string, double>.");
                }
            }

            _logger.LogInformation($"Calculated allocation: {JsonSerializer.Serialize(result)}");

            return result;
        }
    }
}
