using System;
using EfCore.Sharding.Suggestion.Sharding.Abstractions.Shardings;

namespace EfCore.Sharding.Suggestion.Sharding.Impls.Shardings
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 13:57:50
* @Email: 326308290@qq.com
*/
/// <summary>
/// 默认的物理表
/// </summary>
    public class DefaultPhysicTable:IPhysicTable
    {
        public DefaultPhysicTable(string originalName, string tail, Type virtualType)
        {
            OriginalName = originalName;
            Tail = tail;
            VirtualType = virtualType;
        }
        public string OriginalName { get; }
        public string Tail { get;  }
        public Type VirtualType { get;  }
    }
}