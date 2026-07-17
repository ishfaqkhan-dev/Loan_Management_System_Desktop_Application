using System;
using System.Collections.Generic;
using LoanManagementApp.Data;
using LoanManagementApp.Models;

namespace LoanManagementApp.Services
{
    /// <summary>
    /// LoanService / قرض سروس - Core business logic for loans and payments
    /// </summary>
    public class LoanService
    {
        // ─── Dependencies | انحصار ───────────────────────────────────
        private readonly LoanRepository _loanRepo;
        private readonly CustomerRepository _customerRepo;
        private readonly PaymentRepository _paymentRepo;

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public LoanService()
        {
            _loanRepo = new LoanRepository();
            _customerRepo = new CustomerRepository();
            _paymentRepo = new PaymentRepository();
        }

        // ─── Add New Loan | نیا قرض شامل کریں ───────────────────────
        /// <summary>
        /// Add a new loan for a customer with full validation
        /// مکمل جانچ کے ساتھ قرض دار کو نیا قرض شامل کریں
        /// </summary>
        public (bool Success, string Message, int LoanId) AddLoan(Loan loan)
        {
            try
            {
                // ── Validate | جانچیں ──────────────────────────────
                if (loan.CustomerId <= 0)
                    return (false,
                        "Customer is required | قرض دار ضروری ہے", 0);

                if (loan.TotalAmount <= 0)
                    return (false,
                        "Loan amount must be greater than zero | " +
                        "قرض کی رقم صفر سے زیادہ ہونی چاہیے", 0);

                if (loan.TotalInstallments <= 0)
                    return (false,
                        "Installments count must be greater than zero | " +
                        "اقساط کی تعداد صفر سے زیادہ ہونی چاہیے", 0);

                if (loan.InstallmentAmount <= 0)
                    return (false,
                        "Installment amount must be greater than zero | " +
                        "قسط کی رقم صفر سے زیادہ ہونی چاہیے", 0);

                if (loan.EndDate <= loan.StartDate)
                    return (false,
                        "End date must be after start date | " +
                        "آخری تاریخ شروع کی تاریخ کے بعد ہونی چاہیے", 0);

                // Check customer exists | قرض دار موجود ہے یا نہیں
                var customer = _customerRepo.GetCustomerById(loan.CustomerId);
                if (customer == null)
                    return (false,
                        "Customer not found | قرض دار نہیں ملا", 0);

                // ── Set calculated fields | حسابی فیلڈ سیٹ کریں ───
                loan.RemainingAmount = loan.TotalAmount;
                loan.PaidAmount = 0;
                loan.RemainingInstallments = loan.TotalInstallments;
                loan.PaidInstallments = 0;
                loan.Status = LoanStatus.Active;
                loan.IsMerged = false;

                // ── Save loan | قرض محفوظ کریں ──────────────────────
                int newLoanId = _loanRepo.AddLoan(loan);

                // ── Update customer totals | قرض دار کا کل اپ ڈیٹ ──
                UpdateCustomerTotals(loan.CustomerId);

                // ── Update customer's loan start and end dates | قرض دار کی تاریخیں اپ ڈیٹ ──
                _customerRepo.UpdateCustomerLoanDates(loan.CustomerId, loan.StartDate, loan.EndDate);

                return (true,
                    "Loan added successfully | قرض کامیابی سے شامل ہوا",
                    newLoanId);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to add loan | قرض شامل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Loan By ID | شناخت سے قرض حاصل کریں ───────────────────────
        /// <summary>
        /// Get a single loan by its ID | شناخت کے ذریعے قرض حاصل کریں
        /// </summary>
        public Loan? GetLoanById(int loanId)
        {
            try
            {
                return _loanRepo.GetLoanById(loanId);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get loan | قرض حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Add Loan On Existing (Merge) | موجودہ پر قرض ملائیں ────
        /// <summary>
        /// Add new loan while existing loan is still active - merges balances into existing loan
        /// موجودہ قرض فعال ہونے پر نیا قرض - موجودہ قرض میں شامل کریں
        /// </summary>
        public (bool Success, string Message, int LoanId) AddLoanOnExisting(
            int customerId,
            decimal newLoanAmount,
            int newTotalInstallments,
            decimal newInstallmentAmount,
            DateTime startDate,
            DateTime endDate,
            string notes = "")
        {
            try
            {
                // Get active loan | فعال قرض حاصل کریں
                var activeLoan = _loanRepo.GetActiveLoanByCustomer(customerId);

                if (activeLoan == null)
                {
                    // No active loan - just add normally | فعال قرض نہیں - عام طریقے سے شامل
                    var newLoan = new Loan
                    {
                        CustomerId = customerId,
                        TotalAmount = newLoanAmount,
                        TotalInstallments = newTotalInstallments,
                        InstallmentAmount = newInstallmentAmount,
                        StartDate = startDate,
                        EndDate = endDate,
                        Notes = notes
                    };
                    return AddLoan(newLoan);
                }

                // ── Merge into existing loan | موجودہ قرض میں شامل کریں ──
                // Calculate new totals
                decimal newTotalAmount = activeLoan.TotalAmount + newLoanAmount;
                decimal newRemainingAmount = activeLoan.RemainingAmount + newLoanAmount;
                int newTotalInstallmentsCount = activeLoan.TotalInstallments + newTotalInstallments;
                int newRemainingInstallments = activeLoan.RemainingInstallments + newTotalInstallments;
                decimal newInstallmentAmt = newRemainingInstallments > 0
                    ? newRemainingAmount / newRemainingInstallments
                    : 0;

                // Update the existing loan (no new loan created)
                activeLoan.TotalAmount = newTotalAmount;
                activeLoan.RemainingAmount = newRemainingAmount;
                activeLoan.TotalInstallments = newTotalInstallmentsCount;
                activeLoan.RemainingInstallments = newRemainingInstallments;
                activeLoan.InstallmentAmount = newInstallmentAmt;
                activeLoan.EndDate = endDate; // Use new end date
                activeLoan.Notes += $" | Added {newLoanAmount:N0} PKR on {DateTime.Now:dd-MMM-yyyy} with {newTotalInstallments} installments. {notes}";
                activeLoan.UpdatedAt = DateTime.Now;

                bool updated = _loanRepo.UpdateLoan(activeLoan);
                if (!updated)
                    return (false, "Failed to update loan | قرض اپ ڈیٹ ناکام", 0);

                // Update customer totals | قرض دار کا کل اپ ڈیٹ کریں
                UpdateCustomerTotals(customerId);

                // ── Update customer's loan start and end dates | قرض دار کی تاریخیں اپ ڈیٹ ──
                _customerRepo.UpdateCustomerLoanDates(customerId, startDate, endDate);

                return (true,
                    $"New loan amount merged into existing loan. New total: {newTotalAmount:N0} PKR | " +
                    $"نیا قرض موجودہ قرض میں شامل ہو گیا۔ نئی کل: {newTotalAmount:N0} PKR",
                    activeLoan.Id);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to add loan on existing | موجودہ قرض پر قرض ناکام: {ex.Message}");
            }
        }

        /// <summary>
        /// Merge a new loan amount into an existing active loan without increasing the number of installments.
        /// Only updates the remaining balance and recalculates the per‑installment amount for the remaining installments.
        /// </summary>
        public (bool Success, string Message, int LoanId) MergeLoanAmountOnly(int customerId, decimal additionalAmount, string notes = "")
        {
            try
            {
                var activeLoan = _loanRepo.GetActiveLoanByCustomer(customerId);
                if (activeLoan == null)
                    return (false, "No active loan found for this customer.", 0);

                if (activeLoan.RemainingInstallments <= 0)
                    return (false, "No remaining installments to adjust.", 0);

                // Add the new amount to remaining balance
                decimal newRemainingAmount = activeLoan.RemainingAmount + additionalAmount;
                decimal newInstallmentAmount = Math.Ceiling(newRemainingAmount / activeLoan.RemainingInstallments);

                // Update the existing loan
                activeLoan.TotalAmount += additionalAmount;
                activeLoan.RemainingAmount = newRemainingAmount;
                activeLoan.InstallmentAmount = newInstallmentAmount;
                activeLoan.Notes += $" | Added {additionalAmount:N0} PKR (no new installments) on {DateTime.Now:dd-MMM-yyyy}. {notes}";
                activeLoan.UpdatedAt = DateTime.Now;

                bool updated = _loanRepo.UpdateLoan(activeLoan);
                if (!updated)
                    return (false, "Failed to update loan", 0);

                UpdateCustomerTotals(customerId);
                _customerRepo.UpdateCustomerLoanDates(customerId, activeLoan.StartDate, activeLoan.EndDate);

                return (true, $"Added {additionalAmount:N0} PKR to existing loan. New per-installment amount: {newInstallmentAmount:N0} PKR", activeLoan.Id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to merge loan amount: {ex.Message}");
            }
        }

        // ─── Record Payment | ادائیگی ریکارڈ کریں ───────────────────
        /// <summary>
        /// Record an installment payment with full validation and balance update
        /// مکمل جانچ اور بیلنس اپ ڈیٹ کے ساتھ قسط کی ادائیگی ریکارڈ کریں
        /// </summary>
        public (bool Success, string Message, int PaymentId) RecordPayment(
            int loanId,
            int customerId,
            decimal paidAmount,
            DateTime paymentDate,
            PaymentType paymentType = PaymentType.Cash,
            string paymentMethod = "",
            string receivedBy = "",
            string notes = "")
        {
            try
            {
                // ── Validate | جانچیں ──────────────────────────────
                if (paidAmount <= 0)
                    return (false,
                        "Payment amount must be greater than zero | " +
                        "ادائیگی کی رقم صفر سے زیادہ ہونی چاہیے", 0);

                // Get loan | قرض حاصل کریں
                var loan = _loanRepo.GetLoanById(loanId);
                if (loan == null)
                    return (false, "Loan not found | قرض نہیں ملا", 0);

                if (loan.RemainingAmount <= 0)
                    return (false,
                        "This loan is already fully paid | " +
                        "یہ قرض پہلے سے مکمل ادا ہو چکا ہے", 0);

                if (paidAmount > loan.RemainingAmount)
                    return (false,
                        $"Payment ({paidAmount:N0}) exceeds remaining balance " +
                        $"({loan.RemainingAmount:N0}) | " +
                        $"ادائیگی ({paidAmount:N0}) باقی رقم " +
                        $"({loan.RemainingAmount:N0}) سے زیادہ ہے", 0);

                // ── Build payment record | ادائیگی ریکارڈ بنائیں ───
                decimal balanceBefore = loan.RemainingAmount;
                decimal balanceAfter = balanceBefore - paidAmount;
                int installmentNum = loan.PaidInstallments + 1;
                int remainingAfter = loan.RemainingInstallments > 0
                                             ? loan.RemainingInstallments - 1
                                             : 0;

                var payment = new Payment
                {
                    LoanId = loanId,
                    CustomerId = customerId,
                    PaidAmount = paidAmount,
                    BalanceBeforePayment = balanceBefore,
                    RemainingBalanceAfterPayment = balanceAfter,
                    InstallmentNumber = installmentNum,
                    RemainingInstallmentsAfterPayment = remainingAfter,
                    TotalInstallments = loan.TotalInstallments,
                    PaymentDate = paymentDate,
                    PaymentType = paymentType,
                    PaymentMethod = paymentMethod,
                    ReceivedBy = receivedBy,
                    Notes = notes,
                    IsVerified = true
                };

                // ── Save payment | ادائیگی محفوظ کریں ───────────────
                int newPaymentId = _paymentRepo.AddPayment(payment);

                // ── Update loan balances | قرض بیلنس اپ ڈیٹ کریں ───
                _loanRepo.UpdateLoanAfterPayment(loanId, paidAmount);

                // ── Recalculate installment amount for remaining installments (for custom payments)
                // اگر ادا کی گئی رقم مقررہ قسط سے مختلف ہے تو دوبارہ حساب کریں
                if (paidAmount != loan.InstallmentAmount && loan.RemainingAmount > 0)
                {
                    RecalculateInstallmentAmount(loanId);
                }

                // ── Close loan if fully paid | مکمل ادا ہو تو بند کریں
                if (balanceAfter <= 0)
                    _loanRepo.CloseLoan(loanId);

                // ── Update customer totals | قرض دار کا کل اپ ڈیٹ ──
                UpdateCustomerTotals(customerId);

                string successMsg = balanceAfter <= 0
                    ? "Payment recorded. Loan fully paid! | " +
                      "ادائیگی ریکارڈ ہوئی۔ قرض مکمل ادا ہو گیا!"
                    : $"Payment recorded. Remaining: {balanceAfter:N0} PKR | " +
                      $"ادائیگی ریکارڈ ہوئی۔ باقی: {balanceAfter:N0} PKR";

                return (true, successMsg, newPaymentId);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to record payment | ادائیگی ریکارڈ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Update Loan | قرض اپ ڈیٹ کریں ─────────────────────────
        /// <summary>
        /// Update loan details | قرض کی تفصیل اپ ڈیٹ کریں
        /// </summary>
        public (bool Success, string Message) UpdateLoan(Loan loan)
        {
            try
            {
                if (loan.TotalAmount <= 0)
                    return (false,
                        "Loan amount must be greater than zero | " +
                        "قرض کی رقم صفر سے زیادہ ہونی چاہیے");

                if (loan.TotalInstallments <= 0)
                    return (false,
                        "Installments must be greater than zero | " +
                        "اقساط صفر سے زیادہ ہونی چاہیے");

                if (loan.EndDate <= loan.StartDate)
                    return (false,
                        "End date must be after start date | " +
                        "آخری تاریخ شروع کی تاریخ کے بعد ہونی چاہیے");

                bool updated = _loanRepo.UpdateLoan(loan);

                if (updated)
                    UpdateCustomerTotals(loan.CustomerId);

                return updated
                    ? (true, "Loan updated successfully | قرض کامیابی سے اپ ڈیٹ ہوا")
                    : (false, "Failed to update loan | قرض اپ ڈیٹ کرنے میں ناکامی");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update loan | قرض اپ ڈیٹ ناکام: {ex.Message}");
            }
        }

        // ─── Update Remaining Installments | باقی اقساط اپ ڈیٹ کریں ──
        /// <summary>
        /// Update the remaining installments count and per-installment amount for an active loan.
        /// This does NOT affect already paid installments. The total loan amount remains unchanged.
        /// فعال قرض کی باقی اقساط کی تعداد اور فی قسط رقم اپ ڈیٹ کریں۔
        /// یہ پہلے سے ادا شدہ اقساط کو متاثر نہیں کرتا۔ کل قرض کی رقم تبدیل نہیں ہوتی۔
        /// </summary>
        public bool UpdateLoanRemainingInstallments(int loanId, int newRemainingInstallments, decimal newInstallmentAmount)
        {
            try
            {
                var loan = _loanRepo.GetLoanById(loanId);
                if (loan == null) return false;

                // Only update if the loan is still active
                if (loan.Status != LoanStatus.Active) return false;

                // Update remaining installments and per-installment amount
                loan.RemainingInstallments = newRemainingInstallments;
                loan.InstallmentAmount = newInstallmentAmount;
                // Recalculate total installments (paid + remaining)
                loan.TotalInstallments = loan.PaidInstallments + newRemainingInstallments;
                loan.UpdatedAt = DateTime.Now;

                bool updated = _loanRepo.UpdateLoan(loan);
                if (updated)
                {
                    // Also update customer totals (remaining balance is unchanged, but totals might need refresh)
                    UpdateCustomerTotals(loan.CustomerId);
                }
                return updated;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update remaining installments | باقی اقساط اپ ڈیٹ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Delete Loan | قرض حذف کریں ─────────────────────────────
        /// <summary>
        /// Delete loan and update customer totals
        /// قرض حذف کریں اور قرض دار کا کل اپ ڈیٹ کریں
        /// </summary>
        public (bool Success, string Message) DeleteLoan(int loanId)
        {
            try
            {
                var loan = _loanRepo.GetLoanById(loanId);
                if (loan == null)
                    return (false, "Loan not found | قرض نہیں ملا");

                int customerId = loan.CustomerId;

                bool deleted = _loanRepo.DeleteLoan(loanId);

                if (deleted)
                    UpdateCustomerTotals(customerId);

                return deleted
                    ? (true, "Loan deleted successfully | قرض کامیابی سے حذف ہوا")
                    : (false, "Failed to delete loan | قرض حذف کرنے میں ناکامی");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to delete loan | قرض حذف ناکام: {ex.Message}");
            }
        }

        // ─── Delete Payment | ادائیگی حذف کریں ──────────────────────
        /// <summary>
        /// Delete a payment and recalculate loan balances
        /// ادائیگی حذف کریں اور قرض کا بیلنس دوبارہ حساب کریں
        /// </summary>
        public (bool Success, string Message) DeletePayment(int paymentId)
        {
            try
            {
                var payment = _paymentRepo.GetPaymentById(paymentId);
                if (payment == null)
                    return (false, "Payment not found | ادائیگی نہیں ملی");

                int loanId = payment.LoanId;
                int customerId = payment.CustomerId;

                bool deleted = _paymentRepo.DeletePayment(paymentId);

                if (deleted)
                {
                    // Recalculate loan from payments | ادائیگیوں سے قرض دوبارہ حساب
                    RecalculateLoanFromPayments(loanId);
                    UpdateCustomerTotals(customerId);
                }

                return deleted
                    ? (true, "Payment deleted successfully | ادائیگی کامیابی سے حذف ہوئی")
                    : (false, "Failed to delete payment | ادائیگی حذف کرنے میں ناکامی");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to delete payment | ادائیگی حذف ناکام: {ex.Message}");
            }
        }

        // ─── Get Loans By Customer | قرض دار کے قرضے ────────────────
        /// <summary>
        /// Get all loans for a customer | ایک قرض دار کے تمام قرضے حاصل کریں
        /// </summary>
        public List<Loan> GetLoansByCustomer(int customerId)
        {
            try
            {
                return _loanRepo.GetLoansByCustomer(customerId);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get loans | قرضے حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Payments By Loan | قرض کی ادائیگیاں ────────────────
        /// <summary>
        /// Get all payments for a loan | ایک قرض کی تمام ادائیگیاں حاصل کریں
        /// </summary>
        public List<Payment> GetPaymentsByLoan(int loanId)
        {
            try
            {
                return _paymentRepo.GetPaymentsByLoan(loanId);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get payments | ادائیگیاں حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Payments By Customer | قرض دار کی ادائیگیاں ────────
        /// <summary>
        /// Get all payments for a customer | ایک قرض دار کی تمام ادائیگیاں
        /// </summary>
        public List<Payment> GetPaymentsByCustomer(int customerId)
        {
            try
            {
                return _paymentRepo.GetPaymentsByCustomer(customerId);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get customer payments | قرض دار ادائیگیاں ناکام: {ex.Message}");
            }
        }

        // ─── Get Active Loan | فعال قرض حاصل کریں ───────────────────
        /// <summary>
        /// Get currently active loan for a customer
        /// قرض دار کا ابھی فعال قرض حاصل کریں
        /// </summary>
        public Loan? GetActiveLoan(int customerId)
        {
            try
            {
                return _loanRepo.GetActiveLoanByCustomer(customerId);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get active loan | فعال قرض حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Dashboard Summary | ڈیش بورڈ خلاصہ ─────────────────
        /// <summary>
        /// Get loan summary for dashboard cards
        /// ڈیش بورڈ کارڈز کے لیے قرض کا خلاصہ حاصل کریں
        /// </summary>
        public (decimal TotalLoaned, decimal TotalRemaining,
                decimal TotalCollected, int TotalLoans) GetDashboardSummary()
        {
            try
            {
                return _loanRepo.GetLoanSummary();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get dashboard summary | ڈیش بورڈ خلاصہ ناکام: {ex.Message}");
            }
        }

        // ─── Get Overdue Loans | میعاد ختم قرضے ─────────────────────
        /// <summary>
        /// Get all overdue loans | تمام میعاد ختم قرضے حاصل کریں
        /// </summary>
        public List<Loan> GetOverdueLoans()
        {
            try
            {
                return _loanRepo.GetOverdueLoans();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get overdue loans | میعاد ختم قرضے ناکام: {ex.Message}");
            }
        }

        // ─── Get Monthly Chart Data | ماہانہ چارٹ ڈیٹا ─────────────
        /// <summary>
        /// Get monthly collection data for chart
        /// چارٹ کے لیے ماہانہ وصولی کا ڈیٹا حاصل کریں
        /// </summary>
        public List<(string Month, decimal Amount)> GetMonthlyChartData(int months = 6)
        {
            try
            {
                return _paymentRepo.GetMonthlyCollectionData(months);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get chart data | چارٹ ڈیٹا ناکام: {ex.Message}");
            }
        }

        // ─── Calculate Installment Amount | قسط کی رقم حساب کریں ────
        /// <summary>
        /// Calculate per installment amount from total and count
        /// کل اور تعداد سے فی قسط رقم حساب کریں
        /// </summary>
        public decimal CalculateInstallmentAmount(
            decimal totalAmount,
            int totalInstallments)
        {
            if (totalInstallments <= 0 || totalAmount <= 0) return 0;
            return Math.Ceiling(totalAmount / totalInstallments);
        }

        // ─── Private Helpers ─────────────────────────────────────────

        /// <summary>
        /// Update customer total balances from all loans
        /// تمام قرضوں سے قرض دار کا کل بیلنس اپ ڈیٹ کریں
        /// </summary>
        private void UpdateCustomerTotals(int customerId)
        {
            var loans = _loanRepo.GetLoansByCustomer(customerId);

            decimal totalLoan = 0;
            decimal totalPaid = 0;
            decimal totalRemaining = 0;

            foreach (var loan in loans)
            {
                // Only count non-merged loans | صرف غیر ضم شدہ قرضے گنیں
                if (loan.Status != LoanStatus.Merged)
                {
                    totalLoan += loan.TotalAmount;
                    totalPaid += loan.PaidAmount;
                    totalRemaining += loan.RemainingAmount;
                }
            }

            _customerRepo.UpdateCustomerBalance(
                customerId, totalLoan, totalPaid, totalRemaining);
        }

        /// <summary>
        /// Recalculate loan totals from its payment history
        /// ادائیگی تاریخ سے قرض کا کل دوبارہ حساب کریں
        /// </summary>
        private void RecalculateLoanFromPayments(int loanId)
        {
            var loan = _loanRepo.GetLoanById(loanId);
            if (loan == null) return;

            var payments = _paymentRepo.GetPaymentsByLoan(loanId);

            decimal totalPaid = 0;
            int paidInstallments = payments.Count;

            foreach (var p in payments)
                totalPaid += p.PaidAmount;

            loan.PaidAmount = totalPaid;
            loan.RemainingAmount = loan.TotalAmount - totalPaid;
            loan.PaidInstallments = paidInstallments;
            loan.RemainingInstallments = loan.TotalInstallments - paidInstallments;

            // Reopen if balance exists | بیلنس ہو تو دوبارہ کھولیں
            if (loan.RemainingAmount > 0 && loan.Status == LoanStatus.Closed)
                loan.Status = LoanStatus.Active;

            _loanRepo.UpdateLoan(loan);
        }

        /// <summary>
        /// Recalculate the installment amount based on remaining balance and remaining installments.
        /// باقی رقم اور باقی اقساط کی بنیاد پر قسط کی رقم دوبارہ شمار کریں۔
        /// </summary>
        private void RecalculateInstallmentAmount(int loanId)
        {
            var loan = _loanRepo.GetLoanById(loanId);
            if (loan == null) return;
            if (loan.RemainingInstallments <= 0) return;

            decimal newInstallmentAmount = Math.Ceiling(loan.RemainingAmount / loan.RemainingInstallments);
            if (newInstallmentAmount != loan.InstallmentAmount)
            {
                loan.InstallmentAmount = newInstallmentAmount;
                _loanRepo.UpdateLoan(loan);
            }
        }
    }
}