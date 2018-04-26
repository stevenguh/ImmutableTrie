# Immutable Trie
Immutable collections that use trie as their internal data structure, and provide a direct replacement for the .net's implementation of `ImmutableList` and `ImmutableDictionary`.

## ImmutableTrieList
This is an immutable list that's aimed to replace the use of `ImmutableList`. In general, this collection uses less memory and provides higher speed on most operations, except `Insert` and `RemoveAt`. These operations will re-index and reallocate the whole trie. If you want performance gain and in general didn't use `Insert` and `RemoveAt`, you can expect a performance gain by replace the existing usage of `ImmutableList` to this `ImmutableTireList`. This collection is based on [vector trie](https://hypirion.com/musings/understanding-persistent-vector-pt-1).

Methods | Complexity
--- | ---
GetRange | O(log32 N)
Get | O(log32 N)
Add | O(1)
Pop | O(1)
InsertAt | O(N log32 N)
RemoveAt | O(N log32 N)


## ImmutableTrieDict
This is an immutable dictionary that's aimed to replaced the use of `ImmutableDictionary`. This collection uses comparable or less memory and provides as 2x speed up in most operation in compare to .NET's `ImmutableDictionary`.This collection is based on [hash array mapped trie](https://en.wikipedia.org/wiki/Hash_array_mapped_trie).

Methods | Complexity
--- | ---
Get | O(log32 N)
Set | O(log32 N)