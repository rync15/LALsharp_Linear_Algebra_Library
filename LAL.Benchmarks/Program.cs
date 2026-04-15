using System.Diagnostics;
using LAL.LinalgCore;
using LAL.NumericalCore.Optimization;
using LAL.NumericalCore.RootFinding;
using LAL.OdeCore;
using LAL.TensorCore;

static (double avgMs, long avgAllocBytes) Run(Action action, int warmup, int iterations)
{
    for (int i = 0; i < warmup; i++) action();
    long totalTicks = 0;
    long totalAlloc = 0;

    for (int i = 0; i < iterations; i++)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long beforeAlloc = GC.GetAllocatedBytesForCurrentThread();
        long start = Stopwatch.GetTimestamp();
        action();
        long end = Stopwatch.GetTimestamp();
        long afterAlloc = GC.GetAllocatedBytesForCurrentThread();

        totalTicks += (end - start);
        totalAlloc += (afterAlloc - beforeAlloc);
    }

    double avgMs = (totalTicks * 1000.0 / Stopwatch.Frequency) / iterations;
    long avgAlloc = totalAlloc / iterations;
    return (avgMs, avgAlloc);
}

static void WriteReport(string reportPath, string title, string scope, IReadOnlyList<string> tableRows, IReadOnlyList<string> notes)
{
    var lines = new List<string>
    {
        $"# {title}",
        "",
        "- Date: 2026-04-10",
        "- Runtime: .NET 8",
        $"- Scope: {scope}",
        "",
        "| Function | Workload | Avg Latency (ms) | Avg Allocation (bytes) |",
        "|---|---|---:|---:|"
    };

    lines.AddRange(tableRows);
    lines.Add(string.Empty);
    lines.Add("## Notes");
    lines.Add(string.Empty);
    lines.AddRange(notes);

    File.WriteAllLines(reportPath, lines);

    Console.WriteLine($"Report generated: {reportPath}");
    foreach (string line in lines)
    {
        Console.WriteLine(line);
    }
}

var rng = new Random(42);

int n = 1_000_000;
double[] x = new double[n];
double[] y = new double[n];
for (int i = 0; i < n; i++) { x[i] = rng.NextDouble(); y[i] = rng.NextDouble(); }

int rows = 1024;
int cols = 1024;
double[] aGemv = new double[rows * cols];
double[] xGemv = new double[cols];
double[] yGemv = new double[rows];
for (int i = 0; i < aGemv.Length; i++) aGemv[i] = rng.NextDouble();
for (int i = 0; i < xGemv.Length; i++) xGemv[i] = rng.NextDouble();

int m = 256, k = 256, n2 = 256;
double[] aGemm = new double[m * k];
double[] bGemm = new double[k * n2];
double[] cGemm = new double[m * n2];
for (int i = 0; i < aGemm.Length; i++) aGemm[i] = rng.NextDouble();
for (int i = 0; i < bGemm.Length; i++) bGemm[i] = rng.NextDouble();

var axpy = Run(() => Axpy.Compute(0.5, x, y), warmup: 2, iterations: 8);
var dot = Run(() => _ = Dot.Dotu(x, y), warmup: 2, iterations: 8);
var gemv = Run(() => Gemv.Multiply(aGemv, rows, cols, xGemv, yGemv), warmup: 3, iterations: 5);
var gemm = Run(() => Gemm.Multiply(aGemm, bGemm, cGemm, m, n2, k), warmup: 1, iterations: 3);

int reductionsN = 1_000_000;
double[] reductionValues = new double[reductionsN];
for (int i = 0; i < reductionValues.Length; i++) reductionValues[i] = rng.NextDouble();

double[] rkState = [1.0, -0.5, 0.25, 2.0];
double[] rkOut = new double[rkState.Length];

var reductions = Run(() => _ = Reductions.Sum(reductionValues), warmup: 2, iterations: 8);
var rk45 = Run(() =>
{
    _ = Rk45.Step(0.0, 0.01, rkState, rkOut, static (_, state, dydt) =>
    {
        for (int i = 0; i < state.Length; i++)
        {
            dydt[i] = -0.3 * state[i];
        }
    });
}, warmup: 2, iterations: 20);

var brent = Run(() => _ = Brent.Solve(static x => (x * x * x) - x - 2.0, 1.0, 2.0, tolerance: 1e-10, maxIterations: 100), warmup: 2, iterations: 50);
var gradientDescent = Run(() => _ = GradientDescent.SolveScalar(
    static x => (x - 3.0) * (x - 3.0),
    static x => 2.0 * (x - 3.0),
    initialX: 0.0,
    learningRate: 0.1,
    tolerance: 1e-8,
    maxIterations: 1_000), warmup: 2, iterations: 50);

int normsN = 1_000_000;
double[] normValues = new double[normsN];
for (int i = 0; i < normValues.Length; i++) normValues[i] = rng.NextDouble() - 0.5;

int tRows = 512;
int tCols = 512;
double[] transposeInput = new double[tRows * tCols];
double[] transposeOutput = new double[tRows * tCols];
for (int i = 0; i < transposeInput.Length; i++) transposeInput[i] = rng.NextDouble();

double[] jacobianState = [1.0, 2.0, -1.0, 0.5, -0.25, 0.75, 1.5, -2.0];
double[] jacobian = new double[jacobianState.Length * jacobianState.Length];

var norms = Run(() => _ = Norms.L2(normValues), warmup: 2, iterations: 8);
var transpose = Run(() => Transpose.Matrix(transposeInput, tRows, tCols, transposeOutput), warmup: 2, iterations: 8);
var jacobianEstimator = Run(() => JacobianEstimator.EstimateForwardDifference(
    t: 0.0,
    y: jacobianState,
    jacobian: jacobian,
    system: static (_, state, dydt) =>
    {
        for (int i = 0; i < state.Length; i++)
        {
            dydt[i] = (0.2 * state[i]) + (0.1 * i);
        }
    },
    epsilon: 1e-6), warmup: 2, iterations: 30);

string reportDir = Path.Combine("artifacts", "perf");
Directory.CreateDirectory(reportDir);

WriteReport(
    reportPath: Path.Combine(reportDir, "2026-04-10-batch1.md"),
    title: "Batch-1 Performance Baseline",
    scope: "F100, F101, F103, F104",
    tableRows:
    [
        $"| F100 Axpy | n={n} | {axpy.avgMs:F4} | {axpy.avgAllocBytes} |",
        $"| F101 Dotu | n={n} | {dot.avgMs:F4} | {dot.avgAllocBytes} |",
        $"| F103 Gemv | {rows}x{cols} * {cols} | {gemv.avgMs:F4} | {gemv.avgAllocBytes} |",
        $"| F104 Gemm | {m}x{k} * {k}x{n2} | {gemm.avgMs:F4} | {gemm.avgAllocBytes} |"
    ],
    notes:
    [
        "- Baseline measurements are for safe-kernel implementations.",
        "- No unsafe or intrinsics path is enabled in this batch.",
        "- Results are used as reference for future hot-path optimization gates."
    ]);

WriteReport(
    reportPath: Path.Combine(reportDir, "2026-04-10-batch2.md"),
    title: "Batch-2 Performance Baseline",
    scope: "F011, F202, F302, F306",
    tableRows:
    [
        $"| F011 Reductions.Sum | n={reductionsN} | {reductions.avgMs:F4} | {reductions.avgAllocBytes} |",
        $"| F202 RK45.Step | dim={rkState.Length}, dt=0.01 | {rk45.avgMs:F4} | {rk45.avgAllocBytes} |",
        $"| F302 Brent.Solve | cubic root bracket [1,2] | {brent.avgMs:F4} | {brent.avgAllocBytes} |",
        $"| F306 GradientDescent.SolveScalar | quadratic minimization | {gradientDescent.avgMs:F4} | {gradientDescent.avgAllocBytes} |"
    ],
    notes:
    [
        "- Batch-2 baselines are recorded before additional optimization passes.",
        "- Correctness is enforced by the matching unit tests in tests/**.",
        "- These figures are used by Rule 23 as the pre-optimization baseline."
    ]);

WriteReport(
    reportPath: Path.Combine(reportDir, "2026-04-10-batch3.md"),
    title: "Batch-3 Performance Baseline",
    scope: "F102, F105, F203",
    tableRows:
    [
        $"| F102 Norms.L2 | n={normsN} | {norms.avgMs:F4} | {norms.avgAllocBytes} |",
        $"| F105 Transpose.Matrix | {tRows}x{tCols} | {transpose.avgMs:F4} | {transpose.avgAllocBytes} |",
        $"| F203 JacobianEstimator.EstimateForwardDifference | dim={jacobianState.Length} | {jacobianEstimator.avgMs:F4} | {jacobianEstimator.avgAllocBytes} |"
    ],
    notes:
    [
        "- Batch-3 baselines cover newly implemented P1 kernels.",
        "- Measurements represent safe baseline implementations before blocked/vectorized tuning.",
        "- Results feed Rule 11/31 evidence and CI perf regression threshold checks."
    ]);
