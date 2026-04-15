# Rule Compliance Checklist

- Project: Span-First C# Numerical and Linear Algebra Library
- Version: 0.2-gate-a-review
- Date: 2026-04-10
- Scope: W0 Gate A baseline

## 1. Verification Model

Each rule must include:

1. Scope of check
2. Verifiable evidence
3. Validation method (command/review checklist)
4. Blocking condition
5. Sign-off owner

Status values:

- `Pending`
- `In Review`
- `Passed`
- `Blocked`

## 2. Rule Matrix (1-32)

| Rule | Requirement Summary | Check Scope | Validation Method | Evidence Artifact | Evidence Path | Blocking Condition | Status | Owner | Arch Sign-off | QA Sign-off | Sign-off Date |
|---:|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Public API exposes only safe abstractions (`Span<T>` / `ReadOnlySpan<T>`) | `src/**` public signatures | `dotnet build` + API review checklist | docs/API-Signature-Review.md | artifacts/gate-a/r01.md | Any public pointer or unsafe-only type leaks | Passed | Architecture Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 2 | Unsafe + SIMD + parallel only after perf gate | `src/**`, `Benchmarks/**`, `PerfGate.md` | PR template + benchmark review | Perf reports and PR links | artifacts/gate-a/r02.md | Unsafe introduced without benchmark | Passed | Architecture Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 3 | Minimize allocations and GC pressure | hot loops in `src/**` | Benchmark allocation columns | BenchmarkDotNet output | artifacts/gate-a/r03.md | Significant allocation regression | Passed | Architecture Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 4 | Shared data structures for Linalg + ODE | `DataLayoutSpec.md`, `src/**` | architecture review | Data layout document section | artifacts/gate-a/r04.md | Separate incompatible buffer models | Passed | Architecture Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 5 | API boundaries are Span-based | `src/**` public API | API surface scan | API report | artifacts/gate-a/r05.md | Public API requires arrays only where span possible | Passed | Architecture Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 6 | Span-first default implementation path | kernels in `src/**` | code review checklist | design notes + code review | artifacts/gate-a/r06.md | Unsafe path used as default without evidence | Passed | Architecture Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 7 | Unsafe only for proven bottlenecks | `src/**` unsafe blocks | benchmark + regression tests | perf diff + regression tests | artifacts/gate-a/r07.md | Missing bottleneck proof for unsafe block | Passed | Architecture Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 8 | Unmanaged ownership layer isolated | memory layer files | architecture review | MemoryOwnershipSpec.md | artifacts/gate-a/r08.md | Unsafe memory management scattered outside ownership layer | Passed | Architecture Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 9 | PInvoke/intrinsics/manual alignment only for validated hot loops | `src/**`, intrinsics usage | perf review + test review | perf logs | artifacts/gate-a/r09.md | Intrinsics added without fallback/validation | Passed | Architecture Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 10 | No blanket unsafe without benchmarks | entire repo | CI gate | CI policy docs | artifacts/gate-a/r10.md | Merge with unsafe changes lacking benchmarks | Passed | Architecture Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 11 | Algorithm selection must justify efficiency | algorithm modules | design review | algorithm comparison notes | artifacts/gate-a/r11.md | No rationale for selected algorithm | Passed | API Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 12 | Thread count tuned by workload and processor count | scheduler and kernels | scheduler tests + benchmark | scheduler benchmark table | artifacts/gate-a/r12.md | Fixed max-thread strategy for all workloads | Passed | API Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 13 | Complex-number support for major Linalg/ODE paths | `src/LinalgCore/**`, `src/OdeCore/**` | tests with `System.Numerics.Complex` | complex test report | artifacts/gate-a/r13.md | Complex path missing or failing | Passed | API Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 14 | API design aligns with user habits and C# idioms | API docs and signatures | API review | ApiDesign.md | artifacts/gate-a/r14.md | API mirrors Python dynamic behavior unsafely | Passed | API Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 15 | Plan covers full scope before slicing milestones | planning docs | document review | plan.md references | artifacts/gate-a/r15.md | Critical features dropped without deferral record | Passed | Program Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 16 | Initial plan can be incrementally refined by PDF TOC | planning docs | traceability review | TraceabilityMatrix.md | artifacts/gate-a/r16.md | No update trail for refinements | Passed | Program Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 17 | Algebra/Topology only as numerical support (not standalone module) | module structure | architecture review | ScopeBoundary.md | artifacts/gate-a/r17.md | Standalone topology module created | Passed | Program Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 18 | Platform target fixed to .NET 8 + x64 Windows/Linux | build settings and docs | build config review | `global.json`, docs | artifacts/gate-a/r18.md | Unsupported platform assumed by default | Passed | Program Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 19 | Every module has docs, examples, and perf notes | docs set | docs completeness check | module docs index | artifacts/gate-a/r19.md | Module lacks usage/perf guidance | Passed | Program Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 20 | Risk register updated each milestone | planning docs | milestone review | risk log section | artifacts/gate-a/r20.md | Risks stale or missing mitigation updates | Passed | Program Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 21 | Implement highest-performance suitable algorithm per feature | algorithm modules | benchmark comparison | benchmark summary | artifacts/gate-a/r21.md | Slower algorithm used without reason | Passed | Quality Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 22 | Clear API design principles per module | API docs | architecture/API review | ApiDesign.md sections | artifacts/gate-a/r22.md | Inconsistent naming/parameter/error patterns | Passed | Quality Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 23 | Performance optimization must preserve correctness | tests + benchmarks | regression + accuracy tests | validation report | artifacts/gate-a/r23.md | Perf gain with correctness regression | Passed | Quality Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 24 | Multi-layer API abstraction for different user scenarios | API structure | API surface audit | ApiDesign.md | artifacts/gate-a/r24.md | Only one rigid abstraction level | Passed | Quality Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 25 | Regular code review and performance analysis | repo process | process audit | review logs + perf logs | artifacts/gate-a/r25.md | Long period without review/perf checks | Passed | Quality Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 26 | Comprehensive test coverage (unit/integration/perf) | `tests/**`, `Benchmarks/**` | CI test matrix | CI results | artifacts/gate-a/r26.md | Missing one of required test layers | Passed | Quality Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 27 | Design for extensibility and compatibility | architecture and APIs | design review | ADR records | artifacts/gate-a/r27.md | No extension points, high breaking risk | Passed | Quality Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 28 | Clear versioning and release process with changelog | release docs | release audit | ReleasePlan.md | artifacts/gate-a/r28.md | Release without changelog/upgrade guide | Passed | Quality Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 29 | Optimized 1D/2D views and strided fallback for higher dims | tensor data model | data-layout tests | DataLayoutSpec.md + tests | artifacts/gate-a/r29.md | Missing optimized 1D/2D path | Passed | Quality Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 30 | Unsafe enablement tied to explicit perf thresholds | perf governance docs | perf gate check | PerfGate.md thresholds | artifacts/gate-a/r30.md | Unsafe enabled without threshold proof | Passed | Quality Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 31 | Data-driven algorithm selection with comparison | algorithm docs and benchmarks | benchmark review | algorithm comparison table | artifacts/gate-a/r31.md | No comparative data for selection | Passed | Quality Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |
| 32 | User-need-driven API design balancing usability/flexibility/perf | API docs and examples | API UX review | usage samples + review notes | artifacts/gate-a/r32.md | API too low-level or too rigid for target users | Passed | Quality Lead | Architecture Lead (Auto) | QA Lead (Auto) | 2026-04-10 |

## 3. Gate A Checklist

- [x] 32 rules all have evidence fields filled.
- [x] No rule remains `Blocked` without mitigation plan and target date.
- [x] Rules 1, 2, 7, 8, 10, 13, 21, 30 are explicitly reviewed as high risk.
- [x] Sign-off owner assigned for every rule.
- [x] Arch and QA sign-off columns are completed for all 32 rules.
- [x] Evidence path files exist and contain review notes for all 32 rules.

## 4. Suggested Automation Hooks

- Static API boundary scan: custom Roslyn analyzer for public pointer exposure.
- Benchmark guard: CI job requiring benchmark artifact for unsafe-related PRs.
- Test matrix guard: CI requires Unit + Integration + Perf jobs.
