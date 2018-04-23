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
  /// A set of initialization methods for instances of <see cref="ImmutableTrieDictionary{TKey, TValue}"/>.
  /// </summary>
  [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
  public static class ImmutableTrieDictionary
  {
    /// <summary>
    /// Returns an empty collection.
    /// </summary>
    /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
    /// <returns>The immutable collection.</returns>
    [Pure]
    public static ImmutableTrieDictionary<TKey, TValue> Create<TKey, TValue>()
    {
      return ImmutableTrieDictionary<TKey, TValue>.Empty;
    }

    /// <summary>
    /// Returns an empty collection with the specified key comparer.
    /// </summary>
    /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
    /// <param name="keyComparer">The key comparer.</param>
    /// <returns>
    /// The immutable collection.
    /// </returns>
    [Pure]
    public static ImmutableTrieDictionary<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey> keyComparer)
    {
      return ImmutableTrieDictionary<TKey, TValue>.Empty.WithComparers(keyComparer);
    }

    /// <summary>
    /// Returns an empty collection with the specified comparers.
    /// </summary>
    /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
    /// <param name="keyComparer">The key comparer.</param>
    /// <param name="valueComparer">The value comparer.</param>
    /// <returns>
    /// The immutable collection.
    /// </returns>
    [Pure]
    public static ImmutableTrieDictionary<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
    {
      return ImmutableTrieDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer);
    }

    /// <summary>
    /// Creates a new immutable collection prefilled with the specified items.
    /// </summary>
    /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
    /// <param name="items">The items to prepopulate.</param>
    /// <returns>The new immutable collection.</returns>
    [Pure]
    [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static ImmutableTrieDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
      return ImmutableTrieDictionary<TKey, TValue>.Empty.AddRange(items);
    }

    /// <summary>
    /// Creates a new immutable collection prefilled with the specified items.
    /// </summary>
    /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
    /// <param name="keyComparer">The key comparer.</param>
    /// <param name="items">The items to prepopulate.</param>
    /// <returns>The new immutable collection.</returns>
    [Pure]
    [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static ImmutableTrieDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
      return ImmutableTrieDictionary<TKey, TValue>.Empty.WithComparers(keyComparer).AddRange(items);
    }

    /// <summary>
    /// Creates a new immutable collection prefilled with the specified items.
    /// </summary>
    /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
    /// <param name="keyComparer">The key comparer.</param>
    /// <param name="valueComparer">The value comparer.</param>
    /// <param name="items">The items to prepopulate.</param>
    /// <returns>The new immutable collection.</returns>
    [Pure]
    [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static ImmutableTrieDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer, IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
      return ImmutableTrieDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(items);
    }

    /// <summary>
    /// Creates a new immutable dictionary builder.
    /// </summary>
    /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
    /// <returns>The new builder.</returns>
    [Pure]
    public static ImmutableTrieDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>()
    {
      return Create<TKey, TValue>().ToBuilder();
    }

    /// <summary>
    /// Creates a new immutable dictionary builder.
    /// </summary>
    /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
    /// <param name="keyComparer">The key comparer.</param>
    /// <returns>The new builder.</returns>
    [Pure]
    public static ImmutableTrieDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IEqualityComparer<TKey> keyComparer)
    {
      return Create<TKey, TValue>(keyComparer).ToBuilder();
    }

    /// <summary>
    /// Creates a new immutable dictionary builder.
    /// </summary>
    /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
    /// <param name="keyComparer">The key comparer.</param>
    /// <param name="valueComparer">The value comparer.</param>
    /// <returns>The new builder.</returns>
    [Pure]
    public static ImmutableTrieDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
    {
      return Create<TKey, TValue>(keyComparer, valueComparer).ToBuilder();
    }

    /// <summary>
    /// Constructs an immutable dictionary based on some transformation of a sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of element in the sequence.</typeparam>
    /// <typeparam name="TKey">The type of key in the resulting map.</typeparam>
    /// <typeparam name="TValue">The type of value in the resulting map.</typeparam>
    /// <param name="source">The sequence to enumerate to generate the map.</param>
    /// <param name="keySelector">The function that will produce the key for the map from each sequence element.</param>
    /// <param name="elementSelector">The function that will produce the value for the map from each sequence element.</param>
    /// <param name="keyComparer">The key comparer to use for the map.</param>
    /// <param name="valueComparer">The value comparer to use for the map.</param>
    /// <returns>The immutable map.</returns>
    [Pure]
    public static ImmutableTrieDictionary<TKey, TValue> ToImmutableTrieDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
    {
      Requires.NotNull(source, nameof(source));
      Requires.NotNull(keySelector, nameof(keySelector));
      Requires.NotNull(elementSelector, nameof(elementSelector));
      Contract.Ensures(Contract.Result<ImmutableTrieDictionary<TKey, TValue>>() != null);

      return ImmutableTrieDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer)
          .AddRange(source.Select(element => new KeyValuePair<TKey, TValue>(keySelector(element), elementSelector(element))));
    }

    /// <summary>
    /// Constructs an immutable dictionary based on some transformation of a sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of element in the sequence.</typeparam>
    /// <typeparam name="TKey">The type of key in the resulting map.</typeparam>
    /// <typeparam name="TValue">The type of value in the resulting map.</typeparam>
    /// <param name="source">The sequence to enumerate to generate the map.</param>
    /// <param name="keySelector">The function that will produce the key for the map from each sequence element.</param>
    /// <param name="elementSelector">The function that will produce the value for the map from each sequence element.</param>
    /// <param name="keyComparer">The key comparer to use for the map.</param>
    /// <returns>The immutable map.</returns>
    [Pure]
    public static ImmutableTrieDictionary<TKey, TValue> ToImmutableTrieDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey> keyComparer)
    {
      return ToImmutableTrieDictionary(source, keySelector, elementSelector, keyComparer, null);
    }

    /// <summary>
    /// Constructs an immutable dictionary based on some transformation of a sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of element in the sequence.</typeparam>
    /// <typeparam name="TKey">The type of key in the resulting map.</typeparam>
    /// <param name="source">The sequence to enumerate to generate the map.</param>
    /// <param name="keySelector">The function that will produce the key for the map from each sequence element.</param>
    /// <returns>The immutable map.</returns>
    [Pure]
    public static ImmutableTrieDictionary<TKey, TSource> ToImmutableTrieDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
      return ToImmutableTrieDictionary(source, keySelector, v => v, null, null);
    }

    /// <summary>
    /// Constructs an immutable dictionary based on some transformation of a sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of element in the sequence.</typeparam>
    /// <typeparam name="TKey">The type of key in the resulting map.</typeparam>
    /// <param name="source">The sequence to enumerate to generate the map.</param>
    /// <param name="keySelector">The function that will produce the key for the map from each sequence element.</param>
    /// <param name="keyComparer">The key comparer to use for the map.</param>
    /// <returns>The immutable map.</returns>
    [Pure]
    public static ImmutableTrieDictionary<TKey, TSource> ToImmutableTrieDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
    {
      return ToImmutableTrieDictionary(source, keySelector, v => v, keyComparer, null);
    }

    /// <summary>
    /// Constructs an immutable dictionary based on some transformation of a sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of element in the sequence.</typeparam>
    /// <typeparam name="TKey">The type of key in the resulting map.</typeparam>
    /// <typeparam name="TValue">The type of value in the resulting map.</typeparam>
    /// <param name="source">The sequence to enumerate to generate the map.</param>
    /// <param name="keySelector">The function that will produce the key for the map from each sequence element.</param>
    /// <param name="elementSelector">The function that will produce the value for the map from each sequence element.</param>
    /// <returns>The immutable map.</returns>
    [Pure]
    public static ImmutableTrieDictionary<TKey, TValue> ToImmutableTrieDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector)
    {
      return ToImmutableTrieDictionary(source, keySelector, elementSelector, null, null);
    }

    /// <summary>
    /// Creates an immutable dictionary given a sequence of key=value pairs.
    /// </summary>
    /// <typeparam name="TKey">The type of key in the map.</typeparam>
    /// <typeparam name="TValue">The type of value in the map.</typeparam>
    /// <param name="source">The sequence of key=value pairs.</param>
    /// <param name="keyComparer">The key comparer to use when building the immutable map.</param>
    /// <param name="valueComparer">The value comparer to use for the immutable map.</param>
    /// <returns>An immutable map.</returns>
    [Pure]
    [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static ImmutableTrieDictionary<TKey, TValue> ToImmutableTrieDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
    {
      Requires.NotNull(source, nameof(source));
      Contract.Ensures(Contract.Result<ImmutableTrieDictionary<TKey, TValue>>() != null);

      var existingDictionary = source as ImmutableTrieDictionary<TKey, TValue>;
      if (existingDictionary != null)
      {
        return existingDictionary.WithComparers(keyComparer, valueComparer);
      }

      return ImmutableTrieDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(source);
    }

    /// <summary>
    /// Creates an immutable dictionary given a sequence of key=value pairs.
    /// </summary>
    /// <typeparam name="TKey">The type of key in the map.</typeparam>
    /// <typeparam name="TValue">The type of value in the map.</typeparam>
    /// <param name="source">The sequence of key=value pairs.</param>
    /// <param name="keyComparer">The key comparer to use when building the immutable map.</param>
    /// <returns>An immutable map.</returns>
    [Pure]
    [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static ImmutableTrieDictionary<TKey, TValue> ToImmutableTrieDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> keyComparer)
    {
      return ToImmutableTrieDictionary(source, keyComparer, null);
    }

    /// <summary>
    /// Creates an immutable dictionary given a sequence of key=value pairs.
    /// </summary>
    /// <typeparam name="TKey">The type of key in the map.</typeparam>
    /// <typeparam name="TValue">The type of value in the map.</typeparam>
    /// <param name="source">The sequence of key=value pairs.</param>
    /// <returns>An immutable map.</returns>
    [Pure]
    [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static ImmutableTrieDictionary<TKey, TValue> ToImmutableTrieDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
    {
      return ToImmutableTrieDictionary(source, null, null);
    }
  }

  /// <summary>
  /// An immutable unordered dictionary implementation.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  public sealed partial class ImmutableTrieDictionary<TKey, TValue> : IImmutableDictionary<TKey, TValue>, IDictionary<TKey, TValue>, IDictionary
  {
    #region IImmutableDictionary<TKey,TValue> Properties

    /// <summary>
    /// Gets the empty instance.
    /// </summary>
    [ExcludeFromCodeCoverage]
    IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Clear() => this.Clear();

    #endregion

    #region IDictionary<TKey, TValue> Properties

    /// <summary>
    /// Gets the keys.
    /// </summary>
    ICollection<TKey> IDictionary<TKey, TValue>.Keys => new KeysCollectionAccessor<TKey, TValue>(this);

    /// <summary>
    /// Gets the values.
    /// </summary>
    ICollection<TValue> IDictionary<TKey, TValue>.Values => new ValuesCollectionAccessor<TKey, TValue>(this);

    #endregion

    /// <summary>
    /// Gets or sets the <typeparamref name="TValue"/> with the specified key.
    /// </summary>
    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
      get => this[key];
      set => throw new NotSupportedException();
    }

    #region ICollection<KeyValuePair<TKey, TValue>> Properties

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => true;

    #endregion

    #region Public methods



    /// <summary>
    /// See the <see cref="IImmutableDictionary{TKey, TValue}"/> interface.
    /// </summary>
    [Pure]
    [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public ImmutableTrieDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
    {
      Requires.NotNull(pairs, nameof(pairs));
      Contract.Ensures(Contract.Result<ImmutableTrieDictionary<TKey, TValue>>() != null);

      return this.Build(builder => builder.AddRange(pairs));
    }

    /// <summary>
    /// Applies a given set of key=value pairs to an immutable dictionary, replacing any conflicting keys in the resulting dictionary.
    /// </summary>
    /// <param name="items">The key=value pairs to set on the map.  Any keys that conflict with existing keys will overwrite the previous values.</param>
    /// <returns>An immutable dictionary.</returns>
    [Pure]
    [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public ImmutableTrieDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
      Requires.NotNull(items, nameof(items));
      Contract.Ensures(Contract.Result<ImmutableTrieDictionary<TKey, TValue>>() != null);

      return this.Build(builder => {
        foreach(var item in items)
        {
          builder[item.Key] = item.Value; 
        }
      });
    }

    /// <summary>
    /// See the <see cref="IImmutableDictionary{TKey, TValue}"/> interface.
    /// </summary>
    [Pure]
    public ImmutableTrieDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
    {
      Requires.NotNull(keys, nameof(keys));
      Contract.Ensures(Contract.Result<ImmutableTrieDictionary<TKey, TValue>>() != null);

      return this.Build(builder => builder.RemoveRange(keys));
    }

    /// <summary>
    /// Determines whether the specified key contains key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    ///   <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsKey(TKey key) => Helper.ContainsKey(_root, _comparers, key);

    /// <summary>
    /// Determines whether [contains] [the specified key value pair].
    /// </summary>
    /// <param name="pair">The key value pair.</param>
    /// <returns>
    ///   <c>true</c> if [contains] [the specified key value pair]; otherwise, <c>false</c>.
    /// </returns>
    public bool Contains(KeyValuePair<TKey, TValue> pair) => Helper.Contains(_root, _comparers, pair);

    /// <summary>
    /// See the <see cref="IImmutableDictionary{TKey, TValue}"/> interface.
    /// </summary>
    public bool TryGetValue(TKey key, out TValue value) => Helper.TryGetValue(_root, _comparers, key, out value);

    /// <summary>
    /// See the <see cref="IImmutableDictionary{TKey, TValue}"/> interface.
    /// </summary>
    public bool TryGetKey(TKey equalKey, out TKey actualKey) => Helper.TryGetKey(_root, _comparers, equalKey, out actualKey);

    /// <summary>
    /// See the <see cref="IImmutableDictionary{TKey, TValue}"/> interface.
    /// </summary>
    [Pure]
    public ImmutableTrieDictionary<TKey, TValue> WithComparers(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
    {
      if (keyComparer == null)
      {
        keyComparer = EqualityComparer<TKey>.Default;
      }

      if (valueComparer == null)
      {
        valueComparer = EqualityComparer<TValue>.Default;
      }

      if (this.KeyComparer == keyComparer)
      {
        if (this.ValueComparer == valueComparer)
        {
          return this;
        }
        else
        {
          // When the key comparer is the same but the value comparer is different, we don't need a whole new tree
          // because the structure of the tree does not depend on the value comparer.
          // We just need a new root node to store the new value comparer.
          var comparers = _comparers.WithValueComparer(valueComparer);
          return new ImmutableTrieDictionary<TKey, TValue>(_count, _root, comparers);
        }
      }
      else
      {
        // The trie mostly based on the hash, the key was used in the core structure
        var comparers = Comparers.Get(keyComparer, valueComparer);
        var set = EmptyWithComparers(comparers).AddRange(this);
        return set;
      }
    }

    /// <summary>
    /// See the <see cref="IImmutableDictionary{TKey, TValue}"/> interface.
    /// </summary>
    [Pure]
    public ImmutableTrieDictionary<TKey, TValue> WithComparers(IEqualityComparer<TKey> keyComparer)
    {
      return this.WithComparers(keyComparer, _comparers.ValueComparer);
    }

    /// <summary>
    /// Determines whether the <see cref="ImmutableTrieDictionary{TKey, TValue}"/>
    /// contains an element with the specified value.
    /// </summary>
    /// <param name="value">
    /// The value to locate in the <see cref="ImmutableTrieDictionary{TKey, TValue}"/>.
    /// The value can be null for reference types.
    /// </param>
    /// <returns>
    /// true if the <see cref="ImmutableTrieDictionary{TKey, TValue}"/> contains
    /// an element with the specified value; otherwise, false.
    /// </returns>
    [Pure]
    public bool ContainsValue(TValue value) => Helper.ContainsValue(this, _comparers, value);

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(_root);
    }

    #endregion

    #region IImmutableDictionary<TKey,TValue> Methods

    /// <summary>
    /// See the <see cref="IImmutableDictionary{TKey, TValue}"/> interface
    /// </summary>
    [ExcludeFromCodeCoverage]
    IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Add(TKey key, TValue value) => this.Add(key, value);

    /// <summary>
    /// See the <see cref="IImmutableDictionary{TKey, TValue}"/> interface
    /// </summary>
    [ExcludeFromCodeCoverage]
    IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItem(TKey key, TValue value) => this.SetItem(key, value);

    /// <summary>
    /// Applies a given set of key=value pairs to an immutable dictionary, replacing any conflicting keys in the resulting dictionary.
    /// </summary>
    /// <param name="items">The key=value pairs to set on the map.  Any keys that conflict with existing keys will overwrite the previous values.</param>
    /// <returns>An immutable dictionary.</returns>
    IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items) => this.SetItems(items);

    /// <summary>
    /// See the <see cref="IImmutableDictionary{TKey, TValue}"/> interface
    /// </summary>
    [ExcludeFromCodeCoverage]
    IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs) => this.AddRange(pairs);

    /// <summary>
    /// See the <see cref="IImmutableDictionary{TKey, TValue}"/> interface
    /// </summary>
    [ExcludeFromCodeCoverage]
    IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.RemoveRange(IEnumerable<TKey> keys) => this.RemoveRange(keys);

    /// <summary>
    /// See the <see cref="IImmutableDictionary{TKey, TValue}"/> interface
    /// </summary>
    [ExcludeFromCodeCoverage]
    IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Remove(TKey key) => this.Remove(key);

    #endregion

    #region IDictionary<TKey, TValue> Methods

    /// <summary>
    /// Adds an element with the provided key and value to the <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="key">The object to use as the key of the element to add.</param>
    /// <param name="value">The object to use as the value of the element to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the <see cref="IDictionary{TKey, TValue}"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="IDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => throw new NotSupportedException();

    /// <summary>
    /// Removes the element with the specified key from the <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>
    /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="IDictionary{TKey, TValue}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="IDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    bool IDictionary<TKey, TValue>.Remove(TKey key) => throw new NotSupportedException();

    #endregion

    #region ICollection<KeyValuePair<TKey, TValue>> Methods

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => throw new NotSupportedException();

    void ICollection<KeyValuePair<TKey, TValue>>.Clear() => throw new NotSupportedException();

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => throw new NotSupportedException();

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      Requires.NotNull(array, nameof(array));
      Requires.Range(arrayIndex >= 0, nameof(arrayIndex));
      Requires.Range(array.Length >= arrayIndex + this.Count, nameof(arrayIndex));

      foreach (var item in this)
      {
        array[arrayIndex++] = item;
      }
    }

    #endregion

    #region IDictionary Properties

    /// <summary>
    /// Gets a value indicating whether the <see cref="IDictionary"/> object has a fixed size.
    /// </summary>
    /// <returns>true if the <see cref="IDictionary"/> object has a fixed size; otherwise, false.</returns>
    bool IDictionary.IsFixedSize => true;

    /// <summary>
    /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
    /// </summary>
    /// <returns>true if the <see cref="ICollection{T}"/> is read-only; otherwise, false.
    ///   </returns>
    bool IDictionary.IsReadOnly => true;

    /// <summary>
    /// Gets an <see cref="ICollection{T}"/> containing the keys of the <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <returns>
    /// An <see cref="ICollection{T}"/> containing the keys of the object that implements <see cref="IDictionary{TKey, TValue}"/>.
    ///   </returns>
    ICollection IDictionary.Keys
    {
      get { return new KeysCollectionAccessor<TKey, TValue>(this); }
    }

    /// <summary>
    /// Gets an <see cref="ICollection{T}"/> containing the values in the <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <returns>
    /// An <see cref="ICollection{T}"/> containing the values in the object that implements <see cref="IDictionary{TKey, TValue}"/>.
    ///   </returns>
    ICollection IDictionary.Values
    {
      get { return new ValuesCollectionAccessor<TKey, TValue>(this); }
    }

    #endregion

    #region IDictionary Methods

    /// <summary>
    /// Adds an element with the provided key and value to the <see cref="IDictionary"/> object.
    /// </summary>
    /// <param name="key">The <see cref="object"/> to use as the key of the element to add.</param>
    /// <param name="value">The <see cref="object"/> to use as the value of the element to add.</param>
    void IDictionary.Add(object key, object value) => throw new NotSupportedException();

    /// <summary>
    /// Determines whether the <see cref="IDictionary"/> object contains an element with the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the <see cref="IDictionary"/> object.</param>
    /// <returns>
    /// true if the <see cref="IDictionary"/> contains an element with the key; otherwise, false.
    /// </returns>
    bool IDictionary.Contains(object key) => this.ContainsKey((TKey)key);

    /// <summary>
    /// Returns an <see cref="IDictionaryEnumerator"/> object for the <see cref="IDictionary"/> object.
    /// </summary>
    /// <returns>
    /// An <see cref="IDictionaryEnumerator"/> object for the <see cref="IDictionary"/> object.
    /// </returns>
    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
      return new DictionaryEnumerator<TKey, TValue>(this.GetEnumerator());
    }

    /// <summary>
    /// Removes the element with the specified key from the <see cref="IDictionary"/> object.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    void IDictionary.Remove(object key) => throw new NotSupportedException();

    /// <summary>
    /// Gets or sets the element with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    object IDictionary.this[object key]
    {
      get => this[(TKey)key];
      set => throw new NotSupportedException();
    }

    /// <summary>
    /// Clears this instance.
    /// </summary>
    /// <exception cref="System.NotSupportedException"></exception>
    void IDictionary.Clear() => throw new NotSupportedException();

    #endregion

    #region ICollection Methods

    /// <summary>
    /// Copies the elements of the <see cref="ICollection"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ICollection"/>. The <see cref="Array"/> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
    void ICollection.CopyTo(Array array, int arrayIndex)
    {
      Requires.NotNull(array, nameof(array));
      Requires.Range(arrayIndex >= 0, nameof(arrayIndex));
      Requires.Range(array.Length >= arrayIndex + this.Count, nameof(arrayIndex));

      foreach (var item in this)
      {
        array.SetValue(new DictionaryEntry(item.Key, item.Value), arrayIndex++);
      }
    }

    #endregion

    #region ICollection Properties

    /// <summary>
    /// Gets an object that can be used to synchronize access to the <see cref="ICollection"/>.
    /// </summary>
    /// <returns>An object that can be used to synchronize access to the <see cref="ICollection"/>.</returns>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    object ICollection.SyncRoot => this;

    /// <summary>
    /// Gets a value indicating whether access to the <see cref="ICollection"/> is synchronized (thread safe).
    /// </summary>
    /// <returns>true if access to the <see cref="ICollection"/> is synchronized (thread safe); otherwise, false.</returns>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool ICollection.IsSynchronized => true; // This is immutable, so it is always thread-safe.

    #endregion

    #region IEnumerable<KeyValuePair<TKey, TValue>> Members

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
      return this.IsEmpty ?
          Enumerable.Empty<KeyValuePair<TKey, TValue>>().GetEnumerator() :
          this.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    #endregion
  }
}