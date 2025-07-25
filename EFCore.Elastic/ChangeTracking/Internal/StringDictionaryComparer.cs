// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;


namespace Microsoft.EntityFrameworkCore.Cosmos.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class StringDictionaryComparer<TDictionary, TElement> : ValueComparer<object>, IInfrastructure<ValueComparer>
{
    private static readonly bool UseOldBehavior35239 =
        AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue35239", out var enabled35239) && enabled35239;

    private static readonly MethodInfo CompareMethod = typeof(StringDictionaryComparer<TDictionary, TElement>).GetMethod(
        nameof(Compare), BindingFlags.Static | BindingFlags.NonPublic, [typeof(object), typeof(object), typeof(Func<TElement, TElement, bool>)])!;

    private static readonly MethodInfo LegacyCompareMethod = typeof(StringDictionaryComparer<TDictionary, TElement>).GetMethod(
        nameof(Compare), BindingFlags.Static | BindingFlags.NonPublic, [typeof(object), typeof(object), typeof(ValueComparer)])!;

    private static readonly MethodInfo GetHashCodeMethod = typeof(StringDictionaryComparer<TDictionary, TElement>).GetMethod(
        nameof(GetHashCode), BindingFlags.Static | BindingFlags.NonPublic, [typeof(IEnumerable), typeof(Func<TElement, int>)])!;

    private static readonly MethodInfo LegacyGetHashCodeMethod = typeof(StringDictionaryComparer<TDictionary, TElement>).GetMethod(
        nameof(GetHashCode), BindingFlags.Static | BindingFlags.NonPublic, [typeof(IEnumerable), typeof(ValueComparer)])!;

    private static readonly MethodInfo SnapshotMethod = typeof(StringDictionaryComparer<TDictionary, TElement>).GetMethod(
        nameof(Snapshot), BindingFlags.Static | BindingFlags.NonPublic, [typeof(object), typeof(Func<TElement, TElement>)])!;

    private static readonly MethodInfo LegacySnapshotMethod = typeof(StringDictionaryComparer<TDictionary, TElement>).GetMethod(
        nameof(Snapshot), BindingFlags.Static | BindingFlags.NonPublic, [typeof(object), typeof(ValueComparer)])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StringDictionaryComparer(ValueComparer elementComparer)
        : base(
            CompareLambda(elementComparer),
            GetHashCodeLambda(elementComparer),
            SnapshotLambda(elementComparer))
        => ElementComparer = elementComparer;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ValueComparer ElementComparer { get; }

    ValueComparer IInfrastructure<ValueComparer>.Instance
        => ElementComparer;

    private static Expression<Func<object?, object?, bool>> CompareLambda(ValueComparer elementComparer)
    {
        var prm1 = Expression.Parameter(typeof(object), "a");
        var prm2 = Expression.Parameter(typeof(object), "b");

        if (UseOldBehavior35239)
        {
            // (a, b) => Compare(a, b, new Comparer(...))
            return Expression.Lambda<Func<object?, object?, bool>>(
                Expression.Call(
                    LegacyCompareMethod,
                    prm1,
                    prm2,
#pragma warning disable EF9100
                    elementComparer.ConstructorExpression),
#pragma warning restore EF9100
                prm1,
                prm2);
        }

        // we check the compatibility between element type we expect on the Equals methods
        // vs what we actually get from the element comparer
        // if the expected is assignable from actual we can just do simple call...
        if (typeof(TElement).IsAssignableFrom(elementComparer.Type))
        {
            // (a, b) => Compare(a, b, elementComparer.Equals)
            return Expression.Lambda<Func<object?, object?, bool>>(
                Expression.Call(
                    CompareMethod,
                    prm1,
                    prm2,
                    elementComparer.EqualsExpression),
                prm1,
                prm2);
        }

        // ...otherwise we need to rewrite the actual lambda (as we can't change the expected signature)
        // in that case we are rewriting the inner lambda parameters to TElement and cast to the element comparer
        // type argument in the body, so that semantics of the element comparison func don't change
        var newInnerPrm1 = Expression.Parameter(typeof(TElement), "a");
        var newInnerPrm2 = Expression.Parameter(typeof(TElement), "b");

        var newEqualsExpressionBody = elementComparer.ExtractEqualsBody(
            Expression.Convert(newInnerPrm1, elementComparer.Type),
            Expression.Convert(newInnerPrm2, elementComparer.Type));

        return Expression.Lambda<Func<object?, object?, bool>>(
            Expression.Call(
                CompareMethod,
                prm1,
                prm2,
                Expression.Lambda(
                    newEqualsExpressionBody,
                    newInnerPrm1,
                    newInnerPrm2)),
            prm1,
            prm2);
    }

    private static Expression<Func<object, int>> GetHashCodeLambda(ValueComparer elementComparer)
    {
        var prm = Expression.Parameter(typeof(object), "o");

        if (UseOldBehavior35239)
        {
            // o => GetHashCode((IEnumerable)o, new Comparer(...))
            return Expression.Lambda<Func<object, int>>(
                Expression.Call(
                    LegacyGetHashCodeMethod,
                    Expression.Convert(
                        prm,
                        typeof(IEnumerable)),
#pragma warning disable EF9100
                    elementComparer.ConstructorExpression),
#pragma warning restore EF9100
                prm);
        }

        if (typeof(TElement).IsAssignableFrom(elementComparer.Type))
        {
            // o => GetHashCode((IEnumerable)o, elementComparer.GetHashCode)
            return Expression.Lambda<Func<object, int>>(
                Expression.Call(
                    GetHashCodeMethod,
                    Expression.Convert(
                        prm,
                        typeof(IEnumerable)),
                        elementComparer.HashCodeExpression),
                prm);
        }

        var newInnerPrm = Expression.Parameter(typeof(TElement), "o");

        var newInnerBody = elementComparer.ExtractHashCodeBody(
            Expression.Convert(
                newInnerPrm,
                elementComparer.Type));

        return Expression.Lambda<Func<object, int>>(
            Expression.Call(
                GetHashCodeMethod,
                Expression.Convert(
                    prm,
                    typeof(IEnumerable)),
                Expression.Lambda(
                    newInnerBody,
                    newInnerPrm)),
            prm);
    }

    private static Expression<Func<object, object>> SnapshotLambda(ValueComparer elementComparer)
    {
        var prm = Expression.Parameter(typeof(object), "source");

        if (UseOldBehavior35239)
        {
            // source => Snapshot(source, new Comparer(..))
            return Expression.Lambda<Func<object, object>>(
                Expression.Call(
                    LegacySnapshotMethod,
                    prm,
#pragma warning disable EF9100
                    elementComparer.ConstructorExpression),
#pragma warning restore EF9100
                prm);
        }

        // TElement is both argument and return type so the types need to be the same
        if (typeof(TElement) == elementComparer.Type)
        {
            // source => Snapshot(source, elementComparer.Snapshot)
            return Expression.Lambda<Func<object, object>>(
                Expression.Call(
                    SnapshotMethod,
                    prm,
                    elementComparer.SnapshotExpression),
                prm);
        }

        var newInnerPrm = Expression.Parameter(typeof(TElement), "source");

        var newInnerBody = elementComparer.ExtractSnapshotBody(
            Expression.Convert(
                newInnerPrm,
                elementComparer.Type));

        // note we need to also convert the result of inner lambda back to TElement
        return Expression.Lambda<Func<object, object>>(
            Expression.Call(
                SnapshotMethod,
                prm,
                Expression.Lambda(
                    Expression.Convert(
                        newInnerBody,
                        typeof(TElement)),
                    newInnerPrm)),
            prm);
    }

    private static bool Compare(object? a, object? b, Func<TElement?, TElement?, bool> elementCompare)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null)
        {
            return b is null;
        }

        if (b is null)
        {
            return false;
        }

        if (a is IReadOnlyDictionary<string, TElement?> aDictionary && b is IReadOnlyDictionary<string, TElement?> bDictionary)
        {
            if (aDictionary.Count != bDictionary.Count)
            {
                return false;
            }

            foreach (var pair in aDictionary)
            {
                if (!bDictionary.TryGetValue(pair.Key, out var bValue)
                    || !elementCompare(pair.Value, bValue))
                {
                    return false;
                }
            }

            return true;
        }

        throw new InvalidOperationException(
            @"CosmosStrings.BadDictionaryType(
                (a is IDictionary<string, TElement?> ? b : a).GetType().ShortDisplayName(),
                typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(TElement)).ShortDisplayName())");
    }

    private static bool Compare(object? a, object? b, ValueComparer elementComparer)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null)
        {
            return b is null;
        }

        if (b is null)
        {
            return false;
        }

        if (a is IReadOnlyDictionary<string, TElement?> aDictionary && b is IReadOnlyDictionary<string, TElement?> bDictionary)
        {
            if (aDictionary.Count != bDictionary.Count)
            {
                return false;
            }

            foreach (var pair in aDictionary)
            {
                if (!bDictionary.TryGetValue(pair.Key, out var bValue)
                    || !elementComparer.Equals(pair.Value, bValue))
                {
                    return false;
                }
            }

            return true;
        }

        throw new InvalidOperationException(
            @"CosmosStrings.BadDictionaryType(
                (a is IDictionary<string, TElement?> ? b : a).GetType().ShortDisplayName(),
                typeof(IDictionary<,>).MakeGenericType(typeof(string), elementComparer.Type).ShortDisplayName())");
    }

    private static int GetHashCode(IEnumerable source, Func<TElement?, int> elementGetHashCode)
    {
        if (source is not IReadOnlyDictionary<string, TElement?> sourceDictionary)
        {
            throw new InvalidOperationException(
                @"CosmosStrings.BadDictionaryType(
                    source.GetType().ShortDisplayName(),
                    typeof(IList<>).MakeGenericType(typeof(TElement)).ShortDisplayName())");
        }

        var hash = new HashCode();

        foreach (var pair in sourceDictionary)
        {
            hash.Add(pair.Key);
            hash.Add(pair.Value == null ? 0 : elementGetHashCode(pair.Value));
        }

        return hash.ToHashCode();
    }

    private static int GetHashCode(IEnumerable source, ValueComparer elementComparer)
    {
        if (source is not IReadOnlyDictionary<string, TElement?> sourceDictionary)
        {
            throw new InvalidOperationException(
                @"CosmosStrings.BadDictionaryType(
                    source.GetType().ShortDisplayName(),
                    typeof(IList<>).MakeGenericType(elementComparer.Type).ShortDisplayName())");
        }

        var hash = new HashCode();

        foreach (var pair in sourceDictionary)
        {
            hash.Add(pair.Key);
            hash.Add(pair.Value == null ? 0 : elementComparer.GetHashCode(pair.Value));
        }

        return hash.ToHashCode();
    }

    private static IReadOnlyDictionary<string, TElement?> Snapshot(object source, Func<TElement?, TElement?> elementSnapshot)
    {
        if (source is not IReadOnlyDictionary<string, TElement?> sourceDictionary)
        {
            throw new InvalidOperationException(
                @"CosmosStrings.BadDictionaryType(
                    source.GetType().ShortDisplayName(),
                    typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(TElement)).ShortDisplayName())");
        }

        var snapshot = new Dictionary<string, TElement?>();
        foreach (var pair in sourceDictionary)
        {
            snapshot[pair.Key] = pair.Value == null ? default : (TElement?)elementSnapshot(pair.Value);
        }

        return snapshot;
    }

    private static IReadOnlyDictionary<string, TElement?> Snapshot(object source, ValueComparer elementComparer)
    {
        if (source is not IReadOnlyDictionary<string, TElement?> sourceDictionary)
        {
            throw new InvalidOperationException(
                @"CosmosStrings.BadDictionaryType(
                    source.GetType().ShortDisplayName(),
                    typeof(IDictionary<,>).MakeGenericType(typeof(string), elementComparer.Type).ShortDisplayName())");
        }

        var snapshot = new Dictionary<string, TElement?>();
        foreach (var pair in sourceDictionary)
        {
            snapshot[pair.Key] = pair.Value == null ? default : (TElement?)elementComparer.Snapshot(pair.Value);
        }

        return snapshot;
    }
}
