using Microsoft.AspNetCore.SignalR;

namespace IndFusion.Ember.Tests.Infrastructure;

/// <summary>
/// Helper class for setting up test hubs with mocked SignalR infrastructure.
/// </summary>
public static class TestHubHelper
{
    /// <summary>
    /// Sets up a hub with mocked context and clients for testing.
    /// </summary>
    /// <typeparam name="THub">The hub type.</typeparam>
    /// <param name="hub">The hub instance.</param>
    /// <param name="mockContext">The mocked hub caller context.</param>
    /// <param name="mockClients">The mocked hub caller clients.</param>
    /// <param name="mockGroups">The mocked group manager.</param>
    public static void SetupHub<THub>(
        THub hub,
        HubCallerContext mockContext,
        IHubCallerClients mockClients,
        IGroupManager mockGroups)
        where THub : Hub
    {
        // Hub.Context/Clients/Groups are public, settable properties. (Looking them up with
        // BindingFlags.NonPublic returns null and silently leaves them unset, which is why
        // these mocks previously never took effect.)
        const System.Reflection.BindingFlags flags =
            System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.NonPublic
            | System.Reflection.BindingFlags.Instance;

        var contextProperty = typeof(Hub).GetProperty("Context", flags);
        var clientsProperty = typeof(Hub).GetProperty("Clients", flags);
        var groupsProperty = typeof(Hub).GetProperty("Groups", flags);

        contextProperty?.SetValue(hub, mockContext);
        clientsProperty?.SetValue(hub, mockClients);
        groupsProperty?.SetValue(hub, mockGroups);
    }
}

