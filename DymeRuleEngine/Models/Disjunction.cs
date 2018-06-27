using DymeRuleEngine.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DymeRuleEngine.Models
{
    [DebuggerDisplay("{ToString()}")]
    public class Disjunction : IEvaluatable
    {
        public IEnumerable<IEvaluatable> Arguments { get; set; }
        public Disjunction() { }
        public Disjunction(IEnumerable<IEvaluatable> arguments)
        {
            Arguments = arguments;
        }
        public bool Evaluate(Dictionary<string, string> stateOfTheWorld)
        {
            return Arguments.Any(a => FoundAWinner(a.Evaluate(stateOfTheWorld)));
        }

        private bool FoundAWinner(bool result)
        {
            return result == true;
        }


        public override bool Equals(object obj)
        {
            var inputObject = obj as Disjunction;
            return Compare(inputObject.Arguments, Arguments);
        }

        private bool Compare(IEnumerable<IEvaluatable> argumentSet1, IEnumerable<IEvaluatable> argumentSet2)
        {
            return (!argumentSet1.Except(argumentSet2).Any());
        }

        public override string ToString()
        {
            return Arguments.Select(x => x.ToString()).Aggregate((a, b) => a + $" OR " + b);
        }

        public string ToFormattedString(Func<IEvaluatable, string> formatFunction)
        {
            return formatFunction(this);
        }

        public bool RelationallyEquivalentTo(IEvaluatable evaluatable)
        {
            if (evaluatable.GetType() != typeof(Disjunction)) return false;
            var scenario = evaluatable as Disjunction;
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
            foreach (var arg in arguments)
            {
                if (argument.RelationallyEquivalentTo(arg))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
