using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Validation;

namespace ImmutableTrie
{
    /// <summary>
    /// An immutable unordered dictionary implementation.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(ImmutableDictionaryDebuggerProxy<,>))]
    public sealed partial class ImmutableTrieDictionary<TKey, TValue>
    {
        /// <summary>
        /// An empty immutable dictionary with default equality comparers.
        /// </summary>
        public static readonly ImmutableTrieDictionary<TKey, TValue> Empty = new ImmutableTrieDictionary<TKey, TValue>(0, null);

        internal const int BITS = 5; // We need 5 bit for 32-way branching tries
        internal const int WIDTH = 1 << BITS; // 2^5 = 32
        internal const int MASK = WIDTH - 1; // 31, or 0x1f
        internal const int MAX_BITMAP_INDEXED_SIZE = WIDTH / 2; // 16
        internal const int MIN_HASH_ARRAY_MAP_SIZE = WIDTH / 4; // 8

        internal static readonly object NotFound = new object();

        /// <summary>
        /// The number of elements stored.
        /// </summary>
        private readonly int _count;

        /// <summary>
        /// The root node.
        /// </summary>
        private readonly NodeBase _root;

        /// <summary>
        /// The <see cref="Comparers"/> used for this collection.
        /// </summary>
        private readonly Comparers _comparers;

        /// <summary>
        /// How to respond when a key collision is discovered.
        /// </summary>
        internal enum KeyCollisionBehavior
        {
            /// <summary>
            /// Sets the value for the given key, even if that overwrites an existing value.
            /// </summary>
            SetValue,

            /// <summary>
            /// Sets the value for the given key, if only the value is different than existing value.
            /// </summary>
            SetIfValueDifferent,

            /// <summary>
            /// Skips the mutating operation if a key conflict is detected.
            /// </summary>
            Skip,

            /// <summary>
            /// Throw an exception if the key already exists with a different value.
            /// If the same value exists, it will behave like the skip option.
            /// </summary>
            ThrowIfValueDifferent,

            /// <summary>
            /// Throw an exception if the key already exists regardless of its value.
            /// </summary>
            ThrowAlways,
        }

        /// <summary>
        /// The result of a mutation operation.
        /// </summary>
        internal enum OperationResult
        {
            /// <summary>
            /// The change was applied and did not require a change to the number of elements in the collection.
            /// </summary>
            AppliedWithoutSizeChange,

            /// <summary>
            /// The change required element(s) to be added or removed from the collection.
            /// </summary>
            SizeChanged,

            /// <summary>
            /// No change was required (the operation ended in a no-op).
            /// </summary>
            NoChangeRequired,
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableTrieDictionary{TKey, TValue}"/> class/
        /// </summary>
        /// <param name="count">The number of elements stored.</param>
        /// <param name="root">The root node.</param>
        /// <param name="compares">The comparers.</param>
        internal ImmutableTrieDictionary(int count, NodeBase root, Comparers compares = null)
        {
            _count = count;
            _root = root;
            _comparers = compares ?? Comparers.Default;
        }

        #region Public Properties

        /// <summary>
        /// See the <see cref="IImmutableDictionary{TKey, TValue}"/> interface.
        /// </summary>
        public ImmutableTrieDictionary<TKey, TValue> Clear()
        {
            return this.IsEmpty ? this : EmptyWithComparers(_comparers);
        }

        /// <summary>
        /// Gets the number of elements in this collection.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty => _count == 0;

        /// <summary>
        /// Gets the key comparer.
        /// </summary>
        public IEqualityComparer<TKey> KeyComparer => _comparers.KeyComparer;

        /// <summary>
        /// Gets the value comparer used to determine whether values are equal.
        /// </summary>
        public IEqualityComparer<TValue> ValueComparer => _comparers.ValueComparer;

        /// <summary>
        /// Gets the keys in the map.
        /// </summary>
        public IEnumerable<TKey> Keys
        {
            get
            {
                foreach (var item in this)
                {
                    yield return item.Key;
                }
            }
        }

        /// <summary>
        /// Gets the values in the map.
        /// </summary>
        public IEnumerable<TValue> Values
        {
            get
            {
                foreach (var item in this)
                {
                    yield return item.Value;
                }
            }
        }

        #endregion Public Properties

        /// <summary>
        /// Gets the <typeparamref name="TValue"/> with the specified key.
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                Requires.NotNullAllowStructs(key, nameof(key));

                TValue value;
                if (this.TryGetValue(key, out value))
                {
                    return value;
                }

                throw new KeyNotFoundException();
            }
        }

        /// <summary>
        /// Creates a collection with the same contents as this collection that
        /// can be efficiently mutated across multiple operations using standard
        /// mutable interfaces.
        /// </summary>
        /// <remarks>
        /// This is an O(1) operation and results in only a single (small) memory allocation.
        /// The mutable collection that is returned is *not* thread-safe.
        /// </remarks>
        [Pure]
        public Builder ToBuilder()
        {
            // We must not cache the instance created here and return it to various callers.
            // Those who request a mutable collection must get references to the collection
            // that version independently of each other.
            return new Builder(this);
        }

        /// <summary>
        /// Creates a list with a builder action to efficiently mutate across multiple operations using standard mutable interfaces.
        /// </summary>
        /// <param name="action">A <see cref="Action{Builder}"/> used to mutate the builder instance.</param>
        /// <returns>A new list with the action applied to the builder.</returns>
        /// <remarks>
        /// This is a helper method to convert this instance to builder, mutate the builder list, and convert it back to immutable list.
        /// </remarks>
        [Pure]
        public ImmutableTrieDictionary<TKey, TValue> Build(Action<Builder> action)
        {
            Builder b = ToBuilder();
            action(b);
            return b.ToImmutable();
        }

        /// <summary>
        /// See the <see cref="IImmutableDictionary{TKey, TValue}"/> interface.
        /// </summary>
        [Pure]
        public ImmutableTrieDictionary<TKey, TValue> Add(TKey key, TValue value)
        {
            Requires.NotNullAllowStructs(key, nameof(key));
            Contract.Ensures(Contract.Result<ImmutableTrieDictionary<TKey, TValue>>() != null);

            return UpdateItem(key, value, KeyCollisionBehavior.ThrowIfValueDifferent);
        }

        /// <summary>
        /// See the <see cref="IImmutableDictionary{TKey, TValue}"/> interface.
        /// </summary>
        [Pure]
        public ImmutableTrieDictionary<TKey, TValue> SetItem(TKey key, TValue value)
        {
            Requires.NotNullAllowStructs(key, nameof(key));
            Contract.Ensures(Contract.Result<ImmutableTrieDictionary<TKey, TValue>>() != null);
            Contract.Ensures(!Contract.Result<ImmutableTrieDictionary<TKey, TValue>>().IsEmpty);

            return UpdateItem(key, value, KeyCollisionBehavior.SetIfValueDifferent);
        }

        /// <summary>
        /// See the <see cref="IImmutableDictionary{TKey, TValue}"/> interface.
        /// </summary>
        [Pure]
        public ImmutableTrieDictionary<TKey, TValue> Remove(TKey key)
        {
            Requires.NotNullAllowStructs(key, nameof(key));
            Contract.Ensures(Contract.Result<ImmutableTrieDictionary<TKey, TValue>>() != null);

            if (_root == null) { return this; } // Empty
            NodeBase newRoot = _root.Remove(null, 0, _comparers.KeyComparer.GetHashCode(key), key, _comparers, out OperationResult result);
            if (newRoot == _root) { return this; } // No update was made, key not found.
            return new ImmutableTrieDictionary<TKey, TValue>(_count - 1, newRoot, _comparers);
        }

        /// <summary>
        /// Gets an empty collection with the specified comparers.
        /// </summary>
        /// <param name="comparers">The comparers.</param>
        /// <returns>The empty dictionary.</returns>
        [Pure]
        private static ImmutableTrieDictionary<TKey, TValue> EmptyWithComparers(Comparers comparers)
        {
            Requires.NotNull(comparers, nameof(comparers));

            return Empty._comparers == comparers
                ? Empty
                : new ImmutableTrieDictionary<TKey, TValue>(0, null, comparers);
        }

        private ImmutableTrieDictionary<TKey, TValue> UpdateItem(TKey key, TValue value, KeyCollisionBehavior behavior)
        {
            NodeBase newRoot = (_root ?? BitmapIndexedNode.Empty)
              .Update(null, 0, _comparers.KeyComparer.GetHashCode(key), key, value, _comparers, behavior, out OperationResult result);
            if (newRoot == _root) { return this; }
            return new ImmutableTrieDictionary<TKey, TValue>(result == OperationResult.SizeChanged ? _count + 1 : _count, newRoot, _comparers);
        }
    }
}
