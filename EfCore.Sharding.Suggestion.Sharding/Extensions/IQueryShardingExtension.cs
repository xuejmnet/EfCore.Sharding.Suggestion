using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EfCore.Sharding.Suggestion.Sharding.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Sharding.Suggestion.Sharding.Extensions
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 16 December 2020 16:27:38
* @Email: 326308290@qq.com
*/
    public static class IQueryShardingExtension
    {
        /// <summary>
        /// 是否基继承至ShardingEntity
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static bool IsShardingEntity(this Type entityType)
        { 
            if(entityType==null)
                throw new ArgumentNullException(nameof(entityType));
            return typeof(IShardingEntity).IsAssignableFrom(entityType);
        }
        /// <summary>
        /// 是否基继承至ShardingEntity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool IsShardingEntity(this object entity)
        { 
            if(entity==null)
                throw new ArgumentNullException(nameof(entity));
            return typeof(IShardingEntity).IsAssignableFrom(entity.GetType());
        }
        public static IQueryable<T> GetIQueryable<T>(this ShardingDbContext shardingDbContext, bool track=false)where T:class,IShardingEntity
        {
            var q=shardingDbContext.Set<T>() as IQueryable<T>;
            if (!track)
                q = q.AsNoTracking();
            return q;
        }
        /// <summary>
        /// 判断是否为Null或者空
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this object obj)
        {
            if (obj == null)
                return true;
            else
            {
                string objStr = obj.ToString();
                return string.IsNullOrEmpty(objStr);
            }
        }
        /// <summary>
        /// 给IEnumerable拓展ForEach方法
        /// </summary>
        /// <typeparam name="T">模型类</typeparam>
        /// <param name="iEnumberable">数据源</param>
        /// <param name="func">方法</param>
        public static void ForEach<T>(this IEnumerable<T> iEnumberable, Action<T> func)
        {
            foreach (var item in iEnumberable)
            {
                func(item);
            }
        }
        /// <summary>
        /// 删除Skip表达式
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IQueryable<T> RemoveSkip<T>(this IQueryable<T> source)
        {
            return (IQueryable<T>)((IQueryable)source).RemoveSkip();
        }

        /// <summary>
        /// 删除Skip表达式
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IQueryable RemoveSkip(this IQueryable source)
        {
            return source.Provider.CreateQuery(new RemoveSkipVisitor().Visit(source.Expression));
        }

        /// <summary>
        /// 删除Take表达式
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IQueryable<T> RemoveTake<T>(this IQueryable<T> source)
        {
            return (IQueryable<T>)((IQueryable)source).RemoveTake();
        }

        /// <summary>
        /// 删除Take表达式
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IQueryable RemoveTake(this IQueryable source)
        {
            return source.Provider.CreateQuery(new RemoveTakeVisitor().Visit(source.Expression));
        }
        /// <summary>
        /// 获取Skip数量
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static int? GetSkipCount(this IQueryable source)
        {
            var visitor = new SkipVisitor();
            visitor.Visit(source.Expression);

            return visitor.SkipCount;
        }

        /// <summary>
        /// 获取Take数量
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static int? GetTakeCount(this IQueryable source)
        {
            var visitor = new TakeVisitor();
            visitor.Visit(source.Expression);

            return visitor.TakeCount;
        }
        /// <summary>
        /// 获取排序参数
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static (string sortColumn, string sortType) GetOrderBy(this IQueryable source)
        {
            var visitor = new GetOrderByVisitor();
            visitor.Visit(source.Expression);

            return visitor.OrderParam;
        }
        
        /// <summary>
        /// 切换数据源,保留原数据源中的Expression
        /// </summary>
        /// <param name="source">原数据源</param>
        /// <param name="newSource">新数据源</param>
        /// <returns></returns>
        public static IQueryable ReplaceQueryable(this IQueryable source, IQueryable newSource)
        {
            ReplaceQueryableVisitor replaceQueryableVisitor = new ReplaceQueryableVisitor(newSource);
            var newExpre = replaceQueryableVisitor.Visit(source.Expression);

            return newSource.Provider.CreateQuery(newExpre);
        }
        class ReplaceQueryableVisitor : ExpressionVisitor
        {
            private readonly IQueryable _newQuery;
            public ReplaceQueryableVisitor(IQueryable newQuery)
            {
                _newQuery = newQuery;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Value is IQueryable)
                {
                    return Expression.Constant(_newQuery);
                }

                return base.VisitConstant(node);
            }
        }

        class GetOrderByVisitor : ExpressionVisitor
        {
            public (string sortColumn, string sortType) OrderParam { get; set; }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "OrderBy" || node.Method.Name == "OrderByDescending")
                {
                    string sortColumn = (((node.Arguments[1] as UnaryExpression).Operand as LambdaExpression).Body as MemberExpression).Member.Name;
                    string sortType = node.Method.Name == "OrderBy" ? "asc" : "desc";
                    OrderParam = (sortColumn, sortType);
                }
                return base.VisitMethodCall(node);
            }
        }

        class SkipVisitor : ExpressionVisitor
        {
            public int? SkipCount { get; set; }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "Skip")
                {
                    SkipCount = (int)(node.Arguments[1] as ConstantExpression).Value;
                }
                return base.VisitMethodCall(node);
            }
        }

        class TakeVisitor : ExpressionVisitor
        {
            public int? TakeCount { get; set; }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "Take")
                {
                    TakeCount = (int)(node.Arguments[1] as ConstantExpression).Value;
                }
                return base.VisitMethodCall(node);
            }
        }

        /// <summary>
        /// 删除Skip表达式
        /// </summary>
        public class RemoveSkipVisitor : ExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "Skip")
                    return base.Visit(node.Arguments[0]);

                return node;
            }
        }

        /// <summary>
        /// 删除Take表达式
        /// </summary>
        public class RemoveTakeVisitor : ExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "Take")
                    return base.Visit(node.Arguments[0]);

                return node;
            }
        }

    }
}