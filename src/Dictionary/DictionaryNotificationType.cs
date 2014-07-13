namespace Adaptive.Observables
{
    public enum DictionaryNotificationType
    {
        /// <summary>
        /// Indicates hat the initialisation stream has completed.
        /// </summary>
        Initialised,
        /// <summary>
        /// Indicates that the dictionary has been cleared.
        /// </summary>
        Cleared,
        /// <summary>
        /// Indicates that this item, retrieved via a key, existed when it was looked up.
        /// </summary>
        Existing,
        /// <summary>
        /// Indicates that this item was inserted for the first time into the dictionary.
        /// </summary>
        Inserted,
        /// <summary>
        /// Indicates that this item, retrieved via a key, was not found in the dictionary when it was looked up.
        /// </summary>
        Missing,
        /// <summary>
        /// Indicates that this item is an update for an earlier version of the key in the dictionary.
        /// </summary>
        Updated,
        /// <summary>
        /// Indicates that this item replaced an earlier version of the key in the dictionary.
        /// </summary>
        Replaced,
        /// <summary>
        /// Indicates that the item associated with this key has been removed from the dictionary.
        /// </summary>
        Removed
    }
}