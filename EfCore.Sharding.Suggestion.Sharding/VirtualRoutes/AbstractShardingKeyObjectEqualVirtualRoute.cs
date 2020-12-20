using System;
using System.Linq.Expressions;
using EfCore.Sharding.Suggestion.Sharding.Abstractions;

namespace EfCore.Sharding.Suggestion.Sharding.VirtualRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 19 December 2020 16:00:15
* @Email: 326308290@qq.com
*/
    public abstract  class AbstractShardingKeyObjectEqualVirtualRoute<T,TKey>: AbstractShardingOperatorVirtualRoute<T,TKey> where T : class, IShardingEntity
    {

        /// <summary>
        /// 如何路由到具体表 shardingKeyValue:分表的值,operate where的操作值 返回结果:如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表
        /// </summary>
        /// <param name="shardingKeyValue">分表的值</param>
        /// <param name="operate">分表where操作类型</param>
        /// <returns>如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表</returns>
        protected override Expression<Func<string, bool>> GetRouteToFilter(TKey shardingKeyValue, ShardingOperatorEnum operate)
        {
            if (operate == ShardingOperatorEnum.Equal)
                return GetRouteEqualToFilter(shardingKeyValue);
            
            return s => true;
        }

        /// <summary>
        /// 如何路由到具体表 shardingKeyValue:分表的值, 返回结果:如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表
        /// </summary>
        /// <param name="shardingKeyValue">分表的值</param>
        /// <returns>如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表</returns>
        protected abstract Expression<Func<string, bool>> GetRouteEqualToFilter(TKey shardingKeyValue);
    }
}