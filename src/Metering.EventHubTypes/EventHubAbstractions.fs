﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Metering.BaseTypes.EventHub

open System.Runtime.CompilerServices
open Metering.BaseTypes

type SequenceNumber = int64

type DifferenceBetweenTwoSequenceNumbers = SequenceNumber

type PartitionID = PartitionID of string

[<Extension>]
module PartitionID =
    [<Extension>]
    let value (PartitionID x) = x
    let create x = (PartitionID x)

type PartitionKey = PartitionKey of string

[<Extension>]
module PartitionKey =
    [<Extension>]
    let value (PartitionKey x) = x
    let create x = (PartitionKey x)

type PartitionIdentifier =
    | PartitionID of PartitionID
    | PartitionKey of PartitionKey

module PartitionIdentifier =
    let createId = PartitionID.create >> PartitionID
    let createKey = PartitionKey.create >> PartitionKey

type MessagePosition = 
    { PartitionID: PartitionID
      SequenceNumber: SequenceNumber
      // Offset: int64
      PartitionTimestamp: MeteringDateTime }

type StartingPosition =
    | Earliest
    | NextEventAfter of LastProcessedSequenceNumber: SequenceNumber * PartitionTimestamp: MeteringDateTime

module MessagePosition =
    let startingPosition (someMessagePosition: MessagePosition option) : StartingPosition =
        match someMessagePosition with
        | None -> Earliest
        | Some pos -> NextEventAfter(
            LastProcessedSequenceNumber = pos.SequenceNumber, 
            PartitionTimestamp = pos.PartitionTimestamp)
    
    let createData (partitionId: string) (sequenceNumber: int64) (partitionTimestamp: MeteringDateTime) =
        { PartitionID = partitionId |> PartitionID.create
          SequenceNumber = sequenceNumber
          PartitionTimestamp = partitionTimestamp }

type SeekPosition =
    | FromSequenceNumber of SequenceNumber: SequenceNumber 
    | Earliest
    | FromTail

type EventsToCatchup =
    { /// The sequence number observed the last event to be enqueued in the partition.
      LastSequenceNumber: SequenceNumber

      /// The date and time, in UTC, that the last event was enqueued in the partition.
      LastEnqueuedTime: MeteringDateTime

      NumberOfEvents: DifferenceBetweenTwoSequenceNumbers
      TimeDeltaSeconds: float }

/// Indicate whether an event was read from EventHub, or from the associated capture storage.
type EventSource =
    | EventHub
    | Capture of BlobName:string

module EventSource =
    let toStr = 
        function
        | EventHub -> "EventHub"
        | Capture(BlobName=b) -> b 

type EventHubEvent<'TEvent> =
    { MessagePosition: MessagePosition
      EventsToCatchup: EventsToCatchup option
      EventData: 'TEvent

      /// Indicate whether an event was read from EventHub, or from the associated capture storage.      
      Source: EventSource }

type EventHubProcessorEvent<'TState, 'TEvent> =    
    | PartitionInitializing of PartitionID:PartitionID * InitialState:'TState
    | PartitionClosing of PartitionID
    | EventHubEvent of EventHubEvent<'TEvent>
    | EventHubError of PartitionID:PartitionID * Exception:exn

type EventHubName =
    { NamespaceName: string
      FullyQualifiedNamespace: string
      InstanceName: string }

module EventHubName =
    let create nameSpaceName instanceName =
        { NamespaceName = nameSpaceName
          FullyQualifiedNamespace = $"{nameSpaceName}.servicebus.windows.net"
          InstanceName = instanceName }

    let toStr (e: EventHubName) = $"{e.FullyQualifiedNamespace}/{e.InstanceName}"