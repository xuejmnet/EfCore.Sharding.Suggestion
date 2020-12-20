using System;
using System.Collections.Generic;
using EfCore.Sharding.Suggestion.Sharding.Impls.Shardings;

namespace EfCore.Sharding.Suggestion.Sharding.Abstractions.Shardings
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 14:06:31
* @Email: 326308290@qq.com
*/
    public interface IVirtualTable
    {
        /// <summary>
        /// 分表的类型
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// 分表的配置
        /// </summary>
        ShardingEntityConfig ShardingConfig { get; }

        /// <summary>
        /// 获取所有的物理表
        /// </summary>
        /// <returns></returns>
        List<IPhysicTable> GetAllPhysicTables();

        /// <summary>
        /// 路由到具体的物理表
        /// </summary>
        /// <param name="routeConfig"></param>
        /// <returns></returns>
        List<IPhysicTable> RouteTo(RouteConfig routeConfig);

        /// <summary>
        /// 添加物理表
        /// </summary>
        /// <param name="physicTable"></param>
        void AddPhysicTable(IPhysicTable physicTable);

        void SetOriginalTableName(string originalTableName);
        string GetOriginalTableName();
    }

    public interface IVirtualTable<out T> : IVirtualTable where T : class, IShardingEntity
    {
    }
}