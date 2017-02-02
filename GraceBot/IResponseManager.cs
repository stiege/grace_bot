using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraceBot
{
    internal interface IResponseManager
    {

        /// <summary>
        /// Return a value by given key 
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        string GetResponseByKey(string key);

        /// <summary>
        /// Return if contains key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        bool ContainsKey(string key);
    }
}
