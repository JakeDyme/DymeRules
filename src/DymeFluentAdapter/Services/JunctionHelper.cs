using DymeRuleEngine.Contracts;
using DymeRuleEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DymeFluentAdapter.Services
{
    public static class JunctionHelper
    {
        public static void AddArgumentToJunction<T>(IJunction junction, IEvaluatable argument)
        {
            if (IsJunctionOfType<T>(argument))
                junction.Arguments = MergeArgumentsReturnNewSet(junction, argument as IJunction);
            else
                junction.Arguments = AddArgumentReturnNewSet(junction, argument);
        }

        private static bool IsJunctionOfType<T>(IEvaluatable evaluatable)
        {
            return evaluatable.GetType() == typeof(T);
        }

        private static IEnumerable<IEvaluatable> MergeArgumentsReturnNewSet(IJunction junction1, IJunction junction2)
        {
            return junction1.Arguments.Union(junction2.Arguments).Distinct();
        }

        private static IEnumerable<IEvaluatable> AddArgumentReturnNewSet(IJunction junction, IEvaluatable argument)
        {
            return junction.Arguments.Union(new[] { argument });
        }


    }
}
