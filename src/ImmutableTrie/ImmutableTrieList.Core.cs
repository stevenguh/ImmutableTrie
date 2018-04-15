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
    public static readonly ImmutableTrieList<T> Empty = new ImmutableTrieList<T>(0, BITS, null, Node.Empty);

    internal ImmutableTrieList(int count, int shift, Node root, Node tail)
      : base(count, shift, root, tail)
    { }

    /// <summary>
    /// Gets the element at the specified index of the list.
    /// </summary>
    /// <param name="index">The index of the element to retrieve.</param>
    /// <returns>The element at the specified index.</returns>
    public T this[int index] => this.GetItem(index);

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
      Requires.Argument(index + count <= this.Count, nameof(count), "{0} + {1} must be less than or equal to {2}", nameof(index), nameof(count), nameof(this.Count));

      comparer = comparer ?? Comparer<T>.Default;
      return ListSortHelper<T>.Default.BinarySearch(this, index, count, item, comparer);
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
      int newShift = this.Shift;

      if (this.Count - this.TailOffset < WIDTH)
      {
        // Add to tail if there's still room
        newTail = this.Tail.Clone();
        newTail[this.Count & MASK] = value;
        return new ImmutableTrieList<T>(Count + 1, Shift, Root, newTail);
      }

      // Root overflow
      // 1 << (Shift + BITS): max tree capacity
      // + WIDTH: add the tail capacity
      if (this.Count == (1 << (this.Shift + BITS)) + WIDTH)
      {
        // Create new root node.
        newRoot = Node.CreateNew();

        // Create the new path from the tail
        Node path = this.Tail;
        for (int level = this.Shift; level > 0; level -= BITS)
        {
          Node newNode = Node.CreateNew();
          newNode[0] = path;
          path = newNode;
          newShift += BITS;
        }

        newRoot[0] = this.Root;
        newRoot[1] = path;
      }
      else
      {
        newRoot = AppendInNode(this.Root, this.Shift);
      }

      newTail = Node.CreateNew();
      newTail[0] = value;
      return new ImmutableTrieList<T>(Count + 1, newShift, newRoot, newTail);
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
      Requires.ValidState(this.Count > 0, "The list is empty");

      if (Count == 1)
      {
        // PopInNode can't handle when there is only one element left
        return ImmutableTrieList<T>.Empty;
      }

      if (this.Count - 1 > this.TailOffset)
      {
        // Pop in tail
        return new ImmutableTrieList<T>(this.Count - 1, this.Shift, this.Root, this.Tail);
      }

      Node newTail;
      Node newRoot = PopInNode(this.Root, this.Shift, out newTail);
      int newShift = this.Shift;
      if (this.Shift > BITS && newRoot.Array[1] == null)
      {
        // PopInNode may result in extra root, so need to remove addition root
        newRoot = (Node)newRoot[0];
        newShift -= BITS;
      }

      return new ImmutableTrieList<T>(this.Count - 1, newShift, newRoot, newTail);
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

      // if we are setting the value in the tail
      if (index >= this.TailOffset)
      {
        Node newTail = this.Tail.Clone();
        newTail[index & MASK] = value;
        return new ImmutableTrieList<T>(Count, Shift, Root, newTail);
      }

      Node newRoot = SetItemInNode(this.Root, index, this.Shift, value);
      return new ImmutableTrieList<T>(this.Count, this.Shift, newRoot, this.Tail);
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