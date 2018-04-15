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
      this.Count = count;
      this.Shift = shift;
      this.Root = root;
      this.Tail = tail;
      this.Owner = null;
    }

    /// <summary>
    /// Gets a value that indicates whether this list is empty.
    /// </summary>
    /// <value><c>true</c> if the list is empty; otherwise, <c>false</c>.</value>
    public bool IsEmpty => this.Count == 0;

    /// <summary>
    /// Gets the number of elements contained in the list.
    /// </summary>
    /// <value>The number of elements in the list.</value>
    public int Count { get; protected set; } // Setter is for the builder only

    protected int Shift { get; set; } // Setter is for the builder only
    protected Node Root { get; set; } // Setter is for the builder only
    protected Node Tail { get; set; } // Setter is for the builder only
    protected object Owner { get; set; } // Builder use only
    protected int TailOffset => Count < WIDTH ? 0 : ((Count - 1) >> BITS) << BITS;

    protected static Node CreateNewPath(object owner, int level, Node node)
    {
      if (level == 0)
      {
        return node;
      }

      Node returnVal = new Node(owner);
      returnVal.Array[0] = CreateNewPath(owner, level - BITS, node);
      return returnVal;
    }

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
      Node node = this.GetNodeFor(index);
      return (T)node[index & MASK];
    }

    protected void CheckIndex(int index) =>
      Requires.Range(index >= 0 && index < this.Count, nameof(index));

    protected Node GetNodeFor(int index)
    {
      if (index >= this.TailOffset)
      {
        return this.Tail;
      }

      Node node = this.Root;
      for (int level = Shift; level > 0; level -= BITS)
      {
        node = (Node)node.Array[(index >> level) & MASK];
      }

      return node;
    }

    protected Node AppendInNode(Node node, int level, bool ensureEditable = false)
    {
      node = node ?? Node.Empty; // node can be null if there is no root use empty.

      // Copy node for modifiation
      int subIndex = ((this.Count - 1) >> level) & MASK;
      if (level == 0)
      {
        return this.Tail;
      }
      else
      {
        node = ensureEditable ? node.EnsureEditable(this.Owner) : node.Clone();
        node[subIndex] = AppendInNode((Node)node[subIndex], level - BITS, ensureEditable);
        return node;
      }
    }

    protected Node PopInNode(Node node, int level, out Node newTail, bool ensureEditable = false)
    {
      if (level == 0)
      {
        // Move the last leaf node to tail
        // We are chaning the node, so we don't need to copy
        newTail = node;
        return null;
      }

      // Assuming this method will only be called when there is only one element in the tail
      // so the index is the last element in the leave node
      int index = this.Count - 2;
      int subIndex = (index >> level) & MASK;
      node = ensureEditable ? node.EnsureEditable(this.Owner) : node.Clone();
      Node subNode = PopInNode((Node) node[subIndex], level - BITS, out newTail, ensureEditable);
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
      node = ensureEditable ? node.EnsureEditable(this.Owner) : node.Clone();
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

    protected internal sealed class Node
    {
      internal readonly static Node Empty = new Node((object)null);
      internal object Owner;
      internal object[] Array;

      internal Node(object owner)
      {
        this.Owner = owner;
        this.Array = new object[32];
      }

      internal Node(object owner, object[] arr)
      {
        this.Owner = owner;
        this.Array = arr;
      }

      internal Node(Node node)
      {
        this.Owner = node.Owner;
        this.Array = (object[])node.Array.Clone();
      }

      internal static Node CreateNew(object owner = null)
      {
        return new Node(owner);
      }

      public object this[int i]
      {
        get => this.Array[i];
        set => this.Array[i] = value;
      }

      public int Length => this.Array.Length;

      public Node Clone()
      {
        return new Node(this.Owner, (object[])this.Array.Clone());
      }

      public Node EnsureEditable(object owner)
      {
        // Make sure owner is not null. This method should only be called from a builder class
        Requires.NotNull(owner, nameof(owner));

        if (this.Owner == owner)
        {
          return this;
        }

        // Copy the node and set the owner if the owner is different
        return new Node(this.Owner, (object[])this.Array.Clone());
      }
    }
  }
}