using System.Linq;

namespace EfCore.Sharding.Suggestion.Sharding.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 19 December 2020 23:07:36
* @Email: 326308290@qq.com
*/
    public interface IRepository
    {
        IShardingQueryable<T> GetSharding<T>() where T : class, IShardingEntity;
    }
}