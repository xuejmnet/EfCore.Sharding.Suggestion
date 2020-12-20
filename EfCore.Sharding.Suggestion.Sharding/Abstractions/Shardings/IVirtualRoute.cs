using System.Collections.Generic;
using System.Linq;

namespace EfCore.Sharding.Suggestion.Sharding.Abstractions.Shardings
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 13:59:36
* @Email: 326308290@qq.com
*/
    public interface IVirtualRoute<T>where T:class,IShardingEntity
    {
        /// <summary>
        /// 根据查询条件路由返回物理表
        /// </summary>
        /// <param name="allPhysicTables"></param>
        /// <param name="shardingEntityConfig"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        List<IPhysicTable> RouteWithWhere(List<IPhysicTable> allPhysicTables,ShardingEntityConfig shardingEntityConfig,IQueryable<T> queryable);
        /// <summary>
        /// 根据值进行路由
        /// </summary>
        /// <param name="allPhysicTables"></param>
        /// <param name="shardingEntityConfig"></param>
        /// <param name="shardingKeyValue"></param>
        /// <returns></returns>
        IPhysicTable RouteWithValue(List<IPhysicTable> allPhysicTables,ShardingEntityConfig shardingEntityConfig,object shardingKeyValue);
    }
}