using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace IndFusion.Ember.Abstractions.Hubs;

/// <summary>
/// Base class for SignalR hubs following Hexagonal Architecture and Railway-Oriented Programming patterns.
/// Provides generic, type-safe hub functionality with error handling via Result&lt;T&gt; pattern.
/// </summary>
/// <typeparam name="T">The type of data transmitted through the hub.</typeparam>
public abstract class ExxerHub<T> : Hub, IExxerHub<T>
{
    private readonly ILogger<ExxerHub<T>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExxerHub{T}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    protected ExxerHub(ILogger<ExxerHub<T>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public virtual async Task<Result> SendToAllAsync(T data, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.CancelledBeforeSendAll();
            return ResultExtensions.Cancelled();
        }

        if (Clients is null)
        {
            _logger.ClientsNull();
            return Result.WithFailure("Clients property is null");
        }

        if (Clients.All is null)
        {
            _logger.ClientsAllNull();
            return Result.WithFailure("Clients.All property is null");
        }

        if (data is null)
        {
            _logger.DataNull();
            return Result.WithFailure("Data property is null");
        }

        try
        {
            await Clients.All.SendAsync("ReceiveMessage", data, cancellationToken).ConfigureAwait(false);
            _logger.SentToAll();
            return Result.Success();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.CancelledDuringSendAll();
            return ResultExtensions.Cancelled();
        }
        catch (Exception ex)
        {
            _logger.ErrorSendAll(ex);
            return Result.WithFailure($"Failed to send data to all clients: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public virtual async Task<Result> SendToClientAsync(string connectionId, T data, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.CancelledBeforeSendClient();
            return ResultExtensions.Cancelled();
        }

        if (string.IsNullOrWhiteSpace(connectionId))
        {
            return Result.WithFailure("Connection ID cannot be null or empty");
        }

        try
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", data, cancellationToken).ConfigureAwait(false);
            _logger.SentToClient(connectionId);
            return Result.Success();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.CancelledDuringSendClient(connectionId);
            return ResultExtensions.Cancelled();
        }
        catch (Exception ex)
        {
            _logger.ErrorSendClient(ex, connectionId);
            return Result.WithFailure($"Failed to send data to client {connectionId}: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public virtual async Task<Result> SendToGroupAsync(string groupName, T data, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.CancelledBeforeSendGroup();
            return ResultExtensions.Cancelled();
        }

        if (string.IsNullOrWhiteSpace(groupName))
        {
            return Result.WithFailure("Group name cannot be null or empty");
        }

        try
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", data, cancellationToken).ConfigureAwait(false);
            _logger.SentToGroup(groupName);
            return Result.Success();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.CancelledDuringSendGroup(groupName);
            return ResultExtensions.Cancelled();
        }
        catch (Exception ex)
        {
            _logger.ErrorSendGroup(ex, groupName);
            return Result.WithFailure($"Failed to send data to group {groupName}: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public virtual Task<Result<int>> GetConnectionCountAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.CancelledBeforeGetCount();
            return Task.FromResult(ResultExtensions.Cancelled<int>());
        }

        try
        {
            // Note: SignalR doesn't provide direct connection count access
            // This is a placeholder - implementations should track connections via OnConnectedAsync/OnDisconnectedAsync
            _logger.CountNotImplemented();
            return Task.FromResult(Result<int>.WithFailure("Connection count tracking not implemented"));
        }
        catch (Exception ex)
        {
            _logger.ErrorGetCount(ex);
            return Task.FromResult(Result<int>.WithFailure($"Failed to get connection count: {ex.Message}"));
        }
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task OnConnectedAsync()
    {
        _logger.ClientConnected(Context.ConnectionId);
        await base.OnConnectedAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    /// <param name="exception">The exception that caused the disconnection, if any.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.ClientDisconnectedError(exception, Context.ConnectionId);
        }
        else
        {
            _logger.ClientDisconnected(Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    }
}

/// <summary>
/// High-performance source-generated log messages for <see cref="ExxerHub{T}"/>.
/// </summary>
internal static partial class ExxerHubLog
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Warning, Message = "Operation cancelled before sending to all clients")]
    public static partial void CancelledBeforeSendAll(this ILogger logger);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Error, Message = "Clients property is null")]
    public static partial void ClientsNull(this ILogger logger);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Error, Message = "Clients.All property is null")]
    public static partial void ClientsAllNull(this ILogger logger);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Error, Message = "Data property is null")]
    public static partial void DataNull(this ILogger logger);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Debug, Message = "Successfully sent data to all clients")]
    public static partial void SentToAll(this ILogger logger);

    [LoggerMessage(EventId = 1006, Level = LogLevel.Information, Message = "Operation cancelled while sending to all clients")]
    public static partial void CancelledDuringSendAll(this ILogger logger);

    [LoggerMessage(EventId = 1007, Level = LogLevel.Error, Message = "Error sending data to all clients")]
    public static partial void ErrorSendAll(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1008, Level = LogLevel.Warning, Message = "Operation cancelled before sending to client")]
    public static partial void CancelledBeforeSendClient(this ILogger logger);

    [LoggerMessage(EventId = 1009, Level = LogLevel.Debug, Message = "Successfully sent data to client {ConnectionId}")]
    public static partial void SentToClient(this ILogger logger, string connectionId);

    [LoggerMessage(EventId = 1010, Level = LogLevel.Information, Message = "Operation cancelled while sending to client {ConnectionId}")]
    public static partial void CancelledDuringSendClient(this ILogger logger, string connectionId);

    [LoggerMessage(EventId = 1011, Level = LogLevel.Error, Message = "Error sending data to client {ConnectionId}")]
    public static partial void ErrorSendClient(this ILogger logger, Exception exception, string connectionId);

    [LoggerMessage(EventId = 1012, Level = LogLevel.Warning, Message = "Operation cancelled before sending to group")]
    public static partial void CancelledBeforeSendGroup(this ILogger logger);

    [LoggerMessage(EventId = 1013, Level = LogLevel.Debug, Message = "Successfully sent data to group {GroupName}")]
    public static partial void SentToGroup(this ILogger logger, string groupName);

    [LoggerMessage(EventId = 1014, Level = LogLevel.Information, Message = "Operation cancelled while sending to group {GroupName}")]
    public static partial void CancelledDuringSendGroup(this ILogger logger, string groupName);

    [LoggerMessage(EventId = 1015, Level = LogLevel.Error, Message = "Error sending data to group {GroupName}")]
    public static partial void ErrorSendGroup(this ILogger logger, Exception exception, string groupName);

    [LoggerMessage(EventId = 1016, Level = LogLevel.Warning, Message = "Operation cancelled before getting connection count")]
    public static partial void CancelledBeforeGetCount(this ILogger logger);

    [LoggerMessage(EventId = 1017, Level = LogLevel.Warning, Message = "GetConnectionCountAsync not fully implemented - connection tracking required")]
    public static partial void CountNotImplemented(this ILogger logger);

    [LoggerMessage(EventId = 1018, Level = LogLevel.Error, Message = "Error getting connection count")]
    public static partial void ErrorGetCount(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1019, Level = LogLevel.Information, Message = "Client connected: {ConnectionId}")]
    public static partial void ClientConnected(this ILogger logger, string connectionId);

    [LoggerMessage(EventId = 1020, Level = LogLevel.Warning, Message = "Client disconnected with error: {ConnectionId}")]
    public static partial void ClientDisconnectedError(this ILogger logger, Exception exception, string connectionId);

    [LoggerMessage(EventId = 1021, Level = LogLevel.Information, Message = "Client disconnected: {ConnectionId}")]
    public static partial void ClientDisconnected(this ILogger logger, string connectionId);
}
