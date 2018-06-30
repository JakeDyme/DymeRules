using DymeRuleEngine.Models;
using DymeRuleEngine.Contracts;

namespace DymeFluentSyntax.Models
{
    public class ItsAFact
    {
        readonly Proposition _state;
        public ItsAFact(Proposition state)
        {
            _state = state;
        }

        public static ItsAFact That(string attributeName)
        {
            var prop = new Proposition();
            var newFact = new ItsAFact(prop);
            prop.AttributeName = attributeName;
            return newFact;
        }

        private IEvaluatable AsEvaluatable()
        {
            return _state;
        }

        public IEvaluatable Is(string value)
        {
            _state.AttributeValue = value;
            return AsEvaluatable(); ;
        }
        public IEvaluatable IsGreaterThan(string value)
        {
            _state.AttributeValue = value;
            return AsEvaluatable(); ;
        }
        public IEvaluatable IsLessThan(string value)
        {
            _state.AttributeValue = value;
            return AsEvaluatable();
        }
        public IEvaluatable IsNot(string value)
        {
            _state.AttributeValue = value;
            return AsEvaluatable();
        }
        public IEvaluatable Contains(string value)
        {
            _state.AttributeValue = value;
            return AsEvaluatable();
        }
        public IEvaluatable IsIn(string value)
        {
            _state.AttributeValue = value;
            return AsEvaluatable();
        }
    }
}
