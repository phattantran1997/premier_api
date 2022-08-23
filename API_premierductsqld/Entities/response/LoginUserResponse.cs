public class LoginUserResponse
{

    public string Username { get; set; }
    public string Token { get; set; }


    public LoginUserResponse(User user, string token)
    {
        Username = user.Username;
        Token = token;
    }
}