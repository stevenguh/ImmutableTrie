using System;
using System.Collections;
using System.Collections.Generic;
using Validation;

namespace ImmutableTrie
{
  /// <content>
  /// Contains the core logic of the inner Builder class.
  /// </content>
  public sealed partial class ImmutableTrieList<T>
  {
    public sealed partial class Builder : ImmutableTrieListBase<T>
    {
      /// <summary>
      /// The object callers may use to synchronize access to this collection.
      /// </summary>
      private object _syncRoot;

      /// <summary>
      /// This original trie list for optimization.
      /// This is reset to the new list once ToImmutable is called;
      /// however, version of this builder will not be reset.
      /// </summary>
      private ImmutableTrieList<T> _originalList;

      internal Builder(ImmutableTrieList<T> immutable)
        : base(immutable.Origin, immutable.Capacity, immutable.Count, immutable.Shift, immutable.Root, immutable.Tail)
      {
        Version = 0;
        _originalList = immutable;
      }

      /// <summary>
      /// Gets or sets the value for a given index into the list.
      /// </summary>
      /// <param name="index">The index of the desired element.</param>
      /// <returns>The value at the specified index.</returns>
      public T this[int index]
      {
        get => GetItem(index);
        set => SetItem(index, value);
      }

      // TODO: Check all version referene, and make sure they are used correctly
      /// <summary>
      /// A number that increments every time the builder changes its contents.
      /// </summary>
      internal int Version { get; private set; }

      private static Node GetEditableRoot(Node node)
      {
        return new Node(new object(), (object[])node.Array.Clone());
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
      public override int BinarySearch(int index, int count, T item, IComparer<T> comparer)
      {
        Requires.Range(index >= 0, nameof(index));
        Requires.Range(count >= 0, nameof(count));
        Requires.Argument(index + count <= Count, nameof(count), "{0} + {1} must be less than or equal to {2}", nameof(index), nameof(count), nameof(Count));

        comparer = comparer ?? Comparer<T>.Default;
        return ListSortHelper<T>.Default.BinarySearch(this, index, count, item, comparer);
      }

      /// <summary>
      /// Creates an immutable list based on the contents of this instance.
      /// </summary>
      /// <returns>An immutable list.</returns>
      /// <remarks>
      /// This method is an O(1) operation.
      /// </remarks>
      public ImmutableTrieList<T> ToImmutable()
      {
        if (Owner == null)
        {
          // Nothing changes, return the original list
          return _originalList;
        }

        Owner = null;
        return _originalList = new ImmutableTrieList<T>(Origin, Capacity, Count, Shift, Root, Tail);
      }

      /// <summary>
      /// Removes all items from the immutable list.
      /// </summary>
      public void Clear()
      {
        Count = 0;
        Capacity = 0;
        Origin = 0;
        Root = null;
        Tail = Node.CreateNew(this.Owner);
        Shift = BITS;
      }

      /// <summary>
      /// Adds an item to the immutable list.
      /// </summary>
      /// <param name="item">The item to add to the list.</param>
      public void Add(T item)
      {
        EnsureEditable();
        Node newRoot;
        int newShift = Shift;

        if (Capacity - TailOffset < WIDTH)
        {
          // Add to tail if there's still room
          Tail = Tail.EnsureEditable(Owner);
          Tail[Capacity & MASK] = item;
          Count++;
          Capacity++;
          Version++;
          return;
        }

        // Root overflow
        // 1 << (Shift + BITS): max tree capacity
        // + WIDTH: add the tail capacity
        if (Capacity == (1 << (Shift + BITS)) + WIDTH)
        {
          newRoot = CreateOverflowPath();
          newShift += BITS;
        }
        else
        {
          newRoot = AppendInNode(Root, Shift, ensureEditable: true);
        }

        Tail = Node.CreateNew(Owner);
        Tail[0] = item;

        Root = newRoot;
        Shift = newShift;
        Count++;
        Capacity++;
        Version++;
      }

      /// <summary>
      /// Adds the elements of a sequence to the end of this collection.
      /// </summary>
      /// <param name="items">
      /// The sequence whose elements should be appended to this collection.
      /// The sequence itself cannot be null, but it can contain elements that are
      /// null, if type <typeparamref name="T"/> is a reference type.
      /// </param>
      /// <exception cref="ArgumentNullException">
      /// <paramref name="items"/> is null.
      /// </exception>
      public void AddRange(IEnumerable<T> items)
      {
        Requires.NotNull(items, nameof(items));
        foreach (T item in items)
        {
          Add(item);
        }
      }

      public void Pop()
      {
        Requires.ValidState(Count > 0, "The list is empty");
        EnsureEditable();

        if (Count == 1)
        {
          Clear();
          return;
        }

        if (Capacity - 1 > TailOffset)
        {
          // Pop in the tail
          Count--;
          Capacity--;
          Version++;
          return;
        }

        // There is only one element in the tail
        // so the index is the last element in the leave node
        Node newTail;
        Node newRoot = PopInNode(Capacity - 2, Root, Shift, out newTail, ensureEditable: true);
        int newShift = Shift;
        if (Shift > BITS && newRoot.Array[1] == null)
        {
          // PopInNode may result in extra root, so need to remove addition root
          newRoot = (Node)newRoot[0];
          newShift -= BITS;
        }

        Root = newRoot;
        Shift = newShift;
        Tail = newTail;
        Count--;
        Capacity--;
        Version++;
      }
      private void SetItem(int index, T value)
      {
        CheckIndex(index);
        EnsureEditable();

        index += Origin;
        if (index >= TailOffset)
        {
          Tail = Tail.EnsureEditable(Owner);
          Tail[index & MASK] = value;
        }
        else
        {
          Root = SetItemInNode(Root, index, Shift, value, ensureEditable: true);
        }

        Version++;
      }

      private void EnsureEditable() => Owner = Owner ?? new object();
    }
  }
}