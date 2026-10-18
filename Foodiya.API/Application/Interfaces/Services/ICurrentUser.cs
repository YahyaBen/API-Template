namespace Foodiya.Application.Interfaces.Services;

public interface ICurrentUser
{
    Guid UserUuid { get; }
    bool IsAdministrator { get; }
}
