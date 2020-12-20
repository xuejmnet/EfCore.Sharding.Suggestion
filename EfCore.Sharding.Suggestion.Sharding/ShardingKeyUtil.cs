using System;
using System.Linq;
using System.Linq.Expressions;
using Dynamitey;
using EfCore.Sharding.Suggestion.Sharding.VirtualRoutes;
using EfCore.Sharding.Suggestion.Sharding.Extensions;

namespace EfCore.Sharding.Suggestion.Sharding
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 19 December 2020 20:20:29
* @Email: 326308290@qq.com
*/
    public class ShardingKeyUtil
    {
        private ShardingKeyUtil(){}
        
        public static Func<string,bool> GetRouteObjectOperatorFilter<TKey>(IQueryable queryable, ShardingEntityConfig shardingConfig,Func<object,TKey> shardingKeyConvert,Func<TKey,ShardingOperatorEnum,Expression<Func<string,bool>>> keyToTailExpression)
        {
            ShardingOperatorFilterVisitor<TKey> visitor = new ShardingOperatorFilterVisitor<TKey>(shardingConfig,shardingKeyConvert,keyToTailExpression);

            visitor.Visit(queryable.Expression);

            return visitor.GetStringFilterTail();
        }
        private class ShardingOperatorFilterVisitor<TKey> : ExpressionVisitor
        {
            private readonly ShardingEntityConfig _shardingConfig;
            private readonly Func<object, TKey> _shardingKeyConvert;
            private readonly Func<TKey,ShardingOperatorEnum, Expression<Func<string, bool>>> _keyToTailWithFilter;
            private Expression<Func<string, bool>> _where = x => true;

            public ShardingOperatorFilterVisitor(ShardingEntityConfig shardingConfig,Func<object,TKey> shardingKeyConvert,Func<TKey,ShardingOperatorEnum,Expression<Func<string,bool>>> keyToTailWithFilter)
            {
                _shardingConfig = shardingConfig;
                _shardingKeyConvert = shardingKeyConvert;
                _keyToTailWithFilter = keyToTailWithFilter;
            }

            public Func<string, bool> GetStringFilterTail()
            {
                return _where.Compile();
            }
            private bool IsShardingKey(Expression expression)
            {
                return expression is MemberExpression member
                       && member.Expression.Type == _shardingConfig.ShardingEntityType
                       && member.Member.Name == _shardingConfig.ShardingField;
            }

            private bool IsConstant(Expression expression)
            {
                return expression is ConstantExpression
                       || (expression is MemberExpression member && member.Expression is ConstantExpression);
            }

            private object GetFieldValue(Expression expression)
            {
                if (expression is ConstantExpression constant1)
                {
                    return constant1.Value;
                }
                else if (expression is MemberExpression member && member.Expression is ConstantExpression constant2)
                {
                    return Dynamic.InvokeGet(constant2.Value, member.Member.Name);
                }
                else
                {
                    return null;
                }
            }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "Where"
                    && node.Arguments[1] is UnaryExpression unaryExpression
                    && unaryExpression.Operand is LambdaExpression lambdaExpression
                    && lambdaExpression.Body is BinaryExpression binaryExpression
                    )
                {
                    var newWhere = GetWhere(binaryExpression);

                    _where = _where.And(newWhere);
                }

                return base.VisitMethodCall(node);
            }
            private Expression<Func<string, bool>> GetWhere(BinaryExpression binaryExpression)
            {
                Expression<Func<string, bool>> left = x => true;
                Expression<Func<string, bool>> right = x => true;

                //递归获取
                if (binaryExpression.Left is BinaryExpression)
                    left = GetWhere(binaryExpression.Left as BinaryExpression);
                if (binaryExpression.Right is BinaryExpression)
                    right = GetWhere(binaryExpression.Right as BinaryExpression);

                //组合
                if (binaryExpression.NodeType == ExpressionType.AndAlso)
                {
                    return left.And(right);
                }
                else if (binaryExpression.NodeType == ExpressionType.OrElse)
                {
                    return left.Or(right);
                }
                //单个
                else
                {
                    bool paramterAtLeft;
                    long? value = null;

                    if (IsShardingKey(binaryExpression.Left) && IsConstant(binaryExpression.Right))
                    {
                        paramterAtLeft = true;
                        value = (long?)GetFieldValue(binaryExpression.Right);
                    }
                    else if (IsConstant(binaryExpression.Left) && IsShardingKey(binaryExpression.Right))
                    {
                        paramterAtLeft = false;
                        value = (long?)GetFieldValue(binaryExpression.Left);
                    }
                    else
                        return x => true;

                    var op = binaryExpression.NodeType switch
                    {
                        ExpressionType.GreaterThan => paramterAtLeft ? ShardingOperatorEnum.GreaterThan :ShardingOperatorEnum.LessThan,
                        ExpressionType.GreaterThanOrEqual => paramterAtLeft ? ShardingOperatorEnum.GreaterThanOrEqual :ShardingOperatorEnum.LessThanOrEqual,
                        ExpressionType.LessThan => paramterAtLeft ? ShardingOperatorEnum.LessThan : ShardingOperatorEnum.GreaterThan,
                        ExpressionType.LessThanOrEqual => paramterAtLeft ? ShardingOperatorEnum.LessThanOrEqual : ShardingOperatorEnum.GreaterThanOrEqual,
                        ExpressionType.Equal =>ShardingOperatorEnum.Equal,
                        ExpressionType.NotEqual => ShardingOperatorEnum.NotEqual,
                        _ => ShardingOperatorEnum.NotSupport
                    };

                    if (op == ShardingOperatorEnum.NotSupport || value == null)
                        return x => true;

                    
                    
                    var keyValue = _shardingKeyConvert(value);
                    return _keyToTailWithFilter(keyValue,op);
                    // //判断where里面有对应的查询条件的话获取对应值应该是哪个后缀
                    // string realSuffix = ShardingConfig.GetTableTailByField(value.Value);
                    // int index = _allTableSuffixes.IndexOf(realSuffix);
                    //
                    // //超出范围 如果查出的后缀没有在所有的表后缀里面那么就进行处理比如查询>=超级大的或者超级小的时间<=超级小的超级大的时间
                    // if (index == -1)
                    // {
                    //     var newTableSuffixes = _allTableSuffixes.Concat(new string[] { realSuffix }).OrderBy(x => x).ToList();
                    //     int fullIndex = newTableSuffixes.IndexOf(realSuffix);
                    //
                    //     if (fullIndex == 0 && (op == ">=" || op == "!="))
                    //         return x => true;
                    //     else if (fullIndex == newTableSuffixes.Count - 1 && (op == "<=" || op == "!="))
                    //         return x => true;
                    //     else
                    //         return x => false;
                    // }
                    //
                    // var newWhere = DynamicExpressionParser.ParseLambda<int, bool>(
                    //     ParsingConfig.Default, false, $@"it {op} @0", index);
                    //
                    // return newWhere;
                }
            }
        }
    }
}