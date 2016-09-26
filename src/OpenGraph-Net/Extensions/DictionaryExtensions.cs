using System;
using System.Collections.Generic;

namespace OpenGraph_Net.Extensions
{
    /// <summary>
    /// Utils for Dictionnaries
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Get one given key if found.
        /// </summary>
        /// <param name="dico"></param>
        /// <param name="key"></param>
        /// <param name="throwEx"></param>
        /// <returns></returns>
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
