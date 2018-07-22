using DymeInferenceEngine.Services;
using EasyRuleDymeRule.Services;
using DymeRuleEngine.Contracts;
using DymeRuleEngine.Services;
using JsonDymeWorld.Services;
using System.Collections.Generic;
using System.Linq;
using System;
using JsonEasyRule.Contracts;

namespace JsonToEasyRules.Services
{
    public class JsonEasyRuleEvaluator
    {
        EasyRuleDymeRuleConverter _easyRuleDymeRuleSvc;
        DymeInferenceEvaluator _inferenceEngineSvc;
        JsonDymeWorldConverter _jsonDymeWorldSvc;
        DymeRuleEvaluator _ruleEvaluatorSvc;

        public static JsonEasyRuleEvaluator CreateEvaluator()
        {
            return new JsonEasyRuleEvaluator();
        }

        private JsonEasyRuleEvaluator()
        {
            _easyRuleDymeRuleSvc = new EasyRuleDymeRuleConverter();
            _inferenceEngineSvc = new DymeInferenceEvaluator();
            _jsonDymeWorldSvc = new JsonDymeWorldConverter();
            _ruleEvaluatorSvc = new DymeRuleEvaluator();
        }

        public IEnumerable<string> InferEasyRules(IEnumerable<string> jsonObjects, InferenceType inferenceType = InferenceType.Pessimistic)
        {
            var dymeWorlds = ConvertJsonStringsToDymeWorlds(jsonObjects);
            var dymeRules = GetRulesFromWorlds(dymeWorlds, inferenceType);
            var easyRules = ConvertDymeRulesToEasyRules(dymeRules);
            return easyRules;
        }

        private IEnumerable<Dictionary<string, string>> ConvertJsonStringsToDymeWorlds(IEnumerable<string> jsonObjects)
        {
            return jsonObjects.Select(w => _jsonDymeWorldSvc.ConvertJsonToDymeWorld(w));
        }

        public IEnumerable<string> GetFailingRules(string jsonObject, IEnumerable<string> easyRules)
        {
            var dymeWorld = _jsonDymeWorldSvc.ConvertJsonToDymeWorld(jsonObject);
            var dymeRules = easyRules.Select(r => _easyRuleDymeRuleSvc.ConvertEasyRuleToDymeRule(r)).ToList();
            var failingDymeRules = dymeRules.Where(r => !_ruleEvaluatorSvc.IsTrueIn(r, dymeWorld)).ToList();
            var failingEasyRules = failingDymeRules.Select(r => _easyRuleDymeRuleSvc.ConvertDymeRuleToEasyRule(r)).ToList();
            return failingEasyRules;
        }

        public IEnumerable<string> GetFailingWorlds(IEnumerable<string> jsonObjects, string easyRule)
        {
            var dymeWorlds = jsonObjects.Select(w=>new { json = w, dyme = _jsonDymeWorldSvc.ConvertJsonToDymeWorld(w) }).ToList();
            var dymeRule = _easyRuleDymeRuleSvc.ConvertEasyRuleToDymeRule(easyRule);
            var failingDymeWorlds = dymeWorlds.Where(w => !_ruleEvaluatorSvc.IsTrueIn(dymeRule, w.dyme)).ToList();
            var failingJsonWorlds = failingDymeWorlds.Select(w => w.json).ToList();
            return failingJsonWorlds;
        }

        private IEnumerable<IEvaluatable> GetRulesFromWorlds(IEnumerable<Dictionary<string, string>> dymeWorlds, InferenceType inferenceType)
        {
            switch (inferenceType)
            {
                case InferenceType.Pessimistic:
                    return _inferenceEngineSvc.GetRulesPessimisticallyFromWorlds(dymeWorlds);
            }
            throw new Exception("Invalid inference type");
        }

        private IEnumerable<string> ConvertDymeRulesToEasyRules(IEnumerable<IEvaluatable> dymeRules)
        {
            return dymeRules.Select(r => _easyRuleDymeRuleSvc.ConvertDymeRuleToEasyRule(r)).ToList();
        }

    }
}
