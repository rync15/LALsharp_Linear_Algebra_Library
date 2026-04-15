using LAL.OdeCore;

namespace LAL.Tests.OdeCore;

public class StepControllerTests
{
    [Fact]
    public void Propose_AcceptsAndRejectsByError()
    {
        StepControlResult accepted = StepController.Propose(currentStep: 0.1, estimatedError: 1e-6, tolerance: 1e-4);
        StepControlResult rejected = StepController.Propose(currentStep: 0.1, estimatedError: 1e-2, tolerance: 1e-4);

        Assert.True(accepted.Accepted);
        Assert.True(accepted.NextStep >= 0.1);

        Assert.False(rejected.Accepted);
        Assert.True(rejected.NextStep <= 0.1);
    }

    [Fact]
    public void Propose_Float_AcceptsAndRejectsByError()
    {
        StepControlResultFloat accepted = StepController.Propose(currentStep: 0.1f, estimatedError: 1e-6f, tolerance: 1e-4f);
        StepControlResultFloat rejected = StepController.Propose(currentStep: 0.1f, estimatedError: 1e-2f, tolerance: 1e-4f);

        Assert.True(accepted.Accepted);
        Assert.True(accepted.NextStep >= 0.1f);

        Assert.False(rejected.Accepted);
        Assert.True(rejected.NextStep <= 0.1f);
    }
}
