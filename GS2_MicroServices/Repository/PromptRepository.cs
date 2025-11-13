using Dapper;
using Domain;
using MySqlConnector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class PromptRepository : IPromptRepository
    {
        private readonly MySqlConnection _connection;

        public PromptRepository(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
        }

        public async Task<IEnumerable<Prompt>> GetPrompts()
        {
            await _connection.OpenAsync();
            string query = @"SELECT * FROM IAs;";
            var prompts = await _connection.QueryAsync<Prompt>(query);
            return prompts;
        }

        public async Task<Prompt> AddPromptAsync(Prompt prompt)
        {
            await _connection.OpenAsync();

            string sql = @"
                INSERT INTO IAs (Prompt)
                VALUES (@Prompt);
                SELECT LAST_INSERT_ID();
            ";

            int newId = await _connection.QuerySingleAsync<int>(sql, prompt);
            prompt.Id = newId;
            return prompt;
        }

        public async Task UpdatePromptAsync(Prompt prompt)
        {
            await _connection.OpenAsync();

            string sql = @"
                UPDATE IAs
                SET Prompt = @Prompt
                WHERE Id = @Id;
            ";

            await _connection.ExecuteAsync(sql, prompt);
        }

        public async Task DeletePromptAsync(int id)
        {
            await _connection.OpenAsync();

            string sql = "DELETE FROM IAs WHERE Id = @Id;";
            await _connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
