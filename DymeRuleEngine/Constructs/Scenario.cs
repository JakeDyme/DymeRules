using DymeRuleEngine.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DymeRuleEngine.Constructs
{


    [DebuggerDisplay("{ToString()}")]
    public class Scenario : IEvaluatable
    {
        public IEnumerable<IEvaluatable> Arguments { get; set; }
        public Junction Junction { get; set; }
        public Scenario(IEnumerable<IEvaluatable> arguments, Junction junction)
        {
            Arguments = arguments;
            Junction = junction;
        }
        public bool Evaluate(Dictionary<string, string> stateOfTheWorld)
        {
            foreach (var argument in Arguments)
            {
                var result = argument.Evaluate(stateOfTheWorld);
                if (FoundABadOne(result))
                    return false;
                if (FoundAWinner(result))
                    return true;
            }
            if (Junction == Junction.AND)
                return true;
            if (Junction == Junction.OR)
                return false;
            throw new Exception("Unexpected resolution");
        }

        private bool FoundAWinner(bool result)
        {
            return Junction == Junction.OR && result == true;
        }

        private bool FoundABadOne(bool result)
        {
            return Junction == Junction.AND && result == false;
        }

        public override bool Equals(object obj)
        {
            var inputObject = obj as Scenario;
            return Compare(inputObject.Arguments, Arguments)
                && inputObject.Junction.Equals(Junction);
        }

        private bool Compare(IEnumerable<IEvaluatable> argumentSet1, IEnumerable<IEvaluatable> argumentSet2)
        {
            return (!argumentSet1.Except(argumentSet2).Any());
        }

        public override string ToString()
        {
            return Arguments.Select(x => x.ToString()).Aggregate((a, b) => a + $" {Junction} " + b);
        }

        public string ToFormattedString(Func<IEvaluatable, string> formatFunction)
        {
            return formatFunction(this);
        }

        public bool RelationallyEquivalentTo(IEvaluatable evaluatable)
        {
            if (evaluatable.GetType() != typeof(Scenario)) return false;
            var scenario = evaluatable as Scenario;
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
