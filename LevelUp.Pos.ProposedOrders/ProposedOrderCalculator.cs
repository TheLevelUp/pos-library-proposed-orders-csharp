﻿#region Copyright (Apache 2.0)
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// <copyright file="ProposedOrderCalculator.cs" company="SCVNGR, Inc. d/b/a LevelUp">
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

using System;
using LevelUp.Api.Utilities;

namespace LevelUp.Pos.ProposedOrders
{
    public class AdjustedCheckValues
    {
        public int SpendAmount { get; internal set; }
        public int TaxAmount { get; }
        public int ExemptionAmount { get; }

        internal AdjustedCheckValues(int spendAmount, int taxAmount, int exemptionAmount)
        {
            SpendAmount = spendAmount;
            TaxAmount = taxAmount;
            ExemptionAmount = exemptionAmount;
        }
        public AdjustedCheckValuesInDollars ToDollars()
        {
            return new AdjustedCheckValuesInDollars(
                Money.ToDollars(SpendAmount),
                Money.ToDollars(TaxAmount),
                Money.ToDollars(ExemptionAmount));
        }

        public override string ToString()
        {
            return $"SpendAmount={SpendAmount};TaxAmount={TaxAmount};ExemptionAmount={ExemptionAmount};";
        }
    }

    public class AdjustedCheckValuesInDollars
    {
        public decimal SpendAmount { get; }
        public decimal TaxAmount { get; }
        public decimal ExemptionAmount { get; }

        internal AdjustedCheckValuesInDollars(decimal spendAmount, decimal taxAmount, decimal exemptionAmount)
        {
            SpendAmount = spendAmount;
            TaxAmount = taxAmount;
            ExemptionAmount = exemptionAmount;
        }

        public AdjustedCheckValues ToCents()
        {
            return new AdjustedCheckValues(
                Money.ToCents(SpendAmount), 
                Money.ToCents(TaxAmount),
                Money.ToCents(ExemptionAmount));
        }

        public override string ToString()
        {
            return $"SpendAmount=${SpendAmount};TaxAmount=${TaxAmount};ExemptionAmount=${ExemptionAmount};";
        }
    }

    public static class ProposedOrderCalculator
    {
        /// <summary>
        /// Accepts known values from the point-of-sale and gives you an AdjustedCheckValues object containing the 
        /// spend_amount, tax_amount, and exemption_amount to submit a LevelUp Create Proposed Order API request.
        /// </summary>
        /// <param name="totalOutstandingAmount">The current total amount of the check, including tax, in cents.</param>
        /// <param name="totalTaxAmount">The current tax due on the check, in cents.</param>
        /// <param name="totalExemptionAmount">The current total of exempted items on the check, in cents.</param>
        /// <param name="customerPaymentAmount">The amount the customer would like to spend, in cents.</param>
        /// <returns>LevelUp.Pos.ProposedOrderCalculator.CalculateCreateProposedOrderValues</returns>
        public static AdjustedCheckValues CalculateCreateProposedOrderValues(
            int totalOutstandingAmount,
            int totalTaxAmount,
            int totalExemptionAmount,
            int customerPaymentAmount
        )
        {
            int adjustedSpendAmount =
                CalculateAdjustedCustomerPaymentAmount(customerPaymentAmount, totalOutstandingAmount);

            int adjustedTaxAmount = CalculateAdjustedTaxAmount(totalOutstandingAmount, totalTaxAmount,
                adjustedSpendAmount);

            int adjustedExemptionAmount = CalculateAdjustedExemptionAmount(totalOutstandingAmount, totalTaxAmount,
                totalExemptionAmount, adjustedSpendAmount);

            return new AdjustedCheckValues(adjustedSpendAmount, adjustedTaxAmount, adjustedExemptionAmount);
        }

        public static AdjustedCheckValues CalculateCreateProposedOrderValuesFromDollars(
            decimal totalOutstandingAmount,
            decimal totalTaxAmount,
            decimal totalExemptionAmount,
            decimal customerPaymentAmount
        )
        {
            return CalculateCreateProposedOrderValues(
                Money.ToCents(totalOutstandingAmount),
                Money.ToCents(totalTaxAmount),
                Money.ToCents(totalExemptionAmount),
                Money.ToCents(customerPaymentAmount));
        }

        /// <summary>
        /// Accepts known values from the point-of-sale and gives you an AdjustedCheckValues object containing the 
        /// spend_amount, tax_amount, and exemption_amount to submit a LevelUp Complete Order API request.
        /// </summary>
        /// <param name="totalOutstandingAmount">The current total amount of the check, including tax, in cents.</param>
        /// <param name="totalTaxAmount">The current tax due on the check, in cents.</param>
        /// <param name="totalExemptionAmount">The current total of exempted items on the check, in cents.</param>
        /// <param name="customerPaymentAmount">The amount the customer would like to spend, in cents.</param>
        /// <param name="appliedDiscountAmount">The discount amount applied to the point of sale for the customer.</param>
        /// <returns>LevelUp.Pos.ProposedOrderCalculator.CalculateCompleteOrderValues</returns>
        public static AdjustedCheckValues CalculateCompleteOrderValues(
            int totalOutstandingAmount,
            int totalTaxAmount,
            int totalExemptionAmount,
            int customerPaymentAmount,
            int appliedDiscountAmount
        )
        {
            AdjustedCheckValues values = CalculateCreateProposedOrderValues(
                totalOutstandingAmount,
                totalTaxAmount,
                totalExemptionAmount,
                customerPaymentAmount);

            values.SpendAmount = CalculateAdjustedSpendAmountCompleteOrder(totalOutstandingAmount, 
                customerPaymentAmount, appliedDiscountAmount);

            return values;
        }

        public static AdjustedCheckValues CalculateCompleteOrderValuesFromDollars(
            decimal totalOutstandingAmount,
            decimal totalTaxAmount,
            decimal totalExemptionAmount,
            decimal customerPaymentAmount,
            decimal appliedDiscountAmount
        )
        {
            return CalculateCompleteOrderValues(
                Money.ToCents(totalOutstandingAmount),
                Money.ToCents(totalTaxAmount),
                Money.ToCents(totalExemptionAmount),
                Money.ToCents(customerPaymentAmount),
                Money.ToCents(appliedDiscountAmount));
        }

        internal static int CalculateAdjustedCustomerPaymentAmount(int totalOutstandingAmount, int customerPaymentAmount)
        {
            return Math.Max(0, Math.Min(customerPaymentAmount, totalOutstandingAmount));
        }

        internal static int CalculateAdjustedTaxAmount(
            int totalOutstandingAmount,
            int totalTaxAmount,
            int postAdjustedCustomerPaymentAmount)
        {
            totalTaxAmount = Math.Max(0, Math.Min(totalTaxAmount, totalOutstandingAmount));

            bool wasPartialPaymentRequested = postAdjustedCustomerPaymentAmount < totalOutstandingAmount;

            if (wasPartialPaymentRequested)
            {
                int remainingAmountOwedAfterSpend = totalOutstandingAmount - postAdjustedCustomerPaymentAmount;

                totalTaxAmount = Math.Max(0, totalTaxAmount - remainingAmountOwedAfterSpend);
            }

            return totalTaxAmount;
        }

        internal static int CalculateAdjustedExemptionAmount(
            int totalOutstandingAmount,
            int totalTaxAmount,
            int totalExemptionAmount,
            int postAdjustedCustomerPaymentAmount)
        {
            int totalOutstandingAmountLessTax = totalOutstandingAmount - totalTaxAmount;

            bool wasPartialPaymentRequestedWrtSubtotal = postAdjustedCustomerPaymentAmount < totalOutstandingAmountLessTax;

            if (wasPartialPaymentRequestedWrtSubtotal)
            {
                // defer the exemption amount to last possible paying customer or customers
                int totalOutstandingLessTaxAfterPayment =
                    Math.Max(0, totalOutstandingAmountLessTax - postAdjustedCustomerPaymentAmount);

                totalExemptionAmount = Math.Max(0, totalExemptionAmount - totalOutstandingLessTaxAfterPayment);
            }

            int adjustedExemptionAmount = Math.Min(Math.Min(totalExemptionAmount, totalOutstandingAmountLessTax),
                postAdjustedCustomerPaymentAmount);

            return Math.Max(0, adjustedExemptionAmount);
        }

        /// <summary>
        /// Adjusts the `spend_amount` by considering the amount a customer wants to pay, the total due on the check 
        /// after applying the discount (0 if none was available), and the discount amount applied. The user will never 
        /// pay more than their requested spend amount, including any discount amount applied.
        /// </summary>
        /// <remarks>
        /// If we know what is owed now, and we know what discount was applied, then we know what was originally owed.
        /// Using that information, we can determine if the customer attempted a partial payment or not. If a customer
        /// attempts a partial payment, the `spend_amount` is equal to the customerSpendAmount. If the customer is
        /// paying the balance in full, the `spend_amount` is equal to the totalOutstandingAmount + appliedDiscountAmount.
        /// </remarks>
        /// <param name="totalOutstandingAmount">The current total amount of the check, including tax, in cents.</param>
        /// <param name="customerSpendAmount">The amount the customer would like to spend, in cents.</param>
        /// <param name="appliedDiscountAmount">The discount amount applied to the point of sale for the customer.</param>
        /// <returns></returns>
        internal static int CalculateAdjustedSpendAmountCompleteOrder(int totalOutstandingAmount,
            int customerSpendAmount,
            int appliedDiscountAmount)
        {
            int theoreticalTotalOutstandingAmount = totalOutstandingAmount + Math.Abs(appliedDiscountAmount);

            return Math.Max(0, Math.Min(customerSpendAmount, theoreticalTotalOutstandingAmount));
        }
    }
}
