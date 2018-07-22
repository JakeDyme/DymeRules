using DymeRuleEngine.Models;
using DymeRuleEngine.Contracts;
using System.Linq;
using System.Collections.Generic;
using DymeFluentAdapter.Services;

namespace DymeFluentSyntax.Models
{
    public class All
    {
        readonly Conjunction _state;
        public All(Conjunction state)
        {
            _state = state;
        }

        public static All Of(IEvaluatable argument)
        {
            var newJunction = new Conjunction();
            JunctionHelper.AddArgumentToJunction<Conjunction>(newJunction, argument);
            return new All(newJunction);
        }

        public All And(IEvaluatable argument)
        {
            JunctionHelper.AddArgumentToJunction<Conjunction>(_state, argument);
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
