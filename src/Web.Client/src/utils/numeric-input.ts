import type { ClipboardEvent, KeyboardEvent } from "react";

const ALLOWED_CONTROL_KEYS = new Set([
  "Backspace",
  "Delete",
  "ArrowLeft",
  "ArrowRight",
  "ArrowUp",
  "ArrowDown",
  "Tab",
  "Home",
  "End",
  "Enter",
]);

// input type="number" still lets the browser accept e/E/+/-/. (scientific
// notation syntax), so digit-only fields need explicit key/paste filtering.
export function blockNonDigitKeydown(e: KeyboardEvent<HTMLInputElement>) {
  if (e.ctrlKey || e.metaKey || e.altKey) return;
  if (ALLOWED_CONTROL_KEYS.has(e.key)) return;
  if (!/^[0-9]$/.test(e.key)) {
    e.preventDefault();
  }
}

export function sanitizeDigitsOnly(raw: string): string {
  return raw.replace(/[^0-9]/g, "");
}

export function digitsOnlyFromClipboard(e: ClipboardEvent<HTMLInputElement>): string {
  e.preventDefault();
  return sanitizeDigitsOnly(e.clipboardData.getData("text"));
}
