# Gece Mavisi Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Apply the approved TEHYS-style midnight theme to the existing app and installer, preserve all local records, and open the updated app.

**Architecture:** Shared WPF resources own colors and input/table templates. Existing screens keep bindings, actions, navigation and calculations. Auxiliary windows use the same resources; reports retain their print palette.

**Tech Stack:** .NET 10 WPF, Inno Setup 6.6+.

**Spec:** docs/superpowers/specs/2026-11-05-setup-midnight-preview.md (user approved implementation on 2026-11-05).

## Global Constraints

- No changes to record serialization, payroll calculations, stored settings or vault cryptography.
- Preserve installed AppId and data paths; package contains only program and public documentation/license.
- Never open the real encrypted vault for testing; use synthetic fixtures.

## Review Focus

- Selected table and combo rows must remain readable.
- Disabled and validation states must remain distinguishable.
- All auxiliary windows must use dark surfaces without recoloring PDFs.
- Date, occupation and combo popups must remain usable.
- Installation and launch must preserve existing data hashes.

### Task 1: Shared theme and screen colors

- [x] Add focused WPF theme checks and observe baseline failure.
- [x] Apply approved palette, inputs, tables and actions; preserve screen geometry/bindings.
- [x] Check named screens, popups, selections and binding errors using synthetic data.

### Task 2: Installer and package

- [x] Verify available compiler supports modern dark polar.
- [x] Build version 1.0.14 app and data-free installer, preserve existing installation identity.
- [x] Inspect real installer welcome and shortcut pages.

### Task 3: Verification and local installation

- [x] Run calculation/storage unit suite and full WPF smoke, review screenshots.
- [x] Review diff for data/report behavior changes.
- [x] Install, compare original data hashes and open the updated app.

## Execution ledger

Initial backup recorded under artifacts/midnight; user data is outside repository.
Ruling: use a dedicated codex/gece-mavisi branch in the existing checkout, keeping the already prepared installer edits together. No parallel implementation agents.

Task 1 complete: 315 unit tests passed. Full WPF smoke passed after explicit derived Window styles and preview harness adjustment; no binding errors. Focused theme checks passed for actual row contrast, combo/list selected states, seven calendar day headers, month/year/decade navigation, right-arrow date selection and DatePicker propagation. Real data/vault not used in UI fixtures.

Fresh reviewer found HybridGrid local cell foreground overriding row colors. Added actual-cell contrast regression (observed fail, then pass) for normal/starter and selected/unselected rows; cell style now inherits ModernDataGridCell. Re-review reported no remaining important issues. Calendar-specific tests additionally caught and fixed missing year-view visibility triggers.

Task 2 complete: Inno Setup 6.7.3 compiled the dark-polar installer successfully; actual welcome, shortcut and finish screens inspected. Portable ZIP verified against three-file allowlist; installer keeps the same AppId and install path.

Task 3: installation completed; installed EXE matches packaged EXE. All 82 pre-existing data/output/vault file hashes unchanged after installation. Data compatibility checked on a private local copy: 24 teachers, 25 monthly entries, four archived payrolls readable. User additionally authorized publishing the data-free stable release to GitHub.
