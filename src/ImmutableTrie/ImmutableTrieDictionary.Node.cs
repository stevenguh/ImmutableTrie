using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Validation;

namespace ImmutableTrie
{
  /// <content>
  /// Contains the inner Builder class.
  /// </content>
  public sealed partial class ImmutableTrieDictionary<TKey, TValue>
  {
    internal abstract class NodeBase
    {
      internal NodeBase(object owner)
      {
        this.Owner = owner;
      }

      protected object Owner { get; set; }

      protected static int PopCount(int x)
      {
        x = x - ((x >> 1) & 0x55555555);
        x = (x & 0x33333333) + ((x >> 2) & 0x33333333);
        return (((x + (x >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
      }

      protected static int Mask(int hash, int shift)
      {
        return (hash >> shift) & MASK;
      }
      internal abstract NodeBase Update(object owner, int shift, int hash, TKey key, TValue value, Comparers comparers, KeyCollisionBehavior behavior, out OperationResult result);
      internal abstract NodeBase Remove(object owner, int shift, int hash, TKey key, Comparers comparers, out OperationResult result);
      internal abstract object Get(int shift, int hash, Comparers comparers, TKey key, out TKey actualKey);
      internal abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();
      protected static NodeBase MergeIntoNode(object owner, int shift, ValueNode node1, int key1hash, int key2hash, TKey key2, TValue value2)
      {
        if (key1hash == key2hash)
        {
          var newValues = new ValueNode[] { node1, new ValueNode(owner, key2, value2) };
          return new HashCollisionNode(owner, key1hash, 2, newValues);
        }

        return NestToBitmapNode(owner, shift, node1, key1hash, key2hash, key2, value2);
      }

      protected static NodeBase NestToBitmapNode(object owner, int shift, NodeBase node1, int key1hash, int key2hash, TKey key2, TValue value2)
      {
        Debug.Assert(key1hash != key2hash);

        int idx1 = Mask(key1hash, shift);
        int idx2 = Mask(key2hash, shift);

        NodeBase[] nodes;
        if (idx1 == idx2)
        {
          var subNode = NestToBitmapNode(owner, shift + BITS, node1, key1hash, key2hash, key2, value2);
          nodes = new NodeBase[] { subNode };
        }
        else
        {
          var newNode2 = new ValueNode(owner, key2, value2);
          nodes = (idx1 < idx2) ? new NodeBase[] { node1, newNode2 } : new NodeBase[] { newNode2, node1 };
        }

        return new BitmapIndexedNode(owner, (1 << idx1) | (1 << idx2), nodes);
      }
      protected bool IsEditable(object owner) => this.Owner != null && this.Owner == owner;

      protected sealed class NodeEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
      {
        private bool _disposed;
        private NodeBase[] _array;
        private int _runningIndex;
        private IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

        internal NodeEnumerator(NodeBase[] array)
        {
          Requires.NotNull(array, nameof(array));
          _array = array;
          _runningIndex = -1;
          _enumerator = null;
        }

        public KeyValuePair<TKey, TValue> Current
        {
          get
          {
            if (_enumerator != null)
            {
              return _enumerator.Current;
            }

            throw new InvalidOperationException();
          }
        }

        object IEnumerator.Current => this.Current;

        /// <summary>
        /// Disposes of this enumerator.
        /// </summary>
        public void Dispose() => _disposed = true;

        public bool MoveNext()
        {
          ThrowIfDisposed();
          if (_enumerator != null)
          {
            if (_enumerator.MoveNext())
            {
              return true;
            }
          }

          while (++_runningIndex < _array.Length)
          {
            if (_array[_runningIndex] != null)
            {
              _enumerator = _array[_runningIndex].GetEnumerator();
              if (_enumerator.MoveNext())
              {
                return true;
              }
            }
          }

          _enumerator = null;
          return false;
        }

        public void Reset()
        {
          ThrowIfDisposed();
          _runningIndex = -1;
          _enumerator = null;
        }

        private void ThrowIfDisposed()
        {
          if (_disposed)
          {
            throw new ObjectDisposedException(this.GetType().FullName);
          }
        }
      }
    }

    internal class HashArrayMapNode : NodeBase
    {
      private int _count;
      private NodeBase[] _array;

      internal HashArrayMapNode(object owner, int count, NodeBase[] array)
        : base(owner)
      {
        _count = count;
        _array = array;
      }

      internal override NodeBase Update(object owner, int shift, int hash, TKey key, TValue value, Comparers comparers, KeyCollisionBehavior behavior, out OperationResult result)
      {
        int idx = Mask(hash, shift);
        NodeBase node = _array[idx];

        if (node == null)
        {
          NodeBase newSubNode = BitmapIndexedNode.Empty.Update(owner, shift + BITS, hash, key, value, comparers, behavior, out result);
          return SetNode(owner, _count + 1, idx, newSubNode);
        }
        else
        {
          NodeBase newSubNode = node.Update(owner, shift + BITS, hash, key, value, comparers, behavior, out result);
          if (newSubNode == node) { return this; } // No Change
          return SetNode(owner, _count, idx, newSubNode);
        }
      }

      internal override NodeBase Remove(object owner, int shift, int hash, TKey key, Comparers comparers, out OperationResult result)
      {
        int idx = Mask(hash, shift);
        NodeBase node = _array[idx];
        if (node == null)
        {
          result = OperationResult.NoChangeRequired;
          return this;
        }

        NodeBase newSubNode = node.Remove(owner, shift + BITS, hash, key, comparers, out result);
        if (newSubNode == null) // the sub-node is removed.
        {
          return (_count <= MIN_HASH_ARRAY_MAP_SIZE)
            // Compress it to BitmapIndexedNode
            ? PackNodes(owner, idx)
            // just set null if the current count is not small enough to pack
            : SetNode(owner, _count - 1, idx, null);
        }

        return newSubNode == node ? this : SetNode(owner, _count, idx, newSubNode);
      }

      internal override object Get(int shift, int hash, Comparers comparers, TKey key, out TKey actualKey)
      {
        int idx = Mask(hash, shift);
        NodeBase node = _array[idx];
        if (node == null)
        {
          actualKey = default(TKey);
          return NotFound;
        }
        return node.Get(shift + BITS, hash, comparers, key, out actualKey);
      }

      internal override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new NodeEnumerator(_array);

      private NodeBase PackNodes(object owner, int idx)
      {
        NodeBase[] newArray = new NodeBase[_count - 1];
        int j = 0;
        int bitmap = 0;
        for (int i = 0; i < _array.Length; i++)
        {
          // Skip the index we are removing
          if (i == idx) { continue; }

          if (_array[i] != null)
          {
            bitmap |= 1 << i;
            newArray[j++] = _array[i];
          }
        }
        return new BitmapIndexedNode(this.Owner, bitmap, newArray);
      }

      private HashArrayMapNode SetNode(object owner, int count, int idx, NodeBase value)
      {
        HashArrayMapNode node = IsEditable(owner)
          ? this
          : new HashArrayMapNode(owner, _count, (NodeBase[])_array.Clone());
        node._count = count;
        node._array[idx] = value;
        return node;
      }
    }

    internal class BitmapIndexedNode : NodeBase
    {
      internal static BitmapIndexedNode Empty = new BitmapIndexedNode(null, 0, new NodeBase[0]);
      private int _bitmap;
      private NodeBase[] _nodes;
      internal BitmapIndexedNode(object owner, int bitmap, NodeBase[] nodes)
        : base(owner)
      {
        _bitmap = bitmap;
        _nodes = nodes;
      }

      internal override NodeBase Update(object owner, int shift, int hash, TKey key, TValue value, Comparers comparers, KeyCollisionBehavior behavior, out OperationResult result)
      {
        int bit = 1 << Mask(hash, shift);
        int idx = GetIndex(bit);
        if ((_bitmap & bit) != 0)
        { // exist
          NodeBase node = _nodes[idx];
          var newSubNode = node.Update(owner, shift + BITS, hash, key, value, comparers, behavior, out result);
          if (newSubNode == node) { return this; }

          var newNode = EnsureEditable(owner);
          newNode._nodes[idx] = newSubNode;
          return newNode;
        }
        else
        { // not exist
          int count = PopCount(_bitmap);
          if (count == 0)
          {
            result = OperationResult.SizeChanged;
            return new ValueNode(owner, key, value);
          }

          if (count < _nodes.Length && IsEditable(owner))
          {
            // still has room in the array and editable
            result = OperationResult.SizeChanged;
            Array.Copy(_nodes, idx, _nodes, idx + 1, count - idx);
            _nodes[idx] = new ValueNode(owner, key, value);
            _bitmap |= bit;
            return this;
          }

          return (count >= MAX_BITMAP_INDEXED_SIZE)
            ? (NodeBase)ExpandToArrayMap(owner, shift, count, hash, key, value, comparers, behavior, out result)
            : AddToNode(owner, idx, bit, count, key, value, out result);
        }
      }

      internal override NodeBase Remove(object owner, int shift, int hash, TKey key, Comparers comparers, out OperationResult result)
      {
        int bit = 1 << Mask(hash, shift);
        if ((_bitmap & bit) == 0)
        {
          // Not found noop
          result = OperationResult.NoChangeRequired;
          return this;
        }

        int idx = GetIndex(bit);
        NodeBase n = _nodes[idx].Remove(owner, shift + BITS, hash, key, comparers, out result);
        if (n == _nodes[idx]) { return this; }
        if (n != null)
        {
          var newNode = EnsureEditable(owner);
          newNode._nodes[idx] = n;
          return newNode;
        }

        if (_bitmap == bit) { return null; }

        // removed, so resize
        BitmapIndexedNode editable = EnsureEditable(owner);
        editable._bitmap ^= bit;
        Array.Copy(editable._nodes, idx + 1, editable._nodes, idx, editable._nodes.Length - (idx + 1));
        editable._nodes[editable._nodes.Length - 1] = null;
        return editable;
      }

      internal override object Get(int shift, int hash, Comparers comparers, TKey key, out TKey actualKey)
      {
        int bit = 1 << Mask(hash, shift);
        if ((_bitmap & bit) == 0)
        {
          actualKey = default(TKey);
          return NotFound;
        }
        int idx = GetIndex(bit);
        return _nodes[idx].Get(shift + BITS, hash, comparers, key, out actualKey);
      }

      internal override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new NodeEnumerator(_nodes);

      private int GetIndex(int bit)
      {
        return PopCount(_bitmap & (bit - 1));
      }

      private BitmapIndexedNode EnsureEditable(object owner)
      {
        if (IsEditable(owner))
        {
          return this;
        }

        int n = PopCount(_bitmap);
        NodeBase[] newArray = new NodeBase[n];
        Array.Copy(_nodes, newArray, n);
        return new BitmapIndexedNode(owner, _bitmap, newArray);
      }

      private BitmapIndexedNode AddToNode(object owner, int idx, int bit, int count, TKey key, TValue value, out OperationResult result)
      {
        result = OperationResult.SizeChanged;

        NodeBase[] newArray = new NodeBase[count + 1];
        Array.Copy(_nodes, newArray, idx);
        newArray[idx] = new ValueNode(owner, key, value);
        Array.Copy(_nodes, idx, newArray, idx + 1, count - idx);

        if (IsEditable(owner))
        {
          this._nodes = newArray;
          this._bitmap |= bit;
          return this;
        }

        return new BitmapIndexedNode(owner, _bitmap | bit, newArray);
      }

      private HashArrayMapNode ExpandToArrayMap(object owner, int shift, int count, int hash, TKey key, TValue value, Comparers comparers, KeyCollisionBehavior behavior, out OperationResult result)
      {
        NodeBase[] nodes = new NodeBase[WIDTH];
        int index = Mask(hash, shift);
        nodes[index] = Empty.Update(owner, shift + BITS, hash, key, value, comparers, behavior, out result);

        int j = 0;
        for (int i = 0; i < nodes.Length; i++)
        {
          if (((_bitmap >> i) & 1) != 0)
          {
            nodes[i] = _nodes[j++];
          }
        }

        return new HashArrayMapNode(owner, count + 1, nodes);
      }
    }

    internal class HashCollisionNode : NodeBase
    {
      private int _hash;
      private int _count;
      private ValueNode[] _values;

      internal HashCollisionNode(object owner, int keyHash, int count, ValueNode[] values)
        : base(owner)
      {
        _hash = keyHash;
        _count = count;
        _values = values;
      }

      internal override NodeBase Update(object owner, int shift, int hash, TKey key, TValue value, Comparers comparers, KeyCollisionBehavior behavior, out OperationResult result)
      {
        if (hash == _hash)
        {
          int idx = IndexOf(key, comparers);
          if (idx != -1)
          {
            // Found the same key, just update
            var editable = EnsureEditable(owner);
            editable._values[idx] = (ValueNode)editable._values[idx]
              .Update(owner, shift, hash, key, value, comparers, behavior, out result);
            return editable;
          }

          if (_count < _values.Length && IsEditable(owner)) // There are still some spots left
          {
            _values[_count] = new ValueNode(owner, key, value);
            _count++;
            result = OperationResult.SizeChanged;
            return this;
          }

          // Key cannot be found in the existing buckets, nor the list has extra room
          // let's copy the array again
          ValueNode[] newValues = new ValueNode[_count + 1];
          Array.Copy(_values, newValues, _count);
          newValues[_count] = new ValueNode(owner, key, value);

          result = OperationResult.SizeChanged;
          return SetNode(owner, _count + 1, newValues);
        }
        else
        {
          // different hash, nest in a bit mapnode
          result = OperationResult.SizeChanged;
          return NestToBitmapNode(owner, shift, this, _hash, hash, key, value);
        }
      }

      internal override NodeBase Remove(object owner, int shift, int hash, TKey key, Comparers comparers, out OperationResult result)
      {
        int idx = IndexOf(key, comparers);
        if (idx == -1)
        {
          // no found, no-op
          result = OperationResult.NoChangeRequired;
          return this;
        }

        result = OperationResult.SizeChanged;
        if (_count == 1) {return null; } // one count left, return null
        if (_count == 2) { return _values[0]; } // return the value node.

        if (IsEditable(owner))
        {
          _values[idx] = _values[_count - 1];
          _values[_count - 1] = null;
          _count--;
          return this;
        }
        else
        {
          ValueNode[] newValues = new ValueNode[_count - 1];
          Array.Copy(_values, newValues, newValues.Length);
          if (idx < newValues.Length)
          {
            newValues[idx] = _values[_count - 1];
          }

          return SetNode(owner, _count - 1, newValues);
        }
      }

      internal override object Get(int shift, int hash, Comparers comparers, TKey key, out TKey actualKey)
      {
        int idx = IndexOf(key, comparers);
        if (idx >= 0)
        {
          return _values[idx].Get(shift, hash, comparers, key, out actualKey);
        }
        else
        {
          actualKey = default(TKey);
          return NotFound;
        }
      }

      internal override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new NodeEnumerator(_values);

      private int IndexOf(TKey key, Comparers comparers)
      {
        for (int i = 0; i < _count; i++)
        {
          if (comparers.KeyComparer.Equals(key, _values[i].Key)) { return i; }
        }

        return -1;
      }

      private HashCollisionNode SetNode(object owner, int count, ValueNode[] values)
      {
        if (IsEditable(owner))
        {
          _values = values;
          _count = count;
          return this;
        }
        return new HashCollisionNode(owner, _hash, count, values);
      }
      private HashCollisionNode EnsureEditable(object owner)
      {
        if (IsEditable(owner))
        {
          return this;
        }

        ValueNode[] newValues = new ValueNode[_count];
        Array.Copy(_values, newValues, _count); // copy up to count only since we are resizing
        return new HashCollisionNode(owner, _hash, _count, newValues);
      }
    }

    internal class ValueNode : NodeBase
    {
      internal ValueNode(object owner, TKey key, TValue value)
        : base(owner)
      {
        this.Key = key;
        this.Value = value;
      }

      internal TKey Key { get; private set; }
      internal TValue Value { get; private set; }

      internal override object Get(int shift, int hash, Comparers comparers, TKey key, out TKey actualKey)
      {
        if (comparers.KeyComparer.Equals(this.Key, key))
        {
          actualKey = this.Key;
          return this.Value;
        }
        else
        {
          actualKey = default(TKey);
          return NotFound;
        }
      }

      internal override NodeBase Remove(object owner, int shift, int hash, TKey key, Comparers comparers, out OperationResult result)
      {
        if (!comparers.KeyComparer.Equals(this.Key, key))
        {
          result = OperationResult.NoChangeRequired;
          return this;
        }
        else
        {
          result = OperationResult.SizeChanged;
          return null;
        }
      }

      internal override NodeBase Update(object owner, int shift, int hash, TKey key, TValue value, Comparers comparers, KeyCollisionBehavior behavior, out OperationResult result)
      {
        if (comparers.KeyComparer.Equals(this.Key, key))
        {
          switch (behavior)
          {
            case KeyCollisionBehavior.SetValue:
              result = OperationResult.AppliedWithoutSizeChange;
              return SetValue(owner, key, value);
            
            case KeyCollisionBehavior.SetIfValueDifferent:
              if (comparers.ValueComparer.Equals(this.Value, value))
              {
                result = OperationResult.NoChangeRequired;
                return this;
              }
              else
              {
                result = OperationResult.AppliedWithoutSizeChange;
                return SetValue(owner, key, value);
              }

            case KeyCollisionBehavior.Skip:
              result = OperationResult.NoChangeRequired;
              return this;

            case KeyCollisionBehavior.ThrowIfValueDifferent:
              if (!comparers.ValueComparer.Equals(this.Value, value))
              {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Strings.DuplicateKey, key));
              }

              result = OperationResult.NoChangeRequired;
              return this;

            case KeyCollisionBehavior.ThrowAlways:
              throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Strings.DuplicateKey, key));

            default:
              throw new InvalidOperationException(); // unreachable
          }
        }

        result = OperationResult.SizeChanged;
        return MergeIntoNode(owner, shift, this, comparers.KeyComparer.GetHashCode(this.Key), hash, key, value);
      }

      internal override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new ValueNodeEnumerator(this);

      private ValueNode SetValue(object owner, TKey key, TValue value)
      {
        if (IsEditable(owner))
        {
          this.Key = key;
          this.Value = value;
          return this;
        }

        return new ValueNode(owner, key, value);
      }

      private sealed class ValueNodeEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
      {
        private bool _disposed;
        private int _runningIndex;
        private ValueNode _node;

        internal ValueNodeEnumerator(ValueNode node)
        {
          Requires.NotNull(node, nameof(node));
          _runningIndex = -1;
          _node = node;
        }

        public KeyValuePair<TKey, TValue> Current
        {
          get
          {
            if (_runningIndex == 0)
            {
              return new KeyValuePair<TKey, TValue>(_node.Key, _node.Value);
            }

            throw new InvalidOperationException();
          }
        }

        object IEnumerator.Current => this.Current;

        /// <summary>
        /// Disposes of this enumerator.
        /// </summary>
        public void Dispose() => _disposed = true;

        public bool MoveNext()
        {
          ThrowIfDisposed();
          return ++_runningIndex == 0;
        }

        public void Reset()
        {
          ThrowIfDisposed();
          _runningIndex = -1;
        }

        private void ThrowIfDisposed()
        {
          if (_disposed)
          {
            throw new ObjectDisposedException(this.GetType().FullName);
          }
        }
      }
    }
  }
}