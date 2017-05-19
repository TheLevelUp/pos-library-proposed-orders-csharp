﻿#region Copyright (Apache 2.0)
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// <copyright file="UpdateSpendAmountTests.cs" company="SCVNGR, Inc. d/b/a LevelUp">
//   Copyright(c) 2017 SCVNGR, Inc. d/b/a LevelUp. All rights reserved.
// </copyright>
// <license publisher="Apache Software Foundation" date="January 2004" version="2.0">
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
//   in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License
//   is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
//   or implied. See the License for the specific language governing permissions and limitations under
//   the License.
// </license>
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#endregion

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LevelUp.Pos.ProposedOrders.Tests
{
    [TestClass]
    public class UpdateSpendAmountTests
    {
        // Payment requested > total due
        [TestMethod]
        public void UpdateSpend_WhenProposedOrderRequestIs_PayingTooMuch()
        {
            int outstandingTotalOnCheck = 1000;
            int amountCustomerIsPaying = 1200;

            ProposedOrderCalculator.CalculateAdjustedCustomerPaymentAmount(outstandingTotalOnCheck, amountCustomerIsPaying)
                .Should()
                .Be(outstandingTotalOnCheck);
        }

        // Paid In Full
        [TestMethod]
        public void UpdateSpend_WhenProposedOrderRequestIs_PaidInFull()
        {
            int outstandingTotalOnCheck = 1000;
            int amountCustomerIsPaying = 1000;

            ProposedOrderCalculator.CalculateAdjustedCustomerPaymentAmount(outstandingTotalOnCheck, amountCustomerIsPaying)
                .Should()
                .Be(amountCustomerIsPaying);
        }

        // Partial payment, payment requested < subtotal
        [TestMethod]
        public void UpdateSpend_WhenProposedOrderRequestIs_PartialPayment()
        {
            int outstandingTotalOnCheck = 1000;
            int amountCustomerIsPaying = 800;

            ProposedOrderCalculator.CalculateAdjustedCustomerPaymentAmount(outstandingTotalOnCheck, amountCustomerIsPaying)
                .Should()
                .Be(amountCustomerIsPaying);
        }

        // Zero dollar payment
        [TestMethod]
        public void UpdateSpend_WhenProposedOrderRequestIs_PayingZero()
        {
            int outstandingTotalOnCheck = 1000;
            int amountCustomerIsPaying = 0;

            ProposedOrderCalculator.CalculateAdjustedCustomerPaymentAmount(outstandingTotalOnCheck, amountCustomerIsPaying)
                .Should()
                .Be(amountCustomerIsPaying);
        }
    }
}