using System;
using System.Reactive;

namespace Adaptive.Observables
{
    public interface IObservableDictionary<TKey, TValue> : IObservable<DictionaryNotification<TKey, TValue>>, IDisposable
    {
        IObservable<DictionaryNotification<TKey, TValue>> Get(TKey key);
        IObservable<Unit> IsInitialised();
    }
}