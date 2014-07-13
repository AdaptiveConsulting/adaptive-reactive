namespace Adaptive.Observables
{
    public enum DictionaryModificationType
    {
        /// <summary>
        /// Indicates the specified key is being updated or inserted with the specified value.
        /// </summary>
        Upsert,
        /// <summary>
        /// Indicates that the new value should be used to replace any existing value for the specified key.
        /// </summary>
        Replace,
        /// <summary>
        /// Indicates that any existing value for the specified key should be removed.
        /// </summary>
        Remove,
        /// <summary>
        /// Indicates that the entire dictionary should be cleared.
        /// </summary>
        Clear
    }
}