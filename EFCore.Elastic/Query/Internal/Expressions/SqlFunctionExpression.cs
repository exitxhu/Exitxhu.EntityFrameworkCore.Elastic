// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal.Expressions;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlFunctionExpression : SqlExpression
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlFunctionExpression(
        string name,
        IEnumerable<Expression> arguments,
        Type type,
        CoreTypeMapping typeMapping)
        : this(name, isScoringFunction: false, arguments, type, typeMapping)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>

    public SqlFunctionExpression(
        string name,
        bool isScoringFunction,
        IEnumerable<Expression> arguments,
        Type type,
        CoreTypeMapping typeMapping)
        : base(type, typeMapping)
    {
        Name = name;
        Arguments = arguments.ToList();
        IsScoringFunction = isScoringFunction;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Name { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>

    public virtual bool IsScoringFunction { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<Expression> Arguments { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var changed = false;
        var arguments = new Expression[Arguments.Count];
        for (var i = 0; i < arguments.Length; i++)
        {
            arguments[i] = visitor.Visit(Arguments[i]);
            changed |= arguments[i] != Arguments[i];
        }

        return changed
            ? new SqlFunctionExpression(Name, IsScoringFunction, arguments, Type, TypeMapping)
            : this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlFunctionExpression ApplyTypeMapping(CoreTypeMapping typeMapping)
        => new(Name, IsScoringFunction, Arguments, Type, typeMapping ?? TypeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlFunctionExpression Update(IReadOnlyList<Expression> arguments)
        => arguments.SequenceEqual(Arguments)
            ? this
            : new SqlFunctionExpression(Name, IsScoringFunction, arguments, Type, TypeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append(Name);
        expressionPrinter.Append("(");
        expressionPrinter.VisitCollection(Arguments);
        expressionPrinter.Append(")");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool Equals(object obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlFunctionExpression sqlFunctionExpression
                && Equals(sqlFunctionExpression));

    private bool Equals(SqlFunctionExpression sqlFunctionExpression)
        => base.Equals(sqlFunctionExpression)
            && Name == sqlFunctionExpression.Name
            && Arguments.SequenceEqual(sqlFunctionExpression.Arguments);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(Name);
        for (var i = 0; i < Arguments.Count; i++)
        {
            hash.Add(Arguments[i]);
        }

        return hash.ToHashCode();
    }
}
