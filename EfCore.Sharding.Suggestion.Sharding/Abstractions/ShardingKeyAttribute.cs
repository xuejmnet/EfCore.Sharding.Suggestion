using System;

namespace EfCore.Sharding.Suggestion.Sharding.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 16 December 2020 11:04:51
* @Email: 326308290@qq.com
*/
/// <summary>
/// AbstractVirtualRoute 最基础分表规则 需要自己解析如何分表
/// 仅ShardingMode为Custom:以下接口提供自定义分表
/// AbstractShardingKeyObjectEqualVirtualRoute 自定义分表 
/// SimpleShardingKeyStringModVirtualRoute 默认对AbstractShardingKeyObjectEqualVirtualRoute的实现,字符串按取模分表
/// 仅ShardingMode非Custom：以下接口提供自动按时间分表
/// AutoDateVirtualRoute 按系统时间默认分表 
/// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ShardingKeyAttribute:Attribute
    {
        /// <summary>
        /// 分表的模式
        /// </summary>
        public ShardingModeEnum ShardingMode { get; set; } = ShardingModeEnum.Week;
        /// <summary>
        /// 按时间分表的开始时间
        /// </summary>
        public long BeginTableTimeStamp { get; set; }
    }
}