using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using EfCore.Sharding.Suggestion.Sharding.Abstractions;
using EfCore.Sharding.Suggestion.Sharding.Abstractions.Shardings;
using EfCore.Sharding.Suggestion.Sharding.Extensions;

namespace EfCore.Sharding.Suggestion.Sharding.VirtualRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 19 December 2020 13:47:54
* @Email: 326308290@qq.com
*/
    public class AutoDateDayVirtualRoute<T>:AbstractShardingOperatorVirtualRoute<T,long> where T:class,IShardingEntity
    {
        protected override long ConvertShardingKeyValue(object shardingKeyValue)
        {
            return (long) shardingKeyValue;
        }

        protected override Expression<Func<string, bool>> GetRouteToFilter(long shardingKeyValue, ShardingOperatorEnum operate)
        {
            switch (operate)
            {
                case ShardingOperatorEnum.NotSupport:
                    return tail => true;
                    //throw new NotSupportedException(xxxx);
                    break;
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    //yyyyMMdd
                    return tail =>int.Parse(tail) >= int.Parse(shardingKeyValue.ConvertLongToTime().ToString("yyyyMMdd"));
                    break;
                case ShardingOperatorEnum.LessThan:
                case ShardingOperatorEnum.LessThanOrEqual:
                    //yyyyMMdd
                    return tail =>int.Parse(tail) <= int.Parse(shardingKeyValue.ConvertLongToTime().ToString("yyyyMMdd"));
                    break;
                case ShardingOperatorEnum.Equal:
                    //yyyyMMdd
                    return tail =>int.Parse(tail) == int.Parse(shardingKeyValue.ConvertLongToTime().ToString("yyyyMMdd"));
                    break;
                case ShardingOperatorEnum.NotEqual:
                    //yyyyMMdd
                    return tail =>int.Parse(tail) != int.Parse(shardingKeyValue.ConvertLongToTime().ToString("yyyyMMdd"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operate), operate, null);
            }
        }
    }
}