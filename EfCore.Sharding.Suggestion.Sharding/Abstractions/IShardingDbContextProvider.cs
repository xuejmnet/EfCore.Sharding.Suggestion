using System;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Sharding.Suggestion.Sharding.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 16:31:08
* @Email: 326308290@qq.com
*/
    public interface IShardingDbContextProvider
    {
        
        /// <summary>
        /// 创建共享连接的dbcontext
        /// </summary>
        /// <param name="dbContextOptions"></param>
        /// <param name="tail"></param>
        /// <returns></returns>
        ShardingDbContext CreateShareShardingDbContext(DbContextOptions dbContextOptions,string tail);
        /// <summary>
        /// 创建单类型DbContext
        /// </summary>
        /// <param name="tail"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>

        ShardingDbContext CreateSingleShardingDbContext(string tail, Type entityType);
        /// <summary>
        /// 创建独立的连接 需要自己释放
        /// </summary>
        /// <param name="tail"></param>
        /// <returns></returns>
        ShardingDbContext CreateAloneShardingDbContext(string tail);
    }
}