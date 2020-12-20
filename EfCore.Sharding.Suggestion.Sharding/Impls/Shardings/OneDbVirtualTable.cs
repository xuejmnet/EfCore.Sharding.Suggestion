using System;
using System.Collections.Generic;
using System.Linq;
using EfCore.Sharding.Suggestion.Sharding.Abstractions;
using EfCore.Sharding.Suggestion.Sharding.Abstractions.Shardings;
using EfCore.Sharding.Suggestion.Sharding.Extensions;

namespace EfCore.Sharding.Suggestion.Sharding.Impls.Shardings
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 14:20:12
* @Email: 326308290@qq.com
*/
/// <summary>
/// 同数据库虚拟表
/// </summary>
/// <typeparam name="T"></typeparam>
    public class OneDbVirtualTable<T>:IVirtualTable<T> where T:class,IShardingEntity
    {
        public Type EntityType => typeof(T);
        public ShardingEntityConfig ShardingConfig { get; }
        private readonly List<IPhysicTable> _physicTables=new List<IPhysicTable>();
        private readonly IVirtualRoute<T> _route;

        public OneDbVirtualTable(IServiceProvider serviceProvider)
        {
            ShardingConfig = ShardingKeyParser.Parse(EntityType);
            _route = (IVirtualRoute<T>)serviceProvider.GetService(typeof(IVirtualRoute<T>))??throw new NotImplementedException($"未实现:[IVirtualRoute<T>]接口 T:[{EntityType}]");
        }
        public List<IPhysicTable> GetAllPhysicTables()
        {
            return _physicTables;
        }

        public List<IPhysicTable> RouteTo(RouteConfig routeConfig)
        {
            if (routeConfig.UseQueryable())
                return _route.RouteWithWhere(_physicTables,ShardingConfig,(IQueryable<T>)routeConfig.GetQueryable());
            object shardingKeyValue = null;
            if (routeConfig.UseValue())
                shardingKeyValue = routeConfig.GetShardingKeyValue();

            if (routeConfig.UseEntity())
                shardingKeyValue = routeConfig.GetShardingEntity().GetPropertyValue(ShardingConfig.ShardingField);

            if (shardingKeyValue != null)
            {
                var routeWithValue = _route.RouteWithValue(_physicTables,ShardingConfig,shardingKeyValue);
                return new List<IPhysicTable>(1){routeWithValue};
            }
               
            throw new NotImplementedException(nameof(routeConfig));
        }


        public void AddPhysicTable(IPhysicTable physicTable)
        {
            if(!_physicTables.Contains(physicTable))
                _physicTables.Add(physicTable);
        }

        public void SetOriginalTableName(string originalTableName)
        {
            ShardingConfig.ShardingOriginalTable = originalTableName;
        }

        public string GetOriginalTableName()
        {
            return ShardingConfig.ShardingOriginalTable;
        }

    }
}