using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace LAL.Core;

internal enum BinaryOperation
{
    Add,
    Subtract,
    Multiply,
    Divide
}

internal static class PerformancePrimitives
{
    public static void Axpy(double alpha, ReadOnlySpan<double> x, Span<double> y, in DataStructurePerformanceSettings settings)
    {
        int length = y.Length;
        int i = 0;

        if (settings.EnableUnsafe
            && settings.EnableIntrinsics
            && Avx.IsSupported
            && length >= Math.Max(settings.IntrinsicsLengthThreshold, Vector256<double>.Count * 4))
        {
            unsafe
            {
                fixed (double* px = x)
                fixed (double* py = y)
                {
                    Vector256<double> alphaVec = Vector256.Create(alpha);
                    int width = Vector256<double>.Count;
                    int end = length - width;

                    for (; i <= end; i += width)
                    {
                        Vector256<double> xVec = Avx.LoadVector256(px + i);
                        Vector256<double> yVec = Avx.LoadVector256(py + i);
                        Vector256<double> result = Avx.Add(Avx.Multiply(alphaVec, xVec), yVec);
                        Avx.Store(py + i, result);
                    }
                }
            }
        }

        if (settings.EnableSimd
            && Vector.IsHardwareAccelerated
            && (length - i) >= Math.Max(settings.SimdLengthThreshold, Vector<double>.Count * 2))
        {
            int width = Vector<double>.Count;
            int end = length - width;
            Vector<double> alphaVec = new(alpha);

            for (; i <= end; i += width)
            {
                (alphaVec * new Vector<double>(x.Slice(i, width)) + new Vector<double>(y.Slice(i, width)))
                    .CopyTo(y.Slice(i, width));
            }
        }

        for (; i < length; i++)
        {
            y[i] = (alpha * x[i]) + y[i];
        }
    }

    public static void Axpy(float alpha, ReadOnlySpan<float> x, Span<float> y, in DataStructurePerformanceSettings settings)
    {
        int length = y.Length;
        int i = 0;

        if (settings.EnableUnsafe
            && settings.EnableIntrinsics
            && Avx.IsSupported
            && length >= Math.Max(settings.IntrinsicsLengthThreshold, Vector256<float>.Count * 4))
        {
            unsafe
            {
                fixed (float* px = x)
                fixed (float* py = y)
                {
                    Vector256<float> alphaVec = Vector256.Create(alpha);
                    int width = Vector256<float>.Count;
                    int end = length - width;

                    for (; i <= end; i += width)
                    {
                        Vector256<float> xVec = Avx.LoadVector256(px + i);
                        Vector256<float> yVec = Avx.LoadVector256(py + i);
                        Vector256<float> result = Avx.Add(Avx.Multiply(alphaVec, xVec), yVec);
                        Avx.Store(py + i, result);
                    }
                }
            }
        }

        if (settings.EnableSimd
            && Vector.IsHardwareAccelerated
            && (length - i) >= Math.Max(settings.SimdLengthThreshold, Vector<float>.Count * 2))
        {
            int width = Vector<float>.Count;
            int end = length - width;
            Vector<float> alphaVec = new(alpha);

            for (; i <= end; i += width)
            {
                (alphaVec * new Vector<float>(x.Slice(i, width)) + new Vector<float>(y.Slice(i, width)))
                    .CopyTo(y.Slice(i, width));
            }
        }

        for (; i < length; i++)
        {
            y[i] = (alpha * x[i]) + y[i];
        }
    }

    public static double Dot(ReadOnlySpan<double> left, ReadOnlySpan<double> right, in DataStructurePerformanceSettings settings)
    {
        int length = left.Length;
        int i = 0;
        double sum = 0d;

        if (settings.EnableUnsafe
            && settings.EnableIntrinsics
            && Avx.IsSupported
            && length >= Math.Max(settings.IntrinsicsLengthThreshold, Vector256<double>.Count * 4))
        {
            unsafe
            {
                fixed (double* pl = left)
                fixed (double* pr = right)
                {
                    Vector256<double> acc = Vector256<double>.Zero;
                    int width = Vector256<double>.Count;
                    int end = length - width;

                    for (; i <= end; i += width)
                    {
                        Vector256<double> l = Avx.LoadVector256(pl + i);
                        Vector256<double> r = Avx.LoadVector256(pr + i);
                        acc = Fma.IsSupported
                            ? Fma.MultiplyAdd(l, r, acc)
                            : Avx.Add(acc, Avx.Multiply(l, r));
                    }

                    double* lane = stackalloc double[Vector256<double>.Count];
                    Avx.Store(lane, acc);
                    for (int laneIndex = 0; laneIndex < Vector256<double>.Count; laneIndex++)
                    {
                        sum += lane[laneIndex];
                    }
                }
            }
        }

        if (settings.EnableSimd
            && Vector.IsHardwareAccelerated
            && (length - i) >= Math.Max(settings.SimdLengthThreshold, Vector<double>.Count * 2))
        {
            int width = Vector<double>.Count;
            int end = length - width;
            Vector<double> acc = Vector<double>.Zero;

            for (; i <= end; i += width)
            {
                acc += new Vector<double>(left.Slice(i, width)) * new Vector<double>(right.Slice(i, width));
            }

            for (int laneIndex = 0; laneIndex < width; laneIndex++)
            {
                sum += acc[laneIndex];
            }
        }

        for (; i < length; i++)
        {
            sum += left[i] * right[i];
        }

        return sum;
    }

    public static float Dot(ReadOnlySpan<float> left, ReadOnlySpan<float> right, in DataStructurePerformanceSettings settings)
    {
        int length = left.Length;
        int i = 0;
        float sum = 0f;

        if (settings.EnableUnsafe
            && settings.EnableIntrinsics
            && Avx.IsSupported
            && length >= Math.Max(settings.IntrinsicsLengthThreshold, Vector256<float>.Count * 4))
        {
            unsafe
            {
                fixed (float* pl = left)
                fixed (float* pr = right)
                {
                    Vector256<float> acc = Vector256<float>.Zero;
                    int width = Vector256<float>.Count;
                    int end = length - width;

                    for (; i <= end; i += width)
                    {
                        Vector256<float> l = Avx.LoadVector256(pl + i);
                        Vector256<float> r = Avx.LoadVector256(pr + i);
                        acc = Fma.IsSupported
                            ? Fma.MultiplyAdd(l, r, acc)
                            : Avx.Add(acc, Avx.Multiply(l, r));
                    }

                    float* lane = stackalloc float[Vector256<float>.Count];
                    Avx.Store(lane, acc);
                    for (int laneIndex = 0; laneIndex < Vector256<float>.Count; laneIndex++)
                    {
                        sum += lane[laneIndex];
                    }
                }
            }
        }

        if (settings.EnableSimd
            && Vector.IsHardwareAccelerated
            && (length - i) >= Math.Max(settings.SimdLengthThreshold, Vector<float>.Count * 2))
        {
            int width = Vector<float>.Count;
            int end = length - width;
            Vector<float> acc = Vector<float>.Zero;

            for (; i <= end; i += width)
            {
                acc += new Vector<float>(left.Slice(i, width)) * new Vector<float>(right.Slice(i, width));
            }

            for (int laneIndex = 0; laneIndex < width; laneIndex++)
            {
                sum += acc[laneIndex];
            }
        }

        for (; i < length; i++)
        {
            sum += left[i] * right[i];
        }

        return sum;
    }

    public static void Add(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination, in DataStructurePerformanceSettings settings)
    {
        Binary(left, right, destination, BinaryOperation.Add, settings);
    }

    public static void Subtract(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination, in DataStructurePerformanceSettings settings)
    {
        Binary(left, right, destination, BinaryOperation.Subtract, settings);
    }

    public static void Multiply(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination, in DataStructurePerformanceSettings settings)
    {
        Binary(left, right, destination, BinaryOperation.Multiply, settings);
    }

    public static void Divide(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination, in DataStructurePerformanceSettings settings)
    {
        Binary(left, right, destination, BinaryOperation.Divide, settings);
    }

    public static void Add(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination, in DataStructurePerformanceSettings settings)
    {
        Binary(left, right, destination, BinaryOperation.Add, settings);
    }

    public static void Subtract(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination, in DataStructurePerformanceSettings settings)
    {
        Binary(left, right, destination, BinaryOperation.Subtract, settings);
    }

    public static void Multiply(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination, in DataStructurePerformanceSettings settings)
    {
        Binary(left, right, destination, BinaryOperation.Multiply, settings);
    }

    public static void Divide(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination, in DataStructurePerformanceSettings settings)
    {
        Binary(left, right, destination, BinaryOperation.Divide, settings);
    }

    public static void ScaledAdd(ReadOnlySpan<double> y, ReadOnlySpan<double> dydt, double dt, Span<double> yOut, in DataStructurePerformanceSettings settings)
    {
        y.CopyTo(yOut);
        Axpy(dt, dydt, yOut, settings);
    }

    public static void ScaledAdd(ReadOnlySpan<float> y, ReadOnlySpan<float> dydt, float dt, Span<float> yOut, in DataStructurePerformanceSettings settings)
    {
        y.CopyTo(yOut);
        Axpy(dt, dydt, yOut, settings);
    }

    private static void Binary(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination, BinaryOperation operation, in DataStructurePerformanceSettings settings)
    {
        int length = destination.Length;
        int i = 0;

        if (settings.EnableUnsafe
            && settings.EnableIntrinsics
            && Avx.IsSupported
            && length >= Math.Max(settings.IntrinsicsLengthThreshold, Vector256<double>.Count * 4))
        {
            unsafe
            {
                fixed (double* pl = left)
                fixed (double* pr = right)
                fixed (double* pd = destination)
                {
                    int width = Vector256<double>.Count;
                    int end = length - width;

                    for (; i <= end; i += width)
                    {
                        Vector256<double> l = Avx.LoadVector256(pl + i);
                        Vector256<double> r = Avx.LoadVector256(pr + i);
                        Vector256<double> result = operation switch
                        {
                            BinaryOperation.Add => Avx.Add(l, r),
                            BinaryOperation.Subtract => Avx.Subtract(l, r),
                            BinaryOperation.Multiply => Avx.Multiply(l, r),
                            BinaryOperation.Divide => Avx.Divide(l, r),
                            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unsupported binary operation."),
                        };

                        Avx.Store(pd + i, result);
                    }
                }
            }
        }

        if (settings.EnableSimd
            && Vector.IsHardwareAccelerated
            && (length - i) >= Math.Max(settings.SimdLengthThreshold, Vector<double>.Count * 2))
        {
            int width = Vector<double>.Count;
            int end = length - width;

            for (; i <= end; i += width)
            {
                Vector<double> l = new(left.Slice(i, width));
                Vector<double> r = new(right.Slice(i, width));
                Vector<double> result = operation switch
                {
                    BinaryOperation.Add => l + r,
                    BinaryOperation.Subtract => l - r,
                    BinaryOperation.Multiply => l * r,
                    BinaryOperation.Divide => l / r,
                    _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unsupported binary operation."),
                };

                result.CopyTo(destination.Slice(i, width));
            }
        }

        for (; i < length; i++)
        {
            destination[i] = operation switch
            {
                BinaryOperation.Add => left[i] + right[i],
                BinaryOperation.Subtract => left[i] - right[i],
                BinaryOperation.Multiply => left[i] * right[i],
                BinaryOperation.Divide => left[i] / right[i],
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unsupported binary operation."),
            };
        }
    }

    private static void Binary(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination, BinaryOperation operation, in DataStructurePerformanceSettings settings)
    {
        int length = destination.Length;
        int i = 0;

        if (settings.EnableUnsafe
            && settings.EnableIntrinsics
            && Avx.IsSupported
            && length >= Math.Max(settings.IntrinsicsLengthThreshold, Vector256<float>.Count * 4))
        {
            unsafe
            {
                fixed (float* pl = left)
                fixed (float* pr = right)
                fixed (float* pd = destination)
                {
                    int width = Vector256<float>.Count;
                    int end = length - width;

                    for (; i <= end; i += width)
                    {
                        Vector256<float> l = Avx.LoadVector256(pl + i);
                        Vector256<float> r = Avx.LoadVector256(pr + i);
                        Vector256<float> result = operation switch
                        {
                            BinaryOperation.Add => Avx.Add(l, r),
                            BinaryOperation.Subtract => Avx.Subtract(l, r),
                            BinaryOperation.Multiply => Avx.Multiply(l, r),
                            BinaryOperation.Divide => Avx.Divide(l, r),
                            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unsupported binary operation."),
                        };

                        Avx.Store(pd + i, result);
                    }
                }
            }
        }

        if (settings.EnableSimd
            && Vector.IsHardwareAccelerated
            && (length - i) >= Math.Max(settings.SimdLengthThreshold, Vector<float>.Count * 2))
        {
            int width = Vector<float>.Count;
            int end = length - width;

            for (; i <= end; i += width)
            {
                Vector<float> l = new(left.Slice(i, width));
                Vector<float> r = new(right.Slice(i, width));
                Vector<float> result = operation switch
                {
                    BinaryOperation.Add => l + r,
                    BinaryOperation.Subtract => l - r,
                    BinaryOperation.Multiply => l * r,
                    BinaryOperation.Divide => l / r,
                    _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unsupported binary operation."),
                };

                result.CopyTo(destination.Slice(i, width));
            }
        }

        for (; i < length; i++)
        {
            destination[i] = operation switch
            {
                BinaryOperation.Add => left[i] + right[i],
                BinaryOperation.Subtract => left[i] - right[i],
                BinaryOperation.Multiply => left[i] * right[i],
                BinaryOperation.Divide => left[i] / right[i],
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unsupported binary operation."),
            };
        }
    }
}
