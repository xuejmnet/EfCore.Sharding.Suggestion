using System;
using System.Collections.Generic;

namespace EfCore.Sharding.Suggestion.Sharding.Abstractions.Shardings
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 14:10:03
* @Email: 326308290@qq.com
*/
    public interface IVirtualTableManager
    {
        /// <summary>
        /// 获取所有的虚拟表
        /// </summary>
        /// <returns></returns>
        List<IVirtualTable> GetAllVirtualTables();
        /// <summary>
        /// 获取虚拟表
        /// </summary>
        /// <param name="shardingEntityType"></param>
        /// <returns></returns>
        IVirtualTable GetVirtualTable(Type shardingEntityType);
        /// <summary>
        /// 添加虚拟表
        /// </summary>
        /// <param name="virtualTable"></param>
        void AddVirtualTable(IVirtualTable virtualTable);
        /// <summary>
        /// 添加物理表
        /// </summary>
        /// <param name="virtualTable"></param>
        /// <param name="physicTable"></param>
        void AddPhysicTable(IVirtualTable virtualTable, IPhysicTable physicTable);
        /// <summary>
        /// 添加物理表
        /// </summary>
        /// <param name="shardingEntityType"></param>
        /// <param name="physicTable"></param>
        void AddPhysicTable(Type shardingEntityType, IPhysicTable physicTable);

    }
}