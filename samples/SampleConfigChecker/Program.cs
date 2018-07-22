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
            // ----------------- SETUP -----------------------------------------
            // load Easy-Rules...
            var rule = "if (sky) is (blue) then (planet) must be (Earth)";
            
            // load configs...
            var config1 = "{ 'planet': 'Earth', 'sky': 'blue' }";
            var config2 = "{ 'planet': 'Mars',  'sky': 'blue' }";


            // ----------------- EXECUTION ------------------------------------
            // Create rule engine...
            var evaluator = JsonEasyRuleEvaluator.CreateEvaluator();
            
            // Check configs against rule, return bad configs...
            var failingConfigs = evaluator.GetFailingWorlds(new[] { config1, config2 }, rule);


            // ----------------- REPORTING -------------------------------------
            // Output any bad configs...
            Console.WriteLine("The following configs contain errors:");
            foreach (var config in failingConfigs) Console.WriteLine(config);
            Console.ReadLine();


            // ----------------- OUTPUT ----------------------------------------
            // The following configs contain errors:
            // { 'planet': 'Mars',  'sky': 'blue' }
        }




    }
}
