using System.Collections.Generic;

namespace Toxon.Swim.Models
{
    public class SwimMeta
    {
        public SwimMeta(IReadOnlyDictionary<string, string> fields)
        {
            Fields = fields;
        }

        public IReadOnlyDictionary<string, string> Fields { get; }
    }
}