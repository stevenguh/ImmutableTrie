using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using Validation;

namespace ImmutableTrie
{
    /// <content>
    /// Contains the inner <see cref="ImmutableTrieDictionary{TKey, TValue}.Builder"/> class.
    /// </content>
    public sealed partial class ImmutableTrieDictionary<TKey, TValue>
    {
        /// <summary>
        /// A dictionary that mutates with little or no memory allocations,
        /// can produce and/or build on immutable dictionary instances very efficiently.
        /// </summary>
        /// <remarks>
        /// <para>
        /// While <see cref="ImmutableTrieDictionary{TKey, TValue}.AddRange(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        /// and other bulk change methods already provide fast bulk change operations on the collection, this class allows
        /// multiple combinations of changes to be made to a set with equal efficiency.
        /// </para>
        /// <para>
        /// Instance members of this class are <em>not</em> thread-safe.
        /// </para>
        /// </remarks>
        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(typeof(ImmutableTrieDictionaryBuilderDebuggerProxy<,>))]
        public sealed class Builder : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDictionary
        {
            /// <summary>
            /// The identifier used to ensure the node can be mutate directly.
            /// </summary>
            private object _owner;

            /// <summary>
            /// The number of elements stored.
            /// </summary>
            private int _count;

            /// <summary>
            /// The root node.
            /// </summary>
            private NodeBase _root;

            /// <summary>
            /// The <see cref="Comparers"/> used for this collection.
            /// </summary>
            private Comparers _comparers;

            /// <summary>
            /// Caches an immutable instance that represents the current state of the collection.
            /// </summary>
            /// <value>Null if no immutable view has been created for the current version.</value>
            private ImmutableTrieDictionary<TKey, TValue> _immutable;

            /// <summary>
            /// A number that increments every time the builder changes its contents.
            /// </summary>
            private int _version;

            /// <summary>
            /// The object callers may use to synchronize access to this collection.
            /// </summary>
            private object _syncRoot;

            /// <summary>
            /// Initializes a new instance of the <see cref="ImmutableTrieDictionary{TKey, TValue}.Builder"/> class.
            /// </summary>
            /// <param name="dict">The dictionary that serves as the basis for this Builder.</param>
            internal Builder(ImmutableTrieDictionary<TKey, TValue> dict)
            {
                _owner = null;
                _root = dict._root;
                _count = dict._count;
                _comparers = dict._comparers;
                _immutable = dict;
                _version = 0;
            }

            /// <summary>
            /// Gets or sets the key comparer.
            /// </summary>
            /// <value>
            /// The key comparer.
            /// </value>
            public IEqualityComparer<TKey> KeyComparer
            {
                get
                {
                    return _comparers.KeyComparer;
                }

                set
                {
                    Requires.NotNull(value, nameof(value));
                    if (value != this.KeyComparer)
                    {
                        // Since we use the KeyComparer to get hashcode, we need to recreate the whole structure
                        // key comparer change.
                        var comparers = Comparers.Get(value, this.ValueComparer);
                        var result = ImmutableTrieDictionary.CreateBuilder(comparers.KeyComparer, comparers.ValueComparer);
                        result.AddRange(this);

                        _version++;
                        _owner = result._owner;
                        _root = result._root;
                        _count = result._count;
                        _comparers = comparers;
                        _immutable = null;  // invalidate cached immutable
                    }
                }
            }

            /// <summary>
            /// Gets or sets the value comparer.
            /// </summary>
            /// <value>
            /// The value comparer.
            /// </value>
            public IEqualityComparer<TValue> ValueComparer
            {
                get
                {
                    return _comparers.ValueComparer;
                }

                set
                {
                    Requires.NotNull(value, nameof(value));
                    if (value != this.ValueComparer)
                    {
                        // When the key comparer is the same but the value comparer is different, we don't need a whole new tree
                        // because the structure of the tree does not depend on the value comparer.
                        // We just need a new root node to store the new value comparer.
                        _comparers = _comparers.WithValueComparer(value);
                        _immutable = null; // invalidate cached immutable
                    }
                }
            }

            #region IDictionary<TKey, TValue> Properties

            /// <summary>
            /// Gets the number of elements contained in the <see cref="ICollection{T}"/>.
            /// </summary>
            /// <returns>The number of elements contained in the <see cref="ICollection{T}"/>.</returns>
            public int Count => _count;

            /// <summary>
            /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
            /// </summary>
            /// <returns>true if the <see cref="ICollection{T}"/> is read-only; otherwise, false.</returns>
            bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

            /// <summary>
            /// See <see cref="IReadOnlyDictionary{TKey, TValue}"/>
            /// </summary>
            public IEnumerable<TKey> Keys
            {
                get
                {
                    foreach (KeyValuePair<TKey, TValue> item in this)
                    {
                        yield return item.Key;
                    }
                }
            }

            /// <summary>
            /// Gets an <see cref="ICollection{T}"/> containing the keys of the <see cref="IDictionary{TKey, TValue}"/>.
            /// </summary>
            /// <returns>An <see cref="ICollection{T}"/> containing the keys of the object that implements <see cref="IDictionary{TKey, TValue}"/>.</returns>
            ICollection<TKey> IDictionary<TKey, TValue>.Keys => this.Keys.ToArray();

            /// <summary>
            /// See <see cref="IReadOnlyDictionary{TKey, TValue}"/>
            /// </summary>
            public IEnumerable<TValue> Values
            {
                get
                {
                    foreach (KeyValuePair<TKey, TValue> item in this)
                    {
                        yield return item.Value;
                    }
                }
            }

            /// <summary>
            /// Gets an <see cref="ICollection{T}"/> containing the values in the <see cref="IDictionary{TKey, TValue}"/>.
            /// </summary>
            /// <returns>An <see cref="ICollection{T}"/> containing the values in the object that implements <see cref="IDictionary{TKey, TValue}"/>.</returns>
            ICollection<TValue> IDictionary<TKey, TValue>.Values => this.Values.ToArray();

            #endregion IDictionary<TKey, TValue> Properties

            #region IDictionary Properties

            /// <summary>
            /// Gets a value indicating whether the <see cref="IDictionary"/> object has a fixed size.
            /// </summary>
            /// <returns>true if the <see cref="IDictionary"/> object has a fixed size; otherwise, false.</returns>
            bool IDictionary.IsFixedSize => false;

            /// <summary>
            /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
            /// </summary>
            /// <returns>true if the <see cref="ICollection{T}"/> is read-only; otherwise, false.
            ///   </returns>
            bool IDictionary.IsReadOnly => false;

            /// <summary>
            /// Gets an <see cref="ICollection{T}"/> containing the keys of the <see cref="IDictionary{TKey, TValue}"/>.
            /// </summary>
            /// <returns>
            /// An <see cref="ICollection{T}"/> containing the keys of the object that implements <see cref="IDictionary{TKey, TValue}"/>.
            /// </returns>
            ICollection IDictionary.Keys => this.Keys.ToArray();

            /// <summary>
            /// Gets an <see cref="ICollection{T}"/> containing the values in the <see cref="IDictionary{TKey, TValue}"/>.
            /// </summary>
            /// <returns>
            /// An <see cref="ICollection{T}"/> containing the values in the object that implements <see cref="IDictionary{TKey, TValue}"/>.
            /// </returns>
            ICollection IDictionary.Values => this.Values.ToArray();

            #endregion IDictionary Properties

            #region ICollection Properties

            /// <summary>
            /// Gets an object that can be used to synchronize access to the <see cref="ICollection"/>.
            /// </summary>
            /// <returns>An object that can be used to synchronize access to the <see cref="ICollection"/>.</returns>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            object ICollection.SyncRoot
            {
                get
                {
                    if (_syncRoot == null)
                    {
                        System.Threading.Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
                    }

                    return _syncRoot;
                }
            }

            /// <summary>
            /// Gets a value indicating whether access to the <see cref="ICollection"/> is synchronized (thread safe).
            /// </summary>
            /// <returns>true if access to the <see cref="ICollection"/> is synchronized (thread safe); otherwise, false.</returns>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection.IsSynchronized => false;

            #endregion ICollection Properties

            #region IDictionary Methods

            /// <summary>
            /// Adds an element with the provided key and value to the <see cref="IDictionary"/> object.
            /// </summary>
            /// <param name="key">The <see cref="object"/> to use as the key of the element to add.</param>
            /// <param name="value">The <see cref="object"/> to use as the value of the element to add.</param>
            void IDictionary.Add(object key, object value) => this.Add((TKey)key, (TValue)value);

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
            void IDictionary.Remove(object key) => this.Remove((TKey)key);

            /// <summary>
            /// Gets or sets the element with the specified key.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <returns></returns>
            object IDictionary.this[object key]
            {
                get => this[(TKey)key];
                set => this[(TKey)key] = (TValue)value;
            }

            #endregion IDictionary Methods

            #region ICollection methods

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

            #endregion ICollection methods

            /// <summary>
            /// Gets the current version of the contents of this builder.
            /// </summary>
            internal int Version => _version;

            /// <summary>
            /// Gets or sets the element with the specified key.
            /// </summary>
            /// <returns>The element with the specified key.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
            /// <exception cref="KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception>
            /// <exception cref="NotSupportedException">The property is set and the <see cref="IDictionary{TKey, TValue}"/> is read-only.</exception>
            public TValue this[TKey key]
            {
                get
                {
                    TValue value;
                    if (this.TryGetValue(key, out value))
                    {
                        return value;
                    }

                    throw new KeyNotFoundException(string.Format(Strings.Arg_KeyNotFoundWithKey, key.ToString()));
                }

                set
                {
                    UpdateItem(key, value, KeyCollisionBehavior.SetIfValueDifferent);
                }
            }

            #region Public Methods

            /// <summary>
            /// Adds a sequence of values to this collection.
            /// </summary>
            /// <param name="items">The items.</param>
            [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
            public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
            {
                Requires.NotNull(items, nameof(items));

                foreach (var item in items)
                {
                    this.Add(item);
                }
            }

            /// <summary>
            /// Removes any entries from the dictionaries with keys that match those found in the specified sequence.
            /// </summary>
            /// <param name="keys">The keys for entries to remove from the dictionary.</param>
            public void RemoveRange(IEnumerable<TKey> keys)
            {
                Requires.NotNull(keys, nameof(keys));

                foreach (var key in keys)
                {
                    this.Remove(key);
                }
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
            /// </returns>
            public Enumerator GetEnumerator() => new Enumerator(_root, this);

            /// <summary>
            /// Gets the value for a given key if a matching key exists in the dictionary.
            /// </summary>
            /// <param name="key">The key to search for.</param>
            /// <returns>The value for the key, or the default value of type <typeparamref name="TValue"/> if no matching key was found.</returns>
            [Pure]
            public TValue GetValueOrDefault(TKey key) => this.GetValueOrDefault(key, default(TValue));

            /// <summary>
            /// Gets the value for a given key if a matching key exists in the dictionary.
            /// </summary>
            /// <param name="key">The key to search for.</param>
            /// <param name="defaultValue">The default value to return if no matching key is found in the dictionary.</param>
            /// <returns>
            /// The value for the key, or <paramref name="defaultValue"/> if no matching key was found.
            /// </returns>
            [Pure]
            public TValue GetValueOrDefault(TKey key, TValue defaultValue)
            {
                Requires.NotNullAllowStructs(key, nameof(key));

                TValue value;
                if (this.TryGetValue(key, out value))
                {
                    return value;
                }

                return defaultValue;
            }

            /// <summary>
            /// Creates an immutable dictionary based on the contents of this instance.
            /// </summary>
            /// <returns>An immutable map.</returns>
            /// <remarks>
            /// This method is an O(n) operation, and approaches O(1) time as the number of
            /// actual mutations to the set since the last call to this method approaches 0.
            /// </remarks>
            public ImmutableTrieDictionary<TKey, TValue> ToImmutable()
            {
                // TODO: To the saved instance if nothing is changed
                if (_owner == null && _immutable != null)
                {
                    return _immutable;
                }

                _owner = null;
                return (_immutable = new ImmutableTrieDictionary<TKey, TValue>(_count, _root, _comparers));
            }

            #endregion Public Methods

            #region IDictionary<TKey, TValue> Members

            /// <summary>
            /// Adds an element with the provided key and value to the <see cref="IDictionary{TKey, TValue}"/>.
            /// </summary>
            /// <param name="key">The object to use as the key of the element to add.</param>
            /// <param name="value">The object to use as the value of the element to add.</param>
            /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
            /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="IDictionary{TKey, TValue}"/>.</exception>
            /// <exception cref="NotSupportedException">The <see cref="IDictionary{TKey, TValue}"/> is read-only.</exception>
            public void Add(TKey key, TValue value)
            {
                UpdateItem(key, value, KeyCollisionBehavior.ThrowIfValueDifferent);
            }

            /// <summary>
            /// Determines whether the <see cref="IDictionary{TKey, TValue}"/> contains an element with the specified key.
            /// </summary>
            /// <param name="key">The key to locate in the <see cref="IDictionary{TKey, TValue}"/>.</param>
            /// <returns>
            /// true if the <see cref="IDictionary{TKey, TValue}"/> contains an element with the key; otherwise, false.
            /// </returns>
            /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
            public bool ContainsKey(TKey key) => Helper.ContainsKey(_root, _comparers, key);

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
            /// Removes the element with the specified key from the <see cref="IDictionary{TKey, TValue}"/>.
            /// </summary>
            /// <param name="key">The key of the element to remove.</param>
            /// <returns>
            /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="IDictionary{TKey, TValue}"/>.
            /// </returns>
            /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
            ///
            /// <exception cref="NotSupportedException">The <see cref="IDictionary{TKey, TValue}"/> is read-only.</exception>
            public bool Remove(TKey key)
            {
                if (_root == null) { return false; }

                object oldOwner = _owner;
                this.EnsureEditable();
                var newRoot = _root.Remove(_owner, 0, _comparers.KeyComparer.GetHashCode(key), key, _comparers, out OperationResult result);
                if (newRoot == _root && result == OperationResult.NoChangeRequired)
                {
                    _owner = oldOwner;
                    return false;
                }

                _root = newRoot;
                if (result == OperationResult.SizeChanged)
                {
                    _count--;
                    _version++;
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Gets the value associated with the specified key.
            /// </summary>
            /// <param name="key">The key whose value to get.</param>
            /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value of the type <typeparamref name="TValue"/>. This parameter is passed uninitialized.</param>
            /// <returns>
            /// true if the object that implements <see cref="IDictionary{TKey, TValue}"/> contains an element with the specified key; otherwise, false.
            /// </returns>
            /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
            public bool TryGetValue(TKey key, out TValue value) => Helper.TryGetValue(_root, _comparers, key, out value);

            /// <summary>
            /// See the <see cref="IImmutableDictionary{TKey, TValue}"/> interface.
            /// </summary>
            public bool TryGetKey(TKey equalKey, out TKey actualKey) => Helper.TryGetKey(_root, _comparers, equalKey, out actualKey);

            /// <summary>
            /// Adds an item to the <see cref="ICollection{T}"/>.
            /// </summary>
            /// <param name="item">The object to add to the <see cref="ICollection{T}"/>.</param>
            /// <exception cref="NotSupportedException">The <see cref="ICollection{T}"/> is read-only.</exception>
            public void Add(KeyValuePair<TKey, TValue> item) => this.Add(item.Key, item.Value);

            /// <summary>
            /// Removes all items from the <see cref="ICollection{T}"/>.
            /// </summary>
            /// <exception cref="NotSupportedException">The <see cref="ICollection{T}"/> is read-only. </exception>
            public void Clear()
            {
                this.EnsureEditable();
                _root = null;
                _count = 0;
                _version++;
            }

            /// <summary>
            /// Determines whether the <see cref="ICollection{T}"/> contains a specific value.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="ICollection{T}"/>.</param>
            /// <returns>
            /// true if <paramref name="item"/> is found in the <see cref="ICollection{T}"/>; otherwise, false.
            /// </returns>
            public bool Contains(KeyValuePair<TKey, TValue> item) => Helper.Contains(_root, _comparers, item);

            /// <summary>
            /// See the <see cref="ICollection{T}"/> interface.
            /// </summary>
            void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                Requires.NotNull(array, nameof(array));

                foreach (var item in this)
                {
                    array[arrayIndex++] = item;
                }
            }

            #endregion IDictionary<TKey, TValue> Members

            #region ICollection<KeyValuePair<TKey, TValue>> Members

            /// <summary>
            /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}"/>.
            /// </summary>
            /// <param name="item">The object to remove from the <see cref="ICollection{T}"/>.</param>
            /// <returns>
            /// true if <paramref name="item"/> was successfully removed from the <see cref="ICollection{T}"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="ICollection{T}"/>.
            /// </returns>
            /// <exception cref="NotSupportedException">The <see cref="ICollection{T}"/> is read-only.</exception>
            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                // Before removing based on the key, check that the key (if it exists) has the value given in the parameter as well.
                if (this.Contains(item))
                {
                    return this.Remove(item.Key);
                }

                return false;
            }

            #endregion ICollection<KeyValuePair<TKey, TValue>> Members

            #region IEnumerator<T> methods

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
            /// </returns>
            IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => this.GetEnumerator();

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

            #endregion IEnumerator<T> methods

            private void UpdateItem(TKey key, TValue value, KeyCollisionBehavior behavior)
            {
                object oldOwner = _owner;
                this.EnsureEditable();
                var newRoot = (_root ?? BitmapIndexedNode.Empty)
                  .Update(_owner, 0, _comparers.KeyComparer.GetHashCode(key), key, value, _comparers, behavior, out OperationResult result);

                switch (result)
                {
                    case OperationResult.AppliedWithoutSizeChange:
                        _version++;
                        _root = newRoot;
                        break;

                    case OperationResult.SizeChanged:
                        _count++;
                        _version++;
                        _root = newRoot;
                        break;

                    case OperationResult.NoChangeRequired:
                        if (newRoot == _root)
                        {
                            // Since there is no change in this operation, the owner in the trie never did updated
                            // we want to set the old owner because, if the oldOwner is null, and no operation is done
                            // in this builder, we want to able to retrun the old immutable instance
                            _owner = oldOwner;
                        }

                        break;
                }
            }

            private void EnsureEditable() => _owner = _owner ?? new object();
        }

        /// <summary>
        /// A simple view of the immutable collection that the debugger can show to the developer.
        /// </summary>
        internal class ImmutableTrieDictionaryBuilderDebuggerProxy<TKey, TValue>
        {
            /// <summary>
            /// The collection to be enumerated.
            /// </summary>
            private readonly ImmutableTrieDictionary<TKey, TValue>.Builder _map;

            /// <summary>
            /// The simple view of the collection.
            /// </summary>
            private KeyValuePair<TKey, TValue>[] _contents;

            /// <summary>
            /// Initializes a new instance of the <see cref="ImmutableDictionaryBuilderDebuggerProxy{TKey, TValue}"/> class.
            /// </summary>
            /// <param name="map">The collection to display in the debugger</param>
            public ImmutableTrieDictionaryBuilderDebuggerProxy(ImmutableTrieDictionary<TKey, TValue>.Builder map)
            {
                Requires.NotNull(map, nameof(map));
                _map = map;
            }

            /// <summary>
            /// Gets a simple debugger-viewable collection.
            /// </summary>
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public KeyValuePair<TKey, TValue>[] Contents
            {
                get
                {
                    if (_contents == null)
                    {
                        _contents = _map.ToArray();
                    }

                    return _contents;
                }
            }
        }
    }
}
