namespace Foodiya.Domain.Exceptions;

[Serializable]
public class FoodiyaNotFoundException : FoodiyaBaseException
{
    public FoodiyaNotFoundException() { }
    public FoodiyaNotFoundException(string message) : base(message) { }
    public FoodiyaNotFoundException(string message, Exception inner) : base(message, inner) { }
}
