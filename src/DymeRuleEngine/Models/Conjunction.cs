using DymeRuleEngine.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DymeRuleEngine.Models
{
    [DebuggerDisplay("{ToString()}")]
    public class Conjunction: IEvaluatable, IJunction
    {
        public IEnumerable<IEvaluatable> Arguments { get; set; }
        public Conjunction() {
            Arguments = new List<IEvaluatable>();
        }
        public Conjunction(IEnumerable<IEvaluatable> arguments)
        {
            Arguments = arguments;
        }

        public override bool Equals(object obj)
        {
            var inputObject = obj as Conjunction;
            return Compare(inputObject.Arguments, Arguments);
        }

        public override int GetHashCode()
        {
            return (nameof(Conjunction) + ":" + Arguments
                .Select(x => x.GetHashCode().ToString())
                .OrderBy(x => x)
                .Aggregate((a, b) => a + "," + b))
                .GetHashCode();
        }

        private bool Compare(IEnumerable<IEvaluatable> argumentSet1, IEnumerable<IEvaluatable> argumentSet2)
        {
            return (!argumentSet1.Except(argumentSet2).Any());
        }

        public override string ToString()
        {
            return Arguments
                .Select(x => x.ToString())
                .Aggregate((a, b) => a + $" AND " + b);
        }

        public string ToFormattedString(Func<IEvaluatable, string> formatFunction)
        {
            return formatFunction(this);
        }

        public bool RelationallyEquivalentTo(IEvaluatable evaluatable)
        {
            if (evaluatable.GetType() != typeof(Conjunction)) return false;
            var scenario = evaluatable as Conjunction;
            foreach (var arg in Arguments)
            {
                if (!ScenarioContainsArgumentRelation(scenario.Arguments, arg)) return false;
            }

            foreach (var arg in scenario.Arguments)
            {
                if (!ScenarioContainsArgumentRelation(Arguments, arg)) return false;
            }

            return true;
        }

        private bool ScenarioContainsArgumentRelation(IEnumerable<IEvaluatable> arguments, IEvaluatable argument)
        {
            return arguments.Any(arg => argument.RelationallyEquivalentTo(arg));
        }


    }
}
