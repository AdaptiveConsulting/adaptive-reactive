using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Cryptography;

namespace Adaptive.Observables
{
    public static class ObservableDictionaryExtensions
    {
        public static IObservable<DictionaryNotification<TKey, TValue>> ThrowOnMissing<TKey, TValue>(
            this IObservable<DictionaryNotification<TKey, TValue>> source)
        {
            return from item in source
                   from result in item.Type == DictionaryNotificationType.Missing
                                  ? Observable.Throw<DictionaryNotification<TKey, TValue>>(new KeyNotFoundException("Cannot find item for key " + item.Key))
                                  : Observable.Return(item)
                   select item;
        }

        private static IObservable<DictionaryNotification<TKey, TValue>> CompleteOn<TKey, TValue>(
            this IObservable<DictionaryNotification<TKey, TValue>> source, DictionaryNotificationType type)
        {
            return source.TakeWhile(dn => dn.Type != type);
        }

        public static IObservable<DictionaryNotification<TKey, TValue>> CompleteOnRemoved<TKey, TValue>(
            this IObservable<DictionaryNotification<TKey, TValue>> source)
        {
            return source.CompleteOn(DictionaryNotificationType.Removed);
        }

        public static IObservable<DictionaryNotification<TKey, TValue>> CompleteOnCleared<TKey, TValue>(
            this IObservable<DictionaryNotification<TKey, TValue>> source)
        {
            return source.CompleteOn(DictionaryNotificationType.Cleared);
        }

        public static IObservableDictionary<TKey, TValue> ToObservableDictionary<TKey, TValue>(
            this IObservable<DictionaryModification<TKey, TValue>> source)
        {
            return new ObservableDictionary<TKey, TValue>(source, (key, existing, @new) => @new); // TODO Check correct order.
        }

        public static IObservableDictionary<TKey, TValue> ToObservableDictionary<TKey, TValue>(
            this IObservable<DictionaryModification<TKey, TValue>> source, Func<TKey, TValue, TValue, TValue> updateFunction)
        {
            return new ObservableDictionary<TKey, TValue>(source, updateFunction); // TODO Check correct order.
        }

        public static IObservableDictionary<TKey, TValue> ToObservableDictionary<TKey, TValue>(
            this IObservable<DictionaryModification<TKey, TValue>> source,
            IObservable<DictionaryModification<TKey, TValue>> changes)
        {
            return new ObservableDictionary<TKey, TValue>(source, changes, (key, existing, @new) => @new);
        }

        public static IObservableDictionary<TKey, TValue> ToObservableDictionary<TKey, TValue>(
            this IObservable<DictionaryModification<TKey, TValue>> source,
            IObservable<DictionaryModification<TKey, TValue>> changes, Func<TKey, TValue, TValue, TValue> updateFunction)
        {
            return new ObservableDictionary<TKey, TValue>(source, changes, updateFunction); // TODO Check correct order.
        }

        /// <summary>
        /// Returns all items in the observable dictionary after it has finished initialising, or immediately if already initialised.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<IEnumerable<KeyValuePair<TKey, TValue>>> Items<TKey, TValue>(this IObservableDictionary<TKey, TValue> source)
        {
            return (from _ in source.IsInitialised()
                    from existingItems in source.TakeWhile(notification => notification.Type != DictionaryNotificationType.Initialised)
                        .Select(dn => new KeyValuePair<TKey, TValue>(dn.Key, dn.Value))
                        .ToList()
                select existingItems.AsEnumerable());
        }
        
        /// <summary>
        /// Returns a stream of dictionary notifications containing only those notification types included in <paramref name="filter"/>.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IObservable<DictionaryNotification<TKey, TValue>>  Only<TKey, TValue>(
            this IObservable<DictionaryNotification<TKey, TValue>> source, params DictionaryNotificationType[] filter)
        {
            return source.Where(dn => filter.Any(filteredDnt => (dn.Type & filteredDnt) > 0));
        } 
    }
}