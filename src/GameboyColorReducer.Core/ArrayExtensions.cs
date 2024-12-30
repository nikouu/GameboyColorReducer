using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyColorReducer.Core
{
    public static class ArrayExtensions
    {
        // https://docs.microsoft.com/en-us/archive/msdn-magazine/2017/june/essential-net-custom-iterators-with-yield
        /// <summary>
        /// An iterator to iterate through a <see cref="T:[,]"/>.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="T:[,]"/>.</typeparam>
        /// <param name="twoDimensionalArray">The <see cref="T:[,]"/> to iterate through.</param>
        /// <returns>Each element of the <see cref="T:[,]"/> in turn when being iterated.</returns>
        public static IEnumerable<T> ToIEnumerable<T>(this T[,] twoDimensionalArray)
        {
            for (int i = 0; i < twoDimensionalArray.GetLength(0); i++)
            {
                for (int j = 0; j < twoDimensionalArray.GetLength(1); j++)
                {
                    yield return twoDimensionalArray[i, j];
                }
            }
        }

        //https://stackoverflow.com/a/13091986
        /// <summary>
        /// Checks a <see cref="IEnumerable{T}"/> contains all elements of another <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="IEnumerable{T}"/> type.</typeparam>
        /// <param name="containingList">The containing list we will look for the <paramref name="lookupList"/>.</param>
        /// <param name="lookupList">The list to look for in the <paramref name="containingList"/>.</param>
        /// <returns>True if <paramref name="containingList"/> contains all <paramref name="lookupList"/>, otherwise false.</returns>
        public static bool ContainsAll<T>(this IEnumerable<T> containingList, IEnumerable<T> lookupList)
        {
            //var a = ContainsAllNew(containingList, lookupList);
            //var b = ContainsAllOld(containingList, lookupList);
            //if (a != b)
            //    throw new InvalidOperationException();
            //return a;

            return ContainsAllNew(containingList, lookupList);
        }

        public static bool ContainsAllOld<T>(this IEnumerable<T> containingList, IEnumerable<T> lookupList)
        {
            return !lookupList.Except(containingList).Any();
        }

        public static bool ContainsAllNew<T>(this IEnumerable<T> containingList, IEnumerable<T> lookupList)
        {
            if (containingList == null)
                throw new ArgumentNullException(nameof(containingList));
            if (lookupList == null)
                throw new ArgumentNullException(nameof(lookupList));

            foreach (var item in lookupList)
            {
                bool found = false;
                foreach (var element in containingList)
                {
                    if (EqualityComparer<T>.Default.Equals(element, item))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
            }
            return true;
        }
    }
}
