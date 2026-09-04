/** Minimal class-name joiner. Falsy values are dropped. */
export function cn(...values: (string | false | null | undefined)[]): string {
  return values.filter((value): value is string => Boolean(value)).join(' ');
}
