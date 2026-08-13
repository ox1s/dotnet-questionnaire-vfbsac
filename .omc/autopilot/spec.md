# Spec: Update shadcn/ui components to latest upstream + adopt clearly-relevant new components

## Context (research already done via Context7 + live shadcn CLI)

- Project: `src/Web.Client` (Vite + React 19 + TS admin/survey app).
- `components.json`: `style: "radix-lyra"`, `baseColor: "mist"`, plus newer fields (`rtl`, `menuColor`, `menuAccent`, `registries`) confirmed via Context7 (`/websites/ui_shadcn`) to be real, current shadcn schema fields — the project was scaffolded with a recent (Dec 2025+) shadcn CLI/`shadcn create` preset. `"radix-lyra"` is itself an official upstream style name (one of 5 new visual styles: Vega, Nova, Maia, Lyra, Mira — Lyra is "boxy and sharp"), not a custom project invention. Confirmed via `npx shadcn view @shadcn/<name>` — the registry's own file paths are literally `registry/radix-lyra/ui/<name>.tsx`, i.e. our project is on the exact same style base as the live registry.
- Live registry (`npx shadcn search @shadcn`) has 214 items. Currently installed `ui/` components (25): `alert-dialog, alert, avatar, badge, breadcrumb, button, calendar, card, chart, collapsible, dropdown-menu, field, input-group, input, label, popover, select, separator, sheet, sidebar, skeleton, sonner, table, textarea, tooltip` (plus `badge-variants.ts`, `button-variants.ts` — split out from `badge.tsx`/`button.tsx` in a prior session to fix a `react-refresh` lint rule; this split is intentional local structure, NOT something to undo when diffing against upstream).
- Confirmed via `npx shadcn add button --diff -y` that the CLI's diff view is noisy for us: it diffs against the registry's *unsplit* file layout, so it always shows our button-variants.ts split as a fake "diff". Underneath that noise, the actual genuine upstream content changes for `button.tsx` are exactly two Tailwind class tweaks:
  1. `default` variant: `[a]:hover:bg-primary/80` → `hover:bg-primary/80` (upstream dropped the anchor-only hover scoping)
  2. `secondary` variant: `hover:bg-secondary/80` → `hover:bg-[color-mix(in_oklch,var(--secondary),var(--foreground)_5%)]` (upstream moved to an oklch color-mix hover instead of a flat opacity)
  All other variants/sizes in `button.tsx` are byte-identical to upstream already.
- New components confirmed available that we don't have: `accordion, aspect-ratio, button-group, carousel, checkbox, combobox, command, context-menu, dialog, drawer, empty, form, hover-card, input-otp, item, kbd, menubar, native-select, navigation-menu, pagination, progress, radio-group, resizable, scroll-area, slider, switch, tabs, toggle, toggle-group, direction, attachment, bubble, message-scroller, questionnaire, marker, message`.
  - `@shadcn/questionnaire` was checked directly (`npx shadcn view @shadcn/questionnaire`) despite the tempting name match to our domain — it is a generic multi-step-form/AI-elements primitive requiring an extra `@shadcn/react` headless-primitives package plus an app-specific `IconPlaceholder` shim that doesn't exist in this project. Adopting it would mean a survey-taking UX redesign, not a UI component update. **Out of scope, explicitly rejected.**
  - `dialog` was checked against `admin-shared.tsx`'s docs, which explicitly say `AdminModal` is "not Radix Dialog — hand-rolled overlay + `<Card>`" as a deliberate choice. **Out of scope** — swapping the modal primitive is a design decision, not a smell fix.
  - `form` (react-hook-form wrapper) — project doesn't use react-hook-form anywhere (uses plain `useState` + the `useDictionaryCrud` hook). **Out of scope** — would require a form-library adoption, not a component update.
  - `command`, `menubar`, `navigation-menu`, `carousel`, `context-menu`, `resizable`, `input-otp`, `direction`, `attachment`, `bubble`, `message-scroller`, `marker`, `message`, `native-select` — no current use case in this app. **Out of scope.**
  - `empty` — checked (`npx shadcn view @shadcn/empty`): a small, dependency-free set of primitives (`Empty`, `EmptyHeader`, `EmptyMedia`, `EmptyTitle`, `EmptyDescription`, `EmptyContent`). `AdminTable` (in `admin-shared.tsx`) currently renders a plain-text `emptyText` string when a list is empty, used by every admin CRUD page. This is a direct, low-risk, clearly-relevant fit. **In scope.**
  - `button-group`, `combobox`, `item`, `progress`, `slider`, `scroll-area`, `toggle`/`toggle-group`, `checkbox`, `switch`, `tabs`, `pagination`, `accordion`, `hover-card`, `aspect-ratio`, `radio-group`, `kbd` — all technically pluggable somewhere, but none has an existing broken/awkward pattern crying out for it strongly enough to justify autopilot unilaterally restructuring more UI. Flagging as backlog ideas in the final report, not building them now, to keep this run's blast radius bounded and reviewable.

## Scope (2 stories)

### Story A — Update installed shadcn ui/ components to latest upstream content
For each of the 25 currently-installed `src/components/ui/*.tsx` files (skip `badge-variants.ts`/`button-variants.ts` — those are treated as part of `badge.tsx`/`button.tsx` respectively for diffing purposes):
1. Fetch the current upstream source via `npx shadcn view @shadcn/<name>` (ground truth, same technique already validated above).
2. Diff logically against our current combined content (accounting for the button/badge variants-file split — do NOT re-merge them back into one file; that split fixes a real lint rule from a prior session and must be preserved).
3. Apply only genuine upstream content changes (new Tailwind classes/variants, accessibility attribute additions, bug fixes, new exported sub-components). Do NOT apply purely structural/formatting differences that come from our intentional file-splitting, our square-corner (`rounded-none`) Lyra house style (that IS the upstream Lyra style, not a customization — don't "fix" it away), or how imports are organized.
4. Update every consumer if a diff adds/removes an export.

### Story B — Add the `Empty` component and wire it into `AdminTable`'s empty state
1. Add `src/components/ui/empty.tsx` with the exact upstream content from `npx shadcn view @shadcn/empty` (already fetched above, dependency-free).
2. In `src/components/admin/admin-shared.tsx`, find where `AdminTable` renders its `emptyText` prop and replace the plain-text rendering with `<Empty><EmptyHeader><EmptyTitle>{emptyText}</EmptyTitle></EmptyHeader></Empty>` (or an equivalently clean composition — use judgment on whether an `EmptyMedia` icon adds value or is unnecessary noise for a plain "no rows" table state; simplicity wins if unsure).
3. Do NOT change the `emptyText` prop's type/API — every admin page already passes a Russian string (`"Нет нанимателей"`, `"Нет групп"`, etc.); those must keep working unchanged.
4. Verify visually via the dev server that at least one admin list page (e.g. groups, employers) that has an explicit `emptyText` renders the new empty state correctly for an actually-empty list — if impossible to trigger real empty data without a live backend, at minimum confirm via type-checking + a manual code read that the composition is correct.

## Post-implementation notes (added after Phase 4 validation)

- **`cn-*` utility-class skip, logged per the acceptance criterion below:** the live upstream registry's
  radix-lyra base has moved to a `cn-`-prefixed utility layer (`cn-font-heading`, `cn-menu-target`,
  `cn-menu-translucent`, `cn-rtl-flip`, `cn-calendar-caption`, `cn-calendar-dropdown-root`, etc.).
  Confirmed via `node_modules/shadcn/dist/tailwind.css` (this project's installed `shadcn` package is
  4.1.2) that **zero** `cn-*` utilities are defined anywhere in this project's Tailwind bundle or `src/`.
  Porting these class names in would add dead/no-op classes with no visual effect until the project
  upgrades past its current `shadcn` version — so Story A intentionally did NOT introduce `cn-*` classes
  into any of the 14 updated files.
  - The one place a `cn-*` class arrived anyway was the new `empty.tsx` (Story B), which was written
    from a literal `npx shadcn view @shadcn/empty` fetch containing `cn-font-heading` in `EmptyTitle`.
    Since that class is a guaranteed no-op in this project (verified above) and every sibling Title
    component already in this codebase (`CardTitle`, `SheetTitle`, `AlertDialogTitle`) uses the real,
    working `font-heading` token (backed by `--font-heading` in `src/index.css`), `EmptyTitle` was
    normalized to `font-heading` for internal consistency rather than shipping a known-broken class in
    brand-new code. This was caught by Phase 4's code-reviewer pass and confirmed correct by the
    architect pass (both independently verified `cn-*` is undefined project-wide).
  - Existing pre-existing drift on this exact point (not touched by this run, out of scope):
    `alert-dialog.tsx`, `card.tsx`, `sheet.tsx` still use `font-heading` (matches our fix, no drift there);
    `select.tsx`/`breadcrumb.tsx` lack newer `cn-menu-*`/`cn-rtl-flip` classes upstream may have added —
    left alone since those 2 files were outside Story A's per-file audit result set (both came back
    "identical, no change" against what this project's shadcn version actually serves).
- **Visual note for manual eyeballing:** the empty-state text in every `AdminTable` (7 admin list pages)
  changes from `text-muted-foreground text-sm` (grey) to `EmptyTitle`'s `text-sm font-medium` at default
  foreground (darker, semibold) — an intentional consequence of adopting the upstream `Empty` primitive,
  not a bug, but worth a visual check by a human before merging.
- **Out of scope, noted as backlog:** `"use client"` directives (inert, Next.js-only, harmless in this
  Vite app) still remain in `src/components/layout/app-sidebar.tsx`, `nav-projects.tsx`, and
  `team-switcher.tsx` — outside Story A's `components/ui/` scope, cheap follow-up if ever desired.

## Acceptance Criteria

- `npx eslint .` → 0 errors, 0 warnings (whole `src/Web.Client`)
- `npx tsc -b --force --noEmit` → exit 0
- `npm run build` → succeeds
- Every genuine upstream content change identified for Story A is either applied (with the specific before/after noted) or explicitly logged as intentionally skipped with a reason (e.g. "conflicts with our custom X") — no silent no-op "checked everything, nothing to do" without evidence.
- `Empty` component exists at `src/components/ui/empty.tsx`, matches upstream content, has zero new dependencies.
- `AdminTable`'s empty state renders through the new `Empty` primitives; no admin page's existing `emptyText` string or type signature changes.
- No unrelated files touched — this is a UI-primitives update + one composition change, not a broader refactor.
