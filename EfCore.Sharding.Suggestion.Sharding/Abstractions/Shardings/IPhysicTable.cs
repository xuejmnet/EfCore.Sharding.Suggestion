using System;

namespace EfCore.Sharding.Suggestion.Sharding.Abstractions.Shardings
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 13:54:46
* @Email: 326308290@qq.com
*/
    public interface IPhysicTable
    {
        /// <summary>
        /// 表全称
        /// </summary>
        string FullName=>$"{OriginalName}_{Tail}";
        /// <summary>
        /// 原表名称
        /// </summary>
        string OriginalName { get;}
        /// <summary>
        /// 尾巴
        /// </summary>
        string Tail { get;}
        /// <summary>
        /// 映射类类型
        /// </summary>
        Type VirtualType { get; }

    }
}