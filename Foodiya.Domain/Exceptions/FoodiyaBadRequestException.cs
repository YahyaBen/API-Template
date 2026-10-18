namespace Foodiya.Domain.Exceptions;

[Serializable]
public class FoodiyaBadRequestException : FoodiyaBaseException
{
    public FoodiyaBadRequestException() { }
    public FoodiyaBadRequestException(string message) : base(message) { }
    public FoodiyaBadRequestException(string message, Exception inner) : base(message, inner) { }
}
