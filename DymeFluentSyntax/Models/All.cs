using DymeRuleEngine.Models;
using DymeRuleEngine.Contracts;
using System.Linq;

namespace DymeFluentSyntax.Models
{
    public class All
    {
        readonly Conjunction _state;
        public All(Conjunction state)
        {
            _state = state;
        }

        private IEvaluatable AsEvaluatable()
        {
            return _state;
        }

        public static All Of(IEvaluatable argument)
        {
            var newScenario = new Conjunction();
            newScenario.Arguments = new[] { argument };
            var all = new All(newScenario);
            return all;
        }

        public All And(IEvaluatable argument)
        {
            _state.Arguments = _state.Arguments.Concat(new[] { argument });
            return this;
        }

        public IEvaluatable AreTrue()
        {
            return AsEvaluatable();
        }

    }
}
