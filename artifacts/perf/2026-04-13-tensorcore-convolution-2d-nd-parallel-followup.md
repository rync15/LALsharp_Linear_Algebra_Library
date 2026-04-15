# TensorCore Convolution 2D/N-D Gated Parallel Follow-up (2026-04-13)

## What changed
- Added gated parallel paths for TensorCore Convolution 2D and N-D full convolution overloads (double/float/Complex).
- 2D parallel strategy: output-index decomposition (`Parallel.For` over flattened output positions) to avoid write contention.
- N-D parallel strategy: output-index decomposition with thread-local index workspaces to avoid per-iteration allocations and race conditions.
- Existing sequential/generic core paths remain as fallback for smaller workloads.

## Gating rules
- 2D gate: output size + operation estimate threshold.
- N-D gate: output size + operation estimate threshold scaled by rank.
- Goal: avoid parallel overhead on small tensors.

## Validation
- TensorCore tests: `dotnet test LAL.Tests/LAL.Tests.csproj -c Release --filter "FullyQualifiedName~TensorCore"` => 51 passed, 0 failed.
- Full tests: `dotnet test LAL.Tests/LAL.Tests.csproj -c Release` => 189 passed, 0 failed.

## Notes
- No unsafe or explicit hardware intrinsics added in this follow-up; this remains managed and evidence-gated.
