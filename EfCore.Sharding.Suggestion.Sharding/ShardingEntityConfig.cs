using System;
using EfCore.Sharding.Suggestion.Sharding.Abstractions;
using EfCore.Sharding.Suggestion.Sharding.Extensions;

namespace EfCore.Sharding.Suggestion.Sharding
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 16 December 2020 13:24:05
* @Email: 326308290@qq.com
*/
    public class ShardingEntityConfig
    {
        public Type ShardingEntityType { get; set; }
        public string ShardingField { get; set; }
        /// <summary>
        /// 分表的原表名
        /// </summary>
        public string ShardingOriginalTable { get; set; }

        /// <summary>
        /// 分表的模式
        /// </summary>
        public ShardingModeEnum ShardingMode { get; set; }

        /// <summary>
        /// 按时间分表的开始时间
        /// </summary>
        public long BeginTableTimeStamp { get; set; }


        public string GetTableTailByField(object fieldValue)
        {
            if (fieldValue is long timeStamp)
            {
                var valueTime = timeStamp.ConvertLongToTime();
                
                return ShardingMode switch
                {
                    ShardingModeEnum.Day => valueTime.ToString("yyyyMMdd"),
                    ShardingModeEnum.Week => GetWeekTableTail(valueTime),
                    ShardingModeEnum.Month => valueTime.ToString("yyyyMM"),
                    ShardingModeEnum.Year => valueTime.ToString("yyyy"),
                    _ => throw new NotImplementedException("ShardingModeEnum无效")
                };
            }

            throw new NotSupportedException($"{ShardingEntityType}.{ShardingField}不支持仅支持long类型");
        }

        private string GetWeekTableTail(DateTime dateTime)
        {
            var sunday = dateTime.GetSunday();
            var monday = dateTime.GetMonday();
            return $"{dateTime:yyyyMM}{monday:dd}_{sunday:dd}";
        }


        /// <summary>
        /// 根据实体获取表后缀
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetTableTailByEntity(object entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            var property = entity.GetPropertyValue(ShardingField);

            return GetTableTailByField(property);
        }
    }
}