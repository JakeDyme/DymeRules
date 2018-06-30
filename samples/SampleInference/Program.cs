using JsonToEasyRules.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleInference
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create some configs...
            var config1 = "{ 'planet': 'Earth',  'sky': 'blue' }";
            var config2 = "{ 'planet': 'Mars',   'sky': 'red' }";
            var config3 = "{ 'planet': 'Mercury','sky': 'red' }";
            var config4 = "{ 'planet': 'Mars',   'sky': 'orange' }";
            var config5 = "{ 'planet': 'Jupiter','sky': 'orange' }";

            // Create rule engine...
            var evaluator = JsonEasyRuleEvaluator.CreateEvaluator();

            // Infer some rules...
            var newRules = evaluator.InferEasyRules(new[] { config1, config2, config3, config4, config5 });

            // Output any bad configs...
            Console.WriteLine("The following rules were generated:");
            foreach (var rule in newRules) Console.WriteLine(rule);
            Console.ReadLine();

            //Returns:
            // >The following configs contain errors:
            // >{ 'planet': 'Mars',  'sky': 'blue' }

        }
    }
}
