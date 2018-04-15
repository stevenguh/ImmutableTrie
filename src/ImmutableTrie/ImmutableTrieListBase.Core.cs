using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Validation;

namespace ImmutableTrie
{
  /// <summary>
  /// The base class of <see cref="ImmutableTrieList{T}"/> and <see cref="ImmutableTrieList{T}.Builder"/> which contains the core logic.
  /// </summary>
  public abstract partial class ImmutableTrieListBase<T>
  {
    protected const int BITS = 5; // We need 5 bit for 32-way branching tries
    protected const int WIDTH = 1 << BITS; // 2^5 = 32
    protected const int MASK = WIDTH - 1; // 31, or 0x1f

    protected ImmutableTrieListBase(int count, int shift, Node root, Node tail)
    {
      Count = count;
      Shift = shift;
      Root = root;
      Tail = tail;
      Owner = null;
    }

    protected ImmutableTrieListBase(int origin, int capacity, int count, int shift, Node root, Node tail)
    {
      Origin = origin;
      Capacity = capacity;
      Count = count;
      Shift = shift;
      Root = root;
      Tail = tail;
      Owner = null;
    }

    /// <summary>
    /// Gets a value that indicates whether this list is empty.
    /// </summary>
    /// <value><c>true</c> if the list is empty; otherwise, <c>false</c>.</value>
    public bool IsEmpty => Count == 0;

    /// <summary>
    /// Gets the number of elements contained in the list.
    /// </summary>
    /// <value>The number of elements in the list.</value>
    public int Count { get; protected set; } // Setter is for the builder only

    // count + origin
    protected int Capacity { get; set; }
    protected int Origin { get; set; }
    protected int Shift { get; set; } // Setter is for the builder only
    protected Node Root { get; set; } // Setter is for the builder only
    protected Node Tail { get; set; } // Setter is for the builder only
    protected object Owner { get; set; } // Builder use only
    protected int TailOffset =>
      Capacity < WIDTH ? 0 : ((Capacity - 1) >> BITS) << BITS;

    /// <summary>
    /// Tests whether a value is one that might be found in this collection.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <returns><c>true</c> if this value might appear in the collection.</returns>
    /// <devremarks>
    /// This implementation comes from <see cref="List{T}"/>.
    /// </devremarks>
    protected static bool IsCompatibleObject(object value)
    {
      // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
      // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
      return ((value is T) || (value == null && default(T) == null));
    }

    internal T GetItem(int index)
    {
      CheckIndex(index);
      index += Origin;
      Node node = GetNodeFor(index);
      return (T)node[index & MASK];
    }

    protected void CheckIndex(int index) =>
      Requires.Range(index >= 0 && index < Count, nameof(index));

    protected Node GetNodeFor(int index)
    {
      if (index >= TailOffset)
      {
        return Tail;
      }

      Node node = Root;
      for (int level = Shift; level > 0; level -= BITS)
      {
        node = (Node)node.Array[(index >> level) & MASK];
      }

      return node;
    }

    protected Node AppendInNode(Node node, int level, bool ensureEditable = false)
    {
      node = node ?? Node.Empty; // node can be null if there is no root use empty.

      int subIndex = ((Capacity - 1) >> level) & MASK;
      if (level == 0)
      {
        return Tail;
      }
      else
      {
        node = ensureEditable ? node.EnsureEditable(Owner) : node.Clone();
        node[subIndex] = AppendInNode((Node)node[subIndex], level - BITS, ensureEditable);
        return node;
      }
    }

    protected Node PopInNode(int index, Node node, int level, out Node newTail, bool ensureEditable = false)
    {
        node = node ?? Node.Empty; // node can be null if there is no root use empty.

      if (level == 0)
      {
        // Move the last leaf node to tail
        // We are chaning the node, so we don't need to copy
        newTail = node;
        return null;
      }

      int subIndex = (index >> level) & MASK;
      node = ensureEditable ? node.EnsureEditable(Owner) : node.Clone();
      Node subNode = PopInNode(index, (Node)node[subIndex], level - BITS, out newTail, ensureEditable);
      if (subNode == null && subIndex == 0)
      {
        // Leave node removal:
        // This can happen when the last leaf node in the same level is moved to tail
        return null;
      }
      else
      {
        node[subIndex] = subNode;
      }

      return node;
    }

    protected Node SetItemInNode(Node node, int index, int level, object value, bool ensureEditable = false)
    {
      node = ensureEditable ? node.EnsureEditable(Owner) : node.Clone();
      if (level == 0)
      {
        node[index & MASK] = value;
      }
      else
      {
        int subIndex = (index >> level) & MASK;
        Node subNode = ((Node)node.Array[subIndex]);
        node[subIndex] = SetItemInNode(subNode, index, level - BITS, value);
      }

      return node;
    }

    protected Node CreateOverflowPath()
    {
      // Create new root node.
      Node newRoot = Node.CreateNew();

      // Create the new path from the tail
      Node path = Tail;
      for (int level = Shift; level > 0; level -= BITS)
      {
        Node newNode = Node.CreateNew();
        newNode[0] = path;
        path = newNode;
      }

      newRoot[0] = Root;
      newRoot[1] = path;

      return newRoot;
    }

    protected internal sealed class Node
    {
      internal readonly static Node Empty = new Node((object)null);
      internal object Owner;
      internal object[] Array;

      internal Node(object owner)
      {
        Owner = owner;
        Array = new object[32];
      }

      internal Node(object owner, object[] arr)
      {
        Owner = owner;
        Array = arr;
      }

      internal Node(Node node)
      {
        Owner = node.Owner;
        Array = (object[])node.Array.Clone();
      }

      internal static Node CreateNew(object owner = null)
      {
        return new Node(owner);
      }

      public object this[int i]
      {
        get => Array[i];
        set => Array[i] = value;
      }

      public int Length => Array.Length;

      public Node Clone()
      {
        return new Node(Owner, (object[])Array.Clone());
      }

      public Node EnsureEditable(object owner)
      {
        // Make sure owner is not null. This method should only be called from a builder class
        Requires.NotNull(owner, nameof(owner));

        if (Owner == owner)
        {
          return this;
        }

        // Copy the node and set the owner if the owner is different
        return new Node(Owner, (object[])Array.Clone());
      }
    }
  }
}