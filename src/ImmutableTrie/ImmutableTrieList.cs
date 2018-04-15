using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using Validation;

namespace ImmutableTrie
{
  /// <summary>
  /// A set of initialization methods for instances of <see cref="ImmutableTrieList{T}"/>.
  /// </summary>
  public static class ImmutableTrieList
  {
    /// <summary>
    /// Returns an empty collection.
    /// </summary>
    /// <typeparam name="T">The type of items stored by the collection.</typeparam>
    /// <returns>The immutable collection.</returns>
    [Pure]
    public static ImmutableTrieList<T> Create<T>() => ImmutableTrieList<T>.Empty;

    /// <summary>
    /// Creates a new immutable collection prefilled with the specified item.
    /// </summary>
    /// <typeparam name="T">The type of items stored by the collection.</typeparam>
    /// <param name="item">The item to prepopulate.</param>
    /// <returns>The new immutable collection.</returns>
    [Pure]
    public static ImmutableTrieList<T> Create<T>(T item) => ImmutableTrieList<T>.Empty.Add(item);

    /// <summary>
    /// Creates a new immutable collection prefilled with the specified items.
    /// </summary>
    /// <typeparam name="T">The type of items stored by the collection.</typeparam>
    /// <param name="items">The items to prepopulate.</param>
    /// <returns>The new immutable collection.</returns>
    [Pure]
    public static ImmutableTrieList<T> CreateRange<T>(IEnumerable<T> items) => ImmutableTrieList<T>.Empty.AddRange(items);

    /// <summary>
    /// Creates a new immutable collection prefilled with the specified items.
    /// </summary>
    /// <typeparam name="T">The type of items stored by the collection.</typeparam>
    /// <param name="items">The items to prepopulate.</param>
    /// <returns>The new immutable collection.</returns>
    [Pure]
    public static ImmutableTrieList<T> Create<T>(params T[] items) => ImmutableTrieList<T>.Empty.AddRange(items);

    /// <summary>
    /// Creates a new immutable list builder.
    /// </summary>
    /// <typeparam name="T">The type of items stored by the collection.</typeparam>
    /// <returns>The immutable collection builder.</returns>
    [Pure]
    public static ImmutableTrieList<T>.Builder CreateBuilder<T>() => Create<T>().ToBuilder();

    /// <summary>
    /// Enumerates a sequence exactly once and produces an immutable list of its contents.
    /// </summary>
    /// <typeparam name="TSource">The type of element in the sequence.</typeparam>
    /// <param name="source">The sequence to enumerate.</param>
    /// <returns>An immutable list.</returns>
    [Pure]
    public static ImmutableTrieList<TSource> ToImmutableTrieList<TSource>(this IEnumerable<TSource> source)
    {
      var existingList = source as ImmutableTrieList<TSource>;
      if (existingList != null)
      {
        return existingList;
      }

      return ImmutableTrieList<TSource>.Empty.AddRange(source);
    }
  }

  public sealed partial class ImmutableTrieList<T> : ICollection<T>, IEnumerable<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IEnumerable, IList, IImmutableList<T>
  {
    /// <summary>
    /// See the <see cref="IImmutableList{T}"/> interface.
    /// </summary>
    [Pure]
    public ImmutableTrieList<T> AddRange(IEnumerable<T> items) =>
      this.Build(builder => builder.AddRange(items));

    /// <summary>
    /// See the <see cref="IImmutableList{T}"/> interface.
    /// </summary>
    /// <remarks>
    /// This method re-indexes values, it produces a complete copy, which has O(N) complexity and may allocate O(N) memory.
    /// </remarks>
    [Pure]
    public ImmutableTrieList<T> Insert(int index, T item) =>
      this.Build(builder => builder.Insert(index, item));

    /// <summary>
    /// See the <see cref="IImmutableList{T}"/> interface.
    /// </summary>
    [Pure]
    public ImmutableTrieList<T> InsertRange(int index, IEnumerable<T> items) =>
      this.Build(builder => builder.InsertRange(index, items));

#if FEATURE_ITEMREFAPI
        public ref readonly T ItemRef(int index) { throw null; }
#endif

    /// <summary>
    /// See the <see cref="IImmutableList{T}"/> interface.
    /// </summary>
    [Pure]
    public ImmutableTrieList<T> Remove(T value) =>
      this.Remove(value, EqualityComparer<T>.Default);

    /// <summary>
    /// See the <see cref="IImmutableList{T}"/> interface.
    /// </summary>
    [Pure]
    public ImmutableTrieList<T> Remove(T value, IEqualityComparer<T> equalityComparer)
    {
      int index = this.IndexOf(value, equalityComparer);
      return index < 0 ? this : this.RemoveAt(index);
    }

    /// <summary>
    /// Removes all the elements that match the conditions defined by the specified
    /// predicate.
    /// </summary>
    /// <param name="match">
    /// The <see cref="System.Predicate{T}"/> delegate that defines the conditions of the elements
    /// to remove.
    /// </param>
    /// <returns>
    /// The new list.
    /// </returns>
    [Pure]
    public ImmutableTrieList<T> RemoveAll(Predicate<T> match) =>
     this.Build(builder => builder.RemoveAll(match));

    /// <summary>
    /// See the <see cref="IImmutableList{T}"/> interface.
    /// </summary>
    [Pure]
    public ImmutableTrieList<T> RemoveAt(int index) =>
      this.Build(builder => builder.RemoveAt(index));

    /// <summary>
    /// Removes the specified values from this list.
    /// </summary>
    /// <param name="items">The items to remove if matches are found in this list.</param>
    /// <returns>
    /// A new list with the elements removed.
    /// </returns>
    [Pure]
    public ImmutableTrieList<T> RemoveRange(IEnumerable<T> items) => this.RemoveRange(items, EqualityComparer<T>.Default);

    /// <summary>
    /// Removes the specified values from this list.
    /// </summary>
    /// <param name="items">The items to remove if matches are found in this list.</param>
    /// <param name="equalityComparer">
    /// The equality comparer to use in the search.
    /// If <c>null</c>, <see cref="EqualityComparer{T}.Default"/> is used.
    /// </param>
    /// <returns>
    /// A new list with the elements removed.
    /// </returns>
    [Pure]
    public ImmutableTrieList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T> equalityComparer)
    {
      Requires.NotNull(items, nameof(items));

      // Some optimizations may apply if we're an empty list.
      if (this.IsEmpty)
      {
        return this;
      }

      return this.Build(builder =>
      {
        foreach (T item in items)
        {
          int index = builder.IndexOf(item, equalityComparer);
          if (index >= 0)
          {
            builder.RemoveAt(index);
          }
        }
      });
    }

    /// <summary>
    /// Removes the specified values from this list.
    /// </summary>
    /// <param name="index">The starting index to begin removal.</param>
    /// <param name="count">The number of elements to remove.</param>
    /// <returns>A new list with the elements removed.</returns>
    [Pure]
    public ImmutableTrieList<T> RemoveRange(int index, int count)
    {
      Requires.Range(index >= 0 && index <= this.Count, nameof(index));
      Requires.Range(count >= 0 && index + count <= this.Count, nameof(count));

      return this.Build(builder =>
      {
        int remaining = count;
        while (remaining-- > 0)
        {
          builder.RemoveAt(index);
        }
      });
    }

    /// <summary>
    /// See the <see cref="IImmutableList{T}"/> interface.
    /// </summary>
    [Pure]
    public ImmutableTrieList<T> Replace(T oldValue, T newValue) =>
      this.Replace(oldValue, newValue, EqualityComparer<T>.Default);

    /// <summary>
    /// See the <see cref="IImmutableList{T}"/> interface.
    /// </summary>
    [Pure]
    public ImmutableTrieList<T> Replace(T oldValue, T newValue, IEqualityComparer<T> equalityComparer)
    {
      int index = this.IndexOf(oldValue, equalityComparer);
      if (index < 0)
      {
        throw new ArgumentException("Cannot found old value", nameof(oldValue));
      }

      return this.SetItem(index, newValue);
    }

    /// <summary>
    /// Reverses the order of the elements in the entire <see cref="ImmutableTrieList{T}"/>.
    /// </summary>
    /// <returns>The reversed list.</returns>
    [Pure]
    public ImmutableTrieList<T> Reverse() =>
      this.Build(builder => builder.Reverse());

    /// <summary>
    /// Reverses the order of the elements in the specified range.
    /// </summary>
    /// <param name="index">The zero-based starting index of the range to reverse.</param>
    /// <param name="count">The number of elements in the range to reverse.</param> 
    /// <returns>The reversed list.</returns>
    [Pure]
    public ImmutableTrieList<T> Reverse(int index, int count) =>
      this.Build(builder => builder.Reverse(index, count));

    /// <summary>
    /// Sorts the elements in the entire <see cref="ImmutableTrieList{T}"/> using
    /// the default comparer.
    /// </summary>
    [Pure]
    public ImmutableTrieList<T> Sort() =>
      this.Build(builder => builder.Sort());

    /// <summary>
    /// Sorts the elements in the entire <see cref="ImmutableTrieList{T}"/> using
    /// the specified comparer.
    /// </summary>
    /// <param name="comparer">
    /// The <see cref="IComparer{T}"/> implementation to use when comparing
    /// elements, or null to use the default comparer <see cref="Comparer{T}.Default"/>.
    /// </param>
    /// <returns>The sorted list.</returns>
    [Pure]
    public ImmutableTrieList<T> Sort(IComparer<T> comparer) =>
      this.Build(builder => builder.Sort(comparer));

    /// <summary>
    /// Sorts the elements in the entire <see cref="ImmutableTrieList{T}"/> using
    /// the specified <see cref="Comparison{T}"/>.
    /// </summary>
    /// <param name="comparison">
    /// The <see cref="Comparison{T}"/> to use when comparing elements.
    /// </param>
    /// <returns>The sorted list.</returns>
    [Pure]
    public ImmutableTrieList<T> Sort(System.Comparison<T> comparison) =>
      this.Build(builder => builder.Sort(comparison));

    /// <summary>
    /// Sorts the elements in a range of elements in <see cref="ImmutableTrieList{T}"/>
    /// using the specified comparer.
    /// </summary>
    /// <param name="index">
    /// The zero-based starting index of the range to sort.
    /// </param>
    /// <param name="count">
    /// The length of the range to sort.
    /// </param>
    /// <param name="comparer">
    /// The <see cref="IComparer{T}"/> implementation to use when comparing
    /// elements, or null to use the default comparer <see cref="Comparer{T}.Default"/>.
    /// </param>
    /// <returns>The sorted list.</returns>
    [Pure]
    public ImmutableTrieList<T> Sort(int index, int count, IComparer<T> comparer) =>
      this.Build(builder => builder.Sort(index, count, comparer));

    #region ICollection members

    /// <summary>
    /// See <see cref="ICollection"/>.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    object ICollection.SyncRoot => this;

    /// <summary>
    /// See the <see cref="ICollection"/> interface.
    /// </summary>
    /// <devremarks>
    /// This type is immutable, so it is always thread-safe.
    /// </devremarks>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool ICollection.IsSynchronized => true;

    /// <summary>
    /// See the <see cref="ICollection"/> interface.
    /// </summary>
    void ICollection.CopyTo(Array array, int arrayIndex) => this.CopyTo(array, arrayIndex);

    #endregion

    #region ICollection<T> members

    /// <summary>
    /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
    /// </summary>
    /// <returns>true.</returns>
    bool ICollection<T>.IsReadOnly => true;

    /// <summary>
    /// Adds the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    void ICollection<T>.Add(T item) => throw new NotSupportedException();

    /// <summary>
    /// Clears this instance.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    void ICollection<T>.Clear() => throw new NotSupportedException();

    /// <summary>
    /// Removes the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>Nothing. An exception is always thrown.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

    #endregion

    #region IList members

    /// <summary>
    /// Gets or sets the value at the specified index.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">Thrown from getter when <paramref name="index"/> is negative or not less than <see cref="Count"/>.</exception>
    /// <exception cref="NotSupportedException">Always thrown from the setter.</exception>
    object IList.this[int index]
    {
      get => this[index];
      set => throw new NotSupportedException();
    }

    /// <summary>
    /// Gets a value indicating whether the <see cref="IList"/> has a fixed size.
    /// </summary>
    /// <returns>true if the <see cref="IList"/> has a fixed size; otherwise, false.</returns>
    bool IList.IsFixedSize => true;

    /// <summary>
    /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
    /// </summary>
    /// <returns>true if the <see cref="ICollection{T}"/> is read-only; otherwise, false.
    ///   </returns>
    bool IList.IsReadOnly => true;

    /// <summary>
    /// Determines whether the <see cref="IList"/> contains a specific value.
    /// </summary>
    /// <param name="value">The object to locate in the <see cref="IList"/>.</param>
    /// <returns>
    /// true if the <see cref="object"/> is found in the <see cref="IList"/>; otherwise, false.
    /// </returns>
    bool IList.Contains(object value) => IsCompatibleObject(value) && this.Contains((T)value);

    /// <summary>
    /// Determines the index of a specific item in the <see cref="IList"/>.
    /// </summary>
    /// <param name="value">The object to locate in the <see cref="IList"/>.</param>
    /// <returns>
    /// The index of <paramref name="value"/> if found in the list; otherwise, -1.
    /// </returns>
    int IList.IndexOf(object value) => IsCompatibleObject(value) ? this.IndexOf((T)value) : -1;

    /// <summary>
    /// Adds an item to the <see cref="IList"/>.
    /// </summary>
    /// <param name="value">The object to add to the <see cref="IList"/>.</param>
    /// <returns>
    /// Nothing. An exception is always thrown.
    /// </returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    int IList.Add(object value) => throw new NotSupportedException();

    /// <summary>
    /// Clears this instance.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    void IList.Clear() => throw new NotSupportedException();

    /// <summary>
    /// Inserts an item to the <see cref="IList"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which <paramref name="value"/> should be inserted.</param>
    /// <param name="value">The object to insert into the <see cref="IList"/>.</param>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    void IList.Insert(int index, object value) => throw new NotSupportedException();

    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="IList"/>.
    /// </summary>
    /// <param name="value">The object to remove from the <see cref="IList"/>.</param>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    void IList.Remove(object value) => throw new NotSupportedException();

    /// <summary>
    /// Removes the value at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <exception cref="NotSupportedException">Always thrown.</exception>

    void IList.RemoveAt(int index) => throw new NotSupportedException();

    #endregion

    #region IList<T> member

    /// <summary>
    /// Gets or sets the value at the specified index.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">Thrown from getter when <paramref name="index"/> is negative or not less than <see cref="Count"/>.</exception>
    /// <exception cref="NotSupportedException">Always thrown from the setter.</exception>
    T IList<T>.this[int index]
    {
      get => this[index];
      set => throw new NotSupportedException();
    }

    /// <summary>
    /// Inserts the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="item">The item.</param>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    void IList<T>.Insert(int index, T item) => throw new NotSupportedException();

    /// <summary>
    /// Removes the value at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    void IList<T>.RemoveAt(int index) => throw new NotSupportedException();

    #endregion

    #region IImmutableList<T> member

    /// <summary>
    /// See the <see cref="IImmutableList{T}"/> interface.
    /// </summary>
    [ExcludeFromCodeCoverage]
    IImmutableList<T> IImmutableList<T>.Add(T value) => this.Add(value);

    /// <summary>
    /// See the <see cref="IImmutableList{T}"/> interface.
    /// </summary>
    [ExcludeFromCodeCoverage]
    IImmutableList<T> IImmutableList<T>.AddRange(IEnumerable<T> items) => this.AddRange(items);

    /// <summary>
    /// See the <see cref="IImmutableList{T}"/> interface.
    /// </summary>
    [ExcludeFromCodeCoverage]
    IImmutableList<T> IImmutableList<T>.Clear() => this.Clear();

    /// <summary>
    /// Inserts the specified value at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert the value.</param>
    /// <param name="item">The element to add.</param>
    /// <returns>The new immutable list.</returns>
    [ExcludeFromCodeCoverage]
    IImmutableList<T> IImmutableList<T>.Insert(int index, T item) => this.Insert(index, item);

    /// <summary>
    /// Inserts the specified value at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert the value.</param>
    /// <param name="items">The elements to add.</param>
    /// <returns>The new immutable list.</returns>
    [ExcludeFromCodeCoverage]
    IImmutableList<T> IImmutableList<T>.InsertRange(int index, IEnumerable<T> items) => this.InsertRange(index, items);

    /// <summary>
    /// See the <see cref="IImmutableList{T}"/> interface.
    /// </summary>
    [ExcludeFromCodeCoverage]
    IImmutableList<T> IImmutableList<T>.Remove(T value, IEqualityComparer<T> equalityComparer) => this.Remove(value, equalityComparer);

    /// <summary>
    /// See the <see cref="IImmutableList{T}"/> interface.
    /// </summary>
    [ExcludeFromCodeCoverage]
    IImmutableList<T> IImmutableList<T>.RemoveAll(Predicate<T> match) => this.RemoveAll(match);

    /// <summary>
    /// Removes the element at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>A new list with the elements removed.</returns>
    [ExcludeFromCodeCoverage]
    IImmutableList<T> IImmutableList<T>.RemoveAt(int index) => this.RemoveAt(index);

    /// <summary>
    /// See the <see cref="IImmutableList{T}"/> interface.
    /// </summary>
    [ExcludeFromCodeCoverage]
    IImmutableList<T> IImmutableList<T>.RemoveRange(IEnumerable<T> items, IEqualityComparer<T> equalityComparer) =>
  this.RemoveRange(items, equalityComparer);

    /// <summary>
    /// See the <see cref="IImmutableList{T}"/> interface.
    /// </summary>
    [ExcludeFromCodeCoverage]
    IImmutableList<T> IImmutableList<T>.RemoveRange(int index, int count) => this.RemoveRange(index, count);

    /// <summary>
    /// Replaces an element in the list with the specified element.
    /// </summary>
    /// <param name="oldValue">The element to replace.</param>
    /// <param name="newValue">The element to replace the old element with.</param>
    /// <param name="equalityComparer">
    /// The equality comparer to use in the search.
    /// If <c>null</c>, <see cref="EqualityComparer{T}.Default"/> is used.
    /// </param>
    /// <returns>The new list.</returns>
    /// <exception cref="ArgumentException">Thrown when the old value does not exist in the list.</exception>
    [ExcludeFromCodeCoverage]
    IImmutableList<T> IImmutableList<T>.Replace(T oldValue, T newValue, IEqualityComparer<T> equalityComparer) =>
  this.Replace(oldValue, newValue, equalityComparer);

    /// <summary>
    /// Replaces an element in the list at a given position with the specified element.
    /// </summary>
    /// <param name="index">The position in the list of the element to replace.</param>
    /// <param name="value">The element to replace the old element with.</param>
    /// <returns>The new list.</returns>
    /// <remarks>
    /// <see cref="this.EqualityComparer"/> will be used compare the existing item at <paramref name="index"/>
    /// and update only when the value is different than the one stored.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    IImmutableList<T> IImmutableList<T>.SetItem(int index, T value) => this.SetItem(index, value);

    #endregion
  }
}