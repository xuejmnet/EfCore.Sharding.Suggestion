using System.Collections.Generic;
using EfCore.Sharding.Suggestion.Sharding.Abstractions.Shardings;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Sharding.Suggestion.Sharding
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 16 December 2020 16:15:43
* @Email: 326308290@qq.com
*/
    public class ShardingDbContextOptions
    {
        public ShardingDbContextOptions(DbContextOptions dbContextOptions, string tail, List<IVirtualTable> virtualTables)
        {
            DbContextOptions = dbContextOptions;
            Tail = tail;
            VirtualTables = virtualTables;
        }

        public DbContextOptions  DbContextOptions { get; }
        public string Tail { get; }
        public List<IVirtualTable> VirtualTables { get; }
    }
}