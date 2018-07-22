using DymeFluentSyntax.Models;
using DymeRuleEngine.Contracts;
using DymeRuleEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DymeInferenceEngine.Services
{
    public static class ImplicationHelper
    {
        public static IEnumerable<IEvaluatable> MergeImplications(IEnumerable<IEvaluatable> implications)
        {
            var sharedAntecedentImplications = MergeAndReturnImplicationsThatShareAnAntecedent(implications, Junction.OR);
            var sharedConsequentImplications = MergeAndReturnImplicationsThatShareAConsequent(implications, Junction.OR);
            var newImplications = sharedAntecedentImplications.Union(sharedConsequentImplications).Distinct();

            if (newImplications.Any()) return MergeImplications(newImplications);
            return implications;
        }

        private static IEnumerable<IEvaluatable> MergeAndReturnImplicationsThatShareAnAntecedent(IEnumerable<IEvaluatable> implications, Junction usingJunction)
        {
            var commonAntecedentMultipleConsequent_Tuples = implications
            .Select((imp1, x) => new Tuple<IEvaluatable, IEnumerable<IEvaluatable>>(
                (imp1 as Implication).Antecedent,
                implications
                    .Where((imp2, y) => ImplicationsShareAntecedent(imp1 as Implication, imp2 as Implication))
                    .Select(imp2 => (imp2 as Implication).Consequent)))
            .Where(tuple => tuple.Item2.Count() > 1);

            return commonAntecedentMultipleConsequent_Tuples.Where(commonAMultiC_Tuple => commonAMultiC_Tuple.Item2.Any())
                .Select(commonAMultiC_Tuple =>
                    If.When(commonAMultiC_Tuple .Item1)
                    .Then(ConvertArgumentsToJunction(commonAMultiC_Tuple .Item2, usingJunction)))
                .Distinct();
        }

        private static IEnumerable<IEvaluatable> MergeAndReturnImplicationsThatShareAConsequent(IEnumerable<IEvaluatable> implications, Junction usingJunction)
        {
            var multipleAntecedentCommonConsequent_Tuples = implications
            .Select((imp1, x) => new Tuple<IEnumerable<IEvaluatable>, IEvaluatable>(
                implications
                    .Where((imp2, y) => ImplicationsShareConsequent(imp1 as Implication, imp2 as Implication))
                    .Select(imp2 => (imp2 as Implication).Antecedent),
                (imp1 as Implication).Consequent))
            .Where(tuple => tuple.Item1.Count() > 1);

            return multipleAntecedentCommonConsequent_Tuples.Where(MultiACommonC_Tuple => MultiACommonC_Tuple.Item1.Any())
                .Select(MultiACommonC_Tuple =>
                    If.When(ConvertArgumentsToJunction(MultiACommonC_Tuple.Item1, usingJunction))
                    .Then(MultiACommonC_Tuple.Item2)).ToList()
                .Distinct();
        }

        public static IEvaluatable ConvertArgumentsToJunction(IEnumerable<IEvaluatable> arguments, Junction asJunction)
        {
            return asJunction == Junction.AND ? ArgumentsToConjunction(arguments) : ArgumentsToDisjunction(arguments);
        }

        private static IEvaluatable ArgumentsToDisjunction(IEnumerable<IEvaluatable> arguments)
        {
            if (arguments.Count() == 1) return Any.Of(arguments.Single()).IsTrue();
            return arguments.Aggregate((i1, i2) => Any.Of(i1).Or(i2).IsTrue());
        }

        private static IEvaluatable ArgumentsToConjunction(IEnumerable<IEvaluatable> arguments)
        {
            if (arguments.Count() == 1) return All.Of(arguments.Single()).IsTrue();
            return arguments.Aggregate((i1, i2) => All.Of(i1).And(i2).IsTrue());
        }

        private static bool ImplicationsShareAntecedent(Implication implication1, Implication implication2)
        {
            return implication1.Antecedent.Equals(implication2.Antecedent);
        }

        private static bool ImplicationsShareConsequent(Implication implication1, Implication implication2)
        {
            return implication1.Consequent.Equals(implication2.Consequent);
        }
    }
}
