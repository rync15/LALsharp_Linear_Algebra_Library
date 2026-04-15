using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class MatrixAnalysisTests
{
    [Fact]
    public void TraceAndRank_Work()
    {
        double[] matrix =
        [
            1.0, 2.0,
            2.0, 4.0
        ];

        double trace = MatrixAnalysis.Trace(matrix, n: 2);
        int rank = MatrixAnalysis.Rank(matrix, rows: 2, cols: 2);

        Assert.Equal(5.0, trace, 12);
        Assert.Equal(1, rank);
    }

    [Fact]
    public void PseudoInverseSquare_WorksForDiagonalMatrix()
    {
        double[] matrix =
        [
            2.0, 0.0,
            0.0, 4.0
        ];
        double[] pseudoInverse = new double[4];

        bool ok = MatrixAnalysis.PseudoInverseSquare(matrix, n: 2, pseudoInverse);

        Assert.True(ok);
        Assert.InRange(pseudoInverse[0], 0.5 - 1e-12, 0.5 + 1e-12);
        Assert.InRange(pseudoInverse[3], 0.25 - 1e-12, 0.25 + 1e-12);
    }

    [Fact]
    public void DeterminantAndInverse_Work()
    {
        double[] matrix =
        [
            4.0, 7.0,
            2.0, 6.0
        ];

        double det = MatrixAnalysis.Determinant(matrix, n: 2);
        Assert.InRange(det, 10.0 - 1e-12, 10.0 + 1e-12);

        double[] inverse = new double[4];
        bool ok = MatrixAnalysis.Inverse(matrix, n: 2, inverse);

        Assert.True(ok);
        Assert.InRange(inverse[0], 0.6 - 1e-12, 0.6 + 1e-12);
        Assert.InRange(inverse[1], -0.7 - 1e-12, -0.7 + 1e-12);
        Assert.InRange(inverse[2], -0.2 - 1e-12, -0.2 + 1e-12);
        Assert.InRange(inverse[3], 0.4 - 1e-12, 0.4 + 1e-12);
    }

    [Fact]
    public void PseudoInverse_RectangularTall_Works()
    {
        double[] matrix =
        [
            1.0, 0.0,
            0.0, 1.0,
            1.0, 1.0
        ];

        double[] pseudoInverse = new double[2 * 3];
        bool ok = MatrixAnalysis.PseudoInverse(matrix, rows: 3, cols: 2, pseudoInverse);

        Assert.True(ok);

        double[] identityApprox = new double[4];
        MatrixOps.Multiply(pseudoInverse, leftRows: 2, sharedDim: 3, matrix, rightCols: 2, identityApprox);

        Assert.InRange(identityApprox[0], 1.0 - 1e-10, 1.0 + 1e-10);
        Assert.InRange(identityApprox[1], -1e-10, 1e-10);
        Assert.InRange(identityApprox[2], -1e-10, 1e-10);
        Assert.InRange(identityApprox[3], 1.0 - 1e-10, 1.0 + 1e-10);
    }

    [Fact]
    public void DominantEigenvalue_WorksForDiagonalMatrix()
    {
        double[] matrix =
        [
            5.0, 0.0,
            0.0, 2.0
        ];

        double[] eigenvector = new double[2];
        EigenResult result = MatrixAnalysis.DominantEigenvalue(matrix, n: 2, eigenvector, tolerance: 1e-12, maxIterations: 200);

        Assert.True(result.Converged);
        Assert.InRange(result.Eigenvalue, 5.0 - 1e-8, 5.0 + 1e-8);
    }

    [Fact]
    public void Eigenvalues2x2_WorksForSymmetricMatrix()
    {
        double[] matrix =
        [
            2.0, 1.0,
            1.0, 2.0
        ];

        Complex[] eigenvalues = new Complex[2];
        MatrixAnalysis.Eigenvalues2x2(matrix, eigenvalues);

        double first = Math.Max(eigenvalues[0].Real, eigenvalues[1].Real);
        double second = Math.Min(eigenvalues[0].Real, eigenvalues[1].Real);

        Assert.InRange(first, 3.0 - 1e-12, 3.0 + 1e-12);
        Assert.InRange(second, 1.0 - 1e-12, 1.0 + 1e-12);
        Assert.InRange(Math.Abs(eigenvalues[0].Imaginary), 0.0, 1e-12);
        Assert.InRange(Math.Abs(eigenvalues[1].Imaginary), 0.0, 1e-12);
    }
}
