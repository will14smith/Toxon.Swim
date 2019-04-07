using System.Collections.Generic;
using System.Linq;

namespace Toxon.Swim.Models
{
    public class SwimMeta
    {
        public SwimMeta(IReadOnlyDictionary<string, string> fields)
        {
            Fields = fields;
        }

        public IReadOnlyDictionary<string, string> Fields { get; }

        public override string ToString()
        {
            return string.Join(" ", Fields.Select(x => $"{x.Key}={x.Value}"));
        }
    }
}