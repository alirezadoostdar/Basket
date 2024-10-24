namespace Basket;

public interface IUserPrincipal
{
     string UserId { get; set; }
    int In
}

public class UserPrincipal : IUserPrincipal
{
    public string UserId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}