namespace CampusEatsUI.Services.Users;

public class UserService(HttpClient _http) : IUserService
{
    public void CreateUserAsync(string username, string email, string password)
    {
        throw new NotImplementedException();
    }

    public Task<List<Models.Users>> GetAllUsersAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Models.Users> GetUserByCredentialAsync(string email, string plainPassword)
    {
        throw new NotImplementedException();
    }

    public Task<Models.Users> GetUserByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public void UpdateUserAsync(Guid id, string username, string email, string plainPassword)
    {
        throw new NotImplementedException();
    }

    public void DeleteUserAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}