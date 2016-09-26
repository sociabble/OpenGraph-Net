using System;
using System.Collections.Generic;

namespace OpenGraph_Net.Extensions
{
    public static class DictionaryExtensions
    {
        public static string GetStringVal(this IDictionary<string, string> dico, string key, bool throwEx=false)
        {
            try
            {
                string s;
                dico.TryGetValue(key, out s);
                return s;
            }
            catch (Exception)
            {
                if (throwEx) throw;
                //ignored
                return null;
            }
        }
    }
}
