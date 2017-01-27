using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraceBot
{
    internal interface ILocalJsonManager
    {

        /// <summary>
        /// Return a value by given key 
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        string GetValueByKey(string key);
    }
}
