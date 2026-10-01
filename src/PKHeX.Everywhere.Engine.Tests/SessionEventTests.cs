using AwesomeAssertions;

namespace PKHeX.Everywhere.Engine.Tests;

public class SessionEventTests
{
    [Fact]
    public void ACommandThatThrowsDropsTheEventsItRaised()
    {
        var session = new Session();
        var published = new List<IEngineEvent>();
        session.Published += published.Add;

        var throwing = () => session.RunCommand<bool>([Topics.Inventory], () =>
        {
            session.Raise(new ItemChanged(1, 1));
            throw new InvalidOperationException();
        });
        throwing.Should().Throw<InvalidOperationException>();
        session.RunCommand([Topics.Inventory], () =>
        {
            session.Raise(new ItemChanged(2, 0));
            return true;
        });

        published.Should().Equal(new ItemChanged(2, 0));
    }

    [Fact]
    public void ACommandPublishesItsEventsInTheOrderItRaisedThem()
    {
        var session = new Session();
        var published = new List<IEngineEvent>();
        session.Published += published.Add;

        session.RunCommand([Topics.Inventory], () =>
        {
            session.Raise(new ItemChanged(1, 1));
            session.Raise(new ItemChanged(2, 0));
            published.Should().BeEmpty();
            return true;
        });

        published.Should().Equal(new ItemChanged(1, 1), new ItemChanged(2, 0));
    }
}
