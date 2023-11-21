using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace AzureRedisCache.Controllers
{
    public class AzureRedisCacheController : ControllerBase
    {
        private readonly ConnectionMultiplexer _redisConnection;
        private readonly IConfiguration _configuration;

        public AzureRedisCacheController(IConfiguration configuration)
        {
            _configuration = configuration;
            _redisConnection = ConnectionMultiplexer.Connect($"{_configuration["Azure:ConnectionString"]}");
        }

        [HttpGet("expensive-operation")]
        public IActionResult GetExpensiveOperationResult()
        {
            IDatabase cache = _redisConnection.GetDatabase();

            string result = cache.StringGet(_configuration["Azure:CacheKey"]);

            if (string.IsNullOrEmpty(result))
            {
                Console.WriteLine("Expensive operation in progress...");
                result = ExpensiveOperation();

                cache.StringSet(_configuration["Azure:CacheKey"], result, TimeSpan.FromMinutes(5));
                Console.WriteLine("Result stored in cache.");
            }
            else
            {
                Console.WriteLine($"Result retrieved from cache: {result}");
            }

            return Ok(result);
        }

        private string ExpensiveOperation()
        {
            // Simulate an expensive operation (e.g., fetching data from a database)
            Console.WriteLine("Simulating expensive operation...");
            Thread.Sleep(3000); // Simulating a 3-second delay
            return "Result of the expensive operation";
        }
    }
}
