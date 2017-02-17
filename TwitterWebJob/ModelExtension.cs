using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TwitterWebJob
{
    static class ModelExtension
    {
        public static bool IsRequiredPropertyNotNull(this object obj)
        {
            var properties = obj.GetType().GetProperties().ToList();
            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.GetCustomAttribute(typeof(RequiredAttribute)) != null)
                {
                    if (propertyInfo.GetValue(obj) == null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
