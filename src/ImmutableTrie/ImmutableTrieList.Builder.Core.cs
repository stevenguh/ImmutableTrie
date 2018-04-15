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

      private ImmutableTrieList<T> _originalList;

      internal Builder(ImmutableTrieList<T> immutable)
        : base(immutable.Count, immutable.Shift, immutable.Root, immutable.Tail)
      {
        this.Version = 0;
        _originalList = immutable;
      }

      /// <summary>
      /// Gets or sets the value for a given index into the list.
      /// </summary>
      /// <param name="index">The index of the desired element.</param>
      /// <returns>The value at the specified index.</returns>
      public T this[int index]
      {
        get => this.GetItem(index);
        set => this.SetItem(index, value);
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
        Requires.Argument(index + count <= this.Count, nameof(count), "{0} + {1} must be less than or equal to {2}", nameof(index), nameof(count), nameof(this.Count));

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
        if (this.Owner == null)
        {
          // Nothing changes, return the original list
          return _originalList;
        }

        this.Owner = null;
        return _originalList = new ImmutableTrieList<T>(Count, Shift, Root, Tail);
      }

      /// <summary>
      /// Removes all items from the immutable list.
      /// </summary>
      public void Clear()
      {
        Count = 0;
        Root = Node.Empty;
        Shift = BITS;
        // Tail will be trimmed in ToImmutable()
      }

      /// <summary>
      /// Adds an item to the immutable list.
      /// </summary>
      /// <param name="item">The item to add to the list.</param>
      public void Add(T item)
      {
        EnsureEditable();
        Node newRoot;
        int newShift = this.Shift;

        if (this.Count - this.TailOffset < WIDTH)
        {
          // Add to tail if there's still room
          this.Tail = this.Tail.EnsureEditable(this.Owner);
          this.Tail[this.Count & MASK] = item;
          this.Count++;
          this.Version++;
          return;
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
          newRoot = AppendInNode(this.Root, this.Shift, ensureEditable: true);
        }

        this.Tail = Node.CreateNew(this.Owner);
        this.Tail[0] = item;

        this.Root = newRoot;
        this.Shift = newShift;
        this.Count++;
        this.Version++;
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
        Requires.ValidState(this.Count > 0, "The list is empty");
        EnsureEditable();

        if (Count == 1)
        {
          this.Clear();
          return;
        }

        if (this.Count - 1 > this.TailOffset)
        {
          // Pop in the tail
          Count--;
          Version++;
          return;
        }

        Node newTail;
        Node newRoot = PopInNode(this.Root, this.Shift, out newTail, ensureEditable: true);
        int newShift = this.Shift;
        if (this.Shift > BITS && newRoot.Array[1] == null)
        {
          // PopInNode may result in extra root, so need to remove addition root
          newRoot = (Node)newRoot[0];
          newShift -= BITS;
        }

        Root = newRoot;
        Shift = newShift;
        Tail = newTail;
        Count--;
        Version++;
      }
      private void SetItem(int index, T value)
      {
        CheckIndex(index);
        EnsureEditable();

        if (index >= TailOffset)
        {
          this.Tail = this.Tail.EnsureEditable(this.Owner);
          this.Tail[index & MASK] = value;
        }
        else
        {
          this.Root = SetItemInNode(this.Root, index, this.Shift, value, ensureEditable: true);
        }

        this.Version++;
      }

      private void EnsureEditable() => this.Owner = this.Owner ?? new object();
    }
  }
}