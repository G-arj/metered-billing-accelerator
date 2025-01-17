﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Metering.BaseTypes

type ApplicationInternalMeterName = private ApplicationInternalMeterName of string // A meter name used between app and aggregator

module ApplicationInternalMeterName =
    let value (ApplicationInternalMeterName x) = x
    let create x = (ApplicationInternalMeterName x)
