﻿using DymeRuleEngine.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DymeRuleEngine.Models
{

    [DebuggerDisplay("{ToString()}")]
    public class Implication : IEvaluatable
    {
        public IEvaluatable Antecedent { get; set; }
        public IEvaluatable Consequent { get; set; }

        public static Implication Create()
        {
            return new Implication();
        }
        public Implication() { }

        public Implication(IEvaluatable antecedent, IEvaluatable consequent)
        {
            Antecedent = antecedent;
            Consequent = consequent;
        }
        public override bool Equals(object obj)
        {
            var inputObject = obj as Implication;
            return inputObject.Antecedent.Equals(Antecedent)
                && inputObject.Consequent.Equals(Consequent);
        }

        //public bool Evaluate(Dictionary<string, string> world)
        //{
        //    if (NotApplicable(world))
        //        return true;
        //    return Consequent.Evaluate(world);
        //}

        //private bool NotApplicable(Dictionary<string, string> world)
        //{
        //    return Antecedent.Evaluate(world) == false;
        //}

        public override string ToString()
        {
            return $"IF {Antecedent.ToString()} Then {Consequent.ToString()}";
        }

        public bool RelationallyEquivalentTo(IEvaluatable evaluatable)
        {
            if (evaluatable.GetType() == typeof(Implication))
            {
                var implication = evaluatable as Implication;
                return Antecedent.RelationallyEquivalentTo(implication.Antecedent) && Consequent.RelationallyEquivalentTo(implication.Consequent);
            }
            return false;
        }

        public string ToFormattedString(Func<IEvaluatable, string> formatFunction)
        {
            return formatFunction(this);
        }

        public override int GetHashCode()
        {
            return (nameof(Implication) + ":" + ToString()).GetHashCode();
        }

    }
}
