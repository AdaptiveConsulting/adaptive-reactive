using System;

namespace Adaptive.Observables
{
    public enum DictionaryNotificationType
    {
        /// <summary>
        /// Indicates that the initialisation stream has completed, or that all existing items have been yielded.
        /// </summary>
        Initialised = 1,
        /// <summary>
        /// Indicates that this item, retrieved via a key, was not found in the dictionary when it was looked up.
        /// </summary>
        Missing = 2,
        /// <summary>
        /// Indicates that the item associated with this key has been removed from the dictionary.
        /// </summary>
        Removed = 4,
        /// <summary>
        /// Indicates that a specific key has been removed as a result of the dictionary being cleared.
        /// </summary>
        KeyCleared = 8,
        /// <summary>
        /// Indicates that the dictionary as a whole has been cleared.
        /// </summary>
        DictionaryCleared = 16,
        /// <summary>
        /// Indicates that this item, retrieved via a key, existed when it was looked up.
        /// </summary>
        Existing = 32,
        /// <summary>
        /// Indicates that this item was inserted for the first time into the dictionary.
        /// </summary>
        Inserted = 64,
        /// <summary>
        /// Indicates that this item is an update for an earlier version of the key in the dictionary.
        /// </summary>
        Updated = 128,
        /// <summary>
        /// Indicates that this item replaced an earlier version of the key in the dictionary.
        /// </summary>
        Replaced = 256,
        /// <summary>
        /// A flag mask that returns only Initialised, Missing, Removed, KeyCleared and DictionaryCleared notification types.
        /// These are meta notifications that do not contain the current value of the dictionary for the specific key.
        /// </summary>
        Meta = Initialised | Missing | Removed | KeyCleared | DictionaryCleared,
        /// <summary>
        /// A flag mask that returns only Existing, Inserted, Updated and Replaced notification types.
        /// These are valued notifications that contain the current value of the dictionary for the specific key.
        /// </summary>
        Values = Existing | Inserted | Updated | Replaced
    }
}