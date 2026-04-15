using LAL.NumericalCore.Statistics;
using LAL.OdeCore;
using LAL.TensorCore;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace LAL.Core;

[Flags]
public enum PerformanceStrategyFlags
{
    Scalar = 1,
    Simd = 2,
    Intrinsics = 4,
    Unsafe = 8,
    Parallel = 16,
}

public readonly record struct DataStructurePerformanceSettings(
    bool EnableUnsafe,
    bool EnableSimd,
    bool EnableIntrinsics,
    bool EnableParallel,
    int SimdLengthThreshold,
    int IntrinsicsLengthThreshold,
    int ParallelLengthThreshold,
    int? MaxDegreeOfParallelism)
{
    public static DataStructurePerformanceSettings Default => new(
        EnableUnsafe: true,
        EnableSimd: true,
        EnableIntrinsics: true,
        EnableParallel: true,
        SimdLengthThreshold: 64,
        IntrinsicsLengthThreshold: 256,
        ParallelLengthThreshold: 16_384,
        MaxDegreeOfParallelism: null);
}

public readonly record struct ModulePerformanceProfile(
    string Core,
    string Module,
    PerformanceStrategyFlags CurrentStrategies,
    PerformanceStrategyFlags RecommendedStrategies,
    string Notes);

[Flags]
public enum AllocationOptimizationFlags
{
    None = 0,
    StackAlloc = 1,
    ArrayPool = 2,
    WorkspaceReuse = 4,
    ThreadLocalScratch = 8,
}

public readonly record struct AllocationGcThresholds(
    int StackAllocMaxElements,
    int ArrayPoolMinElements,
    int WorkspaceReuseMinElements);

public readonly record struct ModuleAllocationGcProfile(
    string Core,
    string Module,
    AllocationOptimizationFlags CurrentOptimizations,
    AllocationOptimizationFlags RecommendedOptimizations,
    AllocationGcThresholds Thresholds,
    bool Rule4SharedDataStructures,
    bool Rule5SpanBoundaries,
    bool Rule6SpanFirstDefaults,
    bool UnsafeRulesCompliant,
    string Notes);

public readonly record struct AllocationGcGovernanceSummary(
    int ModuleCount,
    int CurrentStackAllocModules,
    int CurrentArrayPoolModules,
    int CurrentWorkspaceReuseModules,
    int CurrentThreadLocalScratchModules,
    int RecommendedStackAllocModules,
    int RecommendedArrayPoolModules,
    int RecommendedWorkspaceReuseModules,
    int RecommendedThreadLocalScratchModules,
    int RuleViolationModules);

public static partial class DataStructureCompatibility
{
    private static readonly object PerformanceSettingsLock = new();
    private static DataStructurePerformanceSettings s_performanceSettings = DataStructurePerformanceSettings.Default;
    private static readonly ModulePerformanceProfile[] s_moduleProfiles = CreateModuleProfiles();
    private static readonly ModuleAllocationGcProfile[] s_moduleAllocationGcProfiles = CreateModuleAllocationGcProfiles();

    public static DataStructurePerformanceSettings GetPerformanceSettings()
    {
        lock (PerformanceSettingsLock)
        {
            return s_performanceSettings;
        }
    }

    public static void SetPerformanceSettings(DataStructurePerformanceSettings settings)
    {
        ValidatePerformanceSettings(settings);

        lock (PerformanceSettingsLock)
        {
            s_performanceSettings = settings;
        }
    }

    public static void ResetPerformanceSettings()
    {
        lock (PerformanceSettingsLock)
        {
            s_performanceSettings = DataStructurePerformanceSettings.Default;
        }
    }

    public static PerformanceStrategyFlags GetRuntimeHardwareCapabilities()
    {
        PerformanceStrategyFlags flags = PerformanceStrategyFlags.Scalar;

        if (Vector.IsHardwareAccelerated)
        {
            flags |= PerformanceStrategyFlags.Simd;
        }

        if (Avx.IsSupported || Sse2.IsSupported || AdvSimd.IsSupported)
        {
            flags |= PerformanceStrategyFlags.Intrinsics;
            flags |= PerformanceStrategyFlags.Unsafe;
        }

        if (Environment.ProcessorCount > 1)
        {
            flags |= PerformanceStrategyFlags.Parallel;
        }

        return flags;
    }

    public static ReadOnlySpan<ModulePerformanceProfile> GetModulePerformanceProfiles()
    {
        return s_moduleProfiles;
    }

    public static bool TryGetModulePerformanceProfile(string core, string module, out ModulePerformanceProfile profile)
    {
        if (string.IsNullOrWhiteSpace(core))
        {
            throw new ArgumentException("Core is required.", nameof(core));
        }

        if (string.IsNullOrWhiteSpace(module))
        {
            throw new ArgumentException("Module is required.", nameof(module));
        }

        for (int i = 0; i < s_moduleProfiles.Length; i++)
        {
            ModulePerformanceProfile candidate = s_moduleProfiles[i];
            if (string.Equals(candidate.Core, core, StringComparison.OrdinalIgnoreCase)
                && string.Equals(candidate.Module, module, StringComparison.OrdinalIgnoreCase))
            {
                profile = candidate;
                return true;
            }
        }

        profile = default;
        return false;
    }

    public static ReadOnlySpan<ModuleAllocationGcProfile> GetModuleAllocationGcProfiles()
    {
        return s_moduleAllocationGcProfiles;
    }

    public static bool TryGetModuleAllocationGcProfile(string core, string module, out ModuleAllocationGcProfile profile)
    {
        if (string.IsNullOrWhiteSpace(core))
        {
            throw new ArgumentException("Core is required.", nameof(core));
        }

        if (string.IsNullOrWhiteSpace(module))
        {
            throw new ArgumentException("Module is required.", nameof(module));
        }

        for (int i = 0; i < s_moduleAllocationGcProfiles.Length; i++)
        {
            ModuleAllocationGcProfile candidate = s_moduleAllocationGcProfiles[i];
            if (string.Equals(candidate.Core, core, StringComparison.OrdinalIgnoreCase)
                && string.Equals(candidate.Module, module, StringComparison.OrdinalIgnoreCase))
            {
                profile = candidate;
                return true;
            }
        }

        profile = default;
        return false;
    }

    public static AllocationGcGovernanceSummary GetAllocationGcGovernanceSummary()
    {
        int ruleViolationModules = 0;
        for (int i = 0; i < s_moduleAllocationGcProfiles.Length; i++)
        {
            ModuleAllocationGcProfile profile = s_moduleAllocationGcProfiles[i];
            if (!profile.Rule4SharedDataStructures
                || !profile.Rule5SpanBoundaries
                || !profile.Rule6SpanFirstDefaults
                || !profile.UnsafeRulesCompliant)
            {
                ruleViolationModules++;
            }
        }

        return new AllocationGcGovernanceSummary(
            ModuleCount: s_moduleAllocationGcProfiles.Length,
            CurrentStackAllocModules: CountModulesWithOptimizationFlag(s_moduleAllocationGcProfiles, AllocationOptimizationFlags.StackAlloc, useRecommended: false),
            CurrentArrayPoolModules: CountModulesWithOptimizationFlag(s_moduleAllocationGcProfiles, AllocationOptimizationFlags.ArrayPool, useRecommended: false),
            CurrentWorkspaceReuseModules: CountModulesWithOptimizationFlag(s_moduleAllocationGcProfiles, AllocationOptimizationFlags.WorkspaceReuse, useRecommended: false),
            CurrentThreadLocalScratchModules: CountModulesWithOptimizationFlag(s_moduleAllocationGcProfiles, AllocationOptimizationFlags.ThreadLocalScratch, useRecommended: false),
            RecommendedStackAllocModules: CountModulesWithOptimizationFlag(s_moduleAllocationGcProfiles, AllocationOptimizationFlags.StackAlloc, useRecommended: true),
            RecommendedArrayPoolModules: CountModulesWithOptimizationFlag(s_moduleAllocationGcProfiles, AllocationOptimizationFlags.ArrayPool, useRecommended: true),
            RecommendedWorkspaceReuseModules: CountModulesWithOptimizationFlag(s_moduleAllocationGcProfiles, AllocationOptimizationFlags.WorkspaceReuse, useRecommended: true),
            RecommendedThreadLocalScratchModules: CountModulesWithOptimizationFlag(s_moduleAllocationGcProfiles, AllocationOptimizationFlags.ThreadLocalScratch, useRecommended: true),
            RuleViolationModules: ruleViolationModules);
    }

    public static void LinalgAxpy(double alpha, NDBuffer<double> x, NDBuffer<double> y)
    {
        EnsureVectorContiguous(x, nameof(x), out int xLength);
        EnsureVectorContiguous(y, nameof(y), out int yLength);

        if (xLength != yLength)
        {
            throw new ArgumentException("Input vectors must have the same length.");
        }

        DataStructurePerformanceSettings settings = GetPerformanceSettings();
        if (ShouldUseParallel(xLength, settings))
        {
            double[] xStorage = x.DangerousStorage;
            double[] yStorage = y.DangerousStorage;

            ExecuteParallelChunks(xLength, settings, (start, count) =>
            {
                PerformancePrimitives.Axpy(alpha, xStorage.AsSpan(start, count), yStorage.AsSpan(start, count), settings);
            });

            return;
        }

        PerformancePrimitives.Axpy(alpha, x.AsReadOnlySpan(), y.AsSpan(), settings);
    }

    public static void LinalgAxpy(float alpha, NDBuffer<float> x, NDBuffer<float> y)
    {
        EnsureVectorContiguous(x, nameof(x), out int xLength);
        EnsureVectorContiguous(y, nameof(y), out int yLength);

        if (xLength != yLength)
        {
            throw new ArgumentException("Input vectors must have the same length.");
        }

        DataStructurePerformanceSettings settings = GetPerformanceSettings();
        if (ShouldUseParallel(xLength, settings))
        {
            float[] xStorage = x.DangerousStorage;
            float[] yStorage = y.DangerousStorage;

            ExecuteParallelChunks(xLength, settings, (start, count) =>
            {
                PerformancePrimitives.Axpy(alpha, xStorage.AsSpan(start, count), yStorage.AsSpan(start, count), settings);
            });

            return;
        }

        PerformancePrimitives.Axpy(alpha, x.AsReadOnlySpan(), y.AsSpan(), settings);
    }

    public static double LinalgDot(NDBuffer<double> left, NDBuffer<double> right)
    {
        EnsureVectorContiguous(left, nameof(left), out int leftLength);
        EnsureVectorContiguous(right, nameof(right), out int rightLength);

        if (leftLength != rightLength)
        {
            throw new ArgumentException("Input vectors must have the same length.");
        }

        DataStructurePerformanceSettings settings = GetPerformanceSettings();
        if (!ShouldUseParallel(leftLength, settings))
        {
            return PerformancePrimitives.Dot(left.AsReadOnlySpan(), right.AsReadOnlySpan(), settings);
        }

        double[] leftStorage = left.DangerousStorage;
        double[] rightStorage = right.DangerousStorage;

        int chunkSize = ComputeChunkSize(leftLength, ResolveParallelism(settings));
        int chunkCount = (leftLength + chunkSize - 1) / chunkSize;
        double[] partialSums = new double[chunkCount];

        Parallel.For(0, chunkCount, CreateParallelOptions(settings), chunkIndex =>
        {
            int start = chunkIndex * chunkSize;
            int count = Math.Min(chunkSize, leftLength - start);
            partialSums[chunkIndex] = PerformancePrimitives.Dot(
                leftStorage.AsSpan(start, count),
                rightStorage.AsSpan(start, count),
                settings);
        });

        double sum = 0d;
        for (int i = 0; i < partialSums.Length; i++)
        {
            sum += partialSums[i];
        }

        return sum;
    }

    public static float LinalgDot(NDBuffer<float> left, NDBuffer<float> right)
    {
        EnsureVectorContiguous(left, nameof(left), out int leftLength);
        EnsureVectorContiguous(right, nameof(right), out int rightLength);

        if (leftLength != rightLength)
        {
            throw new ArgumentException("Input vectors must have the same length.");
        }

        DataStructurePerformanceSettings settings = GetPerformanceSettings();
        if (!ShouldUseParallel(leftLength, settings))
        {
            return PerformancePrimitives.Dot(left.AsReadOnlySpan(), right.AsReadOnlySpan(), settings);
        }

        float[] leftStorage = left.DangerousStorage;
        float[] rightStorage = right.DangerousStorage;

        int chunkSize = ComputeChunkSize(leftLength, ResolveParallelism(settings));
        int chunkCount = (leftLength + chunkSize - 1) / chunkSize;
        float[] partialSums = new float[chunkCount];

        Parallel.For(0, chunkCount, CreateParallelOptions(settings), chunkIndex =>
        {
            int start = chunkIndex * chunkSize;
            int count = Math.Min(chunkSize, leftLength - start);
            partialSums[chunkIndex] = PerformancePrimitives.Dot(
                leftStorage.AsSpan(start, count),
                rightStorage.AsSpan(start, count),
                settings);
        });

        float sum = 0f;
        for (int i = 0; i < partialSums.Length; i++)
        {
            sum += partialSums[i];
        }

        return sum;
    }

    public static void TensorAdd(NDBuffer<double> left, NDBuffer<double> right, NDBuffer<double> destination)
    {
        TensorBinary(left, right, destination, BinaryOperation.Add);
    }

    public static void TensorSubtract(NDBuffer<double> left, NDBuffer<double> right, NDBuffer<double> destination)
    {
        TensorBinary(left, right, destination, BinaryOperation.Subtract);
    }

    public static void TensorMultiply(NDBuffer<double> left, NDBuffer<double> right, NDBuffer<double> destination)
    {
        TensorBinary(left, right, destination, BinaryOperation.Multiply);
    }

    public static void TensorDivide(NDBuffer<double> left, NDBuffer<double> right, NDBuffer<double> destination)
    {
        TensorBinary(left, right, destination, BinaryOperation.Divide);
    }

    public static void TensorAdd(NDBuffer<float> left, NDBuffer<float> right, NDBuffer<float> destination)
    {
        TensorBinary(left, right, destination, BinaryOperation.Add);
    }

    public static void TensorSubtract(NDBuffer<float> left, NDBuffer<float> right, NDBuffer<float> destination)
    {
        TensorBinary(left, right, destination, BinaryOperation.Subtract);
    }

    public static void TensorMultiply(NDBuffer<float> left, NDBuffer<float> right, NDBuffer<float> destination)
    {
        TensorBinary(left, right, destination, BinaryOperation.Multiply);
    }

    public static void TensorDivide(NDBuffer<float> left, NDBuffer<float> right, NDBuffer<float> destination)
    {
        TensorBinary(left, right, destination, BinaryOperation.Divide);
    }

    public static void TensorAdd(NDBuffer<Complex> left, NDBuffer<Complex> right, NDBuffer<Complex> destination)
    {
        EnsureTensorShapes(left, right, destination);
        UFuncArithmetic.Add(left.AsReadOnlySpan(), right.AsReadOnlySpan(), destination.AsSpan());
    }

    public static void TensorSubtract(NDBuffer<Complex> left, NDBuffer<Complex> right, NDBuffer<Complex> destination)
    {
        EnsureTensorShapes(left, right, destination);
        UFuncArithmetic.Subtract(left.AsReadOnlySpan(), right.AsReadOnlySpan(), destination.AsSpan());
    }

    public static void TensorMultiply(NDBuffer<Complex> left, NDBuffer<Complex> right, NDBuffer<Complex> destination)
    {
        EnsureTensorShapes(left, right, destination);
        UFuncArithmetic.Multiply(left.AsReadOnlySpan(), right.AsReadOnlySpan(), destination.AsSpan());
    }

    public static void TensorDivide(NDBuffer<Complex> left, NDBuffer<Complex> right, NDBuffer<Complex> destination)
    {
        EnsureTensorShapes(left, right, destination);
        UFuncArithmetic.Divide(left.AsReadOnlySpan(), right.AsReadOnlySpan(), destination.AsSpan());
    }

    public static void OdeEulerStep(double t, double dt, NDBuffer<double> y, NDBuffer<double> yOut, OdeSystem system)
    {
        EnsureVectorContiguous(y, nameof(y), out int yLength);
        EnsureVectorContiguous(yOut, nameof(yOut), out int outLength);

        if (yLength != outLength)
        {
            throw new ArgumentException("Input and output vectors must have the same length.");
        }

        Euler.Step(t, dt, y.AsReadOnlySpan(), yOut.AsSpan(), system);
    }

    public static void OdeEulerStep(float t, float dt, NDBuffer<float> y, NDBuffer<float> yOut, OdeSystemFloat system)
    {
        EnsureVectorContiguous(y, nameof(y), out int yLength);
        EnsureVectorContiguous(yOut, nameof(yOut), out int outLength);

        if (yLength != outLength)
        {
            throw new ArgumentException("Input and output vectors must have the same length.");
        }

        Euler.Step(t, dt, y.AsReadOnlySpan(), yOut.AsSpan(), system);
    }

    public static void OdeEulerStep(double t, double dt, NDBuffer<Complex> y, NDBuffer<Complex> yOut, OdeSystemComplex system)
    {
        EnsureVectorContiguous(y, nameof(y), out int yLength);
        EnsureVectorContiguous(yOut, nameof(yOut), out int outLength);

        if (yLength != outLength)
        {
            throw new ArgumentException("Input and output vectors must have the same length.");
        }

        Euler.Step(t, dt, y.AsReadOnlySpan(), yOut.AsSpan(), system);
    }

    public static double NumericalCorrelation(NDBuffer<double> x, NDBuffer<double> y)
    {
        EnsureVectorContiguous(x, nameof(x), out int xLength);
        EnsureVectorContiguous(y, nameof(y), out int yLength);

        if (xLength != yLength)
        {
            throw new ArgumentException("Input vectors must have the same length.");
        }

        DataStructurePerformanceSettings settings = GetPerformanceSettings();
        bool allowParallel = ShouldUseParallel(xLength, settings);
        return Covariance.Correlation(x.AsReadOnlySpan(), y.AsReadOnlySpan(), allowParallel);
    }

    public static float NumericalCorrelation(NDBuffer<float> x, NDBuffer<float> y)
    {
        EnsureVectorContiguous(x, nameof(x), out int xLength);
        EnsureVectorContiguous(y, nameof(y), out int yLength);

        if (xLength != yLength)
        {
            throw new ArgumentException("Input vectors must have the same length.");
        }

        DataStructurePerformanceSettings settings = GetPerformanceSettings();
        bool allowParallel = ShouldUseParallel(xLength, settings);
        return Covariance.Correlation(x.AsReadOnlySpan(), y.AsReadOnlySpan(), allowParallel);
    }

    public static Complex NumericalCorrelation(NDBuffer<Complex> x, NDBuffer<Complex> y)
    {
        EnsureVectorContiguous(x, nameof(x), out int xLength);
        EnsureVectorContiguous(y, nameof(y), out int yLength);

        if (xLength != yLength)
        {
            throw new ArgumentException("Input vectors must have the same length.");
        }

        DataStructurePerformanceSettings settings = GetPerformanceSettings();
        bool allowParallel = ShouldUseParallel(xLength, settings);
        return Covariance.Correlation(x.AsReadOnlySpan(), y.AsReadOnlySpan(), allowParallel);
    }

    private static void TensorBinary(NDBuffer<double> left, NDBuffer<double> right, NDBuffer<double> destination, BinaryOperation operation)
    {
        EnsureTensorShapes(left, right, destination);

        DataStructurePerformanceSettings settings = GetPerformanceSettings();
        int length = destination.Length;

        if (ShouldUseParallel(length, settings))
        {
            double[] leftStorage = left.DangerousStorage;
            double[] rightStorage = right.DangerousStorage;
            double[] destinationStorage = destination.DangerousStorage;

            ExecuteParallelChunks(length, settings, (start, count) =>
            {
                ApplyBinary(operation, leftStorage.AsSpan(start, count), rightStorage.AsSpan(start, count), destinationStorage.AsSpan(start, count), settings);
            });

            return;
        }

        ApplyBinary(operation, left.AsReadOnlySpan(), right.AsReadOnlySpan(), destination.AsSpan(), settings);
    }

    private static void TensorBinary(NDBuffer<float> left, NDBuffer<float> right, NDBuffer<float> destination, BinaryOperation operation)
    {
        EnsureTensorShapes(left, right, destination);

        DataStructurePerformanceSettings settings = GetPerformanceSettings();
        int length = destination.Length;

        if (ShouldUseParallel(length, settings))
        {
            float[] leftStorage = left.DangerousStorage;
            float[] rightStorage = right.DangerousStorage;
            float[] destinationStorage = destination.DangerousStorage;

            ExecuteParallelChunks(length, settings, (start, count) =>
            {
                ApplyBinary(operation, leftStorage.AsSpan(start, count), rightStorage.AsSpan(start, count), destinationStorage.AsSpan(start, count), settings);
            });

            return;
        }

        ApplyBinary(operation, left.AsReadOnlySpan(), right.AsReadOnlySpan(), destination.AsSpan(), settings);
    }

    private static void ApplyBinary(BinaryOperation operation, ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination, in DataStructurePerformanceSettings settings)
    {
        switch (operation)
        {
            case BinaryOperation.Add:
                PerformancePrimitives.Add(left, right, destination, settings);
                break;
            case BinaryOperation.Subtract:
                PerformancePrimitives.Subtract(left, right, destination, settings);
                break;
            case BinaryOperation.Multiply:
                PerformancePrimitives.Multiply(left, right, destination, settings);
                break;
            case BinaryOperation.Divide:
                PerformancePrimitives.Divide(left, right, destination, settings);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unsupported operation.");
        }
    }

    private static void ApplyBinary(BinaryOperation operation, ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination, in DataStructurePerformanceSettings settings)
    {
        switch (operation)
        {
            case BinaryOperation.Add:
                PerformancePrimitives.Add(left, right, destination, settings);
                break;
            case BinaryOperation.Subtract:
                PerformancePrimitives.Subtract(left, right, destination, settings);
                break;
            case BinaryOperation.Multiply:
                PerformancePrimitives.Multiply(left, right, destination, settings);
                break;
            case BinaryOperation.Divide:
                PerformancePrimitives.Divide(left, right, destination, settings);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unsupported operation.");
        }
    }

    private static void EnsureTensorShapes<T>(NDBuffer<T> left, NDBuffer<T> right, NDBuffer<T> destination)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        ArgumentNullException.ThrowIfNull(destination);

        if (!left.IsRowMajorContiguous || !right.IsRowMajorContiguous || !destination.IsRowMajorContiguous)
        {
            throw new ArgumentException("Tensor compatibility entry requires row-major contiguous buffers.");
        }

        if (!left.Shape.AsSpan().SequenceEqual(right.Shape) || !left.Shape.AsSpan().SequenceEqual(destination.Shape))
        {
            throw new ArgumentException("Input and destination shapes must match.");
        }
    }

    private static void EnsureVectorContiguous<T>(NDBuffer<T> buffer, string paramName, out int length)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (!buffer.TryGetVectorLength(out length))
        {
            throw new ArgumentException("Buffer must be rank-1 vector.", paramName);
        }

        if (!buffer.IsRowMajorContiguous)
        {
            throw new ArgumentException("Buffer must be row-major contiguous.", paramName);
        }
    }

    private static bool ShouldUseParallel(int length, in DataStructurePerformanceSettings settings)
    {
        return settings.EnableParallel
            && length >= settings.ParallelLengthThreshold
            && ResolveParallelism(settings) > 1;
    }

    private static int ResolveParallelism(in DataStructurePerformanceSettings settings)
    {
        if (settings.MaxDegreeOfParallelism.HasValue)
        {
            return Math.Max(1, settings.MaxDegreeOfParallelism.Value);
        }

        return Math.Max(1, Environment.ProcessorCount);
    }

    private static int ComputeChunkSize(int length, int workers)
    {
        int workerCount = Math.Max(1, workers);
        int dynamicChunk = length / (workerCount * 4);
        return Math.Max(1024, dynamicChunk);
    }

    private static ParallelOptions CreateParallelOptions(in DataStructurePerformanceSettings settings)
    {
        return new ParallelOptions
        {
            MaxDegreeOfParallelism = ResolveParallelism(settings),
        };
    }

    private static void ExecuteParallelChunks(int length, in DataStructurePerformanceSettings settings, Action<int, int> worker)
    {
        int chunkSize = ComputeChunkSize(length, ResolveParallelism(settings));
        int chunkCount = (length + chunkSize - 1) / chunkSize;

        Parallel.For(0, chunkCount, CreateParallelOptions(settings), chunkIndex =>
        {
            int start = chunkIndex * chunkSize;
            int count = Math.Min(chunkSize, length - start);
            worker(start, count);
        });
    }

    private static int CountModulesWithOptimizationFlag(ReadOnlySpan<ModuleAllocationGcProfile> profiles, AllocationOptimizationFlags flag, bool useRecommended)
    {
        int count = 0;

        for (int i = 0; i < profiles.Length; i++)
        {
            AllocationOptimizationFlags value = useRecommended
                ? profiles[i].RecommendedOptimizations
                : profiles[i].CurrentOptimizations;

            if ((value & flag) != 0)
            {
                count++;
            }
        }

        return count;
    }

    private static ModuleAllocationGcProfile[] CreateModuleAllocationGcProfiles()
    {
        ModuleAllocationGcProfile[] profiles = new ModuleAllocationGcProfile[s_moduleProfiles.Length];

        for (int i = 0; i < s_moduleProfiles.Length; i++)
        {
            profiles[i] = CreateModuleAllocationGcProfile(s_moduleProfiles[i]);
        }

        return profiles;
    }

    private static ModuleAllocationGcProfile CreateModuleAllocationGcProfile(in ModulePerformanceProfile perfProfile)
    {
        AllocationOptimizationFlags currentOptimizations = GetCurrentAllocationOptimizations(perfProfile.Core, perfProfile.Module);
        AllocationOptimizationFlags recommendedOptimizations = GetRecommendedAllocationOptimizations(perfProfile, currentOptimizations);
        AllocationGcThresholds thresholds = GetAllocationGcThresholds(perfProfile.Core, perfProfile.Module);
        bool rule6SpanFirstDefaults = (perfProfile.CurrentStrategies & PerformanceStrategyFlags.Scalar) != 0;
        bool unsafeRulesCompliant = IsUnsafeGovernanceCompliant(perfProfile, recommendedOptimizations);

        string notes = string.Concat(
            perfProfile.Notes,
            " Allocation/GC indicators: stackalloc<=",
            thresholds.StackAllocMaxElements.ToString(),
            ", ArrayPool>=",
            thresholds.ArrayPoolMinElements.ToString(),
            ", workspace reuse>=",
            thresholds.WorkspaceReuseMinElements.ToString(),
            ". Rule4/5/6 and unsafe governance are tracked through this profile.");

        return new ModuleAllocationGcProfile(
            Core: perfProfile.Core,
            Module: perfProfile.Module,
            CurrentOptimizations: currentOptimizations,
            RecommendedOptimizations: recommendedOptimizations,
            Thresholds: thresholds,
            Rule4SharedDataStructures: true,
            Rule5SpanBoundaries: true,
            Rule6SpanFirstDefaults: rule6SpanFirstDefaults,
            UnsafeRulesCompliant: unsafeRulesCompliant,
            Notes: notes);
    }

    private static AllocationOptimizationFlags GetCurrentAllocationOptimizations(string core, string module)
    {
        string key = string.Concat(core, ":", module);

        return key switch
        {
            "LinalgCore:Gemm" => AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse | AllocationOptimizationFlags.ThreadLocalScratch,
            "LinalgCore:Gemv" => AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse | AllocationOptimizationFlags.ThreadLocalScratch,

            "NumericalCore:Interpolation.Rbf" => AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,
            "NumericalCore:Interpolation.Spline" => AllocationOptimizationFlags.StackAlloc | AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,
            "NumericalCore:Statistics.Covariance" => AllocationOptimizationFlags.StackAlloc | AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,
            "NumericalCore:Statistics.PearsonCorrelation" => AllocationOptimizationFlags.StackAlloc | AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,

            "OdeCore:Bdf" => AllocationOptimizationFlags.StackAlloc | AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,
            "OdeCore:DenseOutput" => AllocationOptimizationFlags.StackAlloc | AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,
            "OdeCore:Euler" => AllocationOptimizationFlags.StackAlloc | AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,
            "OdeCore:JacobianEstimator" => AllocationOptimizationFlags.StackAlloc | AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,
            "OdeCore:Radau" => AllocationOptimizationFlags.StackAlloc | AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,
            "OdeCore:Rk4" => AllocationOptimizationFlags.StackAlloc | AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,
            "OdeCore:Rk45" => AllocationOptimizationFlags.StackAlloc | AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,

            "TensorCore:ConcatStack" => AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,
            "TensorCore:Convolution" => AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,
            "TensorCore:Einsum" => AllocationOptimizationFlags.StackAlloc | AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,
            "TensorCore:Fft" => AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,
            "TensorCore:Padding" => AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,
            "TensorCore:Reductions" => AllocationOptimizationFlags.StackAlloc | AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,
            "TensorCore:SortSearch" => AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse,

            _ => AllocationOptimizationFlags.WorkspaceReuse,
        };
    }

    private static AllocationOptimizationFlags GetRecommendedAllocationOptimizations(in ModulePerformanceProfile perfProfile, AllocationOptimizationFlags currentOptimizations)
    {
        AllocationOptimizationFlags recommended = currentOptimizations;

        if ((perfProfile.RecommendedStrategies & PerformanceStrategyFlags.Parallel) != 0)
        {
            recommended |= AllocationOptimizationFlags.ArrayPool;
            recommended |= AllocationOptimizationFlags.WorkspaceReuse;
        }

        if ((perfProfile.RecommendedStrategies & (PerformanceStrategyFlags.Simd | PerformanceStrategyFlags.Intrinsics)) != 0)
        {
            recommended |= AllocationOptimizationFlags.WorkspaceReuse;
        }

        if (ShouldRecommendStackAlloc(perfProfile.Core, perfProfile.Module))
        {
            recommended |= AllocationOptimizationFlags.StackAlloc;
        }

        if (ShouldRecommendThreadLocalScratch(perfProfile.Core, perfProfile.Module))
        {
            recommended |= AllocationOptimizationFlags.ThreadLocalScratch;
        }

        if (recommended == AllocationOptimizationFlags.None)
        {
            recommended = AllocationOptimizationFlags.WorkspaceReuse;
        }

        return recommended;
    }

    private static bool ShouldRecommendStackAlloc(string core, string module)
    {
        if (string.Equals(core, "OdeCore", StringComparison.Ordinal))
        {
            return true;
        }

        if (string.Equals(core, "NumericalCore", StringComparison.Ordinal))
        {
            return true;
        }

        return module switch
        {
            "Axpy" => true,
            "Dot" => true,
            "Norms" => true,
            "VectorOps" => true,
            "MatrixAnalysis" => true,
            "Reductions" => true,
            "Cumulative" => true,
            "MaskOps" => true,
            "SortSearch" => true,
            _ => false,
        };
    }

    private static bool ShouldRecommendThreadLocalScratch(string core, string module)
    {
        string key = string.Concat(core, ":", module);
        return key is "LinalgCore:Gemm"
            or "LinalgCore:Gemv"
            or "TensorCore:Convolution"
            or "TensorCore:Einsum";
    }

    private static AllocationGcThresholds GetAllocationGcThresholds(string core, string module)
    {
        string key = string.Concat(core, ":", module);

        return key switch
        {
            "LinalgCore:Gemm" => new AllocationGcThresholds(StackAllocMaxElements: 64, ArrayPoolMinElements: 4096, WorkspaceReuseMinElements: 8192),
            "LinalgCore:Gemv" => new AllocationGcThresholds(StackAllocMaxElements: 128, ArrayPoolMinElements: 1024, WorkspaceReuseMinElements: 2048),

            "NumericalCore:Statistics.Covariance" => new AllocationGcThresholds(StackAllocMaxElements: 256, ArrayPoolMinElements: 1024, WorkspaceReuseMinElements: 2048),
            "NumericalCore:Statistics.PearsonCorrelation" => new AllocationGcThresholds(StackAllocMaxElements: 256, ArrayPoolMinElements: 1024, WorkspaceReuseMinElements: 2048),

            "OdeCore:JacobianEstimator" => new AllocationGcThresholds(StackAllocMaxElements: 256, ArrayPoolMinElements: 1024, WorkspaceReuseMinElements: 2048),
            "OdeCore:Rk45" => new AllocationGcThresholds(StackAllocMaxElements: 256, ArrayPoolMinElements: 1024, WorkspaceReuseMinElements: 2048),

            "TensorCore:Convolution" => new AllocationGcThresholds(StackAllocMaxElements: 96, ArrayPoolMinElements: 2048, WorkspaceReuseMinElements: 4096),
            "TensorCore:Einsum" => new AllocationGcThresholds(StackAllocMaxElements: 128, ArrayPoolMinElements: 2048, WorkspaceReuseMinElements: 4096),
            "TensorCore:Fft" => new AllocationGcThresholds(StackAllocMaxElements: 128, ArrayPoolMinElements: 2048, WorkspaceReuseMinElements: 4096),

            _ => GetDefaultAllocationGcThresholdsForCore(core),
        };
    }

    private static AllocationGcThresholds GetDefaultAllocationGcThresholdsForCore(string core)
    {
        return core switch
        {
            "LinalgCore" => new AllocationGcThresholds(StackAllocMaxElements: 192, ArrayPoolMinElements: 1024, WorkspaceReuseMinElements: 2048),
            "NumericalCore" => new AllocationGcThresholds(StackAllocMaxElements: 192, ArrayPoolMinElements: 512, WorkspaceReuseMinElements: 1024),
            "OdeCore" => new AllocationGcThresholds(StackAllocMaxElements: 256, ArrayPoolMinElements: 512, WorkspaceReuseMinElements: 1024),
            "TensorCore" => new AllocationGcThresholds(StackAllocMaxElements: 128, ArrayPoolMinElements: 1024, WorkspaceReuseMinElements: 2048),
            _ => new AllocationGcThresholds(StackAllocMaxElements: 128, ArrayPoolMinElements: 512, WorkspaceReuseMinElements: 1024),
        };
    }

    private static bool IsUnsafeGovernanceCompliant(in ModulePerformanceProfile perfProfile, AllocationOptimizationFlags recommendedOptimizations)
    {
        bool usesUnsafeOrIntrinsics = (perfProfile.CurrentStrategies & (PerformanceStrategyFlags.Unsafe | PerformanceStrategyFlags.Intrinsics)) != 0;
        if (!usesUnsafeOrIntrinsics)
        {
            return true;
        }

        bool hasScalarFallback = (perfProfile.CurrentStrategies & PerformanceStrategyFlags.Scalar) != 0;
        bool keepsScalarFallbackInRecommendation = (perfProfile.RecommendedStrategies & PerformanceStrategyFlags.Scalar) != 0;
        bool hasGcMitigation = (recommendedOptimizations & (AllocationOptimizationFlags.ArrayPool | AllocationOptimizationFlags.WorkspaceReuse | AllocationOptimizationFlags.ThreadLocalScratch)) != 0;

        return hasScalarFallback && keepsScalarFallbackInRecommendation && hasGcMitigation;
    }

    private static void ValidatePerformanceSettings(in DataStructurePerformanceSettings settings)
    {
        if (settings.SimdLengthThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "SimdLengthThreshold must be positive.");
        }

        if (settings.IntrinsicsLengthThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "IntrinsicsLengthThreshold must be positive.");
        }

        if (settings.ParallelLengthThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "ParallelLengthThreshold must be positive.");
        }

        if (settings.MaxDegreeOfParallelism.HasValue && settings.MaxDegreeOfParallelism.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "MaxDegreeOfParallelism must be positive when specified.");
        }
    }

    private static ModulePerformanceProfile[] CreateModuleProfiles()
    {
        PerformanceStrategyFlags scalar = PerformanceStrategyFlags.Scalar;
        PerformanceStrategyFlags simd = PerformanceStrategyFlags.Simd;
        PerformanceStrategyFlags intrinsics = PerformanceStrategyFlags.Intrinsics;
        PerformanceStrategyFlags unsafeFlag = PerformanceStrategyFlags.Unsafe;
        PerformanceStrategyFlags parallel = PerformanceStrategyFlags.Parallel;

        PerformanceStrategyFlags full = scalar | simd | intrinsics | unsafeFlag | parallel;
        PerformanceStrategyFlags scalarSimd = scalar | simd;
        PerformanceStrategyFlags scalarParallel = scalar | parallel;
        PerformanceStrategyFlags scalarSimdParallel = scalar | simd | parallel;

        return
        [
            new("LinalgCore", "Axpy", full, full, "DataStructureCompatibility entry now uses unsafe+intrinsics kernel with optional parallel chunking."),
            new("LinalgCore", "Cholesky", scalar, scalarSimd, "Factorization path remains scalar; SIMD blocking is recommended."),
            new("LinalgCore", "DenseSolver", scalar, scalarSimdParallel, "Solver benefits from batched row operations and parallel panel updates."),
            new("LinalgCore", "DistanceMetrics", scalar, scalarSimdParallel, "Pairwise kernels are good SIMD+parallel candidates."),
            new("LinalgCore", "Dot", full, full, "Dot path now supports unsafe+intrinsics and parallel reduction entry."),
            new("LinalgCore", "EigenSolver", scalarSimd, scalarSimdParallel, "Iteration can leverage SIMD residual kernels and optional parallel matrix-vector products."),
            new("LinalgCore", "Gemm", full, full, "Stage-2: transposed-B micro-kernel now uses shared unsafe+intrinsics dot path with strategy-gated parallel rows."),
            new("LinalgCore", "Gemv", full, full, "Stage-2: row dot kernels are routed through shared unsafe+intrinsics dot kernel with global strategy gating."),
            new("LinalgCore", "Lu", scalar, scalarSimdParallel, "LU panel and trailing update can be vectorized and parallelized."),
            new("LinalgCore", "MatrixAnalysis", scalarSimd, scalarSimdParallel, "Trace/norm parts are SIMD-ready; decomposition branches can parallelize."),
            new("LinalgCore", "MatrixOps", scalarSimd, full, "Elementwise and multiply branches should share intrinsic kernels."),
            new("LinalgCore", "Norms", scalarSimd, full, "Reduction kernels fit intrinsic accumulation and parallel reduction."),
            new("LinalgCore", "Orthogonalization", scalar, scalarSimdParallel, "Projection/dot loops are suitable for SIMD+parallel batches."),
            new("LinalgCore", "Qr", scalar, scalarSimdParallel, "Householder/Gram-Schmidt steps can vectorize norm and axpy loops."),
            new("LinalgCore", "Schur", scalar, scalarSimd, "Small-matrix kernels can use intrinsic fused updates."),
            new("LinalgCore", "Sparse.Spmv", scalar, scalarParallel, "Sparse kernels are memory-bound; thread-level partitioning is primary lever."),
            new("LinalgCore", "Svd", scalar, scalarSimdParallel, "Jacobi sweeps can SIMD vector blocks and parallel sweep pairs."),
            new("LinalgCore", "Transpose", scalarSimd, scalarSimdParallel, "Blocked transpose can combine SIMD copy and tile-level parallelism."),
            new("LinalgCore", "VectorOps", scalarSimd, full, "Elementwise vector ops can share intrinsic kernels from compatibility entry."),

            new("NumericalCore", "Differentiation.FiniteDifference", scalar, scalarSimd, "Stencil batches can vectorize function-evaluation deltas."),
            new("NumericalCore", "Integration.BasicQuadrature", scalar, scalarSimdParallel, "Interval partitions are naturally parallel and SIMD-friendly."),
            new("NumericalCore", "Integration.GaussianQuadrature", scalar, scalarSimdParallel, "Node/weight accumulations are vectorizable."),
            new("NumericalCore", "Interpolation.Rbf", scalarParallel, scalarSimdParallel, "Distance kernel and matrix assembly benefit from SIMD."),
            new("NumericalCore", "Interpolation.Spline", scalar, scalarSimd, "Tridiagonal sweeps can vectorize coefficient updates."),
            new("NumericalCore", "Optimization.GradientDescent", scalar, scalarSimdParallel, "Batch gradient evaluations can use SIMD+parallel loops."),
            new("NumericalCore", "Optimization.Lbfgs", scalar, scalarSimdParallel, "Two-loop recursion uses dot/axpy style kernels."),
            new("NumericalCore", "Random.Rng", scalar, scalarSimd, "Batch sampling can vectorize state transitions and transforms."),
            new("NumericalCore", "RootFinding.Brent", scalar, scalar, "Branch-heavy scalar algorithm; keep scalar with batched outer parallelism."),
            new("NumericalCore", "RootFinding.Newton", scalar, scalarSimd, "Batched Newton solves can vectorize derivative evaluations."),
            new("NumericalCore", "RootFinding.Secant", scalar, scalar, "Scalar recurrence; optimize by batching independent solves."),
            new("NumericalCore", "Statistics.Covariance", scalarSimdParallel, full, "Already vectorized/parallel; compatibility entry controls parallel gate."),
            new("NumericalCore", "Statistics.PearsonCorrelation", scalarSimdParallel, full, "Correlation kernels map to vectorized covariance reductions."),
            new("NumericalCore", "Statistics.Ziggurat", scalar, scalarSimd, "Random transforms can use SIMD in bulk generation mode."),

            new("OdeCore", "Bdf", scalarSimd, scalarSimdParallel, "Newton iterations can parallelize residual/Jacobian style loops."),
            new("OdeCore", "DenseOutput", scalarSimd, full, "Interpolation is a direct vector operation target."),
            new("OdeCore", "Euler", full, full, "Integrate path now uses shared unsafe+intrinsics kernel."),
            new("OdeCore", "JacobianEstimator", scalarParallel, scalarSimdParallel, "Finite difference columns parallelized; SIMD for vector diffs recommended."),
            new("OdeCore", "Radau", scalarSimd, scalarSimdParallel, "Stage updates can share optimized fused vector kernels."),
            new("OdeCore", "Rk4", scalarSimd, scalarSimdParallel, "Stage accumulation loops are SIMD-ready and parallelizable for large states."),
            new("OdeCore", "Rk45", scalarSimd, scalarSimdParallel, "Error norm and stage updates can use intrinsic kernels."),
            new("OdeCore", "StepController", scalar, scalar, "Control law is scalar and branch-dominant."),

            new("TensorCore", "Broadcasting", scalar, scalarSimdParallel, "Expanded index kernels can parallelize over output span."),
            new("TensorCore", "ComplexOps", scalar, scalarSimd, "Complex split/combine can vectorize real/imag projection."),
            new("TensorCore", "ConcatStack", scalar, scalarSimdParallel, "Block copies are good SIMD+parallel copy candidates."),
            new("TensorCore", "Convolution", full, full, "Stage-2: 1D/2D/ND parallel+SIMD policy now also honors global DataStructureCompatibility strategy gates."),
            new("TensorCore", "Cumulative", scalar, scalarSimdParallel, "Prefix kernels can parallelize segmented scans."),
            new("TensorCore", "Einsum", scalarSimdParallel, full, "Delegated GEMM contractions can consume intrinsic kernels."),
            new("TensorCore", "Fft", scalarSimd, scalarSimdParallel, "Butterfly kernels are vector-friendly and parallel by radix blocks."),
            new("TensorCore", "MaskOps", scalar, scalarSimdParallel, "Predicate application can SIMD compare and parallel output chunks."),
            new("TensorCore", "Padding", scalar, scalarSimdParallel, "Copy/fill operations parallelize well on large tensors."),
            new("TensorCore", "Reductions", scalarSimd, full, "Reduction kernels can use intrinsic accumulators + parallel reduce."),
            new("TensorCore", "ShapeOps", scalar, scalarSimdParallel, "Transpose/reshape copy loops can share SIMD memory kernels."),
            new("TensorCore", "SortSearch", scalarSimd, scalarSimdParallel, "Search loops can SIMD compare and partition in parallel."),
            new("TensorCore", "StridedView", scalar, scalar, "View generation is index arithmetic dominated."),
            new("TensorCore", "TensorShape", scalar, scalar, "Shape computations are scalar control logic."),
            new("TensorCore", "UFuncArithmetic", full, full, "Elementwise arithmetic now uses shared unsafe+intrinsics kernels."),
            new("TensorCore", "UFuncTranscendental", scalar, scalarSimd, "Transcendental paths can use SIMD approximations for bulk mode."),
        ];
    }
}
