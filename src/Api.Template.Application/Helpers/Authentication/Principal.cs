namespace Gateway.Application.Helpers.Authentication;

public interface IPrincipal
{
    Guid UserId { get; set; }
    
    string FullName { get; set; }
    
    string UserName { get; set; }
    
    string Role { get; set; }
}

public class Principal: IPrincipal
{
    public Guid UserId { get; set; }
    
    public string FullName { get; set; }
    
    public string UserName { get; set; }
    
    public string Role { get; set; }
}