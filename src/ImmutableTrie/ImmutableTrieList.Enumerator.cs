using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Validation;

namespace ImmutableTrie
{
  /// <content>
  /// Contains the enumerator for the list.
  /// </content>
  public sealed partial class ImmutableTrieList<T>
  {
    /// <summary>
    /// Enumerates the contents of a trie.
    /// </summary>
    public class Enumerator : IEnumerator<T>, IEnumerator, IDisposable
    {

      /// <summary>
      /// The instance of the list to enumerate.
      /// </summary>
      private ImmutableTrieListBase<T> _list;

      /// <summary>
      /// The starting index of the collection at which to begin enumeration.
      /// </summary>
      private readonly int _startIndex;

      /// <summary>
      /// The end index of the enumeration.
      /// </summary>
      private readonly int _endIndex;

      /// <summary>
      /// The current index for current element. 
      /// </summary>
      private int _runningIndex;

      /// <summary>
      /// A value indicating whether this enumerator walks in reverse order.
      /// </summary>
      private bool _reversed;

      /// <summary>
      /// The version of the builder (when applicable) that is being enumerated.
      /// </summary>
      private int _enumeratingBuilderVersion;

      private bool _disposed;

      internal Enumerator(ImmutableTrieListBase<T> list, int startIndex = -1, int count = -1, bool reversed = false)
      {
        Requires.NotNull(list, nameof(list));
        Requires.Range(startIndex >= -1, nameof(startIndex));
        Requires.Range(count >= -1, nameof(count));
        Requires.Argument(reversed || count == -1 || (startIndex == -1 ? 0 : startIndex) + count <= list.Count, nameof(count), "The specified {0} and {1} do not produce a enumerable range.", nameof(startIndex), nameof(count));
        Requires.Argument(!reversed || count == -1 || (startIndex == -1 ? list.Count - 1 : startIndex) - count + 1 >= 0, nameof(count), "The specified {0} and {1} do not produce a enumerable range.", nameof(startIndex), nameof(count));

        _list = list;
        _enumeratingBuilderVersion = GetCurrentVersion(list);

        count = count == -1 ? list.Count : count;
        _startIndex = startIndex >= 0 ? startIndex : (reversed ? list.Count - 1 : 0);
        _endIndex = reversed ? _startIndex - count + 1 : _startIndex + count - 1;

        _runningIndex = reversed ? _startIndex + 1 : _startIndex - 1;

        _reversed = reversed;
        _disposed = false;
      }

      /// <summary>
      /// The current element.
      /// </summary>
      public T Current
      {
        get
        {
          ThrowIfDisposed();
          ThrowIfChange();
          try
          {
            return _list.GetItem(_runningIndex);
          }
          catch
          {
            throw new InvalidOperationException();
          }
        }
      }

      /// <summary>
      /// The current element.
      /// </summary>
      object IEnumerator.Current => this.Current;

      /// <summary>
      /// Disposes of this enumerator.
      /// </summary>
      public void Dispose() => _disposed = true;

      /// <summary>
      /// Advances enumeration to the next element.
      /// </summary>
      /// <returns>A value indicating whether there is another element in the enumeration.</returns>
      public bool MoveNext()
      {
        ThrowIfDisposed();
        ThrowIfChange();
        _runningIndex = _reversed ? _runningIndex - 1 : _runningIndex + 1;
        return _runningIndex >= 0 && _runningIndex < _list.Count && (_reversed ? _runningIndex >= _endIndex : _runningIndex <= _endIndex);
      }

      /// <summary>
      /// Restarts enumeration.
      /// </summary>
      public void Reset()
      {
        ThrowIfDisposed();
        _enumeratingBuilderVersion = GetCurrentVersion(_list);
        _runningIndex = _reversed ? _startIndex + 1 : _startIndex - 1;
      }

      private static int GetCurrentVersion(ImmutableTrieListBase<T> list) =>
        (list is Builder builder) ? builder.Version : -1;

      private void ThrowIfDisposed()
      {
        if (_disposed)
        {
          throw new ObjectDisposedException(this.GetType().FullName);
        }
      }

      private void ThrowIfChange()
      {
        if (_enumeratingBuilderVersion != GetCurrentVersion(_list))
        {
          throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
        }
      }
    }
  }
}