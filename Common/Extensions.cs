using System;
using System.Collections.Generic;
using System.Text;
using Timberborn.Persistence;

namespace AutomizeMyDroughts.Common
{
    static class Extensions
    {
        public static bool TryGet<T>(this IObjectLoader loader, PropertyKey<T> key, ref T res) {
            if (!loader.Has(key))
                return false;
            res = loader.Get(key);
            return true;
        }
    }
}
