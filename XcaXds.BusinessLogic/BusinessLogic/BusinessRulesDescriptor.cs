using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using XcaXds.BusinessLogic.Models.Custom.BusinessLogic;
using XcaXds.Commons.Commons;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.BusinessLogic.BusinessLogic;

public class BusinessRulesDescriptor : ExpressionVisitor
{
    private readonly StringBuilder _sb = new();

    private static List<object> _parsedExpressions { get; set; } = new();

    public static string BusinessRulesPlainText { get; } = WriteBusinessRulesPlainText();
    public static string BusinessRulesJson { get; } = WriteBusinessRulesJsonFormatted();
    public static string EntriesToObfuscateJson { get; } = WriteEntriesToObfuscateJsonFormatted();

    private static string WriteEntriesToObfuscateJsonFormatted()
    {
        var entryList = new List<ObfuscationEntry>
        {
            new(
                "Citizen Confidentiality Codes",
                [.. BusinessLogicFiltersService.CitizenConfidentialityCodesToObfuscate
                    .Select(c => new CodedValue(c.Item1!, c.Item2!))]
            ),
            new(
                "HealthcarePersonell Confidentiality Codes",
                [.. BusinessLogicFiltersService.HealthcarePersonellConfidentialityCodesToObfuscate
                    .Select(c => new CodedValue(c.Item1!, c.Item2!))]
            )
        };

        return JsonSerializer.Serialize(entryList, Constants.JsonDefaultOptions.DefaultSettings);
    }

    private static string WriteBusinessRulesJsonFormatted()
    {
        var doc = new BusinessRulesDocument();

        foreach (var rule in BusinessLogicFilterer.BusinessLogicRules)
        {
            if (rule.Condition?.Body == null || rule.Filter?.Body == null)
                continue;

            var ruleDto = new RuleDto
            {
                Name = rule.Name,
                Conditions = DescribeConditions(rule.Condition.Body),
                Result = DescribeResult(rule.Filter.Body)
            };

            doc.Rules.Add(ruleDto);
        }

        return JsonSerializer.Serialize(doc, Constants.JsonDefaultOptions.DefaultSettings);
    }

    private static List<object> DescribeConditions(Expression expr)
    {
        var flattened = FlattenAnd(expr)
            .Where(e => !IsNullCheck(e));

        var list = new List<object>();

        foreach (var e in flattened)
        {
            if (e is BinaryExpression bin)
            {
                list.Add(FormatBinaryExpressionJson(bin));
                continue;
            }

            if (e is MethodCallExpression call)
            {
                if (call.Method.Name == "IsAnyOf")
                {
                    list.Add(new
                    {
                        IsAnyOf = GetAnyOf(call)
                    });
                    continue;
                }

                list.Add(new
                {
                    Expression = call.ToString()
                });
            }
        }

        return list;
    }

    private static object GetAnyOf(MethodCallExpression call)
    {
        var gobb = new
        {
            Property = call.Arguments[0].ToString(),
            Values = call.Arguments.Skip(1).SelectMany(expr => GetMember(expr, true)).Select(val => val.Trim('"')).ToList(),
        };
        return gobb;
    }

    private static object DescribeResult(Expression expr)
    {
        if (expr is MethodCallExpression call)
        {
            if (call.Method.Name == "FilterByConfidentiality")
            {
                return new FilterResultDto
                {
                    Type = "FilterByConfidentiality",
                    Allow = ExtractValues(call.Arguments.ElementAtOrDefault(1)),
                    Deny = ExtractValues(call.Arguments.ElementAtOrDefault(2))
                };
            }

            if (call.Method.Name == "DenyAll")
            {
                return new { Type = "DenyAll" };
            }
        }

        return new { Type = expr.ToString() };
    }

    private static List<string> ExtractValues(Expression? expr)
    {
        if (expr == null)
            return [];

        switch (expr)
        {
            case NewArrayExpression arr:
                return arr.Expressions.Select(GetMember).Select(v => v.Replace("\u0022", "")).ToList();

            case ConstantExpression c when c.Value is IEnumerable<object> values:
                return values.Select(v => v.ToString()?.Replace("\u0022", "") ?? "").ToList();

            case MethodCallExpression m:
                // handles .ToList() or .AsEnumerable()
                return ExtractValues(m.Arguments.FirstOrDefault());

            default:
                return [GetMember(expr)];
        }
    }

    private static List<string> GetArrayValues(Expression expr)
    {
        if (expr is NewArrayExpression arr)
            return arr.Expressions.Select(GetMember).Select(v => v.Replace("\u0022", "")).ToList();

        return [GetMember(expr)];
    }

    private static object FormatBinaryExpressionJson(BinaryExpression bin)
    {
        // Handle method returning bool compared to true
        if (bin.NodeType == ExpressionType.Equal &&
            bin.Right is ConstantExpression c1 &&
            c1.Value is bool b1)
        {
            if (bin.Left is MethodCallExpression call)
            {
                return FormatBooleanMethod(call, b1);
            }
        }

        // Handle method returning bool compared to false
        if (bin.NodeType == ExpressionType.Equal &&
            bin.Right is ConstantExpression c2 &&
            c2.Value is bool b2 &&
            bin.Left is MethodCallExpression call2)
        {
            return FormatBooleanMethod(call2, b2);
        }

        return new
        {
            Left = GetMember(bin.Left),
            Op = GetOperator(bin.NodeType),
            Right = GetMember(bin.Right)
        };
    }

    private static object FormatBooleanMethod(MethodCallExpression call, bool expected)
    {
        if (call.Method.Name == "IsAnyOf")
        {
            var field = GetMember(call.Arguments[0]);
            var values = GetArrayValues(call.Arguments[1]);

            return new
            {
                Field = field,
                Op = expected ? "in" : "notIn",
                Values = values
            };
        }

        if (call.Method.Name == "InRange")
        {
            return new
            {
                Field = GetMember(call.Object!),
                Op = expected ? "range" : "notRange",
                Min = GetMember(call.Arguments[0]),
                Max = GetMember(call.Arguments[1])
            };
        }

        return new
        {
            expression = call.ToString(),
            expected
        };
    }

    private static string WriteBusinessRulesPlainText()
    {
        var sb = new StringBuilder();
        foreach (var rule in BusinessLogicFilterer.BusinessLogicRules)
        {
            if (rule.Condition?.Body == null || rule.Filter?.Body == null) continue;

            sb.AppendLine("========= Rule   ==========");
            sb.AppendLine(rule.Name);
            sb.AppendLine("===========================");
            sb.AppendLine(Describe(rule.Condition.Body));
            sb.AppendLine("========= Result ==========");
            sb.AppendLine(Describe(rule.Filter.Body));
            sb.AppendLine();
        }

        foreach (var code in BusinessLogicFiltersService.CitizenConfidentialityCodesToObfuscate)
        {
            sb.AppendLine("========= Citizen Confidentiality Codes To Obfuscate ==========");
            sb.AppendLine($"Class: {code.Item1}, Code: {code.Item2}");
            sb.AppendLine("==============================================================");
        }

        foreach (var code in BusinessLogicFiltersService.HealthcarePersonellConfidentialityCodesToObfuscate)
        {
            sb.AppendLine("========= Healthcare Personell Confidentiality Codes To Obfuscate ==========");
            sb.AppendLine($"Class: {code.Item1}, Code: {code.Item2}");
            sb.AppendLine("============================================================================");
        }

        return sb.ToString();
    }

    public static string Describe(Expression expr)
    {
        var flattened = new List<Expression>();
        foreach (var e in FlattenAnd(expr))
        {
            if (!IsNullCheck(e))
                flattened.Add(e);
        }

        var descriptions = new List<string>();
        foreach (var e in flattened)
        {
            var desc = DescribeSingle(e);
            descriptions.Add(desc);
        }

        return string.Join("\n", descriptions);
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        if (IsNullCheck(node))
        {
            return node;
        }

        _sb.Append("(");
        Visit(node.Left);

        _sb.Append($" {GetOperator(node.NodeType)} ");

        Visit(node.Right);
        _sb.Append(")");

        return node;
    }

    private static string DescribeSingle(Expression expr)
    {
        // Binary expressions: ==, !=, >, <, etc.
        if (expr is BinaryExpression bin)
        {
            return FormatBinaryExpression(bin);
        }

        if (expr is MethodCallExpression call && call.Method.Name == "IsAnyOf")
        {
            var member = GetMember(call.Arguments[0]);

            string values;

            var arg = call.Arguments[1];
            if (arg is NewArrayExpression arr)
            {
                values = string.Join(", ", arr.Expressions.Select(GetMember));
            }
            else
            {
                values = GetMember(arg);
            }

            return $"{member} IsAnyOf ({values})";
        }

        if (expr is MethodCallExpression call2 && call2.Method.Name == "FilterByConfidentiality")
        {
            var member = GetMember(call2.Arguments[0]);

            string values;

            var allowedLevels = call2.Arguments.ElementAt(1);
            if (allowedLevels is MethodCallExpression methodCall && methodCall.Arguments[0] is NewArrayExpression arr)
            {
                values = string.Join(", ", arr.Expressions.Select(GetMember));
            }
            else
            {
                values = GetMember(allowedLevels);
            }

            string denyValues;

            var denyLevels = call2.Arguments.ElementAt(2);
            if (denyLevels is MethodCallExpression methodCall2 && methodCall2.Arguments[0] is NewArrayExpression arr2)
            {
                denyValues = string.Join(", ", arr2.Expressions.Select(GetMember));
            }
            else
            {
                denyValues = GetMember(denyLevels);
            }

            return $"{call2.Method.Name} (Allow: {values}, Deny: {denyValues})";
        }

        // Fallback
        return expr.ToString();
    }

    private static string FormatBinaryExpression(BinaryExpression bin)
    {
        if (bin.Left is MethodCallExpression call && call.Method.Name == "IsAnyOf"
            && bin.Right is ConstantExpression c && c.Value is bool b)
        {
            var member = GetMember(call.Arguments[0]);
            string values;
            var arg = call.Arguments[1];

            if (arg is NewArrayExpression arr)
                values = string.Join(", ", arr.Expressions.Select(GetMember));
            else
                values = GetMember(arg);

            return $"{member} IsAnyOf ({values})";
        }

        // Default binary expression formatting
        return $"{GetMember(bin.Left)} {GetOperator(bin.NodeType)} {GetMember(bin.Right)}";
    }

    private static string[] GetMember(Expression expr, bool asStringArray)
    {
        return expr switch
        {
            NewArrayExpression arr => arr.Expressions.Select(GetMember).ToArray(),
            _ => [GetMember(expr)]
        };
    }

    private static string GetMember(Expression expr)
    {
        if (expr == null)
        {
            return null!;
        }

        _parsedExpressions.Add(expr);

        return expr switch
        {
            MemberExpression m => m.ToString().Replace("logic.", ""),
            ConstantExpression c when c.Value is string s => $"{s}",
            ConstantExpression c when c.Value is Array arr => "[" + string.Join(", ", arr.Cast<object>().Select(x => $"\"{x}\"")) + "]",
            ConstantExpression c => c.Value?.ToString() ?? "null",
            NewArrayExpression arr => "[" + string.Join(", ", arr.Expressions.Select(GetMember)) + "]",
            _ => expr.ToString()
        };
    }

    private static string GetOperator(ExpressionType type)
    {
        return type switch
        {
            ExpressionType.Equal => "==",
            ExpressionType.NotEqual => "!=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.LessThan => "<",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThanOrEqual => "<=",
            _ => type.ToString()
        };
    }

    private static IEnumerable<Expression> FlattenAnd(Expression expr)
    {
        if (expr is BinaryExpression bin && (bin.NodeType == ExpressionType.AndAlso || bin.NodeType == ExpressionType.OrElse))
        {
            foreach (var left in FlattenAnd(bin.Left))
                yield return left;

            foreach (var right in FlattenAnd(bin.Right))
                yield return right;

            yield break;
        }

        yield return expr;
    }

    private static bool IsNullCheck(Expression expr)
    {
        if (expr is not BinaryExpression node)
            return false;

        if (node.NodeType != ExpressionType.NotEqual)
            return false;

        return IsNullConstant(node.Left) || IsNullConstant(node.Right);
    }

    private static bool IsNullConstant(Expression expr)
    {
        return expr is ConstantExpression c && c.Value == null;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression is MemberExpression parent)
        {
            Visit(parent);
            _sb.Append(".");
            _sb.Append(node.Member.Name);
        }
        else
        {
            _sb.Append(node.Member.Name);
        }

        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        _sb.Append(node.Value ?? "null");
        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.Name == "IsAnyOf")
        {
            Visit(node.Object);
            _sb.Append(" IN (");

            for (int i = 0; i < node.Arguments.Count; i++)
            {
                Visit(node.Arguments[i]);
                if (i < node.Arguments.Count - 1)
                    _sb.Append(", ");
            }

            _sb.Append(")");
            return node;
        }

        return base.VisitMethodCall(node);
    }
}
