using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodiya.Domain.Exceptions
{
    [Serializable]
    public class FoodiyaBaseException : Exception
    {
        public FoodiyaBaseException() { }
        public FoodiyaBaseException(string message) : base(message) { }
        public FoodiyaBaseException(string message, Exception inner) : base(message, inner) { }
    }
}
