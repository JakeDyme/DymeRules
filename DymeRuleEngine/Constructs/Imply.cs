﻿using DymeRuleEngine.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DymeRuleEngine.Constructs
{

    [DebuggerDisplay("{ToString()}")]
    public class Imply : IEvaluatable
    {
        public IEvaluatable Antecedent { get; set; }
        public IEvaluatable Consequent { get; set; }

        public static Imply That()
        {
            return new Imply();
        }

        public Imply If(IEvaluatable antecedant)
        {
            Antecedent = antecedant;
            return this;
        }
        public Imply Then(IEvaluatable consequent)
        {
            Consequent = consequent;
            return this;
        }

        public static Imply Create()
        {
            return new Imply();
        }
        private Imply() { }

        public Imply(IEvaluatable antecedent, IEvaluatable consequent)
        {
            Antecedent = antecedent;
            Consequent = consequent;
        }
        public override bool Equals(object obj)
        {
            var inputObject = obj as Imply;
            return inputObject.Antecedent.Equals(Antecedent)
                && inputObject.Consequent.Equals(Consequent);
        }

        public bool Evaluate(Dictionary<string, string> stateOfTheWorld)
        {
            if (NotApplicapable(stateOfTheWorld))
                return true;
            return Consequent.Evaluate(stateOfTheWorld);
        }

        private bool NotApplicapable(Dictionary<string, string> stateOfTheWorld)
        {
            return Antecedent.Evaluate(stateOfTheWorld) == false;
        }

        public override string ToString()
        {
            return $"IF {Antecedent.ToString()} Then {Consequent.ToString()}";
        }

        public bool RelationallyEquivalentTo(IEvaluatable evaluatable)
        {
            if (evaluatable.GetType() == typeof(Imply))
            {
                var implication = evaluatable as Imply;
                return Antecedent.RelationallyEquivalentTo(implication.Antecedent) && Consequent.RelationallyEquivalentTo(implication.Consequent);
            }
            return false;
        }

        public string ToFormattedString(Func<IEvaluatable, string> formatFunction)
        {
            return formatFunction(this);
        }

    }
}
