using System;
using System.Collections;
using System.Collections.Generic;

namespace ImmutableTrie
{
  /// <content>
  /// Contains the inner <see cref="ImmutableTrieDictionary{TKey, TValue}.Enumerator"/> struct.
  /// </content>
  public sealed partial class ImmutableTrieDictionary<TKey, TValue>
  {
    /// <summary>
    /// Enumerates the contents of the collection in an allocation-free manner.
    /// </summary>
    public class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable
    {
      /// <summary>
      /// The builder being enumerated, if applicable.
      /// </summary>
      private readonly Builder _builder;

      /// <summary>
      /// The enumerator over the all the key value pairs in the node.
      /// </summary>
      private IEnumerator<KeyValuePair<TKey, TValue>> _nodeEnumerator;

      private bool _disposed;


      /// <summary>
      /// The version of the builder (when applicable) that is being enumerated.
      /// </summary>
      private int _enumeratingBuilderVersion;

      /// <summary>
      /// Initializes a new instance of the <see cref="ImmutableDictionary{TKey, TValue}.Enumerator"/> struct.
      /// </summary>
      /// <param name="root">The root.</param>
      /// <param name="builder">The builder, if applicable.</param>
      internal Enumerator(NodeBase root, Builder builder = null)
      {
        _builder = builder;
        _nodeEnumerator = root?.GetEnumerator() ?? GetEmptyEnumerator();
        _enumeratingBuilderVersion = GetCurrentVersion(builder);
        _disposed = false;
      }

      /// <summary>
      /// Gets the current element.
      /// </summary>
      public KeyValuePair<TKey, TValue> Current
      {
        get
        {
          this.ThrowIfDisposed();
          this.ThrowIfChanged();

          return _nodeEnumerator.Current;
        }
      }

      /// <summary>
      /// Gets the current element.
      /// </summary>
      object IEnumerator.Current => this.Current;

      /// <summary>
      /// Advances the enumerator to the next element of the collection.
      /// </summary>
      /// <returns>
      /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
      /// </returns>
      /// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created. </exception>
      public bool MoveNext()
      {
        this.ThrowIfDisposed();
        this.ThrowIfChanged();

        return _nodeEnumerator.MoveNext();
      }

      /// <summary>
      /// Sets the enumerator to its initial position, which is before the first element in the collection.
      /// </summary>
      /// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created. </exception>
      public void Reset()
      {
        this.ThrowIfDisposed();

        _enumeratingBuilderVersion = _builder != null ? _builder.Version : -1;
        _nodeEnumerator.Reset();
      }

      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
      /// </summary>
      public void Dispose()
      {
        _nodeEnumerator.Dispose();
        _disposed = true;
      }

      private static int GetCurrentVersion(Builder builder) =>
        builder != null ? builder.Version : -1;

      private static IEnumerator<KeyValuePair<TKey, TValue>> GetEmptyEnumerator()
      {
        yield break;
      }

      private void ThrowIfDisposed()
      {
        if (_disposed)
        {
          throw new ObjectDisposedException(this.GetType().FullName);
        }
      }

      /// <summary>
      /// Throws an exception if the underlying builder's contents have been changed since enumeration started.
      /// </summary>
      /// <exception cref="System.InvalidOperationException">Thrown if the collection has changed.</exception>
      private void ThrowIfChanged()
      {
        if (_enumeratingBuilderVersion != GetCurrentVersion(_builder))
        {
          throw new InvalidOperationException(Strings.CollectionModifiedDuringEnumeration);
        }
      }
    }
  }
}