using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EfCore.Sharding.Suggestion.Sharding.EFCores
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 16 December 2020 16:13:05
* @Email: 326308290@qq.com
*/
    public class ShardingModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context)
        {
            return new ShardingModelCacheKey(context);
        }
    }
}