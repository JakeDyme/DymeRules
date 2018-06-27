using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DymeInferenceEngine.Contracts
{
    public enum BuildRulesFromAttributesUsing
    {
        CartesianImplication,
        ByWorldDeltas,
        ByWorldMatching
    }

    public enum PickAttriubutesBy
    {
        World,
        WorldDeltas,
        WorldMatching
    }
}
