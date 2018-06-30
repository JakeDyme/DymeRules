using JsonToEasyRules.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create Easy-Rule...
            var rule = "if (sky) is (blue) then (planet) must be (Earth)";
            
            // Create some configs...
            var config1 = "{ 'planet': 'Earth', 'sky': 'blue' }";
            var config2 = "{ 'planet': 'Mars',  'sky': 'blue' }";
            

            // Create rule engine...
            var evaluator = JsonEasyRuleEvaluator.CreateEvaluator();
            
            // Check configs against rule...
            var failingConfigs = evaluator.GetFailingWorlds(new[] { config1, config2 }, rule);

            // Output any bad configs...
            Console.WriteLine("The following configs contain errors:");
            foreach (var config in failingConfigs) Console.WriteLine(config);
            Console.ReadLine();

            //Returns:
            // >The following configs contain errors:
            // >{ 'planet': 'Mars',  'sky': 'blue' }
        }




    }
}
