// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Validation;

namespace ImmutableTrie.Tests
{
    internal static class TestExtensionsMethods
    {
        private static readonly double s_GoldenRatio = (1 + Math.Sqrt(5)) / 2;

        internal static IDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(this IImmutableDictionary<TKey, TValue> dictionary)
        {
            Requires.NotNull(dictionary, nameof(dictionary));

            return (IDictionary<TKey, TValue>)dictionary;
        }

        internal static IDictionary<TKey, TValue> ToBuilder<TKey, TValue>(this IImmutableDictionary<TKey, TValue> dictionary)
        {
            Requires.NotNull(dictionary, nameof(dictionary));

            var hashDictionary = dictionary as ImmutableDictionary<TKey, TValue>;
            if (hashDictionary != null)
            {
                return hashDictionary.ToBuilder();
            }

            var sortedDictionary = dictionary as ImmutableSortedDictionary<TKey, TValue>;
            if (sortedDictionary != null)
            {
                return sortedDictionary.ToBuilder();
            }

            var trieDictionary = dictionary as ImmutableTrieDictionary<TKey, TValue>;

            throw new NotSupportedException();
        }
    }
}
