using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EfCore.Sharding.Suggestion.Sharding.Abstractions;
using EfCore.Sharding.Suggestion.Sharding.Abstractions.Shardings;
using EfCore.Sharding.Suggestion.Sharding.Extensions;
using EfCore.Sharding.Suggestion.Sharding.Impls.Shardings;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace EfCore.Sharding.Suggestion.Sharding.Impls
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 16 December 2020 10:55:09
* @Email: 326308290@qq.com
*/
    public class ShardingIQueryable<T> : IShardingQueryable<T> where T : class, IShardingEntity
    {
        private readonly Type _entityType;
        private readonly IVirtualTable<T> _virtualTable;
        private readonly IShardingDbContextProvider _shardingDbContextProvider;
        private IQueryable<T> _source { get; set; }

        public ShardingIQueryable(IQueryable<T> source, IVirtualTableManager virtualTableManager,IShardingDbContextProvider shardingDbContextProvider)
        {
            _entityType = typeof(T);
            _source = source;
            _virtualTable= (IVirtualTable<T>)virtualTableManager.GetVirtualTable(_entityType);
            _shardingDbContextProvider = shardingDbContextProvider;
        }

        private async Task<List<TResult>> GetStaticDataAsync<TResult>(Func<IQueryable, Task<TResult>> query, IQueryable newSource = null)
        {
            newSource = newSource ?? _source;
            //获取读表要访问的表
            var physicTables =_virtualTable.RouteTo(new RouteConfig(newSource,null,null));
            // //临时上下文
            var contexts=new List<ShardingDbContext>(physicTables.Count);
            //获取所有的读表后缀
            try
            {
                var queryTasks = physicTables.Select(physicTable =>
                {
                    var shardingDbContext = _shardingDbContextProvider.CreateAloneShardingDbContext(physicTable.Tail);
                    contexts.Add(shardingDbContext);
                    var targetIQ = shardingDbContext.GetIQueryable<T>();
                    var replacedNewSource = newSource.ReplaceQueryable(targetIQ);
                    return query(replacedNewSource);
                });
                var result = (await Task.WhenAll(queryTasks)).ToList();
                return result;
            }
            finally
            {
                //回收上下文
                contexts.ForEach(o=>o.Dispose());
            }
        }
        private async Task<int> GetCountAsync(IQueryable newSource)
        {
            var results = await GetStaticDataAsync<int>(x => EntityFrameworkQueryableExtensions.CountAsync((dynamic)x), newSource);
            return results.Sum();
        }
        private async Task<TResult> GetSumAsync<TResult>(IQueryable<TResult> newSource)
        {
            var results = await GetStaticDataAsync<TResult>(x => EntityFrameworkQueryableExtensions.SumAsync((dynamic)x), newSource);
            return Enumerable.Sum((dynamic)results);
        }
        private async Task<TResult> GetSumAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            var newSource = _source.Select(selector);
            return await GetSumAsync(newSource);
        }
        private async Task<dynamic> GetDynamicAverageAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            var newSource = _source.Select(selector);
            //总数量
            var allCount = await GetCountAsync(newSource);

            //总合
            var sum = await GetSumAsync(newSource);
            if (sum is int || sum is int? || sum is long || sum is long?)
                return ((double?)(dynamic)sum) / allCount;
            else
                return (dynamic)sum / allCount;
        }

        public IShardingQueryable<T> Where(Expression<Func<T, bool>> predicate)
        {
            _source = _source.Where(predicate);
            return this;
        }

        public IShardingQueryable<T> Skip(int count)
        {
            _source = _source.Skip(count);
            return this;
        }

        public IShardingQueryable<T> Take(int count)
        {
            _source = _source.Take(count);
            return this;
        }

        public IShardingQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _source = _source.OrderBy(keySelector);
            return this;
        }

        public IShardingQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _source = _source.OrderByDescending(keySelector);
            return this;
        }

        public int Count()
        {
            return AsyncHelper.RunSync(CountAsync);
        }

        public async Task<int> CountAsync()
        {
            return await GetCountAsync(_source);
        }

        public List<T> ToList()
        {
            return AsyncHelper.RunSync(ToListAsync);
        }

        public async Task<List<T>> ToListAsync()
        {
            //去除分页,获取前Take+Skip数量
            int? take = _source.GetTakeCount();
            int? skip = _source.GetSkipCount();
            skip ??= 0;
            var (sortColumn, sortType) = _source.GetOrderBy();
            var noPaginSource = _source.RemoveTake().RemoveSkip();
            if (!take.IsNullOrEmpty())
                noPaginSource = noPaginSource.Take(take.GetValueOrDefault(0) + skip.Value);
            
            //获取读表要访问的表
            var physicTables = _virtualTable.RouteTo(new RouteConfig(_source,null,null));
            // //临时上下文
            var contexts=new List<ShardingDbContext>(physicTables.Count);
            
            //获取所有的读表后缀
            List<Task<List<T>>>  queryTasks = physicTables.Select(physicTable =>
            {
                 var shardingDbContext = _shardingDbContextProvider.CreateAloneShardingDbContext(physicTable.Tail);
                contexts.Add(shardingDbContext);
                var newSource = shardingDbContext.GetIQueryable<T>();
                var replacedNewSource = (IQueryable<T>)noPaginSource.ReplaceQueryable(newSource);
                return replacedNewSource
                    .ToListAsync();
            }).ToList();
            var result = (await Task.WhenAll(queryTasks.ToArray())).ToList();
            contexts.ForEach(o=>o.Dispose());
            List<T> all = new List<T>();
            result.ForEach(x => all.AddRange(x));

            //合并数据
            var resList = all;
            if (!string.IsNullOrWhiteSpace(sortColumn) && !string.IsNullOrWhiteSpace(sortType))
                resList = resList.AsQueryable().OrderBy($"{sortColumn} {sortType}").ToList();
            if (skip.Value>0)
                resList = resList.Skip(skip.Value).ToList();
            if (take.HasValue)
                resList = resList.Take(take.Value).ToList();

            return resList;
        }

        public T FirstOrDefault()
        {
            return AsyncHelper.RunSync(FirstOrDefaultAsync);
        }

        public async Task<T> FirstOrDefaultAsync()
        {
            var list = await GetStaticDataAsync(async x => (T)await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync((dynamic)x));
            list.RemoveAll(x => x == null);
            var q = list.AsQueryable();
            var (sortColumn, sortType) = _source.GetOrderBy();
            if (!sortColumn.IsNullOrEmpty())
                q = q.OrderBy($"{sortColumn} {sortType}");

            return q.FirstOrDefault();
        }

        public bool Any(Expression<Func<T, bool>> predicate)
        {
            return AsyncHelper.RunSync(() => AnyAsync(predicate));
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            var newSource = _source.Where(predicate);
            return (await GetStaticDataAsync<bool>(x => EntityFrameworkQueryableExtensions.AnyAsync((dynamic)x), newSource))
                .Any(x => x == true);
        }

        public List<TResult> Distinct<TResult>(Expression<Func<T, TResult>> selector)
        {
            return AsyncHelper.RunSync(() => DistinctAsync(selector));
        }

        public async Task<List<TResult>> DistinctAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            var newSource = _source.Select(selector);

            var results = await GetStaticDataAsync<List<TResult>>(x =>
            {
                var q = Queryable.Distinct((dynamic)x);
                return EntityFrameworkQueryableExtensions.ToListAsync(q);
            }, newSource);

            return results.SelectMany(x => x).Distinct().ToList();
        }

        public TResult Max<TResult>(Expression<Func<T, TResult>> selector)
        {
            return AsyncHelper.RunSync(() => MaxAsync(selector));
        }

        public async Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            var newSource = _source.Select(selector);
            var results = await GetStaticDataAsync<TResult>(x => EntityFrameworkQueryableExtensions.MaxAsync((dynamic)x), newSource);

            return results.Max();
        }

        public TResult Min<TResult>(Expression<Func<T, TResult>> selector)
        {
            return AsyncHelper.RunSync(() => MinAsync(selector));
        }
        public async Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            var newSource = _source.Select(selector);
            var results = await GetStaticDataAsync<TResult>(x => EntityFrameworkQueryableExtensions.MinAsync((dynamic)x), newSource);

            return results.Min();
        }

        public double Average(Expression<Func<T, int>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<double> AverageAsync(Expression<Func<T, int>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }

        public double? Average(Expression<Func<T, int?>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<double?> AverageAsync(Expression<Func<T, int?>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }

        public float Average(Expression<Func<T, float>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<float> AverageAsync(Expression<Func<T, float>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }

        public float? Average(Expression<Func<T, float?>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<float?> AverageAsync(Expression<Func<T, float?>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }

        public double Average(Expression<Func<T, long>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<double> AverageAsync(Expression<Func<T, long>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }
        public double? Average(Expression<Func<T, long?>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<double?> AverageAsync(Expression<Func<T, long?>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }
        public double Average(Expression<Func<T, double>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<double> AverageAsync(Expression<Func<T, double>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }
        public double? Average(Expression<Func<T, double?>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<double?> AverageAsync(Expression<Func<T, double?>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }
        public decimal Average(Expression<Func<T, decimal>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<decimal> AverageAsync(Expression<Func<T, decimal>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }
        public decimal? Average(Expression<Func<T, decimal?>> selector)
        {
            return AsyncHelper.RunSync(() => AverageAsync(selector));
        }
        public async Task<decimal?> AverageAsync(Expression<Func<T, decimal?>> selector)
        {
            return await GetDynamicAverageAsync(selector);
        }
        public decimal Sum(Expression<Func<T, decimal>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<decimal> SumAsync(Expression<Func<T, decimal>> selector)
        {
            return await GetSumAsync(selector);
        }
        public decimal? Sum(Expression<Func<T, decimal?>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<decimal?> SumAsync(Expression<Func<T, decimal?>> selector)
        {
            return await GetSumAsync(selector);
        }
        public double Sum(Expression<Func<T, double>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<double> SumAsync(Expression<Func<T, double>> selector)
        {
            return await GetSumAsync(selector);
        }
        public double? Sum(Expression<Func<T, double?>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<double?> SumAsync(Expression<Func<T, double?>> selector)
        {
            return await GetSumAsync(selector);
        }
        public float Sum(Expression<Func<T, float>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<float> SumAsync(Expression<Func<T, float>> selector)
        {
            return await GetSumAsync(selector);
        }
        public float? Sum(Expression<Func<T, float?>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<float?> SumAsync(Expression<Func<T, float?>> selector)
        {
            return await GetSumAsync(selector);
        }
        public int Sum(Expression<Func<T, int>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<int> SumAsync(Expression<Func<T, int>> selector)
        {
            return await GetSumAsync(selector);
        }
        public int? Sum(Expression<Func<T, int?>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<int?> SumAsync(Expression<Func<T, int?>> selector)
        {
            return await GetSumAsync(selector);
        }
        public long Sum(Expression<Func<T, long>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<long> SumAsync(Expression<Func<T, long>> selector)
        {
            return await GetSumAsync(selector);
        }
        public long? Sum(Expression<Func<T, long?>> selector)
        {
            return AsyncHelper.RunSync(() => SumAsync(selector));
        }
        public async Task<long?> SumAsync(Expression<Func<T, long?>> selector)
        {
            return await GetSumAsync(selector);
        }
    }
}