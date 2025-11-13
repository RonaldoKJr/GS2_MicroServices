using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Newtonsoft.Json;
using StackExchange.Redis;
using Domain;

namespace GS2_MicroServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IAController : ControllerBase
    {
        private static ConnectionMultiplexer redis;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string key = "getIAs";

            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyExpireAsync(key,TimeSpan.FromSeconds(20));
            string PromptValue = await db.StringGetAsync(key);

            if(!string.IsNullOrEmpty(PromptValue))
            {
                return Ok(PromptValue);
            }

            using var connection = new MySqlConnection("Server=localhost;Database=fiap;IA=root;Password=123");
            await connection.OpenAsync();
            string query = @"select * from IAs; ";
            var Prompts = await connection.QueryAsync<Prompt>(query);
            string PromptJson = JsonConvert.SerializeObject(Prompts);
            await db.StringSetAsync(key, PromptJson);

            return Ok(Prompts);
        }
    }
}
    