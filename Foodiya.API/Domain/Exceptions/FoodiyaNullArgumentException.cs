using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodiya.Domain.Exceptions
{
    [Serializable]
    public class FoodiyaNullArgumentException : FoodiyaBaseException
    {
        public FoodiyaNullArgumentException() { }
        public FoodiyaNullArgumentException(string message) : base(message) { }
        public FoodiyaNullArgumentException(string message, Exception inner) : base(message, inner) { }
    }
}
