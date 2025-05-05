// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Text.Json;
// using System.Threading.Tasks;
// using WisVestAPI.Models.DTOs;

// using System.Text.Json.Serialization;

// namespace WisVestAPI.Services
// {
//     public class ProductAllocationService
//     {
//         private readonly string _productJsonFilePath = "Repositories/Matrix/product_json.json";

//         // Option 1: Entry point that takes DTO, converts, and returns full allocation
//         public async Task<Dictionary<string, Dictionary<string, Dictionary<string, double>>>> GetFinalProductAllocations(SubAllocationResultDTO subAllocationResultDTO)
//         {
//             var subAllocationMatrix = subAllocationResultDTO.ToDictionary();
//             return await CalculateProductAllocations(subAllocationMatrix);
//         }

//         // Option 2: Original suballocation logic using Product.PercentageSplit
//         public async Task ApplySubAllocations(SubAllocationResultDTO subAllocationResultDTO)
//         {
//             var subAllocationMatrix = subAllocationResultDTO.ToDictionary();
//             var productData = await LoadProductDataAsync();

//             foreach (var assetClass in subAllocationMatrix.Keys)
//             {
//                 var subAssetClasses = subAllocationMatrix[assetClass];

//                 foreach (var subAssetClass in subAssetClasses.Keys)
//                 {
//                     var percentageSplit = subAssetClasses[subAssetClass];
//                     var products = GetProductsForAssetClass(productData, assetClass, subAssetClass);

//                     if (products == null || products.Count == 0)
//                     {
//                         Console.WriteLine($"No products found for sub-asset class: {subAssetClass} in asset class: {assetClass}");
//                         continue;
//                     }

//                     // Debug: Log all products and their annual returns
//                     Console.WriteLine($"Processing sub-asset class: {subAssetClass} in asset class: {assetClass}");
//                     foreach (var product in products)
//                     {
//                         Console.WriteLine($"Product: {product.ProductName}, Annual Return: {product.AnnualReturn}");
//                     }

//                     double totalReturns = products.Sum(p => p.AnnualReturn);
//                     if (totalReturns <= 0)
//                     {
//                         Console.WriteLine($"Total return is zero or negative for sub-asset {subAssetClass}, skipping allocation.");
//                         continue;
//                     }

//                     foreach (var product in products)
//                     {
//                         var splitRatio = product.AnnualReturn / totalReturns;
//                         product.PercentageSplit = Math.Round(splitRatio * percentageSplit, 2);
//                     }

//                     Console.WriteLine($"Updated percentage splits for sub-asset class: {subAssetClass} in asset class: {assetClass}");
//                 }
//             }

//             Console.WriteLine("Final Product Allocations:");
//             Console.WriteLine(JsonSerializer.Serialize(productData));
//         }

//         // Returns final product-level allocation in clean dictionary format
//         public async Task<Dictionary<string, Dictionary<string, Dictionary<string, double>>>> CalculateProductAllocations(
//             Dictionary<string, Dictionary<string, double>> subAllocationResult)
//         {
//             var productData = await LoadProductDataAsync();

//             var productAllocations = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();

//             foreach (var assetClass in subAllocationResult.Keys)
//             {
//                 var subAssetClasses = subAllocationResult[assetClass];

//                 foreach (var subAssetClass in subAssetClasses.Keys)
//                 {
//                     var percentageSplit = subAssetClasses[subAssetClass];
//                     var products = GetProductsForAssetClass(productData, assetClass, subAssetClass);

//                     if (products == null || products.Count == 0)
//                     {
//                         Console.WriteLine($"No products found for sub-asset class: {subAssetClass} in asset class: {assetClass}");
//                         continue;
//                     }

//                     double totalReturns = products.Sum(p => p.AnnualReturn);
//                     if (totalReturns <= 0)
//                     {
//                         Console.WriteLine($"Total return is zero or negative for sub-asset {subAssetClass}, skipping allocation.");
//                         continue;
//                     }

//                     var productSplit = new Dictionary<string, double>();
//                     foreach (var product in products)
//                     {
//                         var splitRatio = product.AnnualReturn / totalReturns;
//                         var allocation = Math.Round(splitRatio * percentageSplit, 2);
//                         productSplit[product.ProductName] = allocation;
//                     }

//                     if (!productAllocations.ContainsKey(assetClass))
//                     {
//                         productAllocations[assetClass] = new Dictionary<string, Dictionary<string, double>>();
//                     }

//                     productAllocations[assetClass][subAssetClass] = productSplit;
//                 }
//             }

//             Console.WriteLine($"Final Product Allocations: {JsonSerializer.Serialize(productAllocations)}");
//             return productAllocations;
//         }

//         private async Task<Dictionary<string, Dictionary<string, List<Product>>>> LoadProductDataAsync()
//         {
//             if (!File.Exists(_productJsonFilePath))
//                 throw new FileNotFoundException($"Product JSON file not found at {_productJsonFilePath}");

//             var json = await File.ReadAllTextAsync(_productJsonFilePath);
//             Console.WriteLine($"Raw JSON Content: {json}");

//             var productData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<Product>>>>(json);
//             if (productData == null)
//                 throw new InvalidOperationException("Failed to deserialize product data. Ensure the JSON structure is correct.");

//             // Debug: Verify deserialized data
//             Console.WriteLine("Deserialized Product Data:");
//             foreach (var assetClass in productData.Keys)
//             {
//                 foreach (var subAssetClass in productData[assetClass].Keys)
//                 {
//                     Console.WriteLine($"Asset Class: {assetClass}, Sub-Asset Class: {subAssetClass}");
//                     foreach (var product in productData[assetClass][subAssetClass])
//                     {
//                         Console.WriteLine($"- Product: {product.ProductName}, Annual Return: {product.AnnualReturn}");
//                     }
//                 }
//             }

//             // Normalize asset class keys but retain exact sub-asset names
//             return productData.ToDictionary(
//                 assetClass => NormalizeKey(assetClass.Key),  // Normalize asset class keys
//                 assetClass => assetClass.Value.ToDictionary(
//                     subAssetClass => subAssetClass.Key,  // No normalization for sub-assets
//                     subAssetClass => subAssetClass.Value
//                 )
//             );
//         }

//         private List<Product> GetProductsForAssetClass(
//             Dictionary<string, Dictionary<string, List<Product>>> productData,
//             string assetClass,
//             string subAssetClass)
//         {
//             assetClass = NormalizeKey(assetClass);  // Normalize asset class key
//             subAssetClass = subAssetClass.Trim();  // Keep sub-asset class name exact

//             // Debug: Log the normalized keys being checked
//             Console.WriteLine($"Checking for products in Asset Class: {assetClass}, Sub-Asset Class: {subAssetClass}");

//             if (!productData.ContainsKey(assetClass) || !productData[assetClass].ContainsKey(subAssetClass))
//             {
//                 Console.WriteLine($"No products found for asset class '{assetClass}' and sub-asset class '{subAssetClass}'.");
//                 return new List<Product>();
//             }

//             return productData[assetClass][subAssetClass];
//         }

//         private string NormalizeKey(string input)
//         {
//             return input.Trim().ToLower().Replace(" ", "");
//         }
//     }

//     public class Product
// {
//     [JsonPropertyName("product_name")]
//     public string ProductName { get; set; }

//     [JsonPropertyName("annual_return")]
//     public double AnnualReturn { get; set; }

//     [JsonPropertyName("asset_class")]
//     public string AssetClass { get; set; }

//     [JsonPropertyName("sub_asset_class")]
//     public string SubAssetClass { get; set; }

//     [JsonPropertyName("liquidity")]
//     public string Liquidity { get; set; }

//     [JsonPropertyName("pros")]
//     public List<string> Pros { get; set; }

//     [JsonPropertyName("cons")]
//     public List<string> Cons { get; set; }

//     [JsonPropertyName("risk_level")]
//     public string RiskLevel { get; set; }

//     [JsonPropertyName("percentage_split")]
//     public double PercentageSplit { get; set; }
// }

//     public class SubAllocationResultDTO
//     {
//         public Dictionary<string, AssetDTO> Assets { get; set; }

//         public Dictionary<string, Dictionary<string, double>> ToDictionary()
//         {
//             var result = new Dictionary<string, Dictionary<string, double>>();

//             foreach (var assetClass in Assets)
//             {
//                 var normalizedAssetClass = NormalizeKey(assetClass.Key);  // Normalize asset class key
//                 var subAllocations = assetClass.Value.SubAssets.ToDictionary(
//                     subAsset => subAsset.Key,  // Keep sub-asset names intact
//                     subAsset => subAsset.Value
//                 );

//                 result[normalizedAssetClass] = subAllocations;
//             }

//             return result;
//         }

//         private string NormalizeKey(string input)
//         {
//             return input.Trim().ToLower().Replace(" ", "");
//         }
//     }

//     public class AssetDTO
//     {
//         public double Percentage { get; set; }
//         public Dictionary<string, double> SubAssets { get; set; }
//     }
// }


// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Text.Json;
// using System.Threading.Tasks;
// using WisVestAPI.Models.DTOs;
// using System.Text.Json.Serialization;

// namespace WisVestAPI.Services
// {
//     public class ProductAllocationService
//     {
//         private readonly string _productJsonFilePath = "Repositories/Matrix/product_json.json";

//         // Option 1: Entry point that takes DTO, converts, and returns full allocation
//         public async Task<Dictionary<string, Dictionary<string, Dictionary<string, Product>>>> GetFinalProductAllocations(SubAllocationResultDTO subAllocationResultDTO)
//         {
//             var subAllocationMatrix = subAllocationResultDTO.ToDictionary();
//             return await CalculateProductAllocations(subAllocationMatrix);
//         }

//         // Option 2: Original suballocation logic using Product.PercentageSplit
//         public async Task ApplySubAllocations(SubAllocationResultDTO subAllocationResultDTO)
//         {
//             var subAllocationMatrix = subAllocationResultDTO.ToDictionary();
//             var productData = await LoadProductDataAsync();

//             foreach (var assetClass in subAllocationMatrix.Keys)
//             {
//                 var subAssetClasses = subAllocationMatrix[assetClass];

//                 foreach (var subAssetClass in subAssetClasses.Keys)
//                 {
//                     var percentageSplit = subAssetClasses[subAssetClass];
//                     var products = GetProductsForAssetClass(productData, assetClass, subAssetClass);

//                     if (products == null || products.Count == 0)
//                     {
//                         Console.WriteLine($"No products found for sub-asset class: {subAssetClass} in asset class: {assetClass}");
//                         continue;
//                     }

//                     // Debug: Log all products and their annual returns
//                     Console.WriteLine($"Processing sub-asset class: {subAssetClass} in asset class: {assetClass}");
//                     foreach (var product in products)
//                     {
//                         Console.WriteLine($"Product: {product.ProductName}, Annual Return: {product.AnnualReturn}");
//                     }

//                     double totalReturns = products.Sum(p => p.AnnualReturn);
//                     if (totalReturns <= 0)
//                     {
//                         Console.WriteLine($"Total return is zero or negative for sub-asset {subAssetClass}, skipping allocation.");
//                         continue;
//                     }

//                     foreach (var product in products)
//                     {
//                         var splitRatio = product.AnnualReturn / totalReturns;
//                         product.PercentageSplit = Math.Round(splitRatio * percentageSplit, 2);
//                     }

//                     Console.WriteLine($"Updated percentage splits for sub-asset class: {subAssetClass} in asset class: {assetClass}");
//                 }
//             }

//             Console.WriteLine("Final Product Allocations:");
//             Console.WriteLine(JsonSerializer.Serialize(productData));
//         }

//         // Returns final product-level allocation in clean dictionary format
//         public async Task<Dictionary<string, Dictionary<string, Dictionary<string, Product>>>> CalculateProductAllocations(
//             Dictionary<string, Dictionary<string, double>> subAllocationResult)
//         {
//             var productData = await LoadProductDataAsync();

//             var productAllocations = new Dictionary<string, Dictionary<string, Dictionary<string, Product>>>();

//             foreach (var assetClass in subAllocationResult.Keys)
//             {
//                 var subAssetClasses = subAllocationResult[assetClass];

//                 foreach (var subAssetClass in subAssetClasses.Keys)
//                 {
//                     var percentageSplit = subAssetClasses[subAssetClass];
//                     var products = GetProductsForAssetClass(productData, assetClass, subAssetClass);

//                     if (products == null || products.Count == 0)
//                     {
//                         Console.WriteLine($"No products found for sub-asset class: {subAssetClass} in asset class: {assetClass}");
//                         continue;
//                     }

//                     double totalReturns = products.Sum(p => p.AnnualReturn);
//                     if (totalReturns <= 0)
//                     {
//                         Console.WriteLine($"Total return is zero or negative for sub-asset {subAssetClass}, skipping allocation.");
//                         continue;
//                     }

//                     var productSplit = new Dictionary<string, Product>();
//                     foreach (var product in products)
//                     {
//                         var splitRatio = product.AnnualReturn / totalReturns;
//                         var allocation = Math.Round(splitRatio * percentageSplit, 2);
//                         product.PercentageSplit = allocation;  // Update the percentage split in the product
//                         productSplit[product.ProductName] = product; // Store the whole product object
//                     }

//                     if (!productAllocations.ContainsKey(assetClass))
//                     {
//                         productAllocations[assetClass] = new Dictionary<string, Dictionary<string, Product>>();
//                     }

//                     productAllocations[assetClass][subAssetClass] = productSplit;
//                 }
//             }

//             Console.WriteLine($"Final Product Allocations: {JsonSerializer.Serialize(productAllocations)}");
//             return productAllocations;
//         }

//         private async Task<Dictionary<string, Dictionary<string, List<Product>>>> LoadProductDataAsync()
//         {
//             if (!File.Exists(_productJsonFilePath))
//                 throw new FileNotFoundException($"Product JSON file not found at {_productJsonFilePath}");

//             var json = await File.ReadAllTextAsync(_productJsonFilePath);
//             Console.WriteLine($"Raw JSON Content: {json}");

//             var productData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<Product>>>>(json);
//             if (productData == null)
//                 throw new InvalidOperationException("Failed to deserialize product data. Ensure the JSON structure is correct.");

//             // Debug: Verify deserialized data
//             Console.WriteLine("Deserialized Product Data:");
//             foreach (var assetClass in productData.Keys)
//             {
//                 foreach (var subAssetClass in productData[assetClass].Keys)
//                 {
//                     Console.WriteLine($"Asset Class: {assetClass}, Sub-Asset Class: {subAssetClass}");
//                     foreach (var product in productData[assetClass][subAssetClass])
//                     {
//                         Console.WriteLine($"- Product: {product.ProductName}, Annual Return: {product.AnnualReturn}");
//                     }
//                 }
//             }

//             // Normalize asset class keys but retain exact sub-asset names
//             return productData.ToDictionary(
//                 assetClass => NormalizeKey(assetClass.Key),  // Normalize asset class keys
//                 assetClass => assetClass.Value.ToDictionary(
//                     subAssetClass => subAssetClass.Key,  // No normalization for sub-assets
//                     subAssetClass => subAssetClass.Value
//                 )
//             );
//         }

//         private List<Product> GetProductsForAssetClass(
//             Dictionary<string, Dictionary<string, List<Product>>> productData,
//             string assetClass,
//             string subAssetClass)
//         {
//             assetClass = NormalizeKey(assetClass);  // Normalize asset class key
//             subAssetClass = subAssetClass.Trim();  // Keep sub-asset class name exact

//             // Debug: Log the normalized keys being checked
//             Console.WriteLine($"Checking for products in Asset Class: {assetClass}, Sub-Asset Class: {subAssetClass}");

//             if (!productData.ContainsKey(assetClass) || !productData[assetClass].ContainsKey(subAssetClass))
//             {
//                 Console.WriteLine($"No products found for asset class '{assetClass}' and sub-asset class '{subAssetClass}'.");
//                 return new List<Product>();
//             }

//             return productData[assetClass][subAssetClass];
//         }

//         private string NormalizeKey(string input)
//         {
//             return input.Trim().ToLower().Replace(" ", "");
//         }
//     }

//     public class Product
//     {
//         [JsonPropertyName("product_name")]
//         public string ProductName { get; set; }

//         [JsonPropertyName("annual_return")]
//         public double AnnualReturn { get; set; }

//         [JsonPropertyName("asset_class")]
//         public string AssetClass { get; set; }

//         [JsonPropertyName("sub_asset_class")]
//         public string SubAssetClass { get; set; }

//         [JsonPropertyName("liquidity")]
//         public string Liquidity { get; set; }

//         [JsonPropertyName("pros")]
//         public List<string> Pros { get; set; }

//         [JsonPropertyName("cons")]
//         public List<string> Cons { get; set; }

//         [JsonPropertyName("risk_level")]
//         public string RiskLevel { get; set; }

//         [JsonPropertyName("percentage_split")]
//         public double PercentageSplit { get; set; }
//     }

//     public class SubAllocationResultDTO
//     {
//         public Dictionary<string, AssetDTO> Assets { get; set; }

//         public Dictionary<string, Dictionary<string, double>> ToDictionary()
//         {
//             var result = new Dictionary<string, Dictionary<string, double>>();

//             foreach (var assetClass in Assets)
//             {
//                 var normalizedAssetClass = NormalizeKey(assetClass.Key);  // Normalize asset class key
//                 var subAllocations = assetClass.Value.SubAssets.ToDictionary(
//                     subAsset => subAsset.Key,  // Keep sub-asset names intact
//                     subAsset => subAsset.Value
//                 );

//                 result[normalizedAssetClass] = subAllocations;
//             }

//             return result;
//         }

//         private string NormalizeKey(string input)
//         {
//             return input.Trim().ToLower().Replace(" ", "");
//         }
//     }

//     public class AssetDTO
//     {
//         public double Percentage { get; set; }
//         public Dictionary<string, double> SubAssets { get; set; }
//     }
// }



// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Text.Json;
// using System.Text.Json.Serialization;
// using System.Threading.Tasks;
// using WisVestAPI.Models.DTOs;

// namespace WisVestAPI.Services
// {
//     public class ProductAllocationService
//     {
//         private readonly string _productJsonFilePath = "Repositories/Matrix/product_json.json";
//         private readonly InvestmentAmountService _investmentAmountService;

//         public ProductAllocationService()
//         {
//             _investmentAmountService = new InvestmentAmountService();
//         }

//         public async Task<Dictionary<string, Dictionary<string, Dictionary<string, Product>>>> GetFinalProductAllocations(SubAllocationResultDTO subAllocationResultDTO)
//         {
//             var subAllocationMatrix = subAllocationResultDTO.ToDictionary();
//             return await CalculateProductAllocations(subAllocationMatrix);
//         }

//         public async Task ApplySubAllocations(SubAllocationResultDTO subAllocationResultDTO)
//         {
//             var subAllocationMatrix = subAllocationResultDTO.ToDictionary();
//             var productData = await LoadProductDataAsync();

//             foreach (var assetClass in subAllocationMatrix.Keys)
//             {
//                 var subAssetClasses = subAllocationMatrix[assetClass];

//                 foreach (var subAssetClass in subAssetClasses.Keys)
//                 {
//                     var percentageSplit = subAssetClasses[subAssetClass];
//                     var products = GetProductsForAssetClass(productData, assetClass, subAssetClass);

//                     if (products == null || products.Count == 0)
//                     {
//                         Console.WriteLine($"No products found for sub-asset class: {subAssetClass} in asset class: {assetClass}");
//                         continue;
//                     }

//                     Console.WriteLine($"Processing sub-asset class: {subAssetClass} in asset class: {assetClass}");
//                     foreach (var product in products)
//                         Console.WriteLine($"Product: {product.ProductName}, Annual Return: {product.AnnualReturn}");

//                     double totalReturns = products.Sum(p => p.AnnualReturn);
//                     if (totalReturns <= 0)
//                     {
//                         Console.WriteLine($"Total return is zero or negative for sub-asset {subAssetClass}, skipping allocation.");
//                         continue;
//                     }

//                     foreach (var product in products)
//                     {
//                         var splitRatio = product.AnnualReturn / totalReturns;
//                         product.PercentageSplit = Math.Round(splitRatio * percentageSplit, 2);
//                     }

//                     Console.WriteLine($"Updated percentage splits for sub-asset class: {subAssetClass} in asset class: {assetClass}");
//                 }
//             }

//             Console.WriteLine("Final Product Allocations:");
//             Console.WriteLine(JsonSerializer.Serialize(productData));
//         }

//         public async Task<Dictionary<string, Dictionary<string, Dictionary<string, Product>>>> CalculateProductAllocations(
//             Dictionary<string, Dictionary<string, double>> subAllocationResult,int years)
//         {
//             var productData = await LoadProductDataAsync();
//             var productAllocations = new Dictionary<string, Dictionary<string, Dictionary<string, Product>>>();

//             foreach (var assetClass in subAllocationResult.Keys)
//             {
//                 var subAssetClasses = subAllocationResult[assetClass];

//                 foreach (var subAssetClass in subAssetClasses.Keys)
//                 {
//                     var percentageSplit = subAssetClasses[subAssetClass];
//                     var products = GetProductsForAssetClass(productData, assetClass, subAssetClass);

//                     if (products == null || products.Count == 0)
//                     {
//                         Console.WriteLine($"No products found for sub-asset class: {subAssetClass} in asset class: {assetClass}");
//                         continue;
//                     }

//                     double totalReturns = products.Sum(p => p.AnnualReturn);
//                     if (totalReturns <= 0)
//                     {
//                         Console.WriteLine($"Total return is zero or negative for sub-asset {subAssetClass}, skipping allocation.");
//                         continue;
//                     }

//                     var productSplit = new Dictionary<string, Product>();
//                     foreach (var product in products)
//                     {
//                         var splitRatio = product.AnnualReturn / totalReturns;
//                         var allocation = Math.Round(splitRatio * percentageSplit, 2);
//                         product.PercentageSplit = allocation;
//                         productSplit[product.ProductName] = product;
//                     }

//                     if (!productAllocations.ContainsKey(assetClass))
//                         productAllocations[assetClass] = new Dictionary<string, Dictionary<string, Product>>();

//                     productAllocations[assetClass][subAssetClass] = productSplit;
//                 }
//             }

//             Console.WriteLine($"Final Product Allocations: {JsonSerializer.Serialize(productAllocations)}");
//             return productAllocations;
//         }

//         private async Task<Dictionary<string, Dictionary<string, List<Product>>>> LoadProductDataAsync()
//         {
//             if (!File.Exists(_productJsonFilePath))
//                 throw new FileNotFoundException($"Product JSON file not found at {_productJsonFilePath}");

//             var json = await File.ReadAllTextAsync(_productJsonFilePath);
//             Console.WriteLine($"Raw JSON Content: {json}");

//             var productData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<Product>>>>(json);
//             if (productData == null)
//                 throw new InvalidOperationException("Failed to deserialize product data. Ensure the JSON structure is correct.");

//             Console.WriteLine("Deserialized Product Data:");
//             foreach (var assetClass in productData.Keys)
//             {
//                 foreach (var subAssetClass in productData[assetClass].Keys)
//                 {
//                     Console.WriteLine($"Asset Class: {assetClass}, Sub-Asset Class: {subAssetClass}");
//                     foreach (var product in productData[assetClass][subAssetClass])
//                         Console.WriteLine($"- Product: {product.ProductName}, Annual Return: {product.AnnualReturn}");
//                 }
//             }

//             return productData.ToDictionary(
//                 assetClass => NormalizeKey(assetClass.Key),
//                 assetClass => assetClass.Value.ToDictionary(
//                     subAssetClass => subAssetClass.Key,
//                     subAssetClass => subAssetClass.Value
//                 )
//             );
//         }

//         private List<Product> GetProductsForAssetClass(
//             Dictionary<string, Dictionary<string, List<Product>>> productData,
//             string assetClass,
//             string subAssetClass)
//         {
//             assetClass = NormalizeKey(assetClass);
//             subAssetClass = subAssetClass.Trim();

//             Console.WriteLine($"Checking for products in Asset Class: {assetClass}, Sub-Asset Class: {subAssetClass}");

//             if (!productData.ContainsKey(assetClass) || !productData[assetClass].ContainsKey(subAssetClass))
//             {
//                 Console.WriteLine($"No products found for asset class '{assetClass}' and sub-asset class '{subAssetClass}'.");
//                 return new List<Product>();
//             }

//             return productData[assetClass][subAssetClass];
//         }

//         private string NormalizeKey(string input)
//         {
//             return input.Trim().ToLower().Replace(" ", "");
//         }

//         public async Task<List<ProductDTO>> GetProductAllocationAsync(
//             Dictionary<string, Dictionary<string, double>> subAllocations,
//             double targetAmount,
//             string investmentHorizon)
//         {
//             var productData = await LoadProductDataAsync();
//             var result = ComputeSubAllocations(productData, subAllocations);
//             int years = MapHorizonToYears(investmentHorizon);

//             return PrepareProductResponse(result, targetAmount, years);
//         }

//         private int MapHorizonToYears(string horizon)
//         {
//             return horizon.ToLower() switch
//             {
//                 "short" => 2,
//                 "moderate" => 5,
//                 "high" => 9,
//                 _ => 5
//             };
//         }

//         public List<ProductDTO> PrepareProductResponse(
//             Dictionary<string, Dictionary<string, List<Product>>> productData,
//             double targetAmount,
//             int years)
//         {
//             var productResponse = new List<ProductDTO>();

//             foreach (var assetClass in productData)
//             {
//                 foreach (var subAssetClass in assetClass.Value)
//                 {
//                     foreach (var product in subAssetClass.Value)
//                     {
//                         double investmentAmount = _investmentAmountService.CalculateInvestmentAmount(
//                             product.PercentageSplit,
//                             targetAmount,
//                             product.AnnualReturn,
//                             years
//                         );

//                         productResponse.Add(new ProductDTO
//                         {
//                             ProductName = product.ProductName,
//                             AnnualReturn = product.AnnualReturn,
//                             AssetClass = product.AssetClass,
//                             SubAssetClass = product.SubAssetClass,
//                             PercentageSplit = product.PercentageSplit,
//                             InvestmentAmount = investmentAmount
//                         });
//                     }
//                 }
//             }

//             return productResponse;
//         }

//         private Dictionary<string, Dictionary<string, List<Product>>> ComputeSubAllocations(
//             Dictionary<string, Dictionary<string, List<Product>>> productData,
//             Dictionary<string, Dictionary<string, double>> subAllocations)
//         {
//             foreach (var assetClass in subAllocations)
//             {
//                 foreach (var subAssetClass in assetClass.Value)
//                 {
//                     var percentageSplit = subAssetClass.Value;
//                     var products = GetProductsForAssetClass(productData, assetClass.Key, subAssetClass.Key);

//                     if (products == null || products.Count == 0)
//                         continue;

//                     double totalReturn = products.Sum(p => p.AnnualReturn);
//                     foreach (var product in products)
//                     {
//                         var ratio = product.AnnualReturn / totalReturn;
//                         product.PercentageSplit = Math.Round(ratio * percentageSplit, 2);
//                     }
//                 }
//             }

//             return productData;
//         }
//     }

//     public class Product
//     {
//         [JsonPropertyName("product_name")]
//         public string ProductName { get; set; }

//         [JsonPropertyName("annual_return")]
//         public double AnnualReturn { get; set; }

//         [JsonPropertyName("asset_class")]
//         public string AssetClass { get; set; }

//         [JsonPropertyName("sub_asset_class")]
//         public string SubAssetClass { get; set; }

//         [JsonPropertyName("liquidity")]
//         public string Liquidity { get; set; }

//         [JsonPropertyName("pros")]
//         public List<string> Pros { get; set; }

//         [JsonPropertyName("cons")]
//         public List<string> Cons { get; set; }

//         [JsonPropertyName("risk_level")]
//         public string RiskLevel { get; set; }

//         [JsonPropertyName("percentage_split")]
//         public double PercentageSplit { get; set; }
//     }

//     public class SubAllocationResultDTO
//     {
//         public Dictionary<string, AssetDTO> Assets { get; set; }

//         public Dictionary<string, Dictionary<string, double>> ToDictionary()
//         {
//             var result = new Dictionary<string, Dictionary<string, double>>();

//             foreach (var assetClass in Assets)
//             {
//                 var normalizedAssetClass = NormalizeKey(assetClass.Key);
//                 var subAllocations = assetClass.Value.SubAssets.ToDictionary(
//                     subAsset => subAsset.Key,
//                     subAsset => subAsset.Value
//                 );

//                 result[normalizedAssetClass] = subAllocations;
//             }

//             return result;
//         }

//         private string NormalizeKey(string input)
//         {
//             return input.Trim().ToLower().Replace(" ", "");
//         }
//     }

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WisVestAPI.Models.DTOs;
using WisVestAPI.Models.Matrix;
using System.Text.Json.Serialization;

namespace WisVestAPI.Services
{
    public class ProductAllocationService
    {
        private readonly string _productJsonFilePath = "Repositories/Matrix/product_json.json";
        private readonly InvestmentAmountService _investmentAmountService;

        public ProductAllocationService()
        {
            _investmentAmountService = new InvestmentAmountService();
        }

        public async Task<Dictionary<string, Dictionary<string, Dictionary<string, Product>>>> CalculateProductAllocations(
            Dictionary<string, Dictionary<string, double>> subAllocationResult,
            double targetAmount,
    string investmentHorizon)
        {
            var productData = await LoadProductDataAsync();
            var productAllocations = new Dictionary<string, Dictionary<string, Dictionary<string, Product>>>();

            foreach (var assetClass in subAllocationResult.Keys)
            {
                var subAssetClasses = subAllocationResult[assetClass];

                foreach (var subAssetClass in subAssetClasses.Keys)
                {
                    var percentageSplit = subAssetClasses[subAssetClass];
                    var products = GetProductsForAssetClass(productData, assetClass, subAssetClass);

                    if (products == null || products.Count == 0)
                    {
                        Console.WriteLine($"No products found for sub-asset class: {subAssetClass} in asset class: {assetClass}");
                        continue;
                    }

                    double totalReturns = products.Sum(p => p.AnnualReturn);
                    if (totalReturns <= 0)
                    {
                        Console.WriteLine($"Total return is zero or negative for sub-asset {subAssetClass}, skipping allocation.");
                        continue;
                    }

                    var productSplit = new Dictionary<string, Product>();
                    foreach (var product in products)
                    {
                        var splitRatio = product.AnnualReturn / totalReturns;
                        var allocation = Math.Round(splitRatio * percentageSplit, 2);
                        product.PercentageSplit = allocation;

                        product.InvestmentAmount = _investmentAmountService.CalculateInvestmentAmount(
                    allocation,
                    targetAmount,
                    product.AnnualReturn,
                    investmentHorizon
                        );

                        // You could optionally store this investment amount in the product object if needed
                        productSplit[product.ProductName] = product;
                    }

                    if (!productAllocations.ContainsKey(assetClass))
                        productAllocations[assetClass] = new Dictionary<string, Dictionary<string, Product>>();

                    productAllocations[assetClass][subAssetClass] = productSplit;
                }
            }

            Console.WriteLine($"Final Product Allocations: {JsonSerializer.Serialize(productAllocations)}");
            return productAllocations;
        }

        private async Task<Dictionary<string, Dictionary<string, List<Product>>>> LoadProductDataAsync()
        {
            if (!File.Exists(_productJsonFilePath))
                throw new FileNotFoundException($"Product JSON file not found at {_productJsonFilePath}");

            var json = await File.ReadAllTextAsync(_productJsonFilePath);
            Console.WriteLine($"Raw JSON Content: {json}");

            var productData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<Product>>>>(json);
            if (productData == null)
                throw new InvalidOperationException("Failed to deserialize product data. Ensure the JSON structure is correct.");

            return productData.ToDictionary(
        assetClass => NormalizeKey(assetClass.Key),  // Normalize asset class keys
        assetClass => assetClass.Value.ToDictionary(
            subAssetClass => subAssetClass.Key,  // No normalization for sub-assets
            subAssetClass => subAssetClass.Value
        )
    );
}
        private List<Product> GetProductsForAssetClass(
            Dictionary<string, Dictionary<string, List<Product>>> productData,
            string assetClass,
            string subAssetClass)
        {
            assetClass = NormalizeKey(assetClass);
            subAssetClass = subAssetClass.Trim();

            if (!productData.ContainsKey(assetClass) || !productData[assetClass].ContainsKey(subAssetClass))
                return new List<Product>();

            return productData[assetClass][subAssetClass];
        }

        private string NormalizeKey(string input)
        {
            return input.Trim().ToLower().Replace(" ", "");
        }
    }

    public class Product
    {
        [JsonPropertyName("product_name")]
        public string ProductName { get; set; }

        [JsonPropertyName("annual_return")]
        public double AnnualReturn { get; set; }

        [JsonPropertyName("asset_class")]
        public string AssetClass { get; set; }

        [JsonPropertyName("sub_asset_class")]
        public string SubAssetClass { get; set; }

        [JsonPropertyName("liquidity")]
        public string Liquidity { get; set; }

        [JsonPropertyName("pros")]
        public List<string> Pros { get; set; }

        [JsonPropertyName("cons")]
        public List<string> Cons { get; set; }

        [JsonPropertyName("risk_level")]
        public string RiskLevel { get; set; }

        [JsonPropertyName("percentage_split")]
        public double PercentageSplit { get; set; }
        public double InvestmentAmount { get; set; }
    }


    public class AssetDTO
    {
        public double Percentage { get; set; }
        public Dictionary<string, double> SubAssets { get; set; }
    }
}


