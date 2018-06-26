using DymeRuleEngine.Constructs;
using DymeRuleEngine.Contracts;

namespace DymeFluentSyntax
{
    public class If
    {
        Implication _state;

        public If(Implication state)
        {
            _state = state;
        }

        private IEvaluatable AsEvaluatable()
        {
            return _state;
        }

        public static If When(IEvaluatable antecedant)
        {
            var implication = Implication.Create();
            implication.Antecedent = antecedant;
            var imply = new If(implication);
            return imply;
        }

        public IEvaluatable Then(IEvaluatable consequent)
        {
            _state.Consequent = consequent;
            return AsEvaluatable();
        }
    }
}
