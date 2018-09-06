using JsonEasyRule.Services;
using System;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            // ----------------- SETUP -----------------------------------------
            // create Easy-Rules...
            var rule = "if (sky) is (blue) then (planet) must be (Earth)";
            
            // create configs...
            var config1 = "{ 'planet': 'Earth', 'sky': 'blue' }";
            var config2 = "{ 'planet': 'Mars',  'sky': 'blue' }";

            // ----------------- EXECUTION ------------------------------------
            // Check configs against rule, return bad configs...
            var failingConfigs = JsonEasyRuleEvaluator.ReturnFailingWorlds(new[] { config1, config2 }, rule);


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
