namespace Foodiya.Domain.Exceptions;

[Serializable]
public class FoodiyaForbiddenException : FoodiyaBaseException
{
    public FoodiyaForbiddenException() { }
    public FoodiyaForbiddenException(string message) : base(message) { }
    public FoodiyaForbiddenException(string message, Exception inner) : base(message, inner) { }
}
