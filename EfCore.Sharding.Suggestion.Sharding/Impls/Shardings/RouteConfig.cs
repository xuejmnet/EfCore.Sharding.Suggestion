using System.Linq;
using EfCore.Sharding.Suggestion.Sharding.Abstractions;

namespace EfCore.Sharding.Suggestion.Sharding.Impls.Shardings
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 14:15:02
* @Email: 326308290@qq.com
*/
    public class RouteConfig
    {
        private readonly IQueryable _queryable;
        private readonly IShardingEntity _shardingEntity;
        private readonly object _shardingKeyValue;


        public RouteConfig(IQueryable queryable,IShardingEntity shardingEntity,object shardingKeyValue)
        {
            _queryable = queryable;
            _shardingEntity = shardingEntity;
            _shardingKeyValue = shardingKeyValue;
        }

        public IQueryable GetQueryable()
        {
            return _queryable;
        }
        public object GetShardingKeyValue()
        {
            return _shardingKeyValue;
        }

        public IShardingEntity GetShardingEntity()
        {
            return _shardingEntity;
        }

        public bool UseQueryable()
        {
            return _queryable != null;
        }

        public bool UseValue()
        {
            return _shardingKeyValue != null;
        }

        public bool UseEntity()
        {
            return _shardingEntity != null;
        }
    }
}