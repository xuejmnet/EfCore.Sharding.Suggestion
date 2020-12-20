using System;
using System.Collections.Generic;
using EfCore.Sharding.Suggestion.Sharding.Abstractions;
using EfCore.Sharding.Suggestion.Sharding.Abstractions.Shardings;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Sharding.Suggestion.Sharding
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 20 December 2020 10:48:38
* @Email: 326308290@qq.com
*/
    public class ShardingDbContextProvider : IShardingDbContextProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IVirtualTableManager _virtualTableManager;

        public ShardingDbContextProvider(IServiceProvider serviceProvider, IVirtualTableManager virtualTableManager)
        {
            _serviceProvider = serviceProvider;
            _virtualTableManager = virtualTableManager;
        }

        public ShardingDbContext CreateShareShardingDbContext(DbContextOptions dbContextOptions,string tail)
        {
            var op = new ShardingDbContextOptions(dbContextOptions, tail, _virtualTableManager.GetAllVirtualTables());
            return new ShardingDbContext(op);
        }

        public ShardingDbContext CreateSingleShardingDbContext(string tail, Type entityType)
        {
            var virtualTable = _virtualTableManager.GetVirtualTable(entityType);
            var dbContextOptions = (DbContextOptions)_serviceProvider.GetService(typeof(DbContextOptions));
            
            var op = new ShardingDbContextOptions(dbContextOptions, tail, new List<IVirtualTable>() {virtualTable});
            return new ShardingDbContext(op);
        }

        public ShardingDbContext CreateAloneShardingDbContext(string tail)
        {
            var dbContextOptions = (DbContextOptions)_serviceProvider.GetService(typeof(DbContextOptions));
            var op = new ShardingDbContextOptions(dbContextOptions, tail, _virtualTableManager.GetAllVirtualTables());
            return new ShardingDbContext(op);
        }
    }
}