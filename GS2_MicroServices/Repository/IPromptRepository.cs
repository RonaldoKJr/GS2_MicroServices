using Domain;

namespace Repository
{
    public interface IPromptRepository
    {
        Task<IEnumerable<Prompt>> GetAllPromptsAsync();
        Task<Prompt> AddPromptAsync(Prompt prompt);
        Task UpdatePromptAsync(Prompt Prompt);
        Task DeletePromptAsync(int id);
    }
}