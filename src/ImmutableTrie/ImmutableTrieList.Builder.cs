using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Validation;

namespace ImmutableTrie
{
    /// <content>
    /// Contains the inner Builder class.
    /// </content>
    public sealed partial class ImmutableTrieList<T>
    {
        /// <summary>
        /// A list that mutates with little or no memory allocations,
        /// can produce and/or build on immutable list instances very efficiently.
        /// </summary>
        /// <remarks>
        /// <para>
        /// While <see cref="ImmutableTrieList{T}.AddRange"/> and other bulk change methods
        /// already provide fast bulk change operations on the collection, this class allows
        /// multiple combinations of changes to be made to a set with equal efficiency.
        /// </para>
        /// <para>
        /// Instance members of this class are <em>not</em> thread-safe.
        /// </para>
        /// </remarks>
        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(typeof(ImmutableTrieListBuilderDebuggerProxy<>))]
        public sealed partial class Builder : ICollection<T>, IEnumerable<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IEnumerable, IList
        {
            /// <summary>
            /// Gets a value indicating whether this instance is read-only.
            /// </summary>
            /// <value>Always <c>false</c>.</value>
            bool ICollection<T>.IsReadOnly => false;

            /// <summary>
            /// Gets a value indicating whether access to the <see cref="ICollection"/> is synchronized (thread safe).
            /// </summary>
            /// <returns>true if access to the <see cref="ICollection"/> is synchronized (thread safe); otherwise, false.</returns>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection.IsSynchronized => false;

            /// <summary>
            /// Gets an object that can be used to synchronize access to the <see cref="ICollection"/>.
            /// </summary>
            /// <returns>An object that can be used to synchronize access to the <see cref="ICollection"/>.</returns>
            object ICollection.SyncRoot
            {
                get
                {
                    if (_syncRoot == null)
                    {
                        System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
                    }

                    return _syncRoot;
                }
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
            /// <remarks>This operation will slightly reduce any subsequent mutation on this builder.</remarks>
            public override ImmutableTrieList<T> GetRange(int index, int count) => this.ToImmutable().GetRange(index, count);

            /// <summary>
            /// See <see cref="IList{T}"/>
            /// </summary>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <paramref name="index"/> is less than 0.-or-<paramref name="index"/> is greater than <see cref="this.Count"/>.
            /// </exception>
            public void Insert(int index, T item) => this.InsertRange(index, new T[] { item });

            /// <summary>
            /// Inserts the elements of a collection into the <see cref="ImmutableTrieList{T}"/>
            /// at the specified index.
            /// </summary>
            /// <param name="index">
            /// The zero-based index at which the new elements should be inserted.
            /// </param>
            /// <param name="items">
            /// The collection whose elements should be inserted into the <see cref="ImmutableTrieList{T}"/>.
            /// The collection itself cannot be null, but it can contain elements that are
            /// null, if type T is a reference type.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="items"/> is null.
            /// </exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <paramref name="index"/> is less than 0.-or-<paramref name="index"/> is greater than <see cref="this.Count"/>.
            /// </exception>
            public void InsertRange(int index, IEnumerable<T> items)
            {
                Requires.NotNull(items, nameof(items));
                Requires.Range(index >= 0 && index <= this.Count, nameof(index));
                Action<int, T> addOrSet = (i, item) =>
                {
                    if (i >= this.Count)
                    {
                        this.Add(item);
                    }
                    else
                    {
                        this[i] = item;
                    }
                };

                Queue<T> q = new Queue<T>();
                foreach (T item in items)
                {
                    if (index < this.Count)
                    { // Only enqueue when we can get the content
                        q.Enqueue(this[index]);
                    }

                    addOrSet(index++, item);
                }

                while (q.Count != 0)
                {
                    if (index < this.Count)
                    { // Only enqueue when we can get the content
                        q.Enqueue(this[index]);
                    }

                    addOrSet(index++, q.Dequeue());
                }
            }

#if FEATURE_ITEMREFAPI
            public ref readonly T ItemRef(int index) { throw null; }
#endif

            /// <summary>
            /// See <see cref="IList{T}"/>
            /// </summary>
            public bool Remove(T item) => this.Remove(item, EqualityComparer<T>.Default);

            /// <summary>
            /// Removes the first occurrence of the object that matches the specified value from this list.
            /// </summary>
            /// <param name="item">The value of the element to remove from the list.</param>
            /// <param name="equalityComparer">The equality comparer to use in the search.</param>
            /// <returns>true if the item is found and removed; otherwise, false.</returns>
            public bool Remove(T item, IEqualityComparer<T> equalityComparer)
            {
                int index = this.IndexOf(item, equalityComparer);
                if (index < 0)
                {
                    return false;
                }

                this.RemoveAt(index);
                return true;
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
            /// The number of elements removed from the <see cref="ImmutableTrieList{T}"/>.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="match"/> is null.
            /// </exception>
            public int RemoveAll(Predicate<T> match)
            {
                Requires.NotNull(match, nameof(match));
                int removed = 0;
                Enumerator enumerator = new Enumerator(this);
                try
                {
                    int startIndex = 0;
                    while (enumerator.MoveNext())
                    {
                        if (match(enumerator.Current))
                        {
                            this.RemoveAt(startIndex);
                            removed++;
                            enumerator.Dispose();
                            enumerator = new Enumerator(this, startIndex: startIndex);
                        }
                        else
                        {
                            startIndex++;
                        }
                    }
                }
                finally
                {
                    enumerator.Dispose();
                }

                return removed;
            }

            /// <summary>
            /// See <see cref="IList{T}"/>
            /// </summary>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <paramref name="index"/> is less than 0.-or-<paramref name="index"/> is equal to or greater than <see cref="Count"/>.
            /// </exception>
            public void RemoveAt(int index)
            {
                Requires.Range(index >= 0 && index < this.Count, nameof(index));
                for (int i = index + 1; i < this.Count; i++)
                {
                    this[i - 1] = this[i];
                }

                this.Pop();
            }

            /// <summary>
            /// Reverses the order of the elements in the entire <see cref="ImmutableTrieList{T}"/>.
            /// </summary>
            public void Reverse() => this.Reverse(0, this.Count);

            /// <summary>
            /// Reverses the order of the elements in the specified range.
            /// </summary>
            /// <param name="index">The zero-based starting index of the range to reverse.</param>
            /// <param name="count">The number of elements in the range to reverse.</param>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <paramref name="index"/> is less than 0.-or-<paramref name="count"/> is less than 0-or-the sum of <paramref name="index"/> and <paramref name="count"/> is greater than <see cref="Count"/>.
            /// </exception>
            public void Reverse(int index, int count)
            {
                Requires.Range(index >= 0, nameof(index));
                Requires.Range(count >= 0, nameof(count));
                Requires.Range(index + count <= this.Count, nameof(count));

                int startIndex = index;
                int endIndex = index + count - 1;
                while (startIndex < endIndex)
                {
                    T temp = this[startIndex];
                    this[startIndex] = this[endIndex];
                    this[endIndex] = temp;
                    startIndex++;
                    endIndex--;
                }
            }

            /// <summary>
            /// Sorts the elements in the entire <see cref="ImmutableTrieList{T}"/> using
            /// the default comparer.
            /// </summary>
            public void Sort() => this.Sort(Comparer<T>.Default);

            /// <summary>
            /// Sorts the elements in the entire <see cref="ImmutableTrieList{T}"/> using
            /// the specified comparer.
            /// </summary>
            /// <param name="comparer">
            /// The <see cref="IComparer{T}"/> implementation to use when comparing
            /// elements, or null to use the default comparer <see cref="Comparer{T}.Default"/>.
            /// </param>
            public void Sort(IComparer<T> comparer) => this.Sort(0, this.Count, comparer);

            /// <summary>
            /// Sorts the elements in the entire <see cref="ImmutableList{T}"/> using
            /// the specified <see cref="Comparison{T}"/>.
            /// </summary>
            /// <param name="comparison">
            /// The <see cref="Comparison{T}"/> to use when comparing elements.
            /// </param>
            public void Sort(Comparison<T> comparison) => this.Sort(0, this.Count, Comparer<T>.Create(comparison));

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
            /// elements, or null to use <see cref="Comparer{T}.Default"/>.
            /// </param>
            public void Sort(int index, int count, IComparer<T> comparer)
            {
                Requires.Range(index >= 0, nameof(index));
                Requires.Range(count >= 0, nameof(count));
                Requires.Argument(index + count <= this.Count, nameof(count), "{0} + {1} must be less than or equal to {2}", nameof(index), nameof(count), nameof(this.Count));

                comparer = comparer ?? Comparer<T>.Default;
                var helper = ListSortHelper<T>.Default;
                helper.Sort(this, index, count, comparer);
            }

            /// <summary>
            /// See the <see cref="ICollection"/> interface.
            /// </summary>
            void ICollection.CopyTo(Array array, int arrayIndex) => this.CopyTo(array, arrayIndex);

            #region IList members

            /// <summary>
            /// Gets a value indicating whether the <see cref="IList"/> has a fixed size.
            /// </summary>
            /// <returns>true if the <see cref="IList"/> has a fixed size; otherwise, false.</returns>
            bool IList.IsFixedSize => false;

            /// <summary>
            /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
            /// </summary>
            /// <returns>true if the <see cref="ICollection{T}"/> is read-only; otherwise, false.
            ///   </returns>
            bool IList.IsReadOnly => false;

            /// <summary>
            /// Gets or sets the <see cref="System.Object"/> at the specified index.
            /// </summary>
            /// <value>
            /// The <see cref="System.Object"/>.
            /// </value>
            /// <param name="index">The index.</param>
            /// <returns></returns>
            object IList.this[int index]
            {
                get => this[index];
                set => this[index] = (T)value;
            }

            /// <summary>
            /// Adds an item to the <see cref="IList"/>.
            /// </summary>
            /// <param name="value">The object to add to the <see cref="IList"/>.</param>
            /// <returns>
            /// The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection,
            /// </returns>
            int IList.Add(object value)
            {
                this.Add((T)value);
                return this.Count - 1;
            }

            /// <summary>
            /// Clears this instance.
            /// </summary>
            void IList.Clear() => this.Clear();

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
            /// Inserts an item to the <see cref="IList"/> at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index at which <paramref name="value"/> should be inserted.</param>
            /// <param name="value">The object to insert into the <see cref="IList"/>.</param>
            void IList.Insert(int index, object value)
            {
                this.Insert(index, (T)value);
            }

            /// <summary>
            /// Removes the first occurrence of a specific object from the <see cref="IList"/>.
            /// </summary>
            /// <param name="value">The object to remove from the <see cref="IList"/>.</param>
            void IList.Remove(object value)
            {
                if (IsCompatibleObject(value))
                {
                    this.Remove((T)value);
                }
            }

            #endregion IList members
        }

        /// <summary>
        /// A simple view of the immutable list that the debugger can show to the developer.
        /// </summary>
        internal class ImmutableTrieListBuilderDebuggerProxy<T>
        {
            /// <summary>
            /// The collection to be enumerated.
            /// </summary>
            private readonly ImmutableTrieList<T>.Builder _list;

            /// <summary>
            /// The simple view of the collection.
            /// </summary>
            private T[] _cachedContents;

            /// <summary>
            /// Initializes a new instance of the <see cref="ImmutableTrieListBuilderDebuggerProxy{T}"/> class.
            /// </summary>
            /// <param name="builder">The list to display in the debugger</param>
            public ImmutableTrieListBuilderDebuggerProxy(ImmutableTrieList<T>.Builder builder)
            {
                Requires.NotNull(builder, nameof(builder));
                _list = builder;
            }

            /// <summary>
            /// Gets a simple debugger-viewable list.
            /// </summary>
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public T[] Contents
            {
                get
                {
                    if (_cachedContents == null)
                    {
                        //_cachedContents = _list.ToArray(_list.Count);
                        _cachedContents = _list.ToArray();
                    }

                    return _cachedContents;
                }
            }
        }
    }
}
