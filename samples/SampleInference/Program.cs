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
            // ----------------- SETUP -----------------------------------------
            // Create some configs...
            var config1 = "{ 'planet': 'Earth',  'sky': 'blue', 'air': 'breathable' }";
            var config2 = "{ 'planet': 'Mars',   'sky': 'red', 'air': 'toxic' }";
            var config3 = "{ 'planet': 'Mercury','sky': 'red', 'air': 'toxic' }";
            var config4 = "{ 'planet': 'Mars',   'sky': 'orange', 'air': 'toxic' }";
            var config5 = "{ 'planet': 'Jupiter','sky': 'orange', 'air': 'toxic' }";


            // ----------------- EXECUTION -------------------------------------
            // Create rule engine...
            var evaluator = JsonEasyRuleEvaluator.CreateEvaluator();

            // Infer some rules...
            var newRules = evaluator.InferEasyRules(new[] { config1, config2, config3, config4, config5 });


            // ----------------- REPORTING -------------------------------------
            // Output any bad configs...
            Console.WriteLine("The following rules were generated:");
            foreach (var rule in newRules) Console.WriteLine(rule);
            Console.ReadLine();


            // ----------------- OUTPUT ----------------------------------------
            // The following rules were generated:
            // IF (planet) IS (Mars) THEN (air) IS (toxic)
            // IF (sky) IS (red) THEN (air) IS (toxic)
            // IF (sky) IS (orange) THEN (air) IS (toxic)
        }
    }
}
