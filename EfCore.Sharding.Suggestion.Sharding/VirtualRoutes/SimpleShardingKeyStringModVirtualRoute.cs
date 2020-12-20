using System;
using System.Linq.Expressions;
using EfCore.Sharding.Suggestion.Sharding.Abstractions;

namespace EfCore.Sharding.Suggestion.Sharding.VirtualRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 19 December 2020 13:52:32
* @Email: 326308290@qq.com
*/
    public class SimpleShardingKeyStringModVirtualRoute<T>:AbstractShardingKeyObjectEqualVirtualRoute<T,string> where T:class,IShardingEntity
    {
        private readonly int _mod;

        protected SimpleShardingKeyStringModVirtualRoute(int mod)
        {
            _mod = mod;
        }

        protected override string ConvertShardingKeyValue(object shardingKeyValue)
        {
            return shardingKeyValue.ToString();
        }

        protected override Expression<Func<string, bool>> GetRouteEqualToFilter(string shardingKeyValue)
        {
            
            var modKey = Math.Abs(shardingKeyValue.GetHashCode() % _mod);
            return s => s == modKey.ToString();
        }
    }
}