using Domain;

public class PromptService
{
    private readonly ICacheService _cacheService;

    public async Task<Prompt?> GetPromptAsync(int id)
    {
        var cacheKey = $"prompt:{id}";

        // Tentar buscar do cache
        var cachedPrompt = await _cacheService.GetAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedPrompt))
        {
            return JsonConvert.DeserializeObject<Prompt>(cachedPrompt);
        }

        // Buscar do banco e salvar no cache
        var prompt = await _repository.GetByIdAsync(id);
        if (prompt != null)
        {
            var promptJson = JsonConvert.SerializeObject(prompt);
            await _cacheService.SetAsync(cacheKey, promptJson, TimeSpan.FromMinutes(30));
        }

        return prompt;
    }
}
