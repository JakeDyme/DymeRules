using DymeRuleEngine.Contracts;
using DymeRuleEngine.Models;
using System.Collections.Generic;
using System.Linq;

namespace DymeRuleEngine.Helpers
{
    public static class DymeConstructHelper
    {
        public static bool EqualsforConjunction(object obj, IEnumerable<IEvaluatable>arguments) {
            var inputObject = obj as Conjunction;
            return CompareConjunction(inputObject.Arguments, arguments);
        }

        private static bool CompareConjunction(IEnumerable<IEvaluatable> argumentSet1, IEnumerable<IEvaluatable> argumentSet2)
        {
            return (!argumentSet1.Except(argumentSet2).Any());
        }

    }
}
