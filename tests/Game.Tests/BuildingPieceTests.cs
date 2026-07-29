using System.Numerics;
using Game.Domain.Building;
using Game.Domain.Exceptions;
using Xunit;

namespace Game.Tests;

public sealed class BuildingPieceTests
{
    [Fact]
    public void Place_WithSupport_Succeeds()
    {
        var piece = new BuildingPiece("wall", new Vector3(1, 0, 2), SupportKind.Ground);
        Assert.Equal("wall", piece.Kind);
        Assert.Equal(SupportKind.Ground, piece.Support);
    }

    [Fact]
    public void Place_WithoutSupport_ThrowsNoAirPlacement() =>
        Assert.Throws<DomainException>(() => new BuildingPiece("wall", Vector3.Zero, SupportKind.None));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Place_WithEmptyKind_Throws(string kind) =>
        Assert.Throws<DomainException>(() => new BuildingPiece(kind, Vector3.Zero, SupportKind.Ground));
}
