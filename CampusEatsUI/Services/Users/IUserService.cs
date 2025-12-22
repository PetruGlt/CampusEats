namespace CampusEatsUI.Services.Users;

public interface IUserService
{
    public void CreateUserAsync(string username, string email, string password);
    public Task<List<Models.Users>> GetAllUsersAsync();
    public Task<Models.Users> GetUserByCredentialAsync(string email, string plainPassword);
    public Task<Models.Users> GetUserByIdAsync(Guid id);
    public void UpdateUserAsync(Guid id, string username, string email, string plainPassword);
    public void DeleteUserAsync(Guid id);
}