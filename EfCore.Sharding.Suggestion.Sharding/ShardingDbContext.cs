using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EfCore.Sharding.Suggestion.Sharding.Abstractions.Shardings;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Sharding.Suggestion.Sharding
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 16 December 2020 15:28:12
* @Email: 326308290@qq.com
*/
    public class ShardingDbContext : DbContext
    {
        public string Tail { get; }
        public List<IVirtualTable> VirtualTables { get; }
        private static readonly ConcurrentDictionary<Type, Type> _entityTypeConfigurationTypeCaches = new ConcurrentDictionary<Type, Type>();

        public ShardingDbContext(ShardingDbContextOptions shardingDbContextOptions) : base(shardingDbContextOptions.DbContextOptions)
        {
            Tail = shardingDbContextOptions.Tail;
            VirtualTables = shardingDbContextOptions.VirtualTables;
        }

        /// <summary>
        /// 模型构建
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var shardingEntities = VirtualTables.Select(o => o.EntityType).ToList();

            //支持IEntityTypeConfiguration配置
            shardingEntities.ForEach(aEntityType =>
            {
                if (!_entityTypeConfigurationTypeCaches.TryGetValue(aEntityType, out var entityTypeConfigurationType))
                {
                    entityTypeConfigurationType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(o => o.GetTypes())
                        .Where(type => !String.IsNullOrEmpty(type.Namespace))
                        //获取类型namespce不是空的所有接口是范型的当前范型是IEntityTypeConfiguration<>的进行fluent api 映射
                        .Where(type => !type.IsAbstract && type.GetInterfaces().Any(it => it.IsInterface && it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)
                                                                                          &&it.GetGenericArguments().Any()&& aEntityType == it.GetGenericArguments()[0])).FirstOrDefault();
                    _entityTypeConfigurationTypeCaches.TryAdd(aEntityType, entityTypeConfigurationType);
                }

                if (entityTypeConfigurationType == null)
                    throw new Exception($"{aEntityType}的[IBaseEntityTypeConfiguration]未找到");
                dynamic configurationInstance = Activator.CreateInstance(entityTypeConfigurationType);
                modelBuilder.ApplyConfiguration(configurationInstance);
            });

            if (!string.IsNullOrWhiteSpace(Tail))
            {
                shardingEntities.ForEach(shardingEntity =>
                {
                    var entity = modelBuilder.Entity(shardingEntity);

                    var tableName = VirtualTables.FirstOrDefault(o => o.EntityType == shardingEntity)?.GetOriginalTableName();
                    if (string.IsNullOrWhiteSpace(tableName))
                        throw new ArgumentNullException($"{shardingEntity}:无法找到对应的原始表名。");
#if DEBUG
                    Console.WriteLine($"映射表:[tableName]-->[{tableName}_{Tail}]");
#endif
                    entity.ToTable($"{tableName}_{Tail}");
                });
            }

            //字段注释,需要开启程序集XML文档
            //
            // foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            // {
            //     var comments = XmlHelper.GetPropertyCommentBySummary(entityType.ClrType) ?? new Dictionary<string, string>();
            //     foreach (var property in entityType.GetProperties())
            //     {
            //         if (comments.ContainsKey(property.Name))
            //         {
            //             property.SetComment(comments[property.Name]);
            //         }
            //     }
            // }
//
// #if !EFCORE2
//             //字段注释,需要开启程序集XML文档
//             if (ShardingOption.EnableComments)
//             {
//                 foreach (var entityType in modelBuilder.Model.GetEntityTypes())
//                 {
//                     var comments = XmlHelper.GetProperyCommentBySummary(entityType.ClrType) ?? new Dictionary<string, string>();
//                     foreach (var property in entityType.GetProperties())
//                     {
//                         if (comments.ContainsKey(property.Name))
//                         {
//                             property.SetComment(comments[property.Name]);
//                         }
//                     }
//                 }
//             }
// #endif
        }
    }
}