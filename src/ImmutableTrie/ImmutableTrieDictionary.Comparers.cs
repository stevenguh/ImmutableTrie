using System.Collections.Generic;
using Validation;

namespace ImmutableTrie
{
    /// <content>
    /// Contains the inner Builder class.
    /// </content>
    public sealed partial class ImmutableTrieDictionary<TKey, TValue>
    {
        internal sealed class Comparers
        {
            /// <summary>
            /// The default instance to use when all the comparers used are their default values.
            /// </summary>
            internal static readonly Comparers Default = new Comparers(EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);

            /// <summary>
            /// The equality comparer to use for the key.
            /// </summary>
            private readonly IEqualityComparer<TKey> _keyComparer;

            /// <summary>
            /// The value comparer.
            /// </summary>
            private readonly IEqualityComparer<TValue> _valueComparer;

            /// <summary>
            /// Initializes a new instance of the <see cref="Comparers"/> class.
            /// </summary>
            /// <param name="keyComparer">The key only comparer.</param>
            /// <param name="valueComparer">The value comparer.</param>
            private Comparers(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
            {
                Requires.NotNull(keyComparer, nameof(keyComparer));
                Requires.NotNull(valueComparer, nameof(valueComparer));

                _keyComparer = keyComparer;
                _valueComparer = valueComparer;
            }

            /// <summary>
            /// Gets the key comparer.
            /// </summary>
            /// <value>
            /// The key comparer.
            /// </value>
            internal IEqualityComparer<TKey> KeyComparer => _keyComparer;

            /// <summary>
            /// Gets the value comparer.
            /// </summary>
            /// <value>
            /// The value comparer.
            /// </value>
            internal IEqualityComparer<TValue> ValueComparer => _valueComparer;

            /// <summary>
            /// Gets an instance that refers to the specified combination of comparers.
            /// </summary>
            /// <param name="keyComparer">The key comparer.</param>
            /// <param name="valueComparer">The value comparer.</param>
            /// <returns>An instance of <see cref="Comparers"/></returns>
            internal static Comparers Get(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
            {
                Requires.NotNull(keyComparer, nameof(keyComparer));
                Requires.NotNull(valueComparer, nameof(valueComparer));

                return keyComparer == Default.KeyComparer && valueComparer == Default.ValueComparer
                    ? Default
                    : new Comparers(keyComparer, valueComparer);
            }

            /// <summary>
            /// Returns an instance of <see cref="Comparers"/> that shares the same key comparers
            /// with this instance, but uses the specified value comparer.
            /// </summary>
            /// <param name="valueComparer">The new value comparer to use.</param>
            /// <returns>A new instance of <see cref="Comparers"/></returns>
            internal Comparers WithValueComparer(IEqualityComparer<TValue> valueComparer)
            {
                Requires.NotNull(valueComparer, nameof(valueComparer));

                return _valueComparer == valueComparer
                    ? this
                    : Get(this.KeyComparer, valueComparer);
            }
        }
    }
}
