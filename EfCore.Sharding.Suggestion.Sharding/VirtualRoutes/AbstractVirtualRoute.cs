using System.Collections.Generic;
using System.Linq;
using EfCore.Sharding.Suggestion.Sharding.Abstractions;
using EfCore.Sharding.Suggestion.Sharding.Abstractions.Shardings;

namespace EfCore.Sharding.Suggestion.Sharding.VirtualRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 14:33:01
* @Email: 326308290@qq.com
*/
    public abstract class AbstractVirtualRoute<T>:IVirtualRoute<T> where T:class,IShardingEntity
    {
        public abstract List<IPhysicTable> RouteWithWhere(List<IPhysicTable> allPhysicTables, ShardingEntityConfig shardingEntityConfig, IQueryable<T> queryable);

        public abstract IPhysicTable RouteWithValue(List<IPhysicTable> allPhysicTables, ShardingEntityConfig shardingEntityConfig, object shardingKeyValue);
    }
}