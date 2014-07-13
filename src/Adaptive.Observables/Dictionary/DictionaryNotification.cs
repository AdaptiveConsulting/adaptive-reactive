using System.Collections.Generic;

namespace Adaptive.Observables
{
    public sealed class DictionaryNotification<TKey, TValue>
    {
        /// <summary>
        /// The type of the dictionary notification.
        /// </summary>
        public DictionaryNotificationType Type { get; internal set; }
        /// <summary>
        /// The key of the dictionary notification.
        /// </summary>
        public TKey Key { get; internal set; }
        /// <summary>
        /// The value now in the dictionary for the key.
        /// </summary>
        public TValue Value { get; internal set; }
        /// <summary>
        /// For an updated notification type, the value that was previously in the dictionary for the key.
        /// </summary>
        public TValue UpdatedValue { get; internal set; }
        /// <summary>
        /// For an updated notification type, the value that was received and used to update the existing value.
        /// </summary>
        public TValue UpdatingValue { get; internal set; }
        /// <summary>
        /// For a replaced notification type, the value that was previously in the dictionary for the key.
        /// </summary>
        public TValue ReplacedValue { get; internal set; }
        /// <summary>
        /// For a removed notification type, the value that was removed from the dictionary for the key.
        /// </summary>
        public TValue RemovedValue { get; internal set; }
    }

    public static class DictionaryNotification
    {
        public static DictionaryNotification<TKey, TValue> Initialised<TKey, TValue>()
        {
            return new DictionaryNotification<TKey, TValue>
            {
                Type = DictionaryNotificationType.Initialised
            };
        }

        public static DictionaryNotification<TKey, TValue> Cleared<TKey, TValue>()
        {
            return new DictionaryNotification<TKey, TValue>()
            {
                Type = DictionaryNotificationType.Cleared
            };
        }

        public static DictionaryNotification<TKey, TValue> Existing<TKey, TValue>(TKey key, TValue value)
        {
            return new DictionaryNotification<TKey, TValue>
            {
                Type = DictionaryNotificationType.Existing,
                Key = key,
                Value = value
            };
        }

        public static DictionaryNotification<TKey, TValue> Inserted<TKey, TValue>(TKey key, TValue value)
        {
            return new DictionaryNotification<TKey, TValue>
            {
                Type = DictionaryNotificationType.Inserted,
                Key = key,
                Value = value
            };
        }

        public static DictionaryNotification<TKey, TValue> Missing<TKey, TValue>(TKey key)
        {
            return new DictionaryNotification<TKey, TValue>
            {
                Type = DictionaryNotificationType.Missing,
                Key = key
            };
        }

        public static DictionaryNotification<TKey, TValue> Updated<TKey, TValue>(TKey key, TValue value, TValue updating,
            TValue updated)
        {
            return new DictionaryNotification<TKey, TValue>
            {
                Type = DictionaryNotificationType.Updated,
                Key = key,
                Value = value,
                UpdatingValue = updating,
                UpdatedValue = updated
            };
        }

        public static DictionaryNotification<TKey, TValue> Replaced<TKey, TValue>(TKey key, TValue value,
            TValue replacedValue)
        {
            return new DictionaryNotification<TKey, TValue>
            {
                Type = DictionaryNotificationType.Replaced,
                Key = key,
                Value = value,
                ReplacedValue = replacedValue
            };
        }

        public static DictionaryNotification<TKey, TValue> Removed<TKey, TValue>(TKey key, TValue removedValue)
        {
            return new DictionaryNotification<TKey, TValue>
            {
                Type = DictionaryNotificationType.Removed,
                Key = key,
                RemovedValue = removedValue
            };
        } 
    }
}