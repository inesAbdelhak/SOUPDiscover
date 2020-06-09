using System.Collections.Generic;
using System.Text;

namespace SoupDiscover.Common
{
    public static class CSVFileHelper
    {
        /// <summary>
        /// Convert a string array to a CSV line
        /// </summary>
        /// <param name="row">The array of value to convert</param>
        /// <param name="delimiter">delimiter between each values</param>
        /// <returns>The serialized array</returns>
        public static string SerializeToCvsLine(IEnumerable<string> row, char delimiter = ',')
        {
            var builder = new StringBuilder();
            var firstColumn = true;
            foreach (string value in row)
            {
                // Add separator if this isn't the first value
                if (!firstColumn)
                {
                    builder.Append(delimiter);
                }
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }
                // Implement special handling for values that contain comma or quote
                // Enclose in quotes and double up any double quotes
                if (value.IndexOfAny(new char[] { '"', delimiter, '\r', '\n' }) != -1)
                    builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                else
                    builder.Append(value);
                firstColumn = false;
            }
            return builder.ToString();
        }
    }
}
