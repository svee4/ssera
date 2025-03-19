
/**
 * Gets the string keys for an enum backed by a number
 * Does NOT work for enums backed by a string
 * @param enumType 
 * @returns 
 */
export const getEnumKeys = (enumType: any): string[] => Object.values(enumType).filter(v => typeof v === "string");

export const getEnumEntries = <T>(enumType: any): [string, T][] => getEnumKeys(enumType).map(key => [key, enumType[key]]);