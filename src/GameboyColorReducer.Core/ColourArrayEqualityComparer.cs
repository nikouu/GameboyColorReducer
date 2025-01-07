using GameboyColorReducer.Core.Models;

namespace GameboyColorReducer.Core
{
    public class ColourArrayEqualityComparer : IEqualityComparer<Colour[]>
    {
        public bool Equals(Colour[] x, Colour[] y)
        {
            if (x == null || y == null)
                return x == y;

            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (!x[i].Equals(y[i]))
                    return false;
            }

            return true;
        }

        public int GetHashCode(Colour[] obj)
        {
            if (obj == null)
                return 0;

            int hash = 17;
            foreach (var colour in obj)
            {
                hash = hash * 31 + colour.GetHashCode();
            }

            return hash;
        }
    }
}
