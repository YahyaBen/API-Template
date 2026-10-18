using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodiya.Domain.Exceptions
{
    [Serializable]
    public class FoodiyaValueAlreadyExistsException : FoodiyaBaseException
    {
        public FoodiyaValueAlreadyExistsException() { }
        public FoodiyaValueAlreadyExistsException(string message) : base(message) { }
        public FoodiyaValueAlreadyExistsException(string message, Exception inner) : base(message, inner) { }
    }
}
