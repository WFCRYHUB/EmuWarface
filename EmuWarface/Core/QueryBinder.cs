using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EmuWarface.Core
{
    public static class QueryBinder
    {
        public static List<QueryData> Handler = new List<QueryData>();

        public static void Init()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                var methods = type.GetMethods().Where(method => Attribute.IsDefined(method, typeof(QueryAttribute)) && method.IsStatic);
                foreach (var method in methods)
                {
                    var attribute = (QueryAttribute)Attribute.GetCustomAttribute(method, typeof(QueryAttribute));
                    Handler.Add(new QueryData(method, attribute.Names, attribute.Type));
                }
            }

            Log.Info("[QueryBinder] Loaded {0} handlers", Handler.Count);
        }
    }
}
