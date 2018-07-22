using DymeRuleEngine.Models;
using DymeRuleEngine.Contracts;
using System.Linq;
using System.Collections.Generic;
using DymeFluentAdapter.Services;

namespace DymeFluentSyntax.Models
{
    public class Any
    {
        readonly Disjunction _state;
        public Any(Disjunction state)
        {
            _state = state;
        }

        public static Any Of(IEvaluatable argument)
        {
            var newJunction = new Disjunction();
            JunctionHelper.AddArgumentToJunction<Disjunction>(newJunction, argument);
            return new Any(newJunction);
        }

        public Any Or(IEvaluatable argument)
        {
            JunctionHelper.AddArgumentToJunction<Disjunction>(_state, argument);
            return this;
        }

        public IEvaluatable IsTrue()
        {
            return AsEvaluatable();
        }

        private IEvaluatable AsEvaluatable()
        {
            return _state;
        }

    }
}
