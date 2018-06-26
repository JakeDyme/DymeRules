using DymeInferenceEngine;
using EasyRule.Dyme;
using JsonToDymeWorldParser;
using System.Collections.Generic;
using System.Linq;

namespace JsonToEasyRules
{
    public class JsonEasyRules
    {
        EasyRuleDymeParser _easyRuleToDymeRuleParserSvc;
        InferenceEngine _inferenceEngineSvc;
        JsonDymeWorldParser _jsonToDymeWorldParserSvc;
        public JsonEasyRules()
        {
            _easyRuleToDymeRuleParserSvc = new EasyRuleDymeParser();
            _inferenceEngineSvc = new InferenceEngine();
            _jsonToDymeWorldParserSvc = new JsonDymeWorldParser();
        }

        public IEnumerable<string> SubmitJsonWorldsAndGetInferredEasyRules(IEnumerable<string> jsonWorlds)
        {
            var worlds = jsonWorlds.Select(w => _jsonToDymeWorldParserSvc.ParseJson(w));
            var dymeRules = _inferenceEngineSvc.GetRulesForWorlds(worlds, InferenceMethod.Cartesian);
            var easyRules = dymeRules.Select(r => _easyRuleToDymeRuleParserSvc.ConvertDymeRuleToEasyRule(r)).ToList();
            return easyRules;
        }
    }
}
