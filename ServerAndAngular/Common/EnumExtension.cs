﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SoupDiscover.Common
{
    public static class EnumExtension
    {
        private const string delimiter = ";";

        public static IEnumerable<T> Deserialize<T>(string serializedEnum) where T : struct
        {
            if (serializedEnum == null)
            {
                yield break;
            }
            var result = new List<T>();
            foreach (T de in serializedEnum.Split(delimiter).Select(Enum.Parse<T>))
            {
                yield return de;
            }
        }

        public static string Serialize<T>(this T[] enumToSerialize) where T : Enum
        {
            return enumToSerialize == null ? null : string.Join($"{delimiter}", enumToSerialize);
        }
    }

}
