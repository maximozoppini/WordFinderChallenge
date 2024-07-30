using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordFinder.Logic
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// This extension method returns the matrix column as a List<char>
        /// </summary>
        /// <param name="array">jagged array of chars</param>
        /// <param name="col">column index to be returned</param>
        /// <returns></returns>
        public static IEnumerable<char> GetColumn(this char[][] array, int col)
        {
            for (int i = 0; i < array.Length; i++)
            {
                yield return array[i][col];
            }
        }

    }
}
