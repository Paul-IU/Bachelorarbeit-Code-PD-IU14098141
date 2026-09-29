using TacticsCore.Agents;
using TacticsCore.Batch;

namespace TacticsCore.Tests;

/// <summary>
/// Bachelorarbeit-Erweiterung: die Agentenauswahl für einen Batch-Lauf muss
/// sich rein über ein Kennwort steuern lassen, ohne dass dafür Code
/// geändert werden muss (siehe <see cref="AgentConfiguration.FromName"/>).
/// </summary>
public class AgentConfigurationTests
{
    [Theory]
    [InlineData("byactiontype")]
    [InlineData("ByActionType")]
    [InlineData(" BYACTIONTYPE ")] // Groß-/Kleinschreibung und umgebende Leerzeichen dürfen keine Rolle spielen
    public void FromName_ByActionType_LiefertRandomAgentByActionTypeAufBeidenSeiten(string name)
    {
        var config = AgentConfiguration.FromName(name);

        Assert.Equal(AgentNames.ByActionType, config.Name);
        Assert.IsType<RandomAgentByActionType>(config.CreateTeamAAgent());
        Assert.IsType<RandomAgentByActionType>(config.CreateTeamBAgent());
    }

    [Theory]
    [InlineData("uniform")]
    [InlineData("Uniform")]
    [InlineData(" UNIFORM ")]
    public void FromName_Uniform_LiefertRandomAgentUniformAufBeidenSeiten(string name)
    {
        var config = AgentConfiguration.FromName(name);

        Assert.Equal(AgentNames.Uniform, config.Name);
        Assert.IsType<RandomAgentUniform>(config.CreateTeamAAgent());
        Assert.IsType<RandomAgentUniform>(config.CreateTeamBAgent());
    }

    [Fact]
    public void FromName_UnbekanntesKennwort_WirftAussagekraeftigeException()
    {
        var exception = Assert.Throws<ArgumentException>(() => AgentConfiguration.FromName("gierigerAgent"));
        Assert.Contains("gierigerAgent", exception.Message);
    }

    [Fact]
    public void RandomVsRandomUndFromNameByActionType_LiefernDenselbenNamen()
    {
        Assert.Equal(AgentConfiguration.RandomVsRandom().Name, AgentConfiguration.FromName(AgentNames.ByActionType).Name);
    }

    [Fact]
    public void UniformVsUniformUndFromNameUniform_LiefernDenselbenNamen()
    {
        Assert.Equal(AgentConfiguration.UniformVsUniform().Name, AgentConfiguration.FromName(AgentNames.Uniform).Name);
    }
}
