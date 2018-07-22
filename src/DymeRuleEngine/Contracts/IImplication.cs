using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DymeRuleEngine.Contracts
{
    public interface IImplication: IEvaluatable
    {
        /// <summary>
        /// Splits an implications containing conjunctions into basic implications
        /// </summary>
        /// <returns></returns>
        IEnumerable<IEvaluatable> Split();
    }
}
