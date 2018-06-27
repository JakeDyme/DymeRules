using DymeInferenceEngine.Services;
using DymeInferenceEngine.Contracts;
using EasyRuleDymeRule.Services;
using JsonDymeWorld.Services;
using System.Collections.Generic;
using System.Linq;

namespace JsonToEasyRules.Services
{
    public class JsonEasyRuleConverter
    {
        EasyRuleDymeRuleConverter _easyRuleToDymeRuleParserSvc;
        DymeInferenceEvaluator _inferenceEngineSvc;
        JsonDymeWorldConverter _jsonToDymeWorldParserSvc;
        public JsonEasyRuleConverter()
        {
            _easyRuleToDymeRuleParserSvc = new EasyRuleDymeRuleConverter();
            _inferenceEngineSvc = new DymeInferenceEvaluator();
            _jsonToDymeWorldParserSvc = new JsonDymeWorldConverter();
        }

        public IEnumerable<string> SubmitJsonWorldsAndGetInferredEasyRules(IEnumerable<string> jsonWorlds)
        {
            var worlds = jsonWorlds.Select(w => _jsonToDymeWorldParserSvc.ParseJson(w));
            var dymeRules = _inferenceEngineSvc.GetRulesForWorlds(worlds, PickAttriubutesBy.World);
            var easyRules = dymeRules.Select(r => _easyRuleToDymeRuleParserSvc.ConvertDymeRuleToEasyRule(r)).ToList();
            return easyRules;
        }
    }
}
