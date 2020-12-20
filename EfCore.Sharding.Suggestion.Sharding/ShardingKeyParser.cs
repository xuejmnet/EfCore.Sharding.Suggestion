using System;
using System.Linq;
using System.Reflection;
using EfCore.Sharding.Suggestion.Sharding.Abstractions;

namespace EfCore.Sharding.Suggestion.Sharding
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 16 December 2020 11:21:12
* @Email: 326308290@qq.com
*/
    public class ShardingKeyParser
    {
        private ShardingKeyParser(){}

        public static ShardingEntityConfig Parse(Type entityType)
        {
            if(!typeof(IShardingEntity).IsAssignableFrom(entityType))
                throw new NotSupportedException(entityType.ToString());
            PropertyInfo[] shardingProperties = entityType.GetProperties();
            ShardingEntityConfig shardingEntityConfig=null;
            foreach (var shardingProperty in shardingProperties)
            {
                var attribbutes=shardingProperty.GetCustomAttributes(true);
                if (attribbutes.FirstOrDefault(x => x.GetType() == typeof(ShardingKeyAttribute)) is ShardingKeyAttribute shardingKeyAttribute)
                {
                    if(shardingEntityConfig!=null)
                        throw new ArgumentException($"{entityType}存在多个[ShardingKeyAttribute]");
                    shardingEntityConfig = new ShardingEntityConfig()
                    {
                        ShardingMode = shardingKeyAttribute.ShardingMode,
                        ShardingEntityType = entityType,
                        BeginTableTimeStamp = shardingKeyAttribute.BeginTableTimeStamp,
                        ShardingField = shardingProperty.Name
                    };

                }
            }
            return shardingEntityConfig;
        }
    }
}