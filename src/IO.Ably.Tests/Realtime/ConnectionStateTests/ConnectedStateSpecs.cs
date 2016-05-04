using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using IO.Ably.Realtime;
using IO.Ably.Transport;
using IO.Ably.Transport.States.Connection;
using IO.Ably.Types;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace IO.Ably.Tests
{
    public class ConnectedStateSpecs : AblySpecs
    {
        private FakeConnectionContext _context;
        private ConnectionConnectedState _state;

        public ConnectedStateSpecs(ITestOutputHelper output) : base(output)
        {
            _context = new FakeConnectionContext();
            _state = GetState();
        }

        private ConnectionConnectedState GetState(ConnectionInfo info = null)
        {
            return new ConnectionConnectedState(_context, info ?? new ConnectionInfo("", 0, "", ""));
        }

        [Fact]
        public void ConnectedState_CorrectState()
        {
            // Assert
            _state.State.Should().Be(ConnectionStateType.Connected);
        }

        [Fact]
        public async Task ShouldResetsContextConnectionAttempts()
        {
            // Act
            await _state.OnAttachedToContext();

            // Assert
            _context.ResetConnectionAttemptsCalled.Should().BeTrue();
        }

        [Fact]
        public void ShouldSendMessagesUsingTransport()
        {
            var transport = new FakeTransport();
            _context.Transport = transport;

            // Act
            _state.SendMessage(new ProtocolMessage(ProtocolMessage.MessageAction.Attach));

            // Assert
            transport.LastMessageSend.Should().NotBeNull();
            transport.LastMessageSend.action.Should().Be(ProtocolMessage.MessageAction.Attach);
        }

        [Theory]
        [InlineData(ProtocolMessage.MessageAction.Ack)]
        [InlineData(ProtocolMessage.MessageAction.Attach)]
        [InlineData(ProtocolMessage.MessageAction.Attached)]
        [InlineData(ProtocolMessage.MessageAction.Close)]
        [InlineData(ProtocolMessage.MessageAction.Closed)]
        [InlineData(ProtocolMessage.MessageAction.Connect)]
        [InlineData(ProtocolMessage.MessageAction.Connected)]
        [InlineData(ProtocolMessage.MessageAction.Detach)]
        [InlineData(ProtocolMessage.MessageAction.Detached)]
        [InlineData(ProtocolMessage.MessageAction.Disconnect)]
        [InlineData(ProtocolMessage.MessageAction.Heartbeat)]
        [InlineData(ProtocolMessage.MessageAction.Message)]
        [InlineData(ProtocolMessage.MessageAction.Nack)]
        [InlineData(ProtocolMessage.MessageAction.Presence)]
        [InlineData(ProtocolMessage.MessageAction.Sync)]
        public async Task ShouldNotHandleInboundMessageAction(ProtocolMessage.MessageAction action)
        {
            // Act
            bool result = await _state.OnMessageReceived(new ProtocolMessage(action));

            // Assert
            Assert.False(result);
            _context.ShouldHaveNotChangedState();
        }

        [Fact]
        public async Task ShouldHandleInboundDisconnectedMessageAndSetStateToDisconnected()
        {
            // Act
            bool result = await _state.OnMessageReceived(new ProtocolMessage(ProtocolMessage.MessageAction.Disconnected));

            // Assert
            result.Should().BeTrue();
            _context.StateShouldBe<ConnectionDisconnectedState>()
            ;
        }

        [Fact]
        public async Task ShouldHandlesInboundErrorMessageAndGoToFailedState()
        {
            ErrorInfo targetError = new ErrorInfo("test", 123);

            // Act
            bool result = await _state.OnMessageReceived(new ProtocolMessage(ProtocolMessage.MessageAction.Error) { error = targetError });

            // Assert
            result.Should().BeTrue();
            var newState = _context.StateShouldBe<ConnectionFailedState>();
            newState.Error.ShouldBeEquivalentTo(targetError);
        }

        [Fact]
        public void WhenConnectCalled_ShouldDoNothing()
        {
            // Act
            _state.Connect();

            // Asser
            _context.ShouldHaveNotChangedState();
        }

        [Fact]
        public void WhenCloseCalled_ShouldCHangeStateToClosing()
        {
            // Act
            _state.Close();

            // Assert
            _context.StateShouldBe<ConnectionClosingState>();
        }

        [Fact]
        public async Task OnAttachToContext_ShouldSendPendingMessages()
        {
            // Arrange
            var transport = new FakeTransport();
            _context.Transport = transport;
            var targetMessage = new ProtocolMessage(ProtocolMessage.MessageAction.Attach);
            _context.QueuedMessages.Enqueue(targetMessage);

            // Act
            await _state.OnAttachedToContext();

            // Assert
            transport.LastMessageSend.Should().BeSameAs(targetMessage);
            _context.QueuedMessages.Should().BeEmpty();
        }

        [Theory]
        [InlineData(TransportState.Closing)]
        [InlineData(TransportState.Connected)]
        [InlineData(TransportState.Connecting)]
        [InlineData(TransportState.Initialized)]
        public async Task WhenTransportStateChanges_ConnectionStatesShouldNotChange(TransportState transportState)
        {
            // Act
            await _state.OnTransportStateChanged(new ConnectionState.TransportStateInfo(transportState));

            // Assert
            _context.LastSetState.Should().BeNull();
        }

        [Fact]
        public void ConnectedState_TransportGoesDisconnected_SwitchesToDisconnected()
        {
            // Arrange
            Mock<IConnectionContext> context = new Mock<IConnectionContext>();
            context.SetupGet(c => c.Connection).Returns(new Connection(new Mock<IConnectionManager>().Object));
            ConnectionConnectedState state = new ConnectionConnectedState(context.Object, new ConnectionInfo("", 0, "", ""));

            // Act
            state.OnTransportStateChanged(new ConnectionState.TransportStateInfo(TransportState.Closed));

            // Assert
            context.Verify(c => c.SetState(It.IsAny<ConnectionDisconnectedState>()), Times.Once());
        }

        [Fact]
        [Trait("spec", "RTN8b")]
        public void ConnectedState_UpdatesConnectionInformation()
        {
            // Act
            GetState(new ConnectionInfo("test", 12564, "test test", ""));

            // Assert
            var connection = _context.Connection;
            connection.Id.Should().Be("test");
            connection.Serial.Should().Be(12564);
            connection.Key.Should().Be("test test");
        }
    }
}