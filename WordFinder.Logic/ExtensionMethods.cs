using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordFinder.Logic
{
    public static class ExtensionMethods
    {
        public static IEnumerable<char> GetColumn(this char[][] array, int col)
        {
            for (int i = 0; i < array.Length; i++)
            {
                yield return array[i][col];
            }
        }

    }
}
