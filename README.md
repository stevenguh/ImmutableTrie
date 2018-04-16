# Immutable Trie
Immutable collection that uses trie as their internal data structure, and provide a direct replacement for the .net's implementation.

## ImmutableTrieList
This is an immutable list that's aimed to replace the use of `ImmutableList` fron .net . The speed and memory overhead of using this collection will be faster and less compare to `ImmutableList` and `ImmutableArray`, except `Insert` and `RemoveAt`. These operations will re-index and reallocate the whole trie. If you want performance gain and in general didn't use `Insert` and `RemoveAt`, you can expect a performance gain by replace the existing usage of `ImmutableList` to this.
