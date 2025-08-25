/**
 * This is a subset of the main `ConvertToCsv.ts` script of json-editor.
 * If you need a CSV parser for nested objects, DO NOT use this since support for binary data has been removed. Instead, use the one you can find at https://github.com/dinoosauro/json-editor/blob/main/src/Scripts/ConvertToCsv.ts
 */

/**
 * Convert a parsed array to a CSV FIle
 * @returns the CSV string
 */
function convertToCsv({ headers, entries }) {
    let str = `\"${headers.map(item => csvCell(item)).join("\",\"")}\"\n`;
    for (const entry of entries) {
        str += `\"${entry.map(item => csvCell(item)).join("\",\"")}\"\n`;
    }
    return str;
}
/**
 * Escape characters for the CSV cell
 * @param {string} str the string to parse
 * @returns the parsed string
 */
function csvCell(str) {
    return String(typeof str === "undefined" ? "" : str).replace(/\"/g, "\"\"");
}

/**
 * 
 * @param {any} obj the array of objects that should be added in the list
 * @param {string?} pre the position of the current object in the main object. 
 * @param {boolean?} disableArray if the array should be read
 * 
 * Example: `a: "a", b: {d: "d", c: "c"}`
 * 
 * When handling the object `b`, the `pre` value must be `b.`, since we're reading the content inside that nested object.
 * @param disableArray if Arrays should be elaborated or not
 * @returns an object, with the headers of the table and the other rows.
 */
function convertToArray(obj, pre, disableArray) {
    /**
     * The headers of the output table
     * @type string[]
     */
    const headerArr = [];
    /**
     * An array of the rows of the table
     * @type string[][]
     */
    const entryArr = [];
    /**
     * 
     * @param {any} object the object that should be analyzed
     * @param {string} originalKey the key used from the main Object to access it.
     * 
     * Example: `a: "a", b: {d: "d", c: "c"}`
     * 
     * If the object `b` is being accessed, the originalKey must be `b`
     * @param {boolean} disableArr if the script should treat arrays as normal objects.
     */
    function handleObjects(object, originalKey, disableArr) {
        const { headers, entries } = convertToArray([object], `${pre ? `${pre}` : ""}${originalKey}.`, disableArr); // Let's elaborate this new object
        for (const header of headers) { // Let's add a new header if it's a new entry
            if (headerArr.indexOf(header) === -1) headerArr.push(header);
        }
        for (let i = 0; i < entries[0].length; i++) { // And let's add the new entries. Since we're processing one item at the time, we are sure that the entries array contains only one item.
            entryArr[entryArr.length - 1][headerArr.indexOf(headers[i])] = entries[0][i];
        }
    }
    for (const object of obj) { // `object` becomes the object of the current row
        entryArr[entryArr.length] = [];
        for (let key in object) {
            /**
             * The key used to access the current object
             */
            let originalKey = `${key}`;
            /**
             * The _path_ that permits to access the current object
             */
            key = `${pre ?? ""}${key}`;
            if (typeof object[originalKey] === "function") {
                // Don't do anything
            } else if (!disableArray && Array.isArray(object[originalKey])) { // Read the content inside the array
                if (headerArr.indexOf(key) === -1 && object[originalKey].length < 2) headerArr.push(key); // If the length of the array is composed of 2 or more items, its key will be added later
                switch (object[originalKey].length) {
                    case 0: // Add a blank string
                        entryArr[entryArr.length - 1][headerArr.indexOf(key)] = "";
                        break;
                    case 1: { // Add the single file as the content of that ID
                        if (typeof object[originalKey][0] === "object") {
                            handleObjects(object[originalKey][0], originalKey, true)
                        } else {
                            entryArr[entryArr.length - 1][headerArr.indexOf(key)] = object[originalKey].toString()
                        };
                        break;
                    }
                    default: { // We'll have to handle an actual array
                        let [isObject, notObject] = [[], []];
                        for (const item of object[originalKey]) { // Let's categorize all the items inside the array (so, if they're objects or not)
                            (typeof item === "object" && item !== null ? isObject : notObject).push(item);
                        }
                        /**
                         * Let's transform the isObject array, so that we have an object with as a key the header name, and as a value the array of possible options
                         * Example: `[{a: "a", b: "b"}, {a: "c", b: "d"}]` becomes `{a: ["a", "c"], b: ["b", "d"]}`
                         */
                        isObject = isObject.reduce((entry, obj) => {
                            Object.keys(obj).forEach(key => {
                                entry[key] = [...(entry[key] ?? []), obj[key]];
                            });
                            return entry;
                        }, {});
                        for (const object in isObject) { // Now let's add them to the object storage
                            const newKey = `${key}.${object}`
                            if (headerArr.indexOf(newKey) === -1) headerArr.push(newKey);
                            entryArr[entryArr.length - 1][headerArr.indexOf(newKey)] = JSON.stringify(isObject[object]);
                        }
                        if (notObject.length !== 0) { // Let's add also non-objects to the entry storage
                            if (headerArr.indexOf(key) === -1) headerArr.push(key);
                            entryArr[entryArr.length - 1][headerArr.indexOf(key)] = JSON.stringify(notObject);
                        }
                        break;
                    }
                }
            } else if (typeof object[originalKey] === "object") { // Check if the object is an Uint8Array and, in that case, add it only if the user wants so. Otherwise, let's read the content of that object
                handleObjects(object[originalKey], originalKey, false); 
            } else { // Content that can be easily converted using a string
                if (headerArr.indexOf(key) === -1) headerArr.push(key);
                entryArr[entryArr.length - 1][headerArr.indexOf(key)] = object[originalKey]?.toString();
            }
        }
    }
    return {
        headers: headerArr,
        entries: entryArr
    };
}
