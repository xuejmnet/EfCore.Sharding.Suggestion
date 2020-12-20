using System;
using EfCore.Sharding.Suggestion.Sharding.Abstractions;
using EfCore.Sharding.Suggestion.Sharding.Abstractions.Shardings;
using EfCore.Sharding.Suggestion.Sharding.Extensions;
using EfCore.Sharding.Suggestion.Sharding.Impls.Shardings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace EfCore.Sharding.Suggestion.Sharding
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 20 December 2020 10:56:20
* @Email: 326308290@qq.com
*/
    public class ShardingBootstrapper
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly IShardingDbContextProvider _shardingDbContextProvider;

        public ShardingBootstrapper(IServiceProvider serviceProvider,IVirtualTableManager virtualTableManager,IShardingDbContextProvider shardingDbContextProvider)
        {
            _serviceProvider = serviceProvider;
            _virtualTableManager = virtualTableManager;
            _shardingDbContextProvider = shardingDbContextProvider;
        }

        public void Start()
        {
            var virtualTables = _virtualTableManager.GetAllVirtualTables();
            using var scope = _serviceProvider.CreateScope();
            using var context = _shardingDbContextProvider.CreateAloneShardingDbContext(string.Empty);
            foreach (var virtualTable in virtualTables)
            {
                //获取ShardingEntity的实际表名
                var tableName = context.Model.FindEntityType(virtualTable.EntityType).GetTableName();
                virtualTable.SetOriginalTableName(tableName);
                CreateDateTable(virtualTable);
            }
        }

        private void CreateDateTable(IVirtualTable virtualTable)
        {
            var virtualTableShardingConfig = virtualTable.ShardingConfig;
            var beginTime = virtualTableShardingConfig.BeginTableTimeStamp;
            var shardingModeEnum = virtualTableShardingConfig.ShardingMode;
            Func<long, long> nextTime = shardingModeEnum switch
            {
                ShardingModeEnum.Day => x => x.ConvertLongToTime().Date.AddDays(1).ConvertTimeToLong(),
                ShardingModeEnum.Week => x => x.ConvertLongToTime().Date.AddDays(7).ConvertTimeToLong(),
                ShardingModeEnum.Month => x => x.ConvertLongToTime().Date.AddMonths(1).ConvertTimeToLong(),
                ShardingModeEnum.Year => x => x.ConvertLongToTime().Date.AddYears(1).ConvertTimeToLong(),
            };
            var nowTimeStamp = DateTime.Now.Date.ConvertTimeToLong();
            if (beginTime > nowTimeStamp)
                throw new ArgumentException("起始时间不正确无法生成正确的表名");
            var currentTimeStamp = beginTime;
            while (currentTimeStamp <= nowTimeStamp)
            {
                var tail = virtualTableShardingConfig.GetTableTailByField(currentTimeStamp);
                CreateTable(virtualTable, tail);
                //添加物理表
                virtualTable.AddPhysicTable(new DefaultPhysicTable(virtualTable.GetOriginalTableName(),tail,virtualTable.EntityType));
                currentTimeStamp = nextTime(currentTimeStamp);
            }
        }

        private void CreateTable(IVirtualTable virtualTable,string suffix)
        {
            using var dbContext = _shardingDbContextProvider.CreateSingleShardingDbContext(suffix, virtualTable.EntityType);
            var databaseCreator = dbContext.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
            try
            {
                databaseCreator.CreateTables();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.GetType());
            }
        }
    }
}