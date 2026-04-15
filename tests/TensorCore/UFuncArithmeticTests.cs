using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class UFuncArithmeticTests
{
    [Fact]
    public void Add_ComputesElementwiseSum()
    {
        double[] dst = new double[3];

        UFuncArithmetic.Add([1.0, 2.0, 3.0], [4.0, 5.0, 6.0], dst);

        Assert.Equal([5.0, 7.0, 9.0], dst);
    }

    [Fact]
    public void Multiply_ComputesElementwiseProduct()
    {
        double[] dst = new double[3];

        UFuncArithmetic.Multiply([2.0, 3.0, 4.0], [5.0, 6.0, 7.0], dst);

        Assert.Equal([10.0, 18.0, 28.0], dst);
    }

    [Fact]
    public void Divide_ComputesElementwiseQuotient()
    {
        double[] dst = new double[3];

        UFuncArithmetic.Divide([10.0, 18.0, 28.0], [2.0, 3.0, 7.0], dst);

        Assert.Equal([5.0, 6.0, 4.0], dst);
    }

    [Fact]
    public void Power_ComputesScalarAndElementwiseExponent()
    {
        double[] scalarDst = new double[3];
        UFuncArithmetic.Power([2.0, 3.0, 4.0], 2.0, scalarDst);
        Assert.Equal([4.0, 9.0, 16.0], scalarDst);

        double[] vectorDst = new double[3];
        UFuncArithmetic.Power([2.0, 3.0, 4.0], [2.0, 3.0, 1.0], vectorDst);
        Assert.Equal([4.0, 27.0, 4.0], vectorDst);
    }
}
