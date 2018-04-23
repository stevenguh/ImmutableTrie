using System.Collections.Generic;
using Validation;

namespace ImmutableTrie
{
  public sealed partial class ImmutableTrieDictionary<TKey, TValue>
  {
    internal static class Helper
    {
      internal static bool TryGetValue(NodeBase root, Comparers comparers, TKey key, out TValue value)
      {
        Requires.NotNullAllowStructs(key, nameof(key));
        if (root == null)
        {
          value = default(TValue);
          return false;
        }

        object result = root.Get(0, comparers.KeyComparer.GetHashCode(key), comparers, key, out TKey actualKey);
        if (result == NotFound)
        {
          value = default(TValue);
          return false;
        }

        value = (TValue)result;
        return true;
      }

      internal static bool TryGetKey(NodeBase root, Comparers comparers, TKey equalKey, out TKey actualKey)
      {
        Requires.NotNullAllowStructs(equalKey, nameof(equalKey));
        if (root == null)
        {
          actualKey = equalKey;
          return false;
        }

        object result = root.Get(0, comparers.KeyComparer.GetHashCode(equalKey), comparers, equalKey, out actualKey);
        if (result == NotFound)
        {
          actualKey = equalKey;
          return false;
        }

        return true;
      }

      internal static bool ContainsKey(NodeBase root, Comparers comparers, TKey key)
      {
        Requires.NotNullAllowStructs(key, nameof(key));
        return TryGetKey(root, comparers, key, out TKey actualKey);
      }

      internal static bool Contains(NodeBase root, Comparers comparers, KeyValuePair<TKey, TValue> pair)
      {
        if (TryGetValue(root, comparers, pair.Key, out TValue storedValue))
        {
          return comparers.ValueComparer.Equals(pair.Value, storedValue);
        }

        return false;
      }

      internal static bool ContainsValue(IEnumerable<KeyValuePair<TKey, TValue>> enumerable, Comparers comparers, TValue value)
      {
        foreach (KeyValuePair<TKey, TValue> item in enumerable)
        {
          if (comparers.ValueComparer.Equals(value, item.Value))
          {
            return true;
          }
        }
        return false;
      }
    }
  }
}