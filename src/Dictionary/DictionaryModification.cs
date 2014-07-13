namespace Adaptive.Observables
{
    public sealed class DictionaryModification<TKey, TValue>
    {
        /// <summary>
        /// Only used internally to signal intialisation elegantly.
        /// </summary>
        internal static DictionaryModification<TKey, TValue> Initialised = new DictionaryModification<TKey, TValue>(); 

        public DictionaryModificationType Type { get; internal set; }

        public TKey Key { get; internal set; }

        public TValue Value { get; internal set; }

        public override string ToString()
        {
            return string.Format("Key: {0}, Value: {1}, Type: {2}", Key, Value, Type);
        }
    }

    public static class DictionaryModification
    {
        public static DictionaryModification<TKey, TValue> Upset<TKey, TValue>(TKey key, TValue value)
        {
            return new DictionaryModification<TKey, TValue>
            {
                Key = key,
                Value = value,
                Type = DictionaryModificationType.Upsert
            };
        }

        public static DictionaryModification<TKey, TValue> Replace<TKey, TValue>(TKey key, TValue value)
        {
            return new DictionaryModification<TKey, TValue>
            {
                Key = key,
                Value = value,
                Type = DictionaryModificationType.Replace
            };
        }

        public static DictionaryModification<TKey, TValue> Remove<TKey, TValue>(TKey key)
        {
            return new DictionaryModification<TKey, TValue>
            {
                Key = key,
                Type = DictionaryModificationType.Remove
            };
        }

        public static DictionaryModification<TKey, TValue> Clear<TKey, TValue>()
        {
            return new DictionaryModification<TKey, TValue>
            {
                Type = DictionaryModificationType.Clear
            };
        } 
    }
}