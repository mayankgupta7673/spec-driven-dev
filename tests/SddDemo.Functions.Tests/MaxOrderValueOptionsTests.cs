using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SddDemo.Functions.Models;

namespace SddDemo.Functions.Tests;

public sealed class MaxOrderValueOptionsTests
{
    [Fact]
    public void Should_FailFastAtStartup_When_ThresholdIsNotPositive()
    {
        var services = new ServiceCollection();
        services
            .AddOptions<MaxOrderValueOptions>()
            .Configure(o => o.Threshold = 0m)
            .Validate(o => o.Threshold > 0, "MaxOrderValue:Threshold must be greater than zero.");

        using var provider = services.BuildServiceProvider();

        var act = () => provider.GetRequiredService<IOptions<MaxOrderValueOptions>>().Value;

        act.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void Should_UseDefaultThreshold_When_NotConfigured()
    {
        var options = new MaxOrderValueOptions();

        options.Threshold.Should().Be(250_000m);
    }
}
