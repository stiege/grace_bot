using System.Collections.Generic;

namespace GraceBot
{
    internal class ActivityDefinition : IDefinition
    {
        private readonly Dictionary<string, string> _definitions;

        public ActivityDefinition(Dictionary<string, string> definitions)
        {
            _definitions = definitions;
        }

        public string FindDefinition(string subject)
        {
            string result;
            _definitions.TryGetValue(subject.ToUpper(), out result);
            return result;
        }
    }
}
