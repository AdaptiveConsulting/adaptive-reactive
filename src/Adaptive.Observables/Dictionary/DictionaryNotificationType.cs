using System;

namespace Adaptive.Observables
{
    [Flags]
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
        /// Indicates that the dictionary has been cleared.
        /// </summary>
        Cleared = 8,
        /// <summary>
        /// Indicates that this item, retrieved via a key, existed when it was looked up.
        /// </summary>
        Existing = 16,
        /// <summary>
        /// Indicates that this item was inserted for the first time into the dictionary.
        /// </summary>
        Inserted = 32,
        /// <summary>
        /// Indicates that this item is an update for an earlier version of the key in the dictionary.
        /// </summary>
        Updated = 64,
        /// <summary>
        /// Indicates that this item replaced an earlier version of the key in the dictionary.
        /// </summary>
        Replaced = 128,
        /// <summary>
        /// A flag mask that returns only Initialised, Missing, Removed and Cleared notification types.
        /// These are meta notifications that do not contain the current value of the dictionary for the specific key.
        /// </summary>
        Meta = Initialised | Missing | Removed | Cleared,
        /// <summary>
        /// A flag mask that returns only Existing, Inserted, Updated and Replaced notification types.
        /// These are valued notifications that contain the current value of the dictionary for the specific key.
        /// </summary>
        Values = Existing | Inserted | Updated | Replaced
    }
}