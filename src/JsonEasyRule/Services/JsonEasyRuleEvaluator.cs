using DymeInferenceEngine.Services;
using EasyRuleDymeRule.Services;
using DymeRuleEngine.Contracts;
using DymeRuleEngine.Services;
using System.Collections.Generic;
using System.Linq;
using System;
using JsonEasyRule.Contracts;
using DymeInferenceEngine.Contracts;
using JsonEasyRule.Models;

namespace JsonToEasyRules.Services
{
    public class JsonEasyRuleEvaluator
    {
        EasyRuleDymeRuleConverter _easyRuleDymeRuleSvc;
        DymeInferenceEvaluator _inferenceEngineSvc;
        IEvaluator _ruleEvaluatorSvc;
        IWorldReader _worldReader;
        IWorldAnalyser _worldAnalyser;

        public static JsonEasyRuleEvaluator CreateEvaluator()
        {
            return new JsonEasyRuleEvaluator();
        }

        private JsonEasyRuleEvaluator(IMetricService metricService = null)
        {
            _easyRuleDymeRuleSvc = new EasyRuleDymeRuleConverter();
            _worldAnalyser = new JsonPathWorldAnalyser();
            _worldReader = new JsonPathWorldReader();
            _inferenceEngineSvc = new DymeInferenceEvaluator(_worldReader, _worldAnalyser, metricService);
            _ruleEvaluatorSvc = new DymeRuleEvaluator(_worldReader);
        }

        public IEnumerable<string> InferEasyRules(IEnumerable<string> jsonObjects, InferenceType inferenceType = InferenceType.Pessimistic)
        {
            var dymeRules = GetRulesFromWorlds(jsonObjects, inferenceType);
            var easyRules = ConvertDymeRulesToEasyRules(dymeRules);
            return easyRules;
        }


        public IEnumerable<string> GetFailingRules(string jsonObject, IEnumerable<string> easyRules)
        {
            var dymeRules = easyRules.Select(r => _easyRuleDymeRuleSvc.ConvertEasyRuleToDymeRule(r)).ToList();
            var failingDymeRules = dymeRules.Where(r => !_ruleEvaluatorSvc.IsTrueIn(r, jsonObject)).ToList();
            var failingEasyRules = failingDymeRules.Select(r => _easyRuleDymeRuleSvc.ConvertDymeRuleToEasyRule(r)).ToList();
            return failingEasyRules;
        }

        public IEnumerable<string> GetFailingWorlds(IEnumerable<string> jsonObjects, string easyRule)
        {
            var dymeRule = _easyRuleDymeRuleSvc.ConvertEasyRuleToDymeRule(easyRule);
            var failingDymeWorlds = jsonObjects.Where(w => !_ruleEvaluatorSvc.IsTrueIn(dymeRule, w)).ToList();
            var failingJsonWorlds = failingDymeWorlds.ToList();
            return failingJsonWorlds;
        }

        public bool IsTrueIn(string easyRule, string jsonWorld)
        {
            var dymeRule = _easyRuleDymeRuleSvc.ConvertEasyRuleToDymeRule(easyRule);
            return _ruleEvaluatorSvc.IsTrueIn(dymeRule, jsonWorld);
        }

        private IEnumerable<IEvaluatable> GetRulesFromWorlds<WorldType>(IEnumerable<WorldType> dymeWorlds, InferenceType inferenceType)
        {
            switch (inferenceType)
            {
                case InferenceType.Pessimistic:
                    return _inferenceEngineSvc.GetRulesForWorlds(dymeWorlds, InferenceMethod.Pessimistic);
            }
            throw new Exception("Invalid inference type");
        }

        private IEnumerable<string> ConvertDymeRulesToEasyRules(IEnumerable<IEvaluatable> dymeRules)
        {
            return dymeRules.Select(r => _easyRuleDymeRuleSvc.ConvertDymeRuleToEasyRule(r)).ToList();
        }

    }
}
