﻿using System;

namespace TehGM.Wolfringo.Hosting
{
    /// <summary>Configuration options for hosted client.</summary>
    public class HostedWolfClientOptions
    {
        /// <summary>Server URL to connect to.</summary>
        /// <remarks>Defaults to <see cref="WolfClient.DefaultServerURL"/>.</remarks>
        public string ServerURL { get; set; } = WolfClient.DefaultServerURL;
        /// <summary>Device string to use when connecting.</summary>
        /// <remarks>Defaults to <see cref="WolfClient.DefaultDevice"/>.</remarks>
        public string Device { get; set; } = WolfClient.DefaultDevice;
        /// <summary>Token to use when connecting.</summary>
        /// <remarks>If not set, the client will automatically generate a valid token.</remarks>
        public string Token { get; set; } = null;

        // login settings
        /// <summary>Whether the hosted client should automatically login when connected.</summary>
        /// <remarks>Defaults to true.</remarks>
        public bool AutoLogin { get; set; } = true;
        /// <summary>Email address to use as login when automatically logging in.</summary>
        public string LoginEmail { get; set; }
        /// <summary>Password to authenticate with when automatically logging in.</summary>
        public string LoginPassword { get; set; }

        // auto-reconnection
        /// <summary>Whether hosted client should attempt to automatically reconnect when non-manually disconnected.</summary>
        /// <remarks>Defaults to true.</remarks>
        public bool AutoReconnect { get; set; } = true;
        /// <summary>How long the client should wait before automatically reconnecting.</summary>
        /// <remarks>Defaults to 500ms.</remarks>
        public TimeSpan AutoReconnectDelay { get; set; } = TimeSpan.FromSeconds(0.5);

        // caching
        /// <summary>Whether users caching should be enabled.</summary>
        /// <remarks>Defaults to true.</remarks>
        public bool UsersCachingEnabled { get; set; } = true;
        /// <summary>Whether groups caching should be enabled.</summary>
        /// <remarks>Defaults to true.</remarks>
        public bool GroupsCachingEnabled { get; set; } = true;
        /// <summary>Whether charms caching should be enabled.</summary>
        /// <remarks>Defaults to true.</remarks>
        public bool CharmsCachingEnabled { get; set; } = true;
        /// <summary>Whether achievements caching should be enabled.</summary>
        /// <remarks>Defaults to true.</remarks>
        public bool AchievementsCachingEnabled { get; set; } = true;
    }
}