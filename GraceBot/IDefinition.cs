using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraceBot
{
    internal interface IDefinition
    {   
        /// <summary>
        /// Return a string (definition) given a string 
        /// </summary>
        /// <param name="subject">a string to be searched for its definition</param>
        /// <returns>the definition</returns>
        string FindDefinition(string subject);
    }
}
