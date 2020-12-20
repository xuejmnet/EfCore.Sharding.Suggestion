using System.ComponentModel;

namespace EfCore.Sharding.Suggestion.Sharding.VirtualRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 19 December 2020 19:56:57
* @Email: 326308290@qq.com
*/
    public enum ShardingOperatorEnum
    {
        [Description("??")]
        NotSupport,
        [Description(">")]
        GreaterThan,
        [Description(">=")]
        GreaterThanOrEqual,
        [Description("<")]
        LessThan,
        [Description("<=")]
        LessThanOrEqual,
        [Description("==")]
        Equal,
        [Description("!=")]
        NotEqual,
        // Contains
    }
}