---
mode: agent
description: Read an Azure DevOps work item and seed a spec-kit feature (spec.md) from it, carrying the work-item id as the feature id. Enriches the item; never silently overwrites it.
---

# Azure DevOps work item → spec

Turn an existing ADO work item (Epic / Feature / User Story / PBI) into a spec-kit feature so the
spec is grounded in the backlog, not re-typed. The **work-item id is the source of truth** and
becomes the feature correlation id across spec → branch → PR.

## Inputs
- An ADO **work item id** (ask if not provided).
- Optional: target project / area path if not discoverable from the item.

## Tools
Use the Azure DevOps MCP tools:
- `wit_work_item` (or `wit_get_work_item`) to read the item: title, description, acceptance
  criteria, tags, parent/child links, state.
- `search_workitem` if only a keyword/title is known and the id must be found first.
- `wit_work_item_comment_write` to round-trip a link back to the generated spec (see Done).

## Steps
1. **Read the item.** Fetch title, description, acceptance criteria, type, state, parent, and children.
   If the id is ambiguous or missing, use `search_workitem` and confirm the match with the user.
2. **Derive the feature id.** Use the ADO id, e.g. `ado-12345`. This is the feature short name prefix.
3. **Create the feature scaffold** by running the spec-kit script (do not hand-write paths):
   - `.specify/scripts/powershell/create-new-feature.ps1 -Json -ShortName "ado-12345-<slug>" "<work item title>"`
   - Capture `SPEC_FILE` and `BRANCH_NAME` from the JSON output.
4. **Fill `spec.md`** from the work item, mapping fields:
   - ADO **Title** → Feature name.
   - ADO **Description** → Overview + User Story narrative.
   - ADO **Acceptance Criteria** → Acceptance Scenarios (Given/When/Then) and Functional Requirements.
   - ADO **child stories** → prioritized User Stories (P1/P2/P3) in `spec.md`.
   - Put the ADO id and URL in the `**Input**` line so the trace is explicit.
   - Mark anything underspecified as `[NEEDS CLARIFICATION: …]` rather than inventing it.
5. **Preserve the correlation key.** Set the feature id (`ado-12345`) as a tag/marker so later
   `tasks-to-ado` links new child stories back under the same item.

## Mandatory gate
- If the work item already has a linked spec (a prior comment/attachment), **do not overwrite** —
  show a diff summary and ask whether to update, branch a new revision, or abort.
- Do not modify the ADO item's fields. The only write-back is the comment in Done (with approval).

## Done
1. Report: ADO id → `spec.md` path → branch name.
2. With user approval, post a comment on the work item via `wit_work_item_comment_write` linking the
   spec file / branch so the backlog points at the spec.
3. Suggest next step: `/speckit.plan`.
