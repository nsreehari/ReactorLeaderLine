using Reactor.Community.LeaderLine;
using Reactor.Community.LeaderLine.Geometry;
using Xunit;

namespace Reactor.Community.LeaderLine.Tests;

public class SocketResolverTests
{
    private static readonly GeoRect Box = new(100, 100, 200, 100); // centre (200, 150)

    [Fact]
    public void AutoSide_PicksRight_WhenTargetIsToTheRight()
    {
        Assert.Equal(LeaderLineSocket.Right, SocketResolver.ChooseAutoSide(Box, new GeoPoint(600, 150)));
    }

    [Fact]
    public void AutoSide_PicksLeft_WhenTargetIsToTheLeft()
    {
        Assert.Equal(LeaderLineSocket.Left, SocketResolver.ChooseAutoSide(Box, new GeoPoint(-200, 150)));
    }

    [Fact]
    public void AutoSide_PicksBottom_WhenTargetIsBelow()
    {
        Assert.Equal(LeaderLineSocket.Bottom, SocketResolver.ChooseAutoSide(Box, new GeoPoint(200, 900)));
    }

    [Fact]
    public void AutoSide_PicksTop_WhenTargetIsAbove()
    {
        Assert.Equal(LeaderLineSocket.Top, SocketResolver.ChooseAutoSide(Box, new GeoPoint(200, -400)));
    }

    [Fact]
    public void ResolveRect_Right_ReturnsRightMidPointAndOutwardDirection()
    {
        EndpointGeometry e = SocketResolver.ResolveRect(Box, LeaderLineSocket.Right, new GeoPoint(600, 150));

        Assert.Equal(300, e.Point.X, 3);
        Assert.Equal(150, e.Point.Y, 3);
        Assert.Equal(1, e.Direction.X, 3);
        Assert.Equal(0, e.Direction.Y, 3);
    }

    [Fact]
    public void ResolveRect_Top_ReturnsTopMidPointAndUpwardDirection()
    {
        EndpointGeometry e = SocketResolver.ResolveRect(Box, LeaderLineSocket.Top, new GeoPoint(200, -100));

        Assert.Equal(200, e.Point.X, 3);
        Assert.Equal(100, e.Point.Y, 3);
        Assert.Equal(0, e.Direction.X, 3);
        Assert.Equal(-1, e.Direction.Y, 3);
    }

    [Fact]
    public void ResolvePoint_DirectionFacesTarget()
    {
        EndpointGeometry e = SocketResolver.ResolvePoint(new GeoPoint(0, 0), new GeoPoint(10, 0));

        Assert.Equal(1, e.Direction.X, 3);
        Assert.Equal(0, e.Direction.Y, 3);
    }
}
