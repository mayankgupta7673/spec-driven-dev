"""Generate an editable 16:9 PowerPoint of the Spec-Driven Development deck.

Run: python build_pptx.py  (writes spec-driven-development.pptx next to this file)
Mirrors docs/presentation/spec-driven-development.html.
"""
from pathlib import Path
from pptx import Presentation
from pptx.util import Inches, Pt, Emu
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN, MSO_ANCHOR
from pptx.enum.shapes import MSO_SHAPE
from pptx.oxml.ns import qn

# ---- palette (matches the HTML) ----
BG     = RGBColor(0x0B, 0x10, 0x20)
CARD   = RGBColor(0x16, 0x1F, 0x3D)
CARD2  = RGBColor(0x12, 0x21, 0x3F)
INK    = RGBColor(0xE8, 0xED, 0xFF)
MUTED  = RGBColor(0x9F, 0xB0, 0xD8)
BRAND  = RGBColor(0x6E, 0xA8, 0xFE)
BRAND2 = RGBColor(0x7A, 0xF0, 0xC8)
ACCENT = RGBColor(0xFF, 0xD1, 0x66)
DANGER = RGBColor(0xFF, 0x6B, 0x8B)
LINE   = RGBColor(0x28, 0x34, 0x5C)
FONT   = "Segoe UI"
MONOFT = "Consolas"

prs = Presentation()
prs.slide_width = Inches(13.333)
prs.slide_height = Inches(7.5)
BLANK = prs.slide_layouts[6]
EW, EH = prs.slide_width, prs.slide_height


def slide():
    s = prs.slides.add_slide(BLANK)
    s.background.fill.solid()
    s.background.fill.fore_color.rgb = BG
    return s


def _set(p, text, size, color, bold=False, align=PP_ALIGN.LEFT, font=FONT, spacing=1.0):
    p.alignment = align
    p.line_spacing = spacing
    r = p.add_run()
    r.text = text
    r.font.size = Pt(size)
    r.font.bold = bold
    r.font.name = font
    r.font.color.rgb = color
    return r


def box(s, l, t, w, h, text, size, color, bold=False, align=PP_ALIGN.LEFT,
        anchor=MSO_ANCHOR.TOP, font=FONT, spacing=1.0):
    tb = s.shapes.add_textbox(l, t, w, h)
    tf = tb.text_frame
    tf.word_wrap = True
    tf.vertical_anchor = anchor
    tf.margin_left = tf.margin_right = Pt(0)
    tf.margin_top = tf.margin_bottom = Pt(0)
    _set(tf.paragraphs[0], text, size, color, bold, align, font, spacing)
    return tb


def eyebrow(s, text):
    box(s, Inches(0.9), Inches(0.6), Inches(11), Inches(0.4),
        text.upper(), 13, BRAND2, bold=True)


def title(s, text, color=INK, y=1.0):
    box(s, Inches(0.9), Inches(y), Inches(11.5), Inches(1.1),
        text, 34, color, bold=True)


def rrect(s, l, t, w, h, fill=CARD, line=LINE, line_w=1.0, radius=0.08):
    shp = s.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, l, t, w, h)
    shp.fill.solid(); shp.fill.fore_color.rgb = fill
    shp.line.color.rgb = line; shp.line.width = Pt(line_w)
    shp.shadow.inherit = False
    try:
        shp.adjustments[0] = radius
    except Exception:
        pass
    return shp


def card(s, l, t, w, h, head, body, accent=None, head_color=INK):
    c = rrect(s, l, t, w, h)
    if accent:
        bar = s.shapes.add_shape(MSO_SHAPE.RECTANGLE, l, t, w, Pt(4))
        bar.fill.solid(); bar.fill.fore_color.rgb = accent
        bar.line.fill.background(); bar.shadow.inherit = False
    tf = c.text_frame
    tf.word_wrap = True
    tf.margin_left = tf.margin_right = Inches(0.18)
    tf.margin_top = tf.margin_bottom = Inches(0.16)
    _set(tf.paragraphs[0], head, 15, head_color, bold=True)
    if body:
        p = tf.add_paragraph(); p.space_before = Pt(6)
        _set(p, body, 12, MUTED, spacing=1.1)
    return c


def bullets(s, l, t, w, h, items, size=16, gap=10):
    tb = s.shapes.add_textbox(l, t, w, h)
    tf = tb.text_frame; tf.word_wrap = True
    for i, (lead, rest, ok) in enumerate(items):
        p = tf.paragraphs[0] if i == 0 else tf.add_paragraph()
        p.space_after = Pt(gap)
        mark = "✔  " if ok else "✘  "
        r = p.add_run(); r.text = mark
        r.font.size = Pt(size); r.font.name = FONT; r.font.bold = True
        r.font.color.rgb = BRAND2 if ok else DANGER
        r2 = p.add_run(); r2.text = lead + "  "
        r2.font.size = Pt(size); r2.font.name = FONT; r2.font.bold = True
        r2.font.color.rgb = INK
        if rest:
            r3 = p.add_run(); r3.text = rest
            r3.font.size = Pt(size); r3.font.name = FONT
            r3.font.color.rgb = MUTED
    return tb


def mono(s, l, t, w, h, lines):
    c = rrect(s, l, t, w, h, fill=RGBColor(0x0D, 0x14, 0x30))
    tf = c.text_frame; tf.word_wrap = True
    tf.margin_left = tf.margin_right = Inches(0.2)
    tf.margin_top = tf.margin_bottom = Inches(0.16)
    for i, (text, col) in enumerate(lines):
        p = tf.paragraphs[0] if i == 0 else tf.add_paragraph()
        p.space_after = Pt(3)
        _set(p, text, 13, col, font=MONOFT)
    return c


def arrow(s, l, t, w, h=Inches(0.4), color=BRAND):
    a = s.shapes.add_shape(MSO_SHAPE.RIGHT_ARROW, l, t, w, h)
    a.fill.solid(); a.fill.fore_color.rgb = color
    a.line.fill.background(); a.shadow.inherit = False
    return a


def step(s, l, t, w, h, n, tt, sub, hi=False):
    c = rrect(s, l, t, w, h, line=(BRAND2 if hi else LINE), line_w=(1.5 if hi else 1.0))
    tf = c.text_frame; tf.word_wrap = True
    tf.margin_left = tf.margin_right = Inches(0.12); tf.margin_top = Inches(0.1)
    _set(tf.paragraphs[0], n, 10, (ACCENT if hi else BRAND2), bold=True)
    p = tf.add_paragraph(); _set(p, tt, 13, INK, bold=True)
    p2 = tf.add_paragraph(); _set(p2, sub, 10, MUTED)
    return c


# ============ 1 TITLE ============
s = slide()
rrect(s, Inches(0.9), Inches(1.6), Inches(5.0), Inches(0.5),
      fill=RGBColor(0x10, 0x2A, 0x2A), line=RGBColor(0x2E, 0x6E, 0x5E), radius=0.5)
box(s, Inches(1.1), Inches(1.63), Inches(4.8), Inches(0.44),
    "●  Spec-Kit · GitHub Copilot · Azure Functions", 12, BRAND2, anchor=MSO_ANCHOR.MIDDLE)
box(s, Inches(0.9), Inches(2.35), Inches(10), Inches(0.4),
    "A BETTER WAY TO BUILD SOFTWARE WITH AI", 14, BRAND2, bold=True)
box(s, Inches(0.85), Inches(2.8), Inches(11.5), Inches(2.2),
    "Spec-Driven Development", 60, INK, bold=True, spacing=0.95)
box(s, Inches(0.9), Inches(4.9), Inches(9.5), Inches(1.4),
    "Define what and why before deciding how. Turn requirements into a living spec, "
    "a plan, and tasks — then let AI implement against those artifacts, not a throwaway prompt.",
    18, MUTED, spacing=1.2)

# ============ 2 PROBLEM ============
s = slide(); eyebrow(s, "The problem"); title(s, '"Vibe coding" with AI doesn\'t scale')
bullets(s, Inches(0.9), Inches(2.3), Inches(6.3), Inches(4), [
    ("Intent evaporates.", "A prompt is typed, code appears, the \u201cwhy\u201d is gone.", False),
    ("Nothing to review.", "Reviewers see code with no spec to check it against.", False),
    ("Inconsistent output.", "The same ask yields different results every run.", False),
    ("No traceability.", "You can't link a change back to a requirement or ticket.", False),
], size=17, gap=16)
card(s, Inches(7.6), Inches(2.3), Inches(4.8), Inches(3.4),
     "Prompt → Code",
     "The gap between a one-line prompt and production code is filled with the model's "
     "guesses — not your decisions.")
mono(s, Inches(7.8), Inches(4.0), Inches(4.4), Inches(1.5), [
    ("# today", MUTED),
    ('"add an endpoint to submit orders"', INK),
    ("# → 200 lines, unknown assumptions", MUTED),
])

# ============ 3 NOT A COMPILER ============
s = slide(); eyebrow(s, "Reality check"); title(s, "AI is not a compiler")
q = card(s, Inches(0.9), Inches(2.3), Inches(6.1), Inches(3.3), "", "")
tf = q.text_frame; tf.margin_left = tf.margin_right = Inches(0.25); tf.margin_top = Inches(0.25)
_set(tf.paragraphs[0],
     "\u201cI just see things, say things, run things, and copy-paste things, and it mostly works.\u201d",
     20, INK, bold=False, spacing=1.2)
p = tf.add_paragraph(); p.space_before = Pt(14)
_set(p, "— Andrej Karpathy, on \u201cvibe coding\u201d", 13, MUTED)
bullets(s, Inches(7.4), Inches(2.4), Inches(5.0), Inches(3.5), [
    ("Non-deterministic.", "Precise input ≠ perfect output.", False),
    ("An assistant, not a compiler.", "It guesses, and it makes mistakes.", False),
    ("You own the output.", "Review, understand, and test everything.", True),
], size=16, gap=18)

# ============ 4 ANALOGY ============
s = slide(); eyebrow(s, "An analogy"); title(s, "Building a backyard")
card(s, Inches(0.9), Inches(2.2), Inches(5.6), Inches(3.3),
     "🔥  \u201cJust build me something cool\u201d",
     "The contractor assumes your intent. The BBQ arrives — but there's no cutout in the "
     "stone and no gas line. Every change means tearing up what's already built.",
     accent=DANGER)
box(s, Inches(1.15), Inches(4.95), Inches(5.0), Inches(0.4), "Prompt-and-pray", 14, DANGER, bold=True)
card(s, Inches(6.9), Inches(2.2), Inches(5.5), Inches(3.3),
     "📐  \u201cThe blueprint is the source of truth\u201d",
     "Exact dimensions. Appliances chosen first to define the interface. Utilities laid "
     "before pavers. The same blueprint works with any mason.",
     accent=BRAND2)
box(s, Inches(7.15), Inches(4.95), Inches(5.0), Inches(0.4),
    "Human owns \u201cwhat\u201d, agent owns \u201chow\u201d", 14, BRAND2, bold=True)
box(s, Inches(0.9), Inches(5.9), Inches(11.5), Inches(0.6),
    "Run slow to run fast — invest in the spec to accelerate delivery.",
    17, MUTED, align=PP_ALIGN.CENTER)

# ============ 5 WHAT IS SDD ============
s = slide(); eyebrow(s, "The idea"); title(s, "Make the specification the source of truth")
box(s, Inches(0.9), Inches(2.0), Inches(11), Inches(1.0),
    "SDD inserts durable, reviewable artifacts between intent and implementation. The AI agent "
    "gets structured context at every step — consistent, auditable, aligned to the requirement.",
    16, MUTED, spacing=1.2)
cw, gap = Inches(3.7), Inches(0.35)
x0 = Inches(0.9)
for i, (h, b) in enumerate([
    ("Write it down", "Every feature begins as spec.md — what & why, every requirement testable."),
    ("Plan the how", "A plan turns requirements into architecture, contracts, and a data model."),
    ("Execute", "Dependency-ordered tasks drive the AI to build a tested vertical slice."),
]):
    card(s, x0 + i * (cw + gap), Inches(3.3), cw, Inches(2.4), h, b, accent=BRAND)

# ============ 6 THE LOOP ============
s = slide(); eyebrow(s, "The method"); title(s, "One loop, every feature")
steps1 = [("00", "Constitution", "guardrails"), ("01", "Specify", "spec.md"),
          ("02", "Clarify", "de-risk"), ("03", "Plan", "contracts")]
steps2 = [("04", "Tasks", "tasks.md"), ("05", "Analyze", "gate"),
          ("06", "Implement", "code + tests"), ("07", "Converge", "assess")]
sw, sh, sgap = Inches(2.55), Inches(1.25), Inches(0.35)
x0 = Inches(0.9)
for i, (n, t, sub) in enumerate(steps1):
    step(s, x0 + i * (sw + sgap), Inches(2.4), sw, sh, n, t, sub)
    if i < 3:
        arrow(s, x0 + i * (sw + sgap) + sw + Inches(0.02), Inches(2.85), Inches(0.3))
for i, (n, t, sub) in enumerate(steps2):
    step(s, x0 + i * (sw + sgap), Inches(4.0), sw, sh, n, t, sub, hi=(i == 3))
    if i < 3:
        arrow(s, x0 + i * (sw + sgap) + sw + Inches(0.02), Inches(4.45), Inches(0.3))
box(s, Inches(0.9), Inches(5.7), Inches(11.5), Inches(0.7),
    "↺  Repeat implement → converge until the gap report says \u201cConverged\u201d.",
    15, MUTED)

# ============ 7 ARTIFACTS ============
s = slide(); eyebrow(s, "Each phase feeds the next"); title(s, "Artifacts, not vibes")
mono(s, Inches(0.9), Inches(2.3), Inches(5.9), Inches(3.3), [
    ("specs/001-order-submission/", MUTED),
    ("├── spec.md        # what & why", INK),
    ("├── plan.md        # how — architecture", INK),
    ("├── contracts/", INK),
    ("│   └── orders-api.md   # request/response", BRAND2),
    ("└── tasks.md       # ordered checklist", INK),
])
bullets(s, Inches(7.2), Inches(2.5), Inches(5.3), Inches(3), [
    ("Reviewable.", "Each artifact is a PR-sized, human-readable file.", True),
    ("Versioned.", "The spec evolves in git alongside the code.", True),
    ("Structured context.", "The AI reads the prior artifact, not guesses.", True),
], size=16, gap=18)

# ============ 8 TRACEABILITY ============
s = slide(); eyebrow(s, "Fits your backlog"); title(s, "From ticket to merged PR — one id")
nodes = [("JIRA / ADO", "ticket #1234"), ("spec → plan", "→ tasks"),
         ("branch", "1234-feature"), ("Pull Request", "refs #1234"), ("Merged", "")]
nw, nh = Inches(2.1), Inches(1.0)
x0, y = Inches(0.9), Inches(2.8)
step_x = Inches(2.55)
for i, (t, sub) in enumerate(nodes):
    fill = BRAND if i == 4 else CARD
    c = rrect(s, x0 + i * step_x, y, nw, nh, fill=fill)
    tf = c.text_frame; tf.vertical_anchor = MSO_ANCHOR.MIDDLE
    _set(tf.paragraphs[0], t, 14, (BG if i == 4 else INK), bold=True, align=PP_ALIGN.CENTER)
    if sub:
        p = tf.add_paragraph(); _set(p, sub, 11, MUTED, align=PP_ALIGN.CENTER)
    if i < 4:
        arrow(s, x0 + i * step_x + nw + Inches(0.03), y + Inches(0.3), Inches(0.38))
box(s, Inches(0.9), Inches(2.25), Inches(11), Inches(0.4),
    "one correlation id end-to-end", 13, BRAND2)
card(s, Inches(0.9), Inches(4.5), Inches(5.6), Inches(1.6), "Inbound",
     "A ticket seeds spec.md automatically — never re-typed, always enriched.")
card(s, Inches(6.8), Inches(4.5), Inches(5.6), Inches(1.6), "Outbound",
     "tasks.md creates child work items linked back under the same ticket.")

# ============ 9 ARCHITECTURE ============
s = slide(); eyebrow(s, "The demo under the hood"); title(s, "Thin adapters, rich core")
rrect(s, Inches(1.4), Inches(2.3), Inches(3.4), Inches(0.8))
box(s, Inches(1.4), Inches(2.45), Inches(3.4), Inches(0.5), "HTTP trigger", 14, INK,
    align=PP_ALIGN.CENTER)
rrect(s, Inches(1.2), Inches(3.5), Inches(3.8), Inches(0.8))
box(s, Inches(1.2), Inches(3.65), Inches(3.8), Inches(0.5), "SubmissionService", 14, INK,
    align=PP_ALIGN.CENTER)
rrect(s, Inches(0.9), Inches(4.8), Inches(2.6), Inches(0.9), fill=CARD2)
box(s, Inches(0.9), Inches(4.9), Inches(2.6), Inches(0.7),
    "IMessagePublisher\nInMemory | Service Bus", 11, INK, align=PP_ALIGN.CENTER)
rrect(s, Inches(3.8), Inches(4.8), Inches(2.4), Inches(0.9), fill=CARD2)
box(s, Inches(3.8), Inches(4.9), Inches(2.4), Inches(0.7),
    "IStatusStore\nInMemory | Azure SQL", 11, INK, align=PP_ALIGN.CENTER)
bullets(s, Inches(7.2), Inches(2.6), Inches(5.3), Inches(3), [
    ("Triggers ≈ 30 lines.", "Bind, validate, dispatch, return.", True),
    ("Core is Azure-free.", "Unit tests run offline, no cloud.", True),
    ("Swap to Azure", "= two lines in Program.cs.", True),
], size=16, gap=18)

# ============ 10 LIVE DEMO ============
s = slide(); eyebrow(s, "See it run"); title(s, "Live: async order submission")
mono(s, Inches(0.9), Inches(2.3), Inches(6.2), Inches(3.6), [
    ("# 1 — submit (202 + correlation id)", MUTED),
    ("POST /api/orders", ACCENT),
    ('{ "legalEntityId":"ABN-01", "lines":[…] }', INK),
    ('→ 202 { "correlationId":"c5d4…" }', BRAND2),
    ("", INK),
    ("# 2 — query status", MUTED),
    ("GET /api/orders/c5d4…/status", ACCENT),
    ('→ 200 { "state":"Accepted" }', BRAND2),
    ("", INK),
    ("# 3 — resend same clientRequestId", MUTED),
    ("→ same id · published once (idempotent)", BRAND2),
])
bullets(s, Inches(7.5), Inches(2.5), Inches(5.0), Inches(3.5), [
    ("202 + id", "— caller decoupled from slow writes.", True),
    ("Idempotent", "— safe retries, publish exactly once.", True),
    ("Validated", "— bad input → 400, nothing published.", True),
    ("14 tests green", "— all offline.", True),
], size=16, gap=16)

# ============ 11 BENEFITS ============
s = slide(); eyebrow(s, "Why it matters"); title(s, "Outcomes, measured")
kpis = [("1", "source of truth"), ("100%", "changes traceable"),
        ("0", "Azure to run tests"), ("2-line", "swap to Azure")]
x0 = Inches(0.9)
for i, (big, lbl) in enumerate(kpis):
    box(s, x0 + i * Inches(3.0), Inches(1.95), Inches(2.9), Inches(0.9), big, 40, BRAND, bold=True)
    box(s, x0 + i * Inches(3.0), Inches(2.85), Inches(2.9), Inches(0.5), lbl, 12, MUTED)
cards = [("Shared source of truth", "One spec aligns teams and agents."),
         ("Human at high leverage", "Review specs, not lines of code."),
         ("Small iterations", "Frequent reviews prevent drift."),
         ("Knowledge & audit trail", "Documents the why, not just what."),
         ("Reproducible outcomes", "Same spec → similar code."),
         ("AI as collaborator", "Let the agent interview you.")]
cw = Inches(3.7)
for i, (h, b) in enumerate(cards):
    r, cidx = divmod(i, 3)
    card(s, Inches(0.9) + cidx * (cw + Inches(0.3)), Inches(3.7) + r * Inches(1.6),
         cw, Inches(1.4), h, b)

# ============ 12 TOOLCHAIN ============
s = slide(); eyebrow(s, "How we run it"); title(s, "The toolchain")
tools = [("Spec Kit", "Open-source SDD toolkit — the /speckit.* commands & templates."),
         ("GitHub Copilot", "Runs the loop inside VS Code as slash-command skills."),
         ("Azure Functions", ".NET 10 isolated worker — the deployable unit per feature."),
         ("JIRA + ADO", "Backlog in, work items out — full round-trip traceability.")]
cw = Inches(2.85)
for i, (h, b) in enumerate(tools):
    card(s, Inches(0.9) + i * (cw + Inches(0.18)), Inches(2.3), cw, Inches(2.3), h, b, accent=BRAND)
mono(s, Inches(0.9), Inches(4.9), Inches(11.5), Inches(1.3), [
    ("## install & go", MUTED),
    ("uvx --from git+https://github.com/github/spec-kit specify init my-app --integration copilot", INK),
    ("/speckit.specify · /speckit.plan · /speckit.tasks · /speckit.implement", BRAND2),
])

# ============ 13 KEY TAKEAWAYS ============
s = slide(); eyebrow(s, "Remember this"); title(s, "The 3 separations of SDD", y=0.95)
sep = [("Thinking ≠ doing", "Specs define what; agents define how."),
       ("Strategy ≠ tactics", "Review specs, not lines of code."),
       ("Patterns ≠ projects", "Build portable skills, not one-off prompts.")]
cw = Inches(3.7)
for i, (h, b) in enumerate(sep):
    card(s, Inches(0.9) + i * (cw + Inches(0.3)), Inches(1.95), cw, Inches(1.5), h, b, accent=BRAND)
box(s, Inches(0.9), Inches(3.7), Inches(11), Inches(0.6), "The 3 practices that matter", 26, INK, bold=True)
pr = [("Run slow to run fast", "Invest upfront to accelerate delivery."),
      ("Phases with gates", "Validate early, prevent drift."),
      ("Let AI interview you", "Better specs through collaboration.")]
for i, (h, b) in enumerate(pr):
    card(s, Inches(0.9) + i * (cw + Inches(0.3)), Inches(4.4), cw, Inches(1.5), h, b, accent=BRAND2)
box(s, Inches(0.9), Inches(6.15), Inches(11.5), Inches(0.6),
    "AI amplifies what you already have — make sure you're amplifying the right things.  (DORA 2025)",
    15, MUTED)

# ============ 14 CREDITS ============
s = slide(); eyebrow(s, "Sources & further reading"); title(s, "Credits")
cr = [("Spec-Driven Development Using Coding Agents",
       "Arun Gupta, JetBrains — Jfokus 2026. The backyard/blueprint analogy, the 3 separations."),
      ("Spec-driven Development",
       "Simon Martinelli, JUG Switzerland. \u201cAI is not a compiler\u201d, executable specs, guardrails."),
      ("GitHub Spec Kit", "github.com/github/spec-kit — the toolkit powering this demo."),
      ("Community", "ainativedev.io · agentskills.io — patterns, skills, and practices.")]
cw = Inches(5.7)
for i, (h, b) in enumerate(cr):
    r, cidx = divmod(i, 2)
    card(s, Inches(0.9) + cidx * (cw + Inches(0.3)), Inches(2.3) + r * Inches(1.8),
         cw, Inches(1.6), h, b)

# ============ 15 CLOSE ============
s = slide()
box(s, Inches(0.9), Inches(1.9), Inches(11.5), Inches(0.5),
    "CONSTITUTION ONCE · SPECIFY → PLAN → TASKS → IMPLEMENT → CONVERGE PER FEATURE",
    13, BRAND2, bold=True)
box(s, Inches(0.85), Inches(2.5), Inches(11.5), Inches(2.0),
    "Spec first. Then let AI build.", 54, INK, bold=True, spacing=0.95)
box(s, Inches(0.9), Inches(4.5), Inches(10), Inches(1.2),
    "Durable intent, reviewable artifacts, consistent output, and end-to-end traceability — "
    "the gold standard for AI-assisted delivery.", 18, MUTED, spacing=1.2)
rrect(s, Inches(0.9), Inches(5.9), Inches(5.0), Inches(0.55), radius=0.5)
box(s, Inches(1.1), Inches(5.98), Inches(4.8), Inches(0.45),
    "▶  Live demo: func start + demo.http", 13, MUTED, anchor=MSO_ANCHOR.MIDDLE)
rrect(s, Inches(6.1), Inches(5.9), Inches(4.3), Inches(0.55), radius=0.5)
box(s, Inches(6.3), Inches(5.98), Inches(4.1), Inches(0.45),
    "📄  docs/ · ADRs · constitution", 13, MUTED, anchor=MSO_ANCHOR.MIDDLE)

out = Path(__file__).with_name("spec-driven-development.pptx")
prs.save(out)
print("Wrote", out, "with", len(prs.slides._sldIdLst), "slides")
