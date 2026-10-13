# ADR-0001 — Spec-Driven Development as the delivery method

**Status**: Accepted · **Date**: 2026-10-13

## Context

Features built by ad-hoc prompting or undocumented coding drift from intent, are hard to review,
and leave no durable record of *why*. We want AI-assisted delivery that is auditable and repeatable.

## Decision

Adopt **Spec-Driven Development** (GitHub spec-kit). Every feature flows through
`constitution → specify → clarify → plan → tasks → implement → converge`, each phase producing a
Markdown artifact under `specs/<feature>/`. No production code is written before a `spec.md` exists.

## Consequences

- **Positive**: durable intent, reviewable artifacts, consistent AI context, easy onboarding,
  natural traceability to the backlog.
- **Negative**: upfront ceremony per feature; small throwaway spikes may feel heavy (allowed, but the
  spec step is skipped only with an explicit note of increased rework risk).
- Guardrails live in [constitution.md](../../.specify/memory/constitution.md); this is the top-level
  decision all other ADRs serve.
