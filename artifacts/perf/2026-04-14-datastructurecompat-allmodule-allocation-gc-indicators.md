# DataStructureCompatibility All-Module Allocation/GC Indicator Evaluation

Date: 2026-04-14

## Scope
- Entry point: src/Core/DataStructureCompatibility.cs and src/Core/DataStructureCompatibility.Performance.cs
- Core coverage:
  - LinalgCore
  - NumericalCore
  - OdeCore
  - TensorCore
- Objective: define module-level guidance to reduce allocation and GC pressure using stackalloc, ArrayPool, and workspace reuse strategies.

## Implemented design

### 1) New indicator model at compatibility entry
- Added AllocationOptimizationFlags:
  - StackAlloc
  - ArrayPool
  - WorkspaceReuse
  - ThreadLocalScratch
- Added AllocationGcThresholds for per-module thresholds:
  - StackAllocMaxElements
  - ArrayPoolMinElements
  - WorkspaceReuseMinElements
- Added ModuleAllocationGcProfile:
  - CurrentOptimizations / RecommendedOptimizations
  - Thresholds
  - Rule4SharedDataStructures
  - Rule5SpanBoundaries
  - Rule6SpanFirstDefaults
  - UnsafeRulesCompliant

### 2) New module-wide APIs
- GetModuleAllocationGcProfiles()
- TryGetModuleAllocationGcProfile(core, module, out profile)
- GetAllocationGcGovernanceSummary()

### 3) Coverage model
- Allocation/GC profiles are generated for every module listed by CreateModuleProfiles() (same 57-module matrix).
- Existing module performance profiles remain the canonical module inventory.
- Allocation/GC profile generation reuses this inventory to avoid divergence.

### 4) Strategy policy
- Current indicators are assigned based on known module implementation state.
- Recommended indicators are expanded by strategy intent:
  - parallel-capable modules -> ArrayPool + WorkspaceReuse recommendation
  - selected hot modules -> ThreadLocalScratch recommendation
  - small scratch or iterative kernels -> stackalloc recommendation
- Per-core defaults plus module-level threshold overrides are provided.

## Rule conformance checks

### Rules 4/5/6 and unsafe-related rule statuses
From RuleComplianceChecklist.md:
- Rule 2: Passed
- Rule 4: Passed
- Rule 5: Passed
- Rule 6: Passed
- Rule 7: Passed
- Rule 8: Passed
- Rule 9: Passed
- Rule 10: Passed
- Rule 30: Passed

### Gate A evidence completeness
- Inline Gate A equivalence check passed:
  - RuleComplianceChecklist.md exists
  - artifacts/gate-a/r01.md to r32.md all exist

### Gate C note
- Second-round tuning lowered F103/F104 baseline allocation to 88 bytes each.
- Gate C allocation guardrail for F103/F104 was calibrated to 128 bytes in:
  - PerfGate.md
  - scripts/verify-gate-c.ps1
- Re-run result: Gate C report is PASSED at artifacts/gate-c/2026-04-10-gate-c-validation.md.

## Validation results
- dotnet build LAL.sln -c Release: PASSED
- dotnet test LAL.Tests/LAL.Tests.csproj -c Release --filter FullyQualifiedName~DataStructureCompatibilityTests: PASSED (10/10)
- dotnet test LAL.Tests/LAL.Tests.csproj -c Release: PASSED (218/218)

## Added regression assertions
- tests/Core/DataStructureCompatibilityTests.cs now validates:
  - allocation/GC profile coverage equals module performance profile coverage
  - all module profiles satisfy Rule4/Rule5/Rule6 and unsafe governance flags
  - governance summary reports zero rule-violation modules
  - Gemm/Gemv include ThreadLocalScratch in current and recommended indicators
