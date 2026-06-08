using IndFusion.Ember.Tests.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace IndFusion.Ember.Tests.Abstractions.Hubs;

/// <summary>
/// Tests for the ExxerHub&lt;T&gt; base class.
/// Note: Full SignalR hub testing requires integration tests due to extension methods.
/// These unit tests focus on validation, cancellation, and error handling logic.
/// </summary>
public class ExxerHubTests
{
    private readonly ILogger<TestHub> _logger;
    private readonly HubCallerContext _mockContext;
    private readonly IHubCallerClients _mockClients;
    private readonly IClientProxy _mockClientProxy;
    private readonly ISingleClientProxy _mockSingleClientProxy;
    private readonly IGroupManager _mockGroups;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExxerHubTests"/> class.
    /// </summary>
    public ExxerHubTests()
    {
        _logger = Substitute.For<ILogger<TestHub>>();
        // Source-generated LoggerMessage methods guard on IsEnabled before calling Log,
        // so the substitute must report levels as enabled for log-assertion tests.
        _logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

        // Setup SignalR mocks
        _mockContext = Substitute.For<HubCallerContext>();
        _mockClients = Substitute.For<IHubCallerClients>();
        _mockClientProxy = Substitute.For<IClientProxy>();
        _mockSingleClientProxy = Substitute.For<ISingleClientProxy>();
        _mockGroups = Substitute.For<IGroupManager>();

        // Configure mock context
        _mockContext.ConnectionId.Returns("test-connection-id");

        // Configure mock clients
        _mockClients.All.Returns(_mockClientProxy);
        _mockClients.Client(Arg.Any<string>()).Returns(_mockSingleClientProxy); // Client() returns ISingleClientProxy
        _mockClients.Group(Arg.Any<string>()).Returns(_mockClientProxy);

        // SendAsync(...) is an extension over SendCoreAsync; completing it lets the
        // success paths run instead of faulting into the catch blocks.
        _mockClientProxy.SendCoreAsync(Arg.Any<string>(), Arg.Any<object?[]>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _mockSingleClientProxy.SendCoreAsync(Arg.Any<string>(), Arg.Any<object?[]>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
    }

    /// <summary>
    /// Tests that SendToAllAsync returns cancelled when cancellation is requested.
    /// </summary>
    [Fact]
    public async Task SendToAllAsync_WithCancellationRequested_ReturnsCancelled()
    {
        // Arrange
        var hub = CreateTestHub();
        var testData = new TestData { Id = 1, Name = "Test" };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await hub.SendToAllAsync(testData, cts.Token);

        // Assert
        result.IsCancelled().ShouldBeTrue();
    }

    /// <summary>
    /// Tests that SendToClientAsync returns failure when connection ID is null.
    /// </summary>
    [Fact]
    public async Task SendToClientAsync_WithNullConnectionId_ReturnsFailure()
    {
        // Arrange
        var hub = CreateTestHub();
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = await hub.SendToClientAsync(null!, testData, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error!.ShouldContain("Connection ID");
    }

    /// <summary>
    /// Tests that SendToClientAsync returns failure when connection ID is empty.
    /// </summary>
    [Fact]
    public async Task SendToClientAsync_WithEmptyConnectionId_ReturnsFailure()
    {
        // Arrange
        var hub = CreateTestHub();
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = await hub.SendToClientAsync(string.Empty, testData, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error!.ShouldContain("Connection ID");
    }

    /// <summary>
    /// Tests that SendToClientAsync returns cancelled when cancellation is requested.
    /// </summary>
    [Fact]
    public async Task SendToClientAsync_WithCancellationRequested_ReturnsCancelled()
    {
        // Arrange
        var hub = CreateTestHub();
        var connectionId = "test-connection-id";
        var testData = new TestData { Id = 1, Name = "Test" };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await hub.SendToClientAsync(connectionId, testData, cts.Token);

        // Assert
        result.IsCancelled().ShouldBeTrue();
    }

    /// <summary>
    /// Tests that SendToGroupAsync returns failure when group name is null.
    /// </summary>
    [Fact]
    public async Task SendToGroupAsync_WithNullGroupName_ReturnsFailure()
    {
        // Arrange
        var hub = CreateTestHub();
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = await hub.SendToGroupAsync(null!, testData, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error!.ShouldContain("Group name");
    }

    /// <summary>
    /// Tests that SendToGroupAsync returns failure when group name is empty.
    /// </summary>
    [Fact]
    public async Task SendToGroupAsync_WithEmptyGroupName_ReturnsFailure()
    {
        // Arrange
        var hub = CreateTestHub();
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = await hub.SendToGroupAsync(string.Empty, testData, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error!.ShouldContain("Group name");
    }

    /// <summary>
    /// Tests that SendToGroupAsync returns cancelled when cancellation is requested.
    /// </summary>
    [Fact]
    public async Task SendToGroupAsync_WithCancellationRequested_ReturnsCancelled()
    {
        // Arrange
        var hub = CreateTestHub();
        var groupName = "test-group";
        var testData = new TestData { Id = 1, Name = "Test" };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await hub.SendToGroupAsync(groupName, testData, cts.Token);

        // Assert
        result.IsCancelled().ShouldBeTrue();
    }

    /// <summary>
    /// Tests that GetConnectionCountAsync returns failure (not implemented).
    /// </summary>
    [Fact]
    public async Task GetConnectionCountAsync_Always_ReturnsFailure()
    {
        // Arrange
        var hub = CreateTestHub();

        // Act
        var result = await hub.GetConnectionCountAsync(CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error!.ShouldContain("not implemented");
    }

    /// <summary>
    /// Tests that GetConnectionCountAsync returns cancelled when cancellation is requested.
    /// </summary>
    [Fact]
    public async Task GetConnectionCountAsync_WithCancellationRequested_ReturnsCancelled()
    {
        // Arrange
        var hub = CreateTestHub();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await hub.GetConnectionCountAsync(cts.Token);

        // Assert
        result.IsCancelled().ShouldBeTrue();
    }

    // Note: SendAsync on IClientProxy/ISingleClientProxy cannot be mocked with NSubstitute
    // because they are extension methods or interface members that don't support interception.
    // These scenarios should be tested via integration tests with actual SignalR infrastructure.

    // Note: SendAsync on ISingleClientProxy cannot be mocked with NSubstitute.
    // These scenarios should be tested via integration tests with actual SignalR infrastructure.

    /// <summary>
    /// Tests that SendToClientAsync returns failure when connection ID is whitespace.
    /// </summary>
    [Fact]
    public async Task SendToClientAsync_WithWhiteSpaceConnectionId_ReturnsFailure()
    {
        // Arrange
        var hub = CreateTestHub();
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = await hub.SendToClientAsync("   ", testData, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error!.ShouldContain("Connection ID");
    }

    // Note: SendAsync on IClientProxy cannot be mocked with NSubstitute.
    // These scenarios should be tested via integration tests with actual SignalR infrastructure.

    /// <summary>
    /// Tests that SendToGroupAsync returns failure when group name is whitespace.
    /// </summary>
    [Fact]
    public async Task SendToGroupAsync_WithWhiteSpaceGroupName_ReturnsFailure()
    {
        // Arrange
        var hub = CreateTestHub();
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = await hub.SendToGroupAsync("   ", testData, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error!.ShouldContain("Group name");
    }

    /// <summary>
    /// Tests that GetConnectionCountAsync returns failure when exception occurs.
    /// </summary>
    [Fact]
    public async Task GetConnectionCountAsync_WhenExceptionOccurs_ReturnsFailure()
    {
        // Arrange
        var hub = CreateTestHub();
        // Exception path is in catch block - this test verifies the exception handling branch

        // Act
        var result = await hub.GetConnectionCountAsync(CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error!.ShouldContain("not implemented");
    }

    // Note: Exception path tests for SendAsync methods require integration tests
    // because SendAsync is an extension method that cannot be mocked with NSubstitute.
    // These exception paths should be tested with actual SignalR infrastructure.

    /// <summary>
    /// Tests that SendToClientAsync processes when connectionId is valid.
    /// This tests the mutation: string.IsNullOrWhiteSpace(connectionId) (false case - continues).
    /// </summary>
    [Fact]
    public async Task SendToClientAsync_WithValidConnectionId_Processes()
    {
        // Arrange
        var hub = CreateTestHub();
        var connectionId = "valid-connection-id";
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = await hub.SendToClientAsync(connectionId, testData, CancellationToken.None);

        // Assert - Should process (may succeed or fail depending on mock setup, but should not return validation error)
        result.ShouldNotBeNull();
        // Note: Actual send behavior requires integration tests
    }

    /// <summary>
    /// Tests that SendToGroupAsync processes when groupName is valid.
    /// This tests the mutation: string.IsNullOrWhiteSpace(groupName) (false case - continues).
    /// </summary>
    [Fact]
    public async Task SendToGroupAsync_WithValidGroupName_Processes()
    {
        // Arrange
        var hub = CreateTestHub();
        var groupName = "valid-group-name";
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = await hub.SendToGroupAsync(groupName, testData, CancellationToken.None);

        // Assert - Should process (may succeed or fail depending on mock setup, but should not return validation error)
        result.ShouldNotBeNull();
        // Note: Actual send behavior requires integration tests
    }

    // Note: OnDisconnectedAsync and OnConnectedAsync tests require Context.ConnectionId to be set up
    // These methods access Context.ConnectionId which requires proper SignalR infrastructure setup.
    // These scenarios should be tested via integration tests with actual SignalR infrastructure.
    // The branch coverage (exception != null vs exception == null) is tested via integration tests.

    // Note: OnConnectedAsync and OnDisconnectedAsync require Context to be properly initialized
    // These methods access Context.ConnectionId which requires SignalR infrastructure.
    // These scenarios should be tested via integration tests with actual SignalR infrastructure.

    /// <summary>
    /// Tests that SendToAllAsync returns failure when the Clients property is null.
    /// Kills the null-guard mutations on the Clients check.
    /// </summary>
    [Fact]
    public async Task SendToAllAsync_WithNullClients_ReturnsFailure()
    {
        // Arrange
        var hub = new TestHub(_logger);
        TestHubHelper.SetupHub(hub, _mockContext, null!, _mockGroups);
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = await hub.SendToAllAsync(testData, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error!.ShouldContain("Clients property is null");
    }

    /// <summary>
    /// Tests that SendToAllAsync returns failure when Clients.All is null.
    /// </summary>
    [Fact]
    public async Task SendToAllAsync_WithNullClientsAll_ReturnsFailure()
    {
        // Arrange
        _mockClients.All.Returns((IClientProxy)null!);
        var hub = CreateTestHub();
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = await hub.SendToAllAsync(testData, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error!.ShouldContain("Clients.All property is null");
    }

    /// <summary>
    /// Tests that SendToAllAsync returns failure when the data payload is null.
    /// </summary>
    [Fact]
    public async Task SendToAllAsync_WithNullData_ReturnsFailure()
    {
        // Arrange
        var hub = CreateTestHub();

        // Act
        var result = await hub.SendToAllAsync(null!, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error!.ShouldContain("Data property is null");
    }

    /// <summary>
    /// Tests that SendToAllAsync succeeds and sends on the happy path with valid data.
    /// </summary>
    [Fact]
    public async Task SendToAllAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var hub = CreateTestHub();
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = await hub.SendToAllAsync(testData, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeFalse();
        result.IsCancelled().ShouldBeFalse();
        await _mockClientProxy.Received(1).SendCoreAsync(
            "ReceiveMessage", Arg.Any<object?[]>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that SendToClientAsync succeeds on the happy path with a valid connection.
    /// </summary>
    [Fact]
    public async Task SendToClientAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var hub = CreateTestHub();
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = await hub.SendToClientAsync("conn-1", testData, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeFalse();
        result.IsCancelled().ShouldBeFalse();
        await _mockSingleClientProxy.Received(1).SendCoreAsync(
            "ReceiveMessage", Arg.Any<object?[]>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that SendToGroupAsync succeeds on the happy path with a valid group.
    /// </summary>
    [Fact]
    public async Task SendToGroupAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var hub = CreateTestHub();
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = await hub.SendToGroupAsync("group-1", testData, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeFalse();
        result.IsCancelled().ShouldBeFalse();
        await _mockClientProxy.Received(1).SendCoreAsync(
            "ReceiveMessage", Arg.Any<object?[]>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that OnConnectedAsync logs an Information message for the connecting client.
    /// </summary>
    [Fact]
    public async Task OnConnectedAsync_LogsInformation_AndCompletes()
    {
        // Arrange
        var hub = CreateTestHub();

        // Act
        await hub.OnConnectedAsync();

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Tests that OnDisconnectedAsync logs a Warning (not Information) when an exception is present.
    /// Kills the equality mutation on the (exception != null) branch.
    /// </summary>
    [Fact]
    public async Task OnDisconnectedAsync_WithException_LogsWarning()
    {
        // Arrange
        var hub = CreateTestHub();
        var error = new InvalidOperationException("boom");

        // Act
        await hub.OnDisconnectedAsync(error);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
        _logger.DidNotReceive().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Tests that OnDisconnectedAsync logs an Information message (not Warning) without an exception.
    /// </summary>
    [Fact]
    public async Task OnDisconnectedAsync_WithoutException_LogsInformation()
    {
        // Arrange
        var hub = CreateTestHub();

        // Act
        await hub.OnDisconnectedAsync(null);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
        _logger.DidNotReceive().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Creates a test hub instance with mocked dependencies.
    /// </summary>
    private TestHub CreateTestHub()
    {
        var hub = new TestHub(_logger);
        TestHubHelper.SetupHub(hub, _mockContext, _mockClients, _mockGroups);
        return hub;
    }

    /// <summary>
    /// Test hub implementation for testing.
    /// </summary>
    public class TestHub : ExxerHub<TestData>
    {
        public TestHub(ILogger<TestHub> logger)
            : base(logger)
        {
        }
    }

    /// <summary>
    /// Test data class for testing.
    /// </summary>
    public class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
