namespace Koalas;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

public static partial class ExtensionsLinq {
    private static int? _defaultMaxParallel;

    private static int DefaultMaxParallel => _defaultMaxParallel ??= Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.5));

    public static IReadOnlyList<T> ForAll<T>(this IEnumerable<T> items,
                                             Action<T> action) {
        IReadOnlyList<T> list = items.ToReadOnlyList();
        foreach (T item in list) {
            action(item);
        }

        return list;
    }

    public static IReadOnlyList<T> ForAllParallel<T>(this IEnumerable<T> items,
                                                     Action<T> action,
                                                     int? maxParallel = null) {
        IReadOnlyList<T> list = items.ToReadOnlyList();

        ForAll(list.AsParallel()
                   .AsOrdered()
                   .WithDegreeOfParallelism(maxParallel ?? DefaultMaxParallel),
               action);

        return list;
    }

    public static IEnumerable<T> Head<T>(this IEnumerable<T> items, int size = 10) {
        return items.Take(size);
    }

    public static IEnumerable<T> Page<T>(this IEnumerable<T> items, int page, int pageSize) {
        return items.Skip(pageSize * page).Take(pageSize);
    }

    public static int PageCount<T>(this IEnumerable<T> items, int pageSize) {
        return Convert.ToInt32(Math.Ceiling(items.Count() / Convert.ToDouble(pageSize)));
    }

    public static IEnumerable<TTarget> SelectManyParallel<T, TTarget>(this IEnumerable<T> items,
                                                                      Func<T, IEnumerable<TTarget>> transform,
                                                                      int? maxParallel = null) {
        return items.AsParallel()
                    .AsOrdered()
                    .WithDegreeOfParallelism(maxParallel ?? DefaultMaxParallel)
                    .SelectMany(transform);
    }

    public static IEnumerable<TTarget> SelectParallel<T, TTarget>(this IEnumerable<T> items,
                                                                  Func<T, TTarget> transform,
                                                                  int? maxParallel = null) {
        return items.AsParallel()
                    .AsOrdered()
                    .WithDegreeOfParallelism(maxParallel ?? DefaultMaxParallel)
                    .Select(transform);
    }

    public static IEnumerable<T> SkipTake<T>(this IEnumerable<T> items, int skip, int take) {
        return items.Skip(skip).Take(take);
    }

    public static IEnumerable<T> Tail<T>(this IEnumerable<T> items, int size) {
        IReadOnlyList<T> list = items.ToReadOnlyList();
        int listTotalCount = list.Count;
        int skip = Math.Max(0, listTotalCount - size);

        return list.Skip(skip);
    }

    [return: NotNull]
    public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> subject) {
        return subject as IReadOnlyList<T> ?? subject.ToList();
    }
}

public static partial class ExtensionsLinq {
    /// <summary>
    ///     Batches the source sequence into sized buckets.
    /// </summary>
    /// <typeparam name="TSource">Type of elements in <paramref name="source" /> sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="size">Size of buckets.</param>
    /// <returns>A sequence of equally sized buckets containing elements of the source collection.</returns>
    /// <remarks>
    ///     <para>
    ///         This operator uses deferred execution and streams its results
    ///         (buckets are streamed but their content buffered).
    ///     </para>
    ///     <para>
    ///         When more than one bucket is streamed, all buckets except the last
    ///         is guaranteed to have <paramref name="size" /> elements. The last
    ///         bucket may be smaller depending on the remaining elements in the
    ///         <paramref name="source" /> sequence.
    ///     </para>
    ///     <para>
    ///         Each bucket is pre-allocated to <paramref name="size" /> elements.
    ///         If <paramref name="size" /> is set to a very large value, e.g.
    ///         <see cref="int.MaxValue" /> to effectively disable batching by just
    ///         hoping for a single bucket, then it can lead to memory exhaustion
    ///         (<see cref="OutOfMemoryException" />).
    ///     </para>
    /// </remarks>
    public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size) {
        return Batch(source, size, x => x);
    }

    /// <summary>
    ///     Batches the source sequence into sized buckets and applies a projection to each bucket.
    /// </summary>
    /// <typeparam name="TSource">Type of elements in <paramref name="source" /> sequence.</typeparam>
    /// <typeparam name="TResult">Type of result returned by <paramref name="resultSelector" />.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="size">Size of buckets.</param>
    /// <param name="resultSelector">The projection to apply to each bucket.</param>
    /// <returns>A sequence of projections on equally sized buckets containing elements of the source collection.</returns>
    /// <remarks>
    ///     <para>
    ///         This operator uses deferred execution and streams its results
    ///         (buckets are streamed but their content buffered).
    ///     </para>
    ///     <para>
    ///         <para>
    ///             When more than one bucket is streamed, all buckets except the last
    ///             is guaranteed to have <paramref name="size" /> elements. The last
    ///             bucket may be smaller depending on the remaining elements in the
    ///             <paramref name="source" /> sequence.
    ///         </para>
    ///         Each bucket is pre-allocated to <paramref name="size" /> elements.
    ///         If <paramref name="size" /> is set to a very large value, e.g.
    ///         <see cref="int.MaxValue" /> to effectively disable batching by just
    ///         hoping for a single bucket, then it can lead to memory exhaustion
    ///         (<see cref="OutOfMemoryException" />).
    ///     </para>
    /// </remarks>
    public static IEnumerable<TResult> Batch<TSource, TResult>(this IEnumerable<TSource> source,
                                                               int size,
                                                               Func<IEnumerable<TSource>, TResult> resultSelector) {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
        if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

        return _();

        IEnumerable<TResult> _() {
            switch (source) {
                case ICollection<TSource> { Count: 0 }: {
                    break;
                }
                case ICollection<TSource> collection when collection.Count <= size: {
                    var bucket = new TSource[collection.Count];
                    collection.CopyTo(bucket, 0);
                    yield return resultSelector(bucket);

                    break;
                }
                case IReadOnlyCollection<TSource> { Count: 0 }: {
                    break;
                }
                case IReadOnlyList<TSource> list when list.Count <= size: {
                    var bucket = new TSource[list.Count];
                    for (var i = 0; i < list.Count; i++) {
                        bucket[i] = list[i];
                    }

                    yield return resultSelector(bucket);

                    break;
                }
                case IReadOnlyCollection<TSource> collection when collection.Count <= size: {
                    size = collection.Count;
                    goto default;
                }
                default: {
                    TSource[] bucket = null;
                    var count = 0;

                    foreach (TSource item in source) {
                        bucket ??= new TSource[size];
                        bucket[count++] = item;

                        // The bucket is fully buffered before it's yielded
                        if (count != size) {
                            continue;
                        }

                        yield return resultSelector(bucket);

                        bucket = null;
                        count = 0;
                    }

                    // Return the last bucket with all remaining elements
                    if (count > 0) {
                        Array.Resize(ref bucket, count);
                        yield return resultSelector(bucket);
                    }

                    break;
                }
            }
        }
    }
}