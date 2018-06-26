using DymeRuleEngine.Constructs;
using DymeRuleEngine.Contracts;
using System.Linq;

namespace DymeFluentSyntax
{
    public class Any
    {
        readonly Disjunction _state;
        public Any(Disjunction state)
        {
            _state = state;
        }

        private IEvaluatable AsEvaluatable()
        {
            return _state;
        }

        public static Any Of(IEvaluatable argument)
        {
            var newScenario = new Disjunction();
            newScenario.Arguments = new[] { argument };
            var any = new Any(newScenario);
            return any;
        }

        public Any Or(IEvaluatable argument)
        {
            _state.Arguments = _state.Arguments.Concat(new[] { argument });
            return this;
        }

        public IEvaluatable IsTrue()
        {
            return AsEvaluatable();
        }

    }
}
