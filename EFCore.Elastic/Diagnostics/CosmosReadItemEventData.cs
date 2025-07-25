// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;

namespace Microsoft.EntityFrameworkCore.Cosmos.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for Cosmos read-item events.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class CosmosReadItemEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="resourceId">The ID of the resource being read.</param>
    /// <param name="containerId">The ID of the Cosmos container being queried.</param>
    /// <param name="partitionKeyValue">The key value of the Cosmos partition that the query is using.</param>
    /// <param name="logSensitiveData">Indicates whether the application allows logging of sensitive data.</param>
    public CosmosReadItemEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        string resourceId,
        string containerId,
        bool logSensitiveData)
        : base(eventDefinition, messageGenerator)
    {
        ResourceId = resourceId;
        ContainerId = containerId;
        LogSensitiveData = logSensitiveData;
    }

    /// <summary>
    ///     The ID of the Cosmos container being queried.
    /// </summary>
    public virtual string ContainerId { get; }

    /// <summary>
    ///     The ID of the resource being read.
    /// </summary>
    public virtual string ResourceId { get; }

    /// <summary>
    ///     Indicates whether the application allows logging of sensitive data.
    /// </summary>
    public virtual bool LogSensitiveData { get; }
}

