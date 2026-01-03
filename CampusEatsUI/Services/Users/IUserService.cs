namespace CampusEatsUI.Services.Users;

public interface IUserService
{
    public Task<List<Models.Users>> GetAllUsersAsync();
    public Task<Models.Users> GetUserByIdAsync(Guid id);
    public Task UpdateUserAsync(Guid id, string username, string email, string plainPassword);
    public Task DeleteUserAsync(Guid id);
}