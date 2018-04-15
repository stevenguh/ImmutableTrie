using System;
using System.Collections.Generic;
using Validation;

namespace ImmutableTrie
{
  public sealed partial class ImmutableTrieList<T> : ImmutableTrieListBase<T>
  {

    /// <summary>
    /// Gets an empty immutable list.
    /// </summary>
    public static readonly ImmutableTrieList<T> Empty = new ImmutableTrieList<T>(0, 0, 0, BITS, null, Node.Empty);

    internal ImmutableTrieList(int origin, int capacity, int count, int shift, Node root, Node tail)
      : base(origin, capacity, count, shift, root, tail)
    { }

    /// <summary>
    /// Gets the element at the specified index of the list.
    /// </summary>
    /// <param name="index">The index of the element to retrieve.</param>
    /// <returns>The element at the specified index.</returns>
    public T this[int index] => GetItem(index);

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
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0.-or-<paramref name="count"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="index"/> and <paramref name="count"/> do not denote a valid range in the <see cref="ImmutableTrieList{T}"/>.
    /// </exception>
    public override ImmutableTrieList<T> GetRange(int index, int count)
    {
      Requires.Range(index >= 0, nameof(index));
      Requires.Range(count >= 0, nameof(count));
      Requires.Argument(index + count <= Count, nameof(count), "{0} + {1} must be less than or equal to {2}", nameof(index), nameof(count), nameof(Count));

      index += Origin;
      int endIndex = index + count - 1;
      int origin = index & MASK;
      // Both start and end is in the tail
      if (index >= TailOffset && endIndex >= TailOffset)
      {
        return new ImmutableTrieList<T>(origin, count + origin, count, BITS, null, this.Tail);
      }

      Node newRoot = Root;
      Node newTail = Tail;
      int newShift = Shift;

      // Find new root node
      int endIndexInTrie = endIndex >= TailOffset ? TailOffset - 1 : endIndex;
      int subStart = (index >> newShift) & MASK;
      int subEnd = (endIndexInTrie >> newShift) & MASK;
      if (subStart == subEnd)
      {
        while (newShift > 0)
        {
          Node tempNode = (Node)newRoot.Array[subStart];
          int tempShift = newShift - BITS;

          subStart = (index >> tempShift) & MASK;
          subEnd = (endIndexInTrie >> tempShift) & MASK;
          if (subStart != subEnd)
          {
            break;
          }

          newRoot = tempNode;
          newShift = tempShift;
        }
      }

        // Move the node to the tail if the end if not currently in tail
        if (endIndex < TailOffset)
        {
          newRoot = PopInNode(endIndex, newRoot, newShift, out newTail);
          if (newShift > BITS && newRoot.Array[1] == null)
          {
            // PopInNode may result in extra root, so need to remove addition root
            newRoot = (Node)newRoot[0];
            newShift -= BITS;
          }
        }

      return new ImmutableTrieList<T>(origin, count + origin, count, newShift, newRoot, newTail);
    }

    /// <summary>
    /// Adds the specified object to the end of the immutable list.
    /// </summary>
    /// <param name="value">The object to add.</param>
    /// <returns>A new immutable list with the object added</returns>
    public ImmutableTrieList<T> Add(T value)
    {
      Node newRoot;
      Node newTail;
      int newShift = Shift;

      if (Capacity - TailOffset < WIDTH)
      {
        // Add to tail if there's still room
        newTail = Tail.Clone();
        newTail[Capacity & MASK] = value;
        return new ImmutableTrieList<T>(Origin, Capacity + 1, Count + 1, newShift, Root, newTail);
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
        newRoot = AppendInNode(Root, Shift);
      }

      newTail = Node.CreateNew();
      newTail[0] = value;
      return new ImmutableTrieList<T>(Origin, Capacity + 1, Count + 1, newShift, newRoot, newTail);
    }

    /// <summary>
    /// Removes and last item on the list.
    /// </summary>
    /// <returns>A new list with the last item removed.</returns>
    /// <exception cref="InvalidOperationException">
    /// The <see cref="ImmutableTrieList{T}"/> is empty.
    /// </exception>
    public ImmutableTrieList<T> Pop()
    {
      Requires.ValidState(Count > 0, "The list is empty");

      if (Count == 1)
      {
        // PopInNode can't handle when there is only one element left
        return ImmutableTrieList<T>.Empty;
      }

      if (Capacity - 1 > TailOffset)
      {
        // Pop in tail
        return new ImmutableTrieList<T>(Origin, Capacity - 1, Count - 1, Shift, Root, Tail);
      }

      // There is only one element in the tail
      // so the index is the last element in the leave node
      Node newTail;
      Node newRoot = PopInNode(Capacity - 2, Root, Shift, out newTail);
      int newShift = Shift;
      if (Shift > BITS && newRoot.Array[1] == null)
      {
        // PopInNode may result in extra root, so need to remove addition root
        newRoot = (Node)newRoot[0];
        newShift -= BITS;
      }

      return new ImmutableTrieList<T>(Origin, Capacity - 1, Count - 1, newShift, newRoot, newTail);
    }

    /// <summary>
    /// Replaces an element at a given position in the immutable list with the specified element.
    /// </summary>
    /// <param name="index">The position in the list of the element to replace.</param>
    /// <param name="value">The element to replace the old element with.</param>
    /// <returns>The new list with the replaced element, even if it is equal to the old element at that position.</returns>
    public ImmutableTrieList<T> SetItem(int index, T value)
    {
      CheckIndex(index);
      index += Origin;

      // if we are setting the value in the tail
      if (index >= TailOffset)
      {
        Node newTail = Tail.Clone();
        newTail[index & MASK] = value;
        return new ImmutableTrieList<T>(Origin, Capacity, Count, Shift, Root, newTail);
      }

      Node newRoot = SetItemInNode(Root, index, Shift, value);
      return new ImmutableTrieList<T>(Origin, Capacity, Count, Shift, newRoot, Tail);
    }

    /// <summary>
    /// Creates a list that has the same contents as this list and can be efficiently mutated across multiple operations using standard mutable interfaces.
    /// </summary>
    /// <returns>The created list with the same contents as this list.</returns>
    /// <remarks>
    /// This is an O(1) operation and results in only a single (small) memory allocation. The mutable list that is returned is not thread-safe.
    /// </remarks>
    public Builder ToBuilder()
    {
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
    public ImmutableTrieList<T> Build(Action<Builder> action)
    {
      Builder b = ToBuilder();
      action(b);
      return b.ToImmutable();
    }

    /// <summary>
    /// Removes all elements from the immutable list.
    /// </summary>
    /// <returns>An empty list.</returns>
    public ImmutableTrieList<T> Clear() => Empty;
  }
}