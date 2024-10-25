namespace Basket;

public interface IUserPrincipal
{
    string UserId { get; set; }
    int IntUserId { get; }
    bool IsVerified { get; }
}

public class UserPrincipal : IUserPrincipal
{
    public string UserId { get; set; }
    public int IntUserId => Convert.ToInt32(UserId.Substring(2));

    public bool IsVerified => UserId.StartsWith("vf") ;
}