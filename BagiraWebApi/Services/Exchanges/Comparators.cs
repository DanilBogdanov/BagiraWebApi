using BagiraWebApi.Models.Bagira;
using BagiraWebApi.Services.Exchanges.DataModels;
using BagiraWebApi.Services.Exchanges.DataModels.DTO;
using System.Diagnostics.CodeAnalysis;

namespace BagiraWebApi.Services.Exchanges
{
    public static class Comparators
    {
        public class GoodIdComparator : IEqualityComparer<GoodDataVersionDTO>
        {
            public bool Equals(GoodDataVersionDTO? x, GoodDataVersionDTO? y)
            {
                return x?.Id == y?.Id;
            }

            public int GetHashCode([DisallowNull] GoodDataVersionDTO obj)
            {
                return HashCode.Combine(obj.Id);
            }
        }

        public class GoodDataVersionComparator : IEqualityComparer<GoodDataVersionDTO>
        {
            public bool Equals(GoodDataVersionDTO? x, GoodDataVersionDTO? y)
            {
                return x?.Id == y?.Id && x?.DataVersion == y?.DataVersion;
            }

            public int GetHashCode([DisallowNull] GoodDataVersionDTO obj)
            {
                return HashCode.Combine(obj.Id, obj.DataVersion);
            }
        }
    }
}
