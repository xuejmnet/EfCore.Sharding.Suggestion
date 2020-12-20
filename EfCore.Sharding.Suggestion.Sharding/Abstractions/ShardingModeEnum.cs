namespace EfCore.Sharding.Suggestion.Sharding.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 16 December 2020 11:04:35
* @Email: 326308290@qq.com
*/
    public enum ShardingModeEnum
    {
        /// <summary>
        /// 按天分表
        /// </summary>
        Day,
        /// <summary>
        /// 按周分表
        /// </summary>
        Week,
        /// <summary>
        /// 按月分表
        /// </summary>
        Month,
        /// <summary>
        /// 按年分表
        /// </summary>
        Year,
        /// <summary>
        /// 自定义分表
        /// </summary>
        Custom
    }
}