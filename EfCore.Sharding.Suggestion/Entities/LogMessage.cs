using EfCore.Sharding.Suggestion.Sharding.Abstractions;

namespace EfCore.Sharding.Suggestion.Entities
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 17 December 2020 11:46:01
* @Email: 326308290@qq.com
*/
    public class LogMessage:IShardingEntity
    {
        public string Id { get; set; }
        /// <summary>
        /// 当前时间
        /// </summary>
        [ShardingKey(ShardingMode = ShardingModeEnum.Day,BeginTableTimeStamp = 1608091835572)]
        public long CurrentTime { get; set; }
        public string Name { get; set; }
    }
}