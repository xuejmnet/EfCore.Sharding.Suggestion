using System;
using System.Collections.Generic;
using System.Linq;
using EfCore.Sharding.Suggestion.Sharding.Abstractions;
using EfCore.Sharding.Suggestion.Sharding.Abstractions.Shardings;

namespace EfCore.Sharding.Suggestion.Sharding.Impls
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 19 December 2020 23:09:01
* @Email: 326308290@qq.com
*/
    public class DefaultRepository:IRepository
    {
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly IShardingDbContextProvider _shardingDbContextProvider;

        public DefaultRepository(IVirtualTableManager virtualTableManager,IShardingDbContextProvider shardingDbContextProvider)
        {
            _virtualTableManager = virtualTableManager;
            _shardingDbContextProvider = shardingDbContextProvider;
        }
        public IShardingQueryable<T> GetSharding<T>() where T : class, IShardingEntity
        {
            return new ShardingIQueryable<T>(new List<T>(0).AsQueryable(),_virtualTableManager, _shardingDbContextProvider);
        }
    }
}