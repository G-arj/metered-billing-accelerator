﻿namespace Metering.Types

type Plan =
    { PlanId: PlanId
      BillingDimensions: BillingDimension seq }

//type Plans = Map<PlanId, Plan>