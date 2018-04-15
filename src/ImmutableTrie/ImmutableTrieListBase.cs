using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Validation;

namespace ImmutableTrie
{
  /// <summary>
  /// The base class of <see cref="ImmutableTrieList{T}"/> and <see cref="ImmutableTrieList{T}.Builder"/>.
  /// </summary>
  public abstract partial class ImmutableTrieListBase<T> : IEnumerable<T>, IEnumerable
  {
    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    public ImmutableTrieList<T>.Enumerator GetEnumerator() => new ImmutableTrieList<T>.Enumerator(this);

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// See <see cref="IList{T}"/>
    /// </summary>
    public bool Contains(T item)
    {
      return this.IndexOf(item) >= 0;
    }

    /// <summary>
    /// Creates a shallow copy of a range of elements in the source <see cref="ImmutableTrieList{T}"/>.
    /// </summary>
    /// <param name="index">
    /// The zero-based <see cref="ImmutableTrieList{T}"/> index at which the range
    /// starts.
    /// </param>
    /// <param name="count">
    /// The number of elements in the range.
    /// </param>
    /// <returns>
    /// A shallow copy of a range of elements in the source <see cref="ImmutableTrieList{T}"/>.
    /// </returns>
    /// <remarks>This method re-indexes values, it produces a complete copy, which has O(N) complexity.</remarks>
    public ImmutableTrieList<T> GetRange(int index, int count) => ImmutableTrieList.CreateRange(this.Skip(index).Take(count));

    /// <summary>
    /// Searches the entire sorted <see cref="ImmutableTrieList{T}"/> for an element
    /// using the default comparer and returns the zero-based index of the element.
    /// </summary>
    /// <param name="item">The object to locate. The value can be null for reference types.</param>
    /// <returns>
    /// The zero-based index of item in the sorted <see cref="ImmutableTrieList{T}"/>,
    /// if item is found; otherwise, a negative number that is the bitwise complement
    /// of the index of the next element that is larger than item or, if there is
    /// no larger element, the bitwise complement of <see cref="ImmutableTrieList{T}"/>.Count.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The default comparer <see cref="Comparer{T}.Default"/> cannot
    /// find an implementation of the System.IComparable&lt;T&gt; generic interface or
    /// the System.IComparable interface for type T.
    /// </exception>
    public int BinarySearch(T item)
    {
      return this.BinarySearch(item, null);
    }

    /// <summary>
    ///  Searches the entire sorted <see cref="ImmutableTrieList{T}"/> for an element
    ///  using the specified comparer and returns the zero-based index of the element.
    /// </summary>
    /// <param name="item">The object to locate. The value can be null for reference types.</param>
    /// <param name="comparer">
    /// The <see cref="IComparer{T}"/>  implementation to use when comparing
    /// elements.-or-null to use the default comparer <see cref="Comparer{T}.Default"/>.
    /// </param>
    /// <returns>
    /// The zero-based index of item in the sorted <see cref="ImmutableTrieList{T}"/>,
    /// if item is found; otherwise, a negative number that is the bitwise complement
    /// of the index of the next element that is larger than item or, if there is
    /// no larger element, the bitwise complement of <see cref="ImmutableTrieList{T}"/>.Count.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// comparer is null, and the default comparer <see cref="Comparer{T}.Default"/>
    /// cannot find an implementation of the System.IComparable&lt;T&gt; generic interface
    /// or the System.IComparable interface for type T.
    /// </exception>
    public int BinarySearch(T item, IComparer<T> comparer)
    {
      return this.BinarySearch(0, this.Count, item, comparer);
    }

    /// <summary>
    /// Searches a range of elements in the sorted <see cref="ImmutableTrieList{T}"/>
    /// for an element using the specified comparer and returns the zero-based index
    /// of the element.
    /// </summary>
    /// <param name="index">The zero-based starting index of the range to search.</param>
    /// <param name="count"> The length of the range to search.</param>
    /// <param name="item">The object to locate. The value can be null for reference types.</param>
    /// <param name="comparer">
    /// The <see cref="IComparer{T}"/> implementation to use when comparing
    /// elements, or null to use the default comparer <see cref="Comparer{T}.Default"/>.
    /// </param>
    /// <returns>
    /// The zero-based index of item in the sorted <see cref="ImmutableTrieList{T}"/>,
    /// if item is found; otherwise, a negative number that is the bitwise complement
    /// of the index of the next element that is larger than item or, if there is
    /// no larger element, the bitwise complement of <see cref="ImmutableTrieList{T}.Count"/>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0.-or-<paramref name="count"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="index"/> and <paramref name="count"/> do not denote a valid range in the <see cref="ImmutableTrieList{T}"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="comparer"/> is null, and the default comparer <see cref="Comparer{T}.Default"/>
    /// cannot find an implementation of the <see cref="IComparable{T}"/> generic interface
    /// or the <see cref="IComparable"/> interface for type <typeparamref name="T"/>.
    /// </exception>
    public abstract int BinarySearch(int index, int count, T item, IComparer<T> comparer);

    /// <summary>
    /// See <see cref="IList{T}"/>
    /// </summary>
    public int IndexOf(T item)
    {
      return this.IndexOf(item, EqualityComparer<T>.Default);
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the
    /// first occurrence within the range of elements in the ImmutableTrieList&lt;T&gt;
    /// that extends from the specified index to the last element.
    /// </summary>
    /// <param name="item">
    /// The object to locate in the ImmutableTrieList&lt;T&gt;. The value
    /// can be null for reference types.
    /// </param>
    /// <param name="index">
    /// The zero-based starting index of the search. 0 (zero) is valid in an empty
    /// list.
    /// </param>
    /// <returns>
    /// The zero-based index of the first occurrence of item within the range of
    /// elements in the ImmutableTrieList&lt;T&gt; that extends from index
    /// to the last element, if found; otherwise, -1.
    /// </returns>
    [Pure]
    public int IndexOf(T item, int index) =>
        this.IndexOf(item, index, this.Count - index, EqualityComparer<T>.Default);

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the
    /// first occurrence within the range of elements in the ImmutableTrieList&lt;T&gt;
    /// that starts at the specified index and contains the specified number of elements.
    /// </summary>
    /// <param name="item">
    /// The object to locate in the ImmutableTrieList&lt;T&gt;. The value
    /// can be null for reference types.
    /// </param>
    /// <param name="index">
    /// The zero-based starting index of the search. 0 (zero) is valid in an empty
    /// list.
    /// </param>
    /// <param name="count">
    /// The number of elements in the section to search.
    /// </param>
    /// <returns>
    /// The zero-based index of the first occurrence of item within the range of
    /// elements in the ImmutableTrieList&lt;T&gt; that starts at index and
    /// contains count number of elements, if found; otherwise, -1.
    /// </returns>
    [Pure]
    public int IndexOf(T item, int index, int count) =>
        this.IndexOf(item, index, count, EqualityComparer<T>.Default);

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the
    /// first occurrence within the range of elements in the <see cref="ImmutableTrieList{T}"/>
    /// that starts at the specified index and contains the specified number of elements.
    /// </summary>
    /// <param name="item">
    /// The object to locate in the <see cref="ImmutableTrieList{T}"/>. The value
    /// can be null for reference types.
    /// </param>
    /// <param name="equalityComparer">The equality comparer to use for testing the match of two elements.</param>
    /// <returns>
    /// The zero-based index of the first occurrence of <paramref name="item"/> within the entire
    /// <see cref="ImmutableTrieList{T}"/>, if found; otherwise, -1.
    /// </returns>
    [Pure]
    public int IndexOf(T item, IEqualityComparer<T> equalityComparer) => this.IndexOf(item, 0, this.Count, equalityComparer);

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the
    /// first occurrence within the range of elements in the <see cref="ImmutableTrieList{T}"/>
    /// that starts at the specified index and contains the specified number of elements.
    /// </summary>
    /// <param name="item">
    /// The object to locate in the <see cref="ImmutableTrieList{T}"/>. The value
    /// can be null for reference types.
    /// </param>
    /// <param name="index">
    /// The zero-based starting index of the search. 0 (zero) is valid in an empty
    /// list.
    /// </param>
    /// <param name="count">
    /// The number of elements in the section to search.
    /// </param>
    /// <param name="equalityComparer">
    /// The equality comparer to use in the search.
    /// If <c>null</c>, <see cref="EqualityComparer{T}.Default"/> is used.
    /// </param>
    /// <returns>
    /// The zero-based index of the first occurrence of <paramref name="item"/> within the range of
    /// elements in the <see cref="ImmutableTrieList{T}"/> that starts at <paramref name="index"/> and
    /// contains <paramref name="count"/> number of elements, if found; otherwise, -1.
    /// </returns>
    [Pure]
    public int IndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer)
    {
      Requires.Range(index >= 0, nameof(index));
      Requires.Range(count >= 0, nameof(count));
      Requires.Range(count <= this.Count, nameof(count));
      Requires.Range(index + count <= this.Count, nameof(count));

      equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
      using (var enumerator = new ImmutableTrieList<T>.Enumerator(this, startIndex: index, count: count))
      {
        while (enumerator.MoveNext())
        {
          if (equalityComparer.Equals(item, enumerator.Current))
          {
            return index;
          }

          index++;
        }
      }

      return -1;
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the
    /// last occurrence within the range of elements in the <see cref="ImmutableTrieList{T}"/>
    /// that contains the specified number of elements and ends at the specified
    /// index.
    /// </summary>
    /// <param name="item">
    /// The object to locate in the <see cref="ImmutableTrieList{T}"/>. The value
    /// can be null for reference types.
    /// </param>
    /// <returns>
    /// The zero-based index of the last occurrence of item within the range of elements
    /// in the <see cref="ImmutableTrieList{T}"/> that contains count number of elements
    /// and ends at index, if found; otherwise, -1.
    /// </returns>
    [Pure]
    public int LastIndexOf(T item)
    {
      if (this.Count == 0)
      {
        return -1;
      }

      return this.LastIndexOf(item, this.Count - 1, this.Count, EqualityComparer<T>.Default);
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the
    /// last occurrence within the range of elements in the <see cref="ImmutableTrieList{T}"/>
    /// that contains the specified number of elements and ends at the specified
    /// index.
    /// </summary>
    /// <param name="item">
    /// The object to locate in the <see cref="ImmutableTrieList{T}"/>. The value
    /// can be null for reference types.
    /// </param>
    /// <param name="startIndex">The zero-based starting index of the backward search.</param>
    /// <returns>
    /// The zero-based index of the last occurrence of item within the range of elements
    /// in the <see cref="ImmutableTrieList{T}"/> that contains count number of elements
    /// and ends at index, if found; otherwise, -1.
    /// </returns>
    [Pure]
    public int LastIndexOf(T item, int startIndex)
    {
      if (this.Count == 0 && startIndex == 0)
      {
        return -1;
      }

      return this.LastIndexOf(item, startIndex, startIndex + 1, EqualityComparer<T>.Default);
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the
    /// last occurrence within the range of elements in the <see cref="ImmutableTrieList{T}"/>
    /// that contains the specified number of elements and ends at the specified
    /// index.
    /// </summary>
    /// <param name="item">
    /// The object to locate in the <see cref="ImmutableTrieList{T}"/>. The value
    /// can be null for reference types.
    /// </param>
    /// <param name="startIndex">The zero-based starting index of the backward search.</param>
    /// <param name="count">The number of elements in the section to search.</param>
    /// <returns>
    /// The zero-based index of the last occurrence of item within the range of elements
    /// in the <see cref="ImmutableTrieList{T}"/> that contains count number of elements
    /// and ends at index, if found; otherwise, -1.
    /// </returns>
    [Pure]
    public int LastIndexOf(T item, int startIndex, int count) =>
        this.LastIndexOf(item, startIndex, count, EqualityComparer<T>.Default);

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the
    /// last occurrence within the range of elements in the <see cref="ImmutableTrieList{T}"/>
    /// that contains the specified number of elements and ends at the specified
    /// index.
    /// </summary>
    /// <param name="item">
    /// The object to locate in the <see cref="ImmutableTrieList{T}"/>. The value
    /// can be null for reference types.
    /// </param>
    /// <param name="index">The zero-based starting index of the backward search.</param>
    /// <param name="count">The number of elements in the section to search.</param>
    /// <param name="equalityComparer">The equality comparer to use for testing the match of two elements.</param>
    /// <returns>
    /// The zero-based index of the last occurrence of <paramref name="item"/> within the range of elements
    /// in the <see cref="ImmutableTrieList{T}"/> that contains <paramref name="count"/> number of elements
    /// and ends at <paramref name="index"/>, if found; otherwise, -1.
    /// </returns>
    [Pure]
    public int LastIndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer)
    {
      Requires.Range(index >= 0, nameof(index));
      Requires.Range(count >= 0 && count <= this.Count, nameof(count));
      Requires.Argument(
        index - count + 1 >= 0,
        nameof(count),
        "The specified {0} and {1} do not produce a enumerable range.",
        nameof(index),
        nameof(count));

      equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
      using (var enumerator = new ImmutableTrieList<T>.Enumerator(this, startIndex: index, count: count, reversed: true))
      {
        while (enumerator.MoveNext())
        {
          if (equalityComparer.Equals(item, enumerator.Current))
          {
            return index;
          }

          index--;
        }
      }

      return -1;
    }

    /// <summary>
    /// Performs the specified action on each element of the list.
    /// </summary>
    /// <param name="action">The <see cref="System.Action{T}"/> delegate to perform on each element of the list.</param>
    public void ForEach(Action<T> action)
    {
      Requires.NotNull(action, nameof(action));

      foreach (T item in this)
      {
        action(item);
      }
    }

    /// <summary>
    /// Copies the entire <see cref="ImmutableTrieList{T}"/> to a compatible one-dimensional
    /// array, starting at the beginning of the target array.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements
    /// copied from <see cref="ImmutableTrieList{T}"/>. The <see cref="Array"/> must have
    /// zero-based indexing.
    /// </param>
    public void CopyTo(T[] array)
    {
      Requires.NotNull(array, nameof(array));
      Requires.Range(array.Length >= this.Count, nameof(array));

      int index = 0;
      foreach (var element in this)
      {
        array[index++] = element;
      }
    }

    /// <summary>
    /// Copies the entire <see cref="ImmutableTrieList{T}"/> to a compatible one-dimensional
    /// array, starting at the specified index of the target array.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements
    /// copied from <see cref="ImmutableTrieList{T}"/>. The <see cref="Array"/> must have
    /// zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">
    /// The zero-based index in <paramref name="array"/> at which copying begins.
    /// </param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      Requires.NotNull(array, nameof(array));
      Requires.Range(arrayIndex >= 0, nameof(arrayIndex));
      Requires.Range(array.Length >= arrayIndex + this.Count, nameof(arrayIndex));

      foreach (T element in this)
      {
        array[arrayIndex++] = element;
      }
    }

    /// <summary>
    /// Copies a range of elements from the <see cref="ImmutableTrieList{T}"/> to
    /// a compatible one-dimensional array, starting at the specified index of the
    /// target array.
    /// </summary>
    /// <param name="index">
    /// The zero-based index in the source <see cref="ImmutableTrieList{T}"/> at
    /// which copying begins.
    /// </param>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements
    /// copied from <see cref="ImmutableTrieList{T}"/>. The <see cref="Array"/> must have
    /// zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
    /// <param name="count">The number of elements to copy.</param>
    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
      Requires.NotNull(array, nameof(array));
      Requires.Range(index >= 0, nameof(index));
      Requires.Range(count >= 0, nameof(count));
      Requires.Range(index + count <= this.Count, nameof(count));
      Requires.Range(arrayIndex >= 0, nameof(arrayIndex));
      Requires.Range(arrayIndex + count <= array.Length, nameof(arrayIndex));

      using (var enumerator = new ImmutableTrieList<T>.Enumerator(this, startIndex: index, count: count))
      {
        while (enumerator.MoveNext())
        {
          array[arrayIndex++] = enumerator.Current;
        }
      }
    }

    /// <summary>
    /// Copies the elements of the <see cref="ICollection"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ICollection"/>. The <see cref="Array"/> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
    public void CopyTo(Array array, int arrayIndex)
    {
      Requires.NotNull(array, nameof(array));
      Requires.Range(arrayIndex >= 0, nameof(arrayIndex));
      Requires.Range(array.Length >= arrayIndex + this.Count, nameof(arrayIndex));

      foreach (T element in this)
      {
        array.SetValue(element, arrayIndex++);
      }
    }

    /// <summary>
    /// Converts the elements in the current <see cref="ImmutableTrieList{T}"/> to
    /// another type, and returns a list containing the converted elements.
    /// </summary>
    /// <param name="converter">
    /// A <see cref="Func{T, TResult}"/> delegate that converts each element from
    /// one type to another type.
    /// </param>
    /// <typeparam name="TOutput">
    /// The type of the elements of the target array.
    /// </typeparam>
    /// <returns>
    /// A node tree with the transformed list.
    /// </returns>
    public ImmutableTrieList<TOutput> ConvertAll<TOutput>(Func<T, TOutput> converter) =>
      ImmutableTrieList<TOutput>.Empty.AddRange(System.Linq.Enumerable.Select(this, converter));

    /// <summary>
    /// Determines whether every element in the <see cref="ImmutableTrieList{T}"/>
    /// matches the conditions defined by the specified predicate.
    /// </summary>
    /// <param name="match">
    /// The <see cref="Predicate{T}"/> delegate that defines the conditions to check against
    /// the elements.
    /// </param>
    /// <returns>
    /// true if every element in the <see cref="ImmutableTrieList{T}"/> matches the
    /// conditions defined by the specified predicate; otherwise, false. If the list
    /// has no elements, the return value is true.
    /// </returns>
    public bool TrueForAll(Predicate<T> match)
    {
      Requires.NotNull(match, nameof(match));
      foreach (T item in this)
      {
        if (!match(item))
        {
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Determines whether the <see cref="ImmutableTrieList{T}"/> contains elements
    /// that match the conditions defined by the specified predicate.
    /// </summary>
    /// <param name="match">
    /// The <see cref="Predicate{T}"/> delegate that defines the conditions of the elements
    /// to search for.
    /// </param>
    /// <returns>
    /// true if the <see cref="ImmutableTrieList{T}"/> contains one or more elements
    /// that match the conditions defined by the specified predicate; otherwise,
    /// false.
    /// </returns>
    public bool Exists(Predicate<T> match)
    {
      Requires.NotNull(match, nameof(match));

      foreach (T item in this)
      {
        if (match(item))
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified
    /// predicate, and returns the first occurrence within the entire <see cref="ImmutableTrieList{T}"/>.
    /// </summary>
    /// <param name="match">
    /// The <see cref="Predicate{T}"/> delegate that defines the conditions of the element
    /// to search for.
    /// </param>
    /// <returns>
    /// The first element that matches the conditions defined by the specified predicate,
    /// if found; otherwise, the default value for type <typeparamref name="T"/>.
    /// </returns>
    public T Find(Predicate<T> match)
    {
      Requires.NotNull(match, nameof(match));

      foreach (var item in this)
      {
        if (match(item))
        {
          return item;
        }
      }

      return default(T);
    }

    /// <summary>
    /// Retrieves all the elements that match the conditions defined by the specified
    /// predicate.
    /// </summary>
    /// <param name="match">
    /// The <see cref="Predicate{T}"/> delegate that defines the conditions of the elements
    /// to search for.
    /// </param>
    /// <returns>
    /// A <see cref="ImmutableTrieList{T}"/> containing all the elements that match
    /// the conditions defined by the specified predicate, if found; otherwise, an
    /// empty <see cref="ImmutableTrieList{T}"/>.
    /// </returns>
    public ImmutableTrieList<T> FindAll(Predicate<T> match)
    {
      Requires.NotNull(match, nameof(match));
      if (this.IsEmpty)
      {
        return ImmutableTrieList<T>.Empty;
      }

      List<T> list = null;
      foreach (var item in this)
      {
        if (match(item))
        {
          if (list == null)
          {
            list = new List<T>();
          }

          list.Add(item);
        }
      }

      return list != null ?
          ImmutableTrieList.CreateRange(list) :
          ImmutableTrieList<T>.Empty;
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified
    /// predicate, and returns the zero-based index of the first occurrence within
    /// the entire <see cref="ImmutableTrieList{T}"/>.
    /// </summary>
    /// <param name="match">
    /// The <see cref="Predicate{T}"/> delegate that defines the conditions of the element
    /// to search for.
    /// </param>
    /// <returns>
    /// The zero-based index of the first occurrence of an element that matches the
    /// conditions defined by <paramref name="match"/>, if found; otherwise, -1.
    /// </returns>
    public int FindIndex(Predicate<T> match)
    {
      Requires.NotNull(match, nameof(match));
      return this.FindIndex(0, Count, match);
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified
    /// predicate, and returns the zero-based index of the first occurrence within
    /// the range of elements in the <see cref="ImmutableTrieList{T}"/> that extends
    /// from the specified index to the last element.
    /// </summary>
    /// <param name="startIndex">The zero-based starting index of the search.</param>
    /// <param name="match">The <see cref="Predicate{T}"/> delegate that defines the conditions of the element to search for.</param>
    /// <returns>
    /// The zero-based index of the first occurrence of an element that matches the
    /// conditions defined by <paramref name="match"/>, if found; otherwise, -1.
    /// </returns>
    public int FindIndex(int startIndex, Predicate<T> match)
    {
      Requires.NotNull(match, nameof(match));
      Requires.Range(startIndex >= 0 && startIndex <= this.Count, nameof(startIndex));

      return this.FindIndex(startIndex, this.Count - startIndex, match);
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified
    /// predicate, and returns the zero-based index of the first occurrence within
    /// the range of elements in the <see cref="ImmutableTrieList{T}"/> that starts
    /// at the specified index and contains the specified number of elements.
    /// </summary>
    /// <param name="startIndex">The zero-based starting index of the search.</param>
    /// <param name="count">The number of elements in the section to search.</param>
    /// <param name="match">The <see cref="Predicate{T}"/> delegate that defines the conditions of the element to search for.</param>
    /// <returns>
    /// The zero-based index of the first occurrence of an element that matches the
    /// conditions defined by <paramref name="match"/>, if found; otherwise, -1.
    /// </returns>
    public int FindIndex(int startIndex, int count, Predicate<T> match)
    {
      Requires.NotNull(match, nameof(match));
      Requires.Range(startIndex >= 0, nameof(startIndex));
      Requires.Range(count >= 0, nameof(count));
      Requires.Range(startIndex + count <= this.Count, nameof(count));

      using (var enumerator = new ImmutableTrieList<T>.Enumerator(this, startIndex: startIndex, count: count))
      {
        int index = startIndex;
        while (enumerator.MoveNext())
        {
          if (match(enumerator.Current))
          {
            return index;
          }

          index++;
        }
      }

      return -1;
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified
    /// predicate, and returns the last occurrence within the entire <see cref="ImmutableTrieList{T}"/>.
    /// </summary>
    /// <param name="match">
    /// The <see cref="Predicate{T}"/> delegate that defines the conditions of the element
    /// to search for.
    /// </param>
    /// <returns>
    /// The last element that matches the conditions defined by the specified predicate,
    /// if found; otherwise, the default value for type <typeparamref name="T"/>.
    /// </returns>
    public T FindLast(Predicate<T> match)
    {
      Requires.NotNull(match, nameof(match));
      using (var enumerator = new ImmutableTrieList<T>.Enumerator(this, reversed: true))
      {
        while (enumerator.MoveNext())
        {
          if (match(enumerator.Current))
          {
            return enumerator.Current;
          }
        }
      }

      return default(T);
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified
    /// predicate, and returns the zero-based index of the last occurrence within
    /// the entire <see cref="ImmutableTrieList{T}"/>.
    /// </summary>
    /// <param name="match">
    /// The <see cref="Predicate{T}"/> delegate that defines the conditions of the element
    /// to search for.
    /// </param>
    /// <returns>
    /// The zero-based index of the last occurrence of an element that matches the
    /// conditions defined by <paramref name="match"/>, if found; otherwise, -1.
    /// </returns>
    public int FindLastIndex(Predicate<T> match)
    {
      Requires.NotNull(match, nameof(match));
      return this.IsEmpty ? -1 : this.FindLastIndex(this.Count - 1, this.Count, match);
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified
    /// predicate, and returns the zero-based index of the last occurrence within
    /// the range of elements in the <see cref="ImmutableTrieList{T}"/> that extends
    /// from the first element to the specified index.
    /// </summary>
    /// <param name="startIndex">The zero-based starting index of the backward search.</param>
    /// <param name="match">The <see cref="Predicate{T}"/> delegate that defines the conditions of the element
    /// to search for.</param>
    /// <returns>
    /// The zero-based index of the last occurrence of an element that matches the
    /// conditions defined by <paramref name="match"/>, if found; otherwise, -1.
    /// </returns>
    public int FindLastIndex(int startIndex, Predicate<T> match)
    {
      Requires.NotNull(match, nameof(match));
      Requires.Range(startIndex >= 0, nameof(startIndex));
      Requires.Range(startIndex == 0 || startIndex < this.Count, nameof(startIndex));

      return this.IsEmpty ? -1 : this.FindLastIndex(startIndex, startIndex + 1, match);
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified
    /// predicate, and returns the zero-based index of the last occurrence within
    /// the range of elements in the <see cref="ImmutableTrieList{T}"/> that contains
    /// the specified number of elements and ends at the specified index.
    /// </summary>
    /// <param name="startIndex">The zero-based starting index of the backward search.</param>
    /// <param name="count">The number of elements in the section to search.</param>
    /// <param name="match">
    /// The <see cref="Predicate{T}"/> delegate that defines the conditions of the element
    /// to search for.
    /// </param>
    /// <returns>
    /// The zero-based index of the last occurrence of an element that matches the
    /// conditions defined by <paramref name="match"/>, if found; otherwise, -1.
    /// </returns>
    public int FindLastIndex(int startIndex, int count, Predicate<T> match)
    {
      Requires.NotNull(match, nameof(match));
      Requires.Range(startIndex >= 0, nameof(startIndex));
      Requires.Range(count <= this.Count, nameof(count));
      Requires.Range(startIndex - count + 1 >= 0, nameof(startIndex));

      using (var enumerator = new ImmutableTrieList<T>.Enumerator(this, startIndex: startIndex, count: count, reversed: true))
      {
        int index = startIndex;
        while (enumerator.MoveNext())
        {
          if (match(enumerator.Current))
          {
            return index;
          }

          index--;
        }
      }

      return -1;
    }
  }
}