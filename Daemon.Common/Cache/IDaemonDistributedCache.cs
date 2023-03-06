namespace Daemon.Common.Cache
{
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.EntityFrameworkCore.Storage;
    using System;
    using System.Collections.Generic;
    public interface IDaemonDistributedCache : IDistributedCache
    {
        /// <summary>
        ///  Determines whether the specified key exsits.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns><see langword="true"/> if key exists else <see langword="false"/>.</returns>
        bool Exists(string key);

        /// <summary>
        ///  Determines whether the specified field in the hash stored at key exsits.
        /// </summary>
        /// <param name="key">The key of the hash.</param>
        /// <param name="field">The field to in the hash to check for.</param>
        /// <returns><see langword="true"/> if key and field exist else <see langword="false"/>.</returns>
        bool HashExists<T>(string key, string field);

        /// <summary>
        /// Get all fields and values of the hash stored at key.
        /// Requires all value types to be homogeneous.
        /// </summary>
        /// <param name="key">The key of the hash to get all entries from.</param>
        /// <typeparam name="T">Type of the field values.</typeparam>
        /// <returns>Dictionary of hash fields or an empty collection if key does not exist.</returns>
        Dictionary<string, T> HashGetAll<T>(string key);

        /// <summary>
        /// Returns the values associated with the specified fields in the hash stored at key.
        /// For every field that does not exist in the hash, a null value is returned.
        /// If key does not exist, a list of null values is returned.
        /// Requires all value types to be homogeneous.
        /// </summary>
        /// <param name="key">The key of the hash.</param>
        /// <param name="fields">The fields in the hash to get.</param>
        /// <typeparam name="T">Type of the field values.</typeparam>
        /// <returns>List of deserialized field values in the order they were requested.</returns>
        List<T> HashGet<T>(string key, List<string> fields);

        /// <summary>
        /// Returns the value associated with the specified field in the hash stored at key.
        /// </summary>
        /// <param name="key">The key of the hash.</param>
        /// <param name="field">The field in the hash to get.</param>
        /// <typeparam name="T">Type of the field values.</typeparam>
        /// <returns>Deserialized field value or null if key or field does not exist.</returns>
        T HashGet<T>(string key, string field);

        /// <summary>
        /// Sets the specified fields to their respective values in the hash stored at key.
        /// This command overwrites any specified fields that already exist in the hash, leaving other unspecified fields untouched.
        /// If key does not exist, a new key holding a hash is created.
        /// Requires all value types to be homogeneous.
        /// </summary>
        /// <param name="key">The key of the hash.</param>
        /// <param name="entries">The entries to set in the hash.</param>
        /// <typeparam name="T">The type of the hash field values.</typeparam>
        void HashSet<T>(string key, Dictionary<string, T> entries);

        /// <summary>
        /// Sets field in the hash stored at key to value.
        /// If key does not exist, a new key holding a hash is created.
        /// If field already exists in the hash, it is overwritten.
        /// </summary>
        /// <param name="key">The key of the hash.</param>
        /// <param name="field">The field to set in the hash.</param>
        /// <param name="value">The value to set.</param>
        /// <typeparam name="T">Type of the field value.</typeparam>
        void HashSet<T>(string key, string field, T value);

        /// <summary>
        /// Removes the specified fields from the hash stored at key.
        /// Non-existing fields are ignored. Non-existing keys are treated as empty hashes and this command returns 0.
        /// </summary>
        /// <param name="key">The key of the hash.</param>
        /// <param name="fields">The fields of the hash to delete.</param>
        /// <typeparam name="T">Type of the hash field value.</typeparam>
        /// <returns>The number of fields that were removed.</returns>
        long HashDelete<T>(string key, IEnumerable<string> fields);

        /// <summary>
        /// Removes the specified field from the hash stored at key.
        /// Non-existing fields are ignored. Non-existing keys are treated as empty hashes and this command returns false.
        /// </summary>
        /// <param name="key">The key of the hash.</param>
        /// <param name="field">The field of the hash to delete.</param>
        /// <typeparam name="T">Type of the hash field value.</typeparam>
        /// <returns>True if field was found and removed, false otherwise.</returns>
        bool HashDelete<T>(string key, string field);

        // IDatabase GetDatabase(int db = -1, object asyncState = null);
    }
}
