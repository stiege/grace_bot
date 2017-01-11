using System.Collections.Generic;

namespace GraceBot
{
    internal class ActivityDefinition : IDefinition
    {
        private readonly Dictionary<string, string> _definitions;

        // constructor
        public ActivityDefinition(Dictionary<string, string> definitions)
        {
            _definitions = definitions;
        }

        // Implement the method for IDefinition interface. 
        // Return the definition (if found) given an English word .
        public string FindDefinition(string subject)
        {
            string result;
            _definitions.TryGetValue(subject.ToUpper(), out result);
            return result;
        }
    }
}
