using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GomLib.Models
{
    public enum SchematicResearchChance
    {
        Undefined = -1,
        None = 0,
        VeryLow = 1,
        Low = 2,
        MediumLow = 3,
        Medium = 4,
        High = 5,
        VeryHigh = 6,
        Max = 7
    }

    public static class SchematicResearchChanceExtensions
    {
        public static SchematicResearchChance ToSchematicResearchChance(this string str)
        {
            if (string.IsNullOrEmpty(str)) return SchematicResearchChance.Undefined;

            switch (str)
            {
                case "prfSchematicResearchChanceNone": return SchematicResearchChance.None;
                case "prfSchematicResearchChanceVeryLow": return SchematicResearchChance.VeryLow;
                case "prfSchematicResearchChanceLow": return SchematicResearchChance.Low;
                case "prfSchematicResearchChanceMediumLow": return SchematicResearchChance.MediumLow;
                case "prfSchematicResearchChanceMedium": return SchematicResearchChance.Medium;
                case "prfSchematicResearchChanceHigh": return SchematicResearchChance.High;
                case "prfSchematicResearchChanceVeryHigh": return SchematicResearchChance.VeryHigh;
                case "prfSchematicResearchChanceMax": return SchematicResearchChance.Max;
                default: throw new InvalidOperationException("Invalid SchematicResearchChance:" + str);
            }
        }

        public static SchematicResearchChance ToSchematicResearchChance(this ScriptEnum val)
        {
            if (val == null) { return ToSchematicResearchChance(string.Empty); }
            return ToSchematicResearchChance(val.ToString());
        }
    }
}
