namespace LAL.OdeCore;

public readonly record struct StepControlResult(double NextStep, bool Accepted);
public readonly record struct StepControlResultFloat(float NextStep, bool Accepted);

internal static class StepController
{
    public static StepControlResult Propose(
        double currentStep,
        double estimatedError,
        double tolerance,
        double safety = 0.9,
        double minScale = 0.2,
        double maxScale = 5.0)
    {
        if (currentStep <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentStep), "Current step must be positive.");
        }

        if (estimatedError < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(estimatedError), "Estimated error must be non-negative.");
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        if (safety <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(safety), "Safety factor must be positive.");
        }

        if (minScale <= 0 || maxScale <= 0 || minScale > maxScale)
        {
            throw new ArgumentOutOfRangeException(nameof(minScale), "Scale bounds must be positive and min <= max.");
        }

        bool accepted = estimatedError <= tolerance;
        double normalizedError = Math.Max(estimatedError, 1e-16);
        double scale = safety * Math.Pow(tolerance / normalizedError, 0.2);

        if (accepted)
        {
            scale = Math.Clamp(scale, 1d, maxScale);
        }
        else
        {
            scale = Math.Clamp(scale, minScale, 1d);
        }

        return new StepControlResult(currentStep * scale, accepted);
    }

    public static StepControlResultFloat Propose(
        float currentStep,
        float estimatedError,
        float tolerance,
        float safety = 0.9f,
        float minScale = 0.2f,
        float maxScale = 5.0f)
    {
        if (currentStep <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentStep), "Current step must be positive.");
        }

        if (estimatedError < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(estimatedError), "Estimated error must be non-negative.");
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        if (safety <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(safety), "Safety factor must be positive.");
        }

        if (minScale <= 0 || maxScale <= 0 || minScale > maxScale)
        {
            throw new ArgumentOutOfRangeException(nameof(minScale), "Scale bounds must be positive and min <= max.");
        }

        bool accepted = estimatedError <= tolerance;
        float normalizedError = MathF.Max(estimatedError, 1e-12f);
        float scale = safety * MathF.Pow(tolerance / normalizedError, 0.2f);

        if (accepted)
        {
            scale = Math.Clamp(scale, 1f, maxScale);
        }
        else
        {
            scale = Math.Clamp(scale, minScale, 1f);
        }

        return new StepControlResultFloat(currentStep * scale, accepted);
    }
}

