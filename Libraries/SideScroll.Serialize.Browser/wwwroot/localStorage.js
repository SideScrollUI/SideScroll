// Browser localStorage helper for SideScroll.Serialize.Browser
// Provides synchronous localStorage operations via JSImport
export const BrowserStorage = {
    /**
     * Saves data to localStorage
     * @param {string} key - The storage key
     * @param {string} jsonData - JSON string to store
     * @returns {boolean} - Success status
     */
    save: function(key, jsonData) {
        try {
            localStorage.setItem(key, jsonData);
            console.log(`✓ Saved to localStorage: ${key} (${jsonData.length} bytes)`);
            return true;
        } catch (e) {
            console.error(`Failed to save to localStorage: ${key}`, e);
            return false;
        }
    },

    /**
     * Loads data from localStorage
     * @param {string} key - The storage key
     * @returns {string|null} - JSON string or null if not found
     */
    load: function(key) {
        try {
            const data = localStorage.getItem(key);
            if (data) {
                console.log(`✓ Loaded from localStorage: ${key} (${data.length} bytes)`);
            } else {
                console.log(`No data found in localStorage for key: ${key}`);
            }
            return data;
        } catch (e) {
            console.error(`Failed to load from localStorage: ${key}`, e);
            return null;
        }
    },

    /**
     * Checks if a key exists in localStorage
     * @param {string} key - The storage key
     * @returns {boolean} - True if key exists
     */
    exists: function(key) {
        return localStorage.getItem(key) !== null;
    },

    /**
     * Removes data from localStorage
     * @param {string} key - The storage key
     * @returns {boolean} - Success status
     */
    remove: function(key) {
        try {
            localStorage.removeItem(key);
            console.log(`✓ Removed from localStorage: ${key}`);
            return true;
        } catch (e) {
            console.error(`Failed to remove from localStorage: ${key}`, e);
            return false;
        }
    },

    /**
     * Gets all keys from localStorage with a specific prefix
     * @param {string} prefix - Key prefix to filter by
     * @returns {string[]} - Array of matching keys
     */
    getKeys: function(prefix) {
        const keys = [];
        for (let i = 0; i < localStorage.length; i++) {
            const key = localStorage.key(i);
            if (key && (!prefix || key.startsWith(prefix))) {
                keys.push(key);
            }
        }
        return keys;
    },

    /**
     * Gets all keys from localStorage with a specific prefix as JSON string
     * Required by SerializerLocalStorage.GetAllKeys()
     * @param {string} prefix - Key prefix to filter by
     * @returns {string} - JSON array of matching keys
     */
    getKeysJson: function(prefix) {
        const keys = this.getKeys(prefix);
        return JSON.stringify(keys);
    },

    /**
     * Gets storage statistics
     * @returns {object} - Storage stats including itemCount, estimatedSize, estimatedSizeMB
     */
    getStats: function() {
        let totalSize = 0;
        for (let i = 0; i < localStorage.length; i++) {
            const key = localStorage.key(i);
            if (key) {
                const value = localStorage.getItem(key);
                if (value) {
                    totalSize += key.length + value.length;
                }
            }
        }
        return {
            itemCount: localStorage.length,
            estimatedSize: totalSize,
            estimatedSizeMB: (totalSize / (1024 * 1024)).toFixed(2)
        };
    }
};

// Export to globalThis for JSImport access
globalThis.BrowserStorage = BrowserStorage;
