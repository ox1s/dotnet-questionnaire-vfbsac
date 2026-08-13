# Implementation Plan

Two independent stories, no file overlap — run in parallel.

**Story A** (Opus, high judgment): audit + update all 25 installed `src/components/ui/*.tsx` files
against `npx shadcn view @shadcn/<name>` ground truth. Apply only genuine upstream content deltas.
Preserve the `button-variants.ts`/`badge-variants.ts` split (react-refresh fix from a prior session).
Preserve the Lyra square-corner (`rounded-none`) style — that's upstream, not local customization.

**Story B** (Sonnet, contained): add `src/components/ui/empty.tsx` (upstream content, already fetched,
zero deps) and wire it into `AdminTable`'s empty-state render in `src/components/admin/admin-shared.tsx`
(prop `emptyText` at line 69/218, rendered at line 263). Keep the `emptyText?: string` API unchanged.

Verification for both: `npx eslint .`, `npx tsc -b --force --noEmit`, `npm run build` from `src/Web.Client`.
