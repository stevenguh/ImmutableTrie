# Immutable Trie ([nuget](https://www.nuget.org/packages/ImmutableTrie/))
Immutable collections that use trie as their internal data structure, and provide a direct replacement for the .net's implementation of `ImmutableList` and `ImmutableDictionary`.

## ImmutableTrieDictionary
This is an immutable dictionary that's aimed to replaced the use of `ImmutableDictionary`. This collection uses comparable or less memory and provides as 2x speed up in most operation in compare to .NET's `ImmutableDictionary`. This collection is based on [hash array mapped trie](https://en.wikipedia.org/wiki/Hash_array_mapped_trie).

A [proposal](https://github.com/dotnet/corefx/issues/29346) was made in [corefx](https://github.com/dotnet/corefx)'s repo to use this data structure as the internal structure of `ImmutableDictionary`.

Methods | Complexity
--- | ---
Get | O(log32 N)
Set | O(log32 N)
Remove | O(log32 N)

## ImmutableTrieList
This is an immutable list that's aimed to replace the use of `ImmutableList`. In general, this collection uses less memory and provides higher speed on most operations, except `Insert` and `RemoveAt`. These operations will re-index and reallocate the whole trie. If you want performance gain and in general didn't use `Insert` and `RemoveAt`, you can expect a performance gain by replace the existing usage of `ImmutableList` to this `ImmutableTireList`. This collection is based on [vector trie](https://hypirion.com/musings/understanding-persistent-vector-pt-1).

A [proposal](https://github.com/dotnet/corefx/issues/28848) was made in [corefx](https://github.com/dotnet/corefx)'s repo to use this data structure as the internal structure of `ImmutableList`. However, due to the expensive `InsertAt` and `RemoveAt` operation, which will change the performance of the already shipped `ImmutableList`, the proposal was dismissed.

Methods | Complexity
--- | ---
GetRange | O(log32 N)
Get | O(log32 N)
Add | O(1)
Pop | O(1)
InsertAt | O(N log32 N)
RemoveAt | O(N log32 N)
