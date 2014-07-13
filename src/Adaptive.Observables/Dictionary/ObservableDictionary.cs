using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;

namespace Adaptive.Observables
{
    internal class ObservableDictionary<TKey, TValue> : IObservableDictionary<TKey, TValue>
    {
        private readonly IObservable<DictionaryModification<TKey, TValue>> _initialStream;
        private readonly IObservable<DictionaryModification<TKey, TValue>> _modificationStream;

        private readonly Dictionary<TKey, TValue> _state;
        private readonly Subject<DictionaryNotification<TKey, TValue>> _subject;
        // How do we document this? With a delegate?
        private readonly Func<TKey, TValue, TValue, TValue> _updateFunction;
        private readonly CompositeDisposable _disposables;
        private readonly SingleAssignmentDisposable _sourceSubscription;
        private readonly RefCountDisposable _refCountDisposable;
        private readonly IObservable<Unit> _streamInitialisation;
        private readonly Observer _observer;
        private readonly object _lock = new object();

        // todo mutable state to be replaced with a state machine, obs dict made lock free over a concurrent dictionary.
        private bool _isConnected = false;
        private Exception _error;
        private bool _isCompleted;
        private bool _isDisposed;

        private ObservableDictionary()
        {
            _state = new Dictionary<TKey, TValue>();
            _subject = new Subject<DictionaryNotification<TKey, TValue>>();
            _sourceSubscription = new SingleAssignmentDisposable();
            _refCountDisposable = new RefCountDisposable(_sourceSubscription);
            _disposables = new CompositeDisposable(_sourceSubscription, _refCountDisposable, _subject);
            _observer = new Observer(this);

            var connectableObservable =
                _subject.Only(DictionaryNotificationType.Initialised).Select(_ => Unit.Default).Take(1).PublishLast();
            _streamInitialisation = connectableObservable;
            _disposables.Add(connectableObservable.Connect());
        }

        public ObservableDictionary(IObservable<DictionaryModification<TKey, TValue>> changes,
            Func<TKey, TValue, TValue, TValue> updateFunction)
            : this(Observable.Empty<DictionaryModification<TKey, TValue>>(), changes, updateFunction)
        {

        }

        public ObservableDictionary(IObservable<DictionaryModification<TKey, TValue>> initial,
            IObservable<DictionaryModification<TKey, TValue>> modifications,
            Func<TKey, TValue, TValue, TValue> updateFunction) : this()
        {
            _initialStream = initial;
            _modificationStream = modifications;
            _updateFunction = updateFunction;
        }

        public IDisposable Subscribe(IObserver<DictionaryNotification<TKey, TValue>> observer)
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    return
                        Observable.Throw<DictionaryNotification<TKey, TValue>>(
                            new ObjectDisposedException(this.GetType().Name)).Subscribe(observer);
                }

                if (_error != null)
                {
                    return Observable.Throw<DictionaryNotification<TKey, TValue>>(_error).Subscribe(observer);
                }

                var existingValues = _state.ToList();

                foreach (var existingValue in existingValues)
                {
                    observer.OnNext(DictionaryNotification.Existing(existingValue.Key, existingValue.Value));
                }
                observer.OnNext(DictionaryNotification.Initialised<TKey, TValue>());

                return new CompositeDisposable(_subject.Subscribe(observer), Connect());
            }
        }

        private IDisposable Connect()
        {
            lock (_lock)
            {
                if (!_isConnected)
                {
                    _sourceSubscription.Disposable = Observable.Concat(
                        _initialStream,
                        Observable.Return(DictionaryModification<TKey, TValue>.Initialised),
                        _modificationStream)
                        .Subscribe(_observer);
                    _isConnected = true;
                }
            }
            return _refCountDisposable.GetDisposable();
        }

        public IObservable<DictionaryNotification<TKey, TValue>> Get(TKey key)
        {
            return Observable.Create<DictionaryNotification<TKey, TValue>>(observer =>
            {
                lock (_lock)
                {
                    TValue existingValue;
                    if (_state.TryGetValue(key, out existingValue))
                    {
                        observer.OnNext(DictionaryNotification.Existing(key, existingValue));
                    }
                    else
                    {
                        observer.OnNext(DictionaryNotification.Missing<TKey, TValue>(key));
                    }
                    return new CompositeDisposable(_subject.Subscribe(observer), Connect());
                }
            });
        }

        public IObservable<Unit> IsInitialised()
        {
            return _streamInitialisation;
        }

        public void Dispose()
        {

        }

        private class Observer : IObserver<DictionaryModification<TKey, TValue>>
        {
            private readonly ObservableDictionary<TKey, TValue> _dictionary;

            public Observer(ObservableDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
            }

            public void OnNext(DictionaryModification<TKey, TValue> value)
            {
                if (DictionaryModification<TKey, TValue>.Initialised == value)
                {
                    _dictionary._subject.OnNext(DictionaryNotification.Initialised<TKey, TValue>());
                }
                else
                {
                    switch (value.Type)
                    {
                        case DictionaryModificationType.Upsert:
                            Upsert(value);
                            break;
                        case DictionaryModificationType.Replace:
                            Replace(value);
                            break;
                        case DictionaryModificationType.Remove:
                            Remove(value);
                            break;
                        case DictionaryModificationType.Clear:
                            Clear();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            private void Upsert(DictionaryModification<TKey, TValue> modification)
            {
                TValue existingValue;
                if (_dictionary._state.TryGetValue(modification.Key, out existingValue))
                {
                    try
                    {
                        // TODO Check order is correct here.
                        var newValue = _dictionary._updateFunction.Invoke(modification.Key, existingValue,
                            modification.Value);

                        _dictionary._state[modification.Key] = newValue;
                        _dictionary._subject.OnNext(DictionaryNotification.Updated(modification.Key, newValue,
                            modification.Value, existingValue));
                    }
                    catch (Exception ex)
                    {
                        // TODO Check that this errors the whole stream.
                        _dictionary._error = ex;
                        _dictionary._subject.OnError(
                            new ObservableDictionaryUpdateException("Exception thrown when updating " + modification, ex));
                    }
                }
                else
                {
                    _dictionary._state[modification.Key] = modification.Value;
                    _dictionary._subject.OnNext(DictionaryNotification.Inserted(modification.Key, modification.Value));
                }
            }

            private void Replace(DictionaryModification<TKey, TValue> modification)
            {
                TValue existingValue;
                if (_dictionary._state.TryGetValue(modification.Key, out existingValue))
                {
                    _dictionary._subject.OnNext(DictionaryNotification.Replaced(modification.Key, modification.Value,
                        existingValue));
                }
                else
                {
                    // TODO Should we do an upsert when the item to be replaced doesn't exist, or signal replaced with a missing value?
                    Upsert(modification);
                }
            }

            private void Remove(DictionaryModification<TKey, TValue> modification)
            {
                TValue removedValue;
                if (_dictionary._state.TryGetValue(modification.Key, out removedValue))
                {
                    _dictionary._state.Remove(modification.Key);
                    // TODO Should we signal 'removed' for a value that didn't exist..?
                    _dictionary._subject.OnNext(DictionaryNotification.Removed(modification.Key, removedValue));
                }
            }

            private void Clear()
            {
                // todo emit a Removed<> for each item in the DB, rather than just clearing?
                // todo not atomic without locks.
                _dictionary._state.Clear();
                _dictionary._subject.OnNext(DictionaryNotification.Cleared<TKey, TValue>());
            }

            public void OnError(Exception error)
            {
                if (_dictionary._error != null && !_dictionary._isCompleted)
                {
                    _dictionary._error = error;
                    _dictionary._isCompleted = true;
                    _dictionary._subject.OnError(error);
                }
            }

            public void OnCompleted()
            {
                if (!_dictionary._isCompleted)
                {
                    _dictionary._isCompleted = true;
                    _dictionary._subject.OnCompleted();
                }
            }
        }
    }
}