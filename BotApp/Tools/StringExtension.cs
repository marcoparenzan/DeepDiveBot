using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BotApp
{
    public static class StringExtension
    {
        public static bool IsEmpty(this string that) => string.IsNullOrWhiteSpace(that);
        public static bool IsNotEmpty(this string that) => !string.IsNullOrWhiteSpace(that);

        public static string IfEmpty(this string that, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(that) ? defaultValue : that;
        }

        public static string IfEmpty(this string that, params string[] defaultValues)
        {
            if (that.IsNotEmpty())
            {
                return that;
            }

            for (var i = 0; i < defaultValues.Length; i++)
            {
                if (defaultValues[i].IsNotEmpty())
                {
                    return defaultValues[i];
                }
            }

            return that;
        }

        public static bool IsOneOf(this string that, params string[] defaultValues)
        {
            if (that.IsEmpty())
            {
                return false;
            }

            for (var i = 0; i < defaultValues.Length; i++)
            {
                if (that.Equals(defaultValues[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static string NumberArgs(this string instance, params object[] args)
        {
            if (args == null) return instance;
            if (args.Length == 0) return instance;

            var current = instance;
            for (var i = 0; i < args.Length; i++)
            {
                current = current.Replace($"${i}", args[i].ToString());
            }

            return current;
        }
    }
}
