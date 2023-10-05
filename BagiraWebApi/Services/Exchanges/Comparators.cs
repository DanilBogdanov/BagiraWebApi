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

        public class GoodStorageIdComparator : IEqualityComparer<GoodStorage>
        {
            public bool Equals(GoodStorage? x, GoodStorage? y)
            {
                return x?.Id == y?.Id;
            }

            public int GetHashCode([DisallowNull] GoodStorage obj)
            {
                return HashCode.Combine(obj.Id);
            }
        }

        public class GoodStorageNameComparator : IEqualityComparer<GoodStorage>
        {
            public bool Equals(GoodStorage? x, GoodStorage? y)
            {
                return x?.Id == y?.Id && x?.Name == y?.Name;
            }

            public int GetHashCode([DisallowNull] GoodStorage obj)
            {
                return HashCode.Combine(obj.Id, obj.Name);
            }
        }

        public class GoodPriceTypeIdComparator : IEqualityComparer<GoodPriceType>
        {
            public bool Equals(GoodPriceType? x, GoodPriceType? y)
            {
                return x?.Id == y?.Id;
            }

            public int GetHashCode([DisallowNull] GoodPriceType obj)
            {
                return HashCode.Combine(obj.Id);
            }
        }

        public class GoodPriceTypeNameComparator : IEqualityComparer<GoodPriceType>
        {
            public bool Equals(GoodPriceType? x, GoodPriceType? y)
            {
                return x?.Id == y?.Id && x?.Name == y?.Name;
            }

            public int GetHashCode([DisallowNull] GoodPriceType obj)
            {
                return HashCode.Combine(obj.Id, obj.Name);
            }
        }

        public class GoodPriceIdComparator : IEqualityComparer<GoodPrice>
        {
            public bool Equals(GoodPrice? x, GoodPrice? y)
            {
                return x?.GoodId == y?.GoodId && x?.PriceTypeId == y?.PriceTypeId;
            }

            public int GetHashCode([DisallowNull] GoodPrice obj)
            {
                return HashCode.Combine(obj.GoodId, obj.PriceTypeId);
            }
        }

        public class GoodPriceFullComparator : IEqualityComparer<GoodPrice>
        {
            public bool Equals(GoodPrice? x, GoodPrice? y)
            {
                return x?.GoodId == y?.GoodId
                    && x?.PriceTypeId == y?.PriceTypeId
                    && x?.Price == y?.Price;
            }

            public int GetHashCode([DisallowNull] GoodPrice obj)
            {
                return HashCode.Combine(obj.GoodId, obj.PriceTypeId, obj.Price);
            }
        }
    }
}
