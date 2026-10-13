---
mode: agent
description: Map a feature's tasks.md to Azure DevOps work items (Epic → Feature → Story) with full traceability. Requires explicit approval before creating anything.
---

# Tasks → Azure DevOps work items

Convert the current feature's `tasks.md` into an Azure DevOps backlog with traceability back to
the spec. Use the Azure DevOps MCP tools (`wit_work_item_write`, `wit_work_item_link_write`, etc.).

## Inputs
- The active feature directory under `specs/<feature>/` (ask if ambiguous).
- Read `spec.md` (for the Feature/Epic summary) and `tasks.md` (for the Stories).

## Rules
1. **One Epic** per spec (title = feature name, description links the spec file + feature id).
2. **One Feature** per user story group in `tasks.md` (the `US1`, `US2`… groupings).
3. **One Story** per task row. Carry the task id (e.g. `T003`) into the Story title as a prefix.
4. Set the **feature id as a tag** on every work item so spec ↔ ADO ↔ branch ↔ PR all correlate.
5. Preserve dependency order: use `wit_work_item_link_write` with `Predecessor/Successor` links
   for tasks that `tasks.md` marks as dependent; parallel `[P]` tasks get no predecessor link.
6. Acceptance criteria on each Story = the task's description + the relevant requirement from `spec.md`.

## Mandatory gate (do NOT skip)
Before creating ANY work item:
1. Produce a **preview table**: Epic / Feature / Story hierarchy, titles, tags, and links.
2. Ask the user to confirm the target **project** and **area/iteration path**.
3. Wait for explicit approval. Only then call the write tools.

## Output
After creation, print a mapping table: `task id → work item id → URL`, and remind the user to
reference the Story id in the implementing branch name and PR description.
