import { describe, it, expect } from "vitest";

// The sidebar only keeps its expanded groups because AppShell is the single
// place that mounts AdminLayout — React Router leaves a layout route's element
// alone while you navigate between its children. A page that wraps itself in
// its own layout quietly reintroduces the remount, and no behavioural test
// would fail, so guard the invariant structurally.
//
// Imports are checked rather than JSX so that prose mentioning the component
// (in a comment, say) doesn't trip the test.
const ALLOWED = ["/src/components/layout/app-shell.tsx"];
const IMPORTS_ADMIN_LAYOUT = /import\s*\{[^}]*\bAdminLayout\b[^}]*\}\s*from/;

const sources = import.meta.glob("/src/**/*.{ts,tsx}", {
  query: "?raw",
  import: "default",
  eager: true,
}) as Record<string, string>;

describe("AdminLayout mounting", () => {
  it("is mounted only by AppShell", () => {
    const offenders = Object.entries(sources)
      .filter(([path]) => !path.endsWith(".test.ts") && !path.endsWith(".test.tsx"))
      .filter(([, source]) => IMPORTS_ADMIN_LAYOUT.test(source))
      .map(([path]) => path)
      .filter((path) => !ALLOWED.includes(path));

    expect(offenders).toEqual([]);
  });

  it("scans the whole source tree", () => {
    // A glob that silently stopped matching would make the check above vacuous.
    // Assert against known files rather than a count, so growth can't weaken it.
    const paths = Object.keys(sources);
    expect(paths).toContain("/src/App.tsx");
    expect(paths).toContain("/src/components/admin/admin-shared.tsx");
    expect(paths).toContain("/src/utils/auth.ts");
  });
});
