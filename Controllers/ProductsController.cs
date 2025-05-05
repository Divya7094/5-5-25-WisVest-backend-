// using Microsoft.AspNetCore.Mvc;
// using WisVestAPI.Models;
// using System.Text.Json;

// namespace WisVestAPI.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class ProductsController : ControllerBase
//     {
//         private readonly IWebHostEnvironment _env;

//         public ProductsController(IWebHostEnvironment env)
//         {
//             _env = env;
//         }

//         [HttpGet]
//         public IActionResult GetProducts()
//         {
//             var filePath = Path.Combine(_env.ContentRootPath, "Repositories", "products_data.json");

//             if (!System.IO.File.Exists(filePath))
//                 return NotFound("Product data file not found.");

//             var json = System.IO.File.ReadAllText(filePath);
//             var products = JsonSerializer.Deserialize<List<Product>>(json);
//             return Ok(products);
//         }
//     }
// }

using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using WisVestAPI.Models;
using WisVestAPI.Models.DTOs;
using WisVestAPI.Models.Matrix;

namespace WisVestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly string _jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Repositories","Matrix","product_json.json");


[HttpGet("products")]
public async Task<IActionResult> LoadProducts()
{
    try
    {
        var productJsonFilePath = "Repositories/Matrix/product_json.json";

        if (!System.IO.File.Exists(productJsonFilePath))
            return NotFound($"Product JSON file not found at {productJsonFilePath}");

        var json = await System.IO.File.ReadAllTextAsync(productJsonFilePath);

        // Deserialize into a nested dictionary structure
        var productData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<Product>>>>(json);

        if (productData == null)
            return BadRequest("Failed to deserialize product data. Ensure the JSON structure is correct.");

        // Flatten the nested structure into a single list of products
        var products = new List<Product>();
        foreach (var assetClass in productData.Values)
        {
            foreach (var subAssetClass in assetClass.Values)
            {
                products.AddRange(subAssetClass);
            }
        }

        var productDTOs = products.Select(p => new ProductDTO
        {
            ProductName = p.ProductName,
            AnnualReturn = p.AnnualReturn,
            AssetClass = p.AssetClass,
            SubAssetClass = p.SubAssetClass,
            Liquidity = p.Liquidity,
            Pros = p.Pros,
            Cons = p.Cons,
            RiskLevel = p.RiskLevel,
            description = p.ProductName
        }).ToList();

        return Ok(productDTOs);
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Error reading JSON file: {ex.Message}");
        return StatusCode(500, $"Error reading JSON file: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
    }
}
    }
}
