using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.Utilities
{
    public  static class NullAndProperttyChecks
    {
        public static bool IsAnyNullOrEmpty( this  object obj)
        {
            if (Object.ReferenceEquals(obj, null))
                return true;

            return obj.GetType().GetProperties()
                .Any(x => IsNullOrEmpty(x.GetValue(obj)));
        }

        private static bool IsNullOrEmpty(this object value)
        {
            if (Object.ReferenceEquals(value, null))
                return true;

            var type = value.GetType();
            return type.IsValueType
                && Object.Equals(value, Activator.CreateInstance(type));
        }
        public static object IsPropertyNull(this object obj, string propertyName)
        {
           // var value = Object.ReferenceEquals(obj, null);
            var type = obj.GetType();
            
           
            if (type != null)
            {
                var prop = type.GetProperty(propertyName);
                var isPropNull = prop.GetValue(type) == null;
                if (isPropNull == true)
                {
                    dynamic objAfter = (dynamic)obj;
                    objAfter.Naration = null;
                    object objNow = (object)objAfter;

                    if (objNow != null)
                    {
                        return objNow;
                    }
                    return obj;
                }
            }
            return obj;
        }
    }
}
