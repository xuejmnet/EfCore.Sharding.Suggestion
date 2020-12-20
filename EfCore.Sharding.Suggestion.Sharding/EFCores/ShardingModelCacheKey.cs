using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EfCore.Sharding.Suggestion.Sharding.EFCores
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 16 December 2020 16:22:17
* @Email: 326308290@qq.com
*/
    internal sealed class ShardingModelCacheKey : ModelCacheKey
    {
        string _tail { get; }
        public ShardingModelCacheKey(DbContext context) : base(context)
        {
            this._tail = (context as ShardingDbContext)?.Tail ?? string.Empty;
        }

        protected override bool Equals(ModelCacheKey other)
        {
            return base.Equals(other) && (other as ShardingModelCacheKey)?._tail == _tail;
        }

        public override int GetHashCode()
        {
            var hashCode = base.GetHashCode() * 397;
            if (!string.IsNullOrWhiteSpace(_tail))
            {
                hashCode ^= _tail.GetHashCode();
            }
            return hashCode;
        }
    }
}