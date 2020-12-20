using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EfCore.Sharding.Suggestion.Sharding.Abstractions;
using EfCore.Sharding.Suggestion.Sharding.Abstractions.Shardings;

namespace EfCore.Sharding.Suggestion.Sharding.VirtualRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 19 December 2020 19:55:24
* @Email: 326308290@qq.com
*/
    public abstract class AbstractShardingOperatorVirtualRoute<T, TKey> : AbstractVirtualRoute<T> where T : class, IShardingEntity
    {
        public override List<IPhysicTable> RouteWithWhere(List<IPhysicTable> allPhysicTables, ShardingEntityConfig shardingEntityConfig, IQueryable<T> queryable)
        {
            //获取所有需要路由的表后缀
            var filter = ShardingKeyUtil.GetRouteObjectOperatorFilter(queryable, shardingEntityConfig, ConvertShardingKeyValue, GetRouteToFilter);
            var physicTables = allPhysicTables.Where(o => filter(o.Tail)).ToList();
            if (physicTables.Count > 1)
                throw new Exception($"表:{string.Join(",", physicTables.Select(o => $"[{o.FullName}]"))}");
            return physicTables;
        }

        /// <summary>
        /// 如何转换 shardingKeyValue:分表的值
        /// </summary>
        /// <param name="shardingKeyValue">分表的值</param>
        /// <returns></returns>
        protected abstract TKey ConvertShardingKeyValue(object shardingKeyValue);

        /// <summary>
        /// 如何路由到具体表 shardingKeyValue:分表的值, 返回结果:如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表
        /// </summary>
        /// <param name="shardingKeyValue">分表的值</param>
        /// <param name="operate">操作</param>
        /// <returns>如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表</returns>
        protected abstract Expression<Func<string, bool>> GetRouteToFilter(TKey shardingKeyValue, ShardingOperatorEnum operate);

        public override IPhysicTable RouteWithValue(List<IPhysicTable> allPhysicTables, ShardingEntityConfig shardingEntityConfig, object shardingKeyValue)
        {
            var filter = GetRouteToFilter(ConvertShardingKeyValue(shardingKeyValue), ShardingOperatorEnum.Equal).Compile();
            var physicTable = allPhysicTables.FirstOrDefault(o => filter(o.Tail));
            if (physicTable == null)
                throw new Exception($"{shardingEntityConfig.ShardingEntityType} -> [{shardingEntityConfig.ShardingField}] -> <{shardingEntityConfig.ShardingMode}> -> 【{shardingKeyValue}】");
            return physicTable;
        }
    }
}