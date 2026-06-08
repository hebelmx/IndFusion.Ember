using IndFusion.Ember.Infrastructure.Connection;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace IndFusion.Ember.Abstractions.Dashboards;

/// <summary>
/// Base class for dashboard components that display real-time data via SignalR.
/// Provides automatic connection management, message batching, and reconnection logic.
/// </summary>
/// <typeparam name="T">The type of data displayed in the dashboard.</typeparam>
public abstract class Dashboard<T> : IDashboard<T>, IAsyncDisposable
{
    private readonly ILogger<Dashboard<T>> _logger;
    private readonly HubConnection? _hubConnection;
    private readonly ReconnectionStrategy _reconnectionStrategy;
    private ConnectionState _connectionState;
    private readonly List<T> _data;

    /// <summary>
    /// Initializes a new instance of the <see cref="Dashboard{T}"/> class.
    /// </summary>
    /// <param name="hubConnection">The SignalR hub connection.</param>
    /// <param name="reconnectionStrategy">The reconnection strategy configuration.</param>
    /// <param name="logger">The logger instance.</param>
    protected Dashboard(
        HubConnection? hubConnection,
        ReconnectionStrategy? reconnectionStrategy,
        ILogger<Dashboard<T>> logger)
    {
        _hubConnection = hubConnection;
        _reconnectionStrategy = reconnectionStrategy ?? new ReconnectionStrategy();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionState = ConnectionState.Disconnected;
        _data = new List<T>();
    }

    /// <inheritdoc />
    public ConnectionState ConnectionState => _connectionState;

    /// <inheritdoc />
    public IReadOnlyList<T> Data => _data.AsReadOnly();

    /// <inheritdoc />
    public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

    /// <inheritdoc />
    public event EventHandler<DataReceivedEventArgs<T>>? DataReceived;

    /// <inheritdoc />
    public virtual async Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.CancelledBeforeConnecting();
            return ResultExtensions.Cancelled();
        }

        if (_hubConnection == null)
        {
            return Result.WithFailure("Hub connection is not configured");
        }

        try
        {
            UpdateConnectionState(ConnectionState.Connecting);

            _hubConnection.On<T>("ReceiveMessage", OnMessageReceived);

            await _hubConnection.StartAsync(cancellationToken).ConfigureAwait(false);

            UpdateConnectionState(ConnectionState.Connected);

            _logger.ConnectedToHub();
            return Result.Success();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.ConnectionCancelled();
            UpdateConnectionState(ConnectionState.Disconnected);
            return ResultExtensions.Cancelled();
        }
        catch (Exception ex)
        {
            _logger.ErrorConnecting(ex);
            UpdateConnectionState(ConnectionState.Failed);
            return Result.WithFailure($"Failed to connect: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public virtual async Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ResultExtensions.Cancelled();
        }

        if (_hubConnection == null)
        {
            return Result.Success();
        }

        try
        {
            await _hubConnection.StopAsync(cancellationToken).ConfigureAwait(false);
            UpdateConnectionState(ConnectionState.Disconnected);
            _logger.DisconnectedFromHub();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.ErrorDisconnecting(ex);
            return Result.WithFailure($"Failed to disconnect: {ex.Message}");
        }
    }

    /// <summary>
    /// Called when a message is received from the hub.
    /// </summary>
    /// <param name="data">The received data.</param>
    protected virtual void OnMessageReceived(T data)
    {
        if (data == null)
        {
            _logger.ReceivedNullMessage();
            return;
        }

        _data.Add(data);
        _logger.ReceivedMessage(_data.Count);

        var args = new DataReceivedEventArgs<T>(data);
        DataReceived?.Invoke(this, args);
    }

    private void UpdateConnectionState(ConnectionState newState)
    {
        if (_connectionState == newState)
        {
            return;
        }

        var previousState = _connectionState;
        _connectionState = newState;

        _logger.ConnectionStateChangedLog(previousState, newState);

        var args = new ConnectionStateChangedEventArgs(previousState, newState);
        ConnectionStateChanged?.Invoke(this, args);
    }

    /// <summary>
    /// Disposes the dashboard and disconnects from the hub.
    /// </summary>
    public virtual async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync().ConfigureAwait(false);
        }

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// High-performance source-generated log messages for <see cref="Dashboard{T}"/>.
/// </summary>
internal static partial class DashboardLog
{
    [LoggerMessage(EventId = 1101, Level = LogLevel.Warning, Message = "Operation cancelled before connecting")]
    public static partial void CancelledBeforeConnecting(this ILogger logger);

    [LoggerMessage(EventId = 1102, Level = LogLevel.Information, Message = "Successfully connected to SignalR hub")]
    public static partial void ConnectedToHub(this ILogger logger);

    [LoggerMessage(EventId = 1103, Level = LogLevel.Information, Message = "Connection cancelled")]
    public static partial void ConnectionCancelled(this ILogger logger);

    [LoggerMessage(EventId = 1104, Level = LogLevel.Error, Message = "Error connecting to SignalR hub")]
    public static partial void ErrorConnecting(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1105, Level = LogLevel.Information, Message = "Disconnected from SignalR hub")]
    public static partial void DisconnectedFromHub(this ILogger logger);

    [LoggerMessage(EventId = 1106, Level = LogLevel.Error, Message = "Error disconnecting from SignalR hub")]
    public static partial void ErrorDisconnecting(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1107, Level = LogLevel.Warning, Message = "Received null message")]
    public static partial void ReceivedNullMessage(this ILogger logger);

    [LoggerMessage(EventId = 1108, Level = LogLevel.Debug, Message = "Received message, total data count: {Count}")]
    public static partial void ReceivedMessage(this ILogger logger, int count);

    [LoggerMessage(EventId = 1109, Level = LogLevel.Debug, Message = "Connection state changed from {PreviousState} to {NewState}")]
    public static partial void ConnectionStateChangedLog(this ILogger logger, ConnectionState previousState, ConnectionState newState);
}

