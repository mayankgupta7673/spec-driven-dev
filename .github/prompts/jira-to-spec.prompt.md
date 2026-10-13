---
mode: agent
description: Read a JIRA issue and seed a spec-kit feature (spec.md) from it, carrying the JIRA key as the feature id. Enriches the issue; never silently overwrites it.
---

# JIRA issue → spec

Turn an existing JIRA issue (Epic / Story / Task) into a spec-kit feature so the spec is grounded
in the backlog. The **JIRA key is the source of truth** and becomes the feature correlation id
across spec → branch → PR.

## Inputs
- A JIRA **issue key** (e.g. `PROJ-1234`) — ask if not provided.
- Access method (detect in this order):
  1. A **JIRA MCP server** if one is configured (preferred) — use its "get issue" / "search issues" tools.
  2. Otherwise the **JIRA REST API**: `GET {JIRA_BASE_URL}/rest/api/3/issue/{key}`
     with auth from env (`JIRA_BASE_URL`, `JIRA_EMAIL`, `JIRA_API_TOKEN`).
     Never print or hard-code the token; read it from the environment only.

## Steps
1. **Read the issue.** Fetch summary, description (ADF/markdown), acceptance criteria field,
   issue type, status, labels, epic link, and sub-tasks/child stories.
   If the key is unknown, search by summary and confirm the match with the user.
2. **Derive the feature id.** Use the JIRA key lowercased, e.g. `proj-1234`. This is the short-name prefix.
3. **Create the feature scaffold** by running the spec-kit script (do not hand-write paths):
   - `.specify/scripts/powershell/create-new-feature.ps1 -Json -ShortName "proj-1234-<slug>" "<issue summary>"`
   - Capture `SPEC_FILE` and `BRANCH_NAME` from the JSON output.
4. **Fill `spec.md`** from the issue, mapping fields:
   - JIRA **Summary** → Feature name.
   - JIRA **Description** → Overview + User Story narrative.
   - JIRA **Acceptance Criteria** → Acceptance Scenarios (Given/When/Then) and Functional Requirements.
   - JIRA **sub-tasks / child stories** → prioritized User Stories (P1/P2/P3).
   - Put the JIRA key and URL in the `**Input**` line so the trace is explicit.
   - Mark anything underspecified as `[NEEDS CLARIFICATION: …]` rather than inventing it.
5. **Preserve the correlation key.** Set the feature id (`proj-1234`) as a marker so downstream
   ADO/GitHub items link back to the originating JIRA key.

## Mandatory gate
- If a spec already exists for this key, **do not overwrite** — show a diff summary and ask whether
  to update, branch a new revision, or abort.
- Do not modify JIRA issue fields. Any write-back (a comment linking the spec) requires explicit approval.

## Done
1. Report: JIRA key → `spec.md` path → branch name.
2. With user approval, add a JIRA comment linking the spec file / branch (via the MCP tool or
   `POST /rest/api/3/issue/{key}/comment`).
3. Suggest next step: `/speckit.plan`.
