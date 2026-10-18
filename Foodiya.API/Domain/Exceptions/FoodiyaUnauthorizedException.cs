namespace Foodiya.Domain.Exceptions;

[Serializable]
public class FoodiyaUnauthorizedException : FoodiyaBaseException
{
    public FoodiyaUnauthorizedException() { }
    public FoodiyaUnauthorizedException(string message) : base(message) { }
    public FoodiyaUnauthorizedException(string message, Exception inner) : base(message, inner) { }
}
