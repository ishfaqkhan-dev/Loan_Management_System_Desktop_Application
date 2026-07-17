using System;
using System.Collections.Generic;
using LoanManagementApp.Models;
using Microsoft.Data.Sqlite;

namespace LoanManagementApp.Data
{
    /// <summary>
    /// PaymentRepository / ادائیگی ریپوزٹری - All CRUD operations for Payments
    /// </summary>
    public class PaymentRepository
    {
        // ─── Add Payment | ادائیگی شامل کریں ────────────────────────
        /// <summary>
        /// Add new payment record | نئی ادائیگی ریکارڈ شامل کریں
        /// </summary>
        public int AddPayment(Payment payment)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var transaction = connection.BeginTransaction();

                try
                {
                    using var cmd = connection.CreateCommand();
                    cmd.Transaction = transaction;

                    payment.CreatedAt = DateTime.Now;
                    payment.PaymentDate = payment.PaymentDate == default
                        ? DateTime.Now
                        : payment.PaymentDate;

                    // Generate voucher number | وصولی نمبر بنائیں
                    if (string.IsNullOrEmpty(payment.VoucherNumber))
                        payment.VoucherNumber = GenerateVoucherNumber(
                            connection, cmd, transaction);

                    cmd.Parameters.Clear();
                    cmd.CommandText = @"
                        INSERT INTO Payments
                        (
                            LoanId, CustomerId,
                            PaidAmount, RemainingBalanceAfterPayment,
                            BalanceBeforePayment, InstallmentNumber,
                            RemainingInstallmentsAfterPayment, TotalInstallments,
                            PaymentDate, PaymentType, PaymentMethod,
                            VoucherNumber, ReceivedBy, Notes,
                            IsVerified, CreatedAt
                        )
                        VALUES
                        (
                            @LoanId, @CustomerId,
                            @PaidAmount, @RemainingBalanceAfterPayment,
                            @BalanceBeforePayment, @InstallmentNumber,
                            @RemainingInstallmentsAfterPayment, @TotalInstallments,
                            @PaymentDate, @PaymentType, @PaymentMethod,
                            @VoucherNumber, @ReceivedBy, @Notes,
                            @IsVerified, @CreatedAt
                        );
                        SELECT last_insert_rowid();";

                    BindPaymentParameters(cmd, payment);

                    var newId = Convert.ToInt32(cmd.ExecuteScalar());

                    transaction.Commit();
                    return newId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to add payment | ادائیگی شامل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Update Payment | ادائیگی اپ ڈیٹ کریں ──────────────────
        /// <summary>
        /// Update existing payment record | موجودہ ادائیگی ریکارڈ اپ ڈیٹ کریں
        /// </summary>
        public bool UpdatePayment(Payment payment)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Payments SET
                        PaidAmount                          = @PaidAmount,
                        RemainingBalanceAfterPayment        = @RemainingBalanceAfterPayment,
                        BalanceBeforePayment                = @BalanceBeforePayment,
                        InstallmentNumber                   = @InstallmentNumber,
                        RemainingInstallmentsAfterPayment   = @RemainingInstallmentsAfterPayment,
                        TotalInstallments                   = @TotalInstallments,
                        PaymentDate                         = @PaymentDate,
                        PaymentType                         = @PaymentType,
                        PaymentMethod                       = @PaymentMethod,
                        VoucherNumber                       = @VoucherNumber,
                        ReceivedBy                          = @ReceivedBy,
                        Notes                               = @Notes,
                        IsVerified                          = @IsVerified
                    WHERE Id = @Id;";

                BindPaymentParameters(cmd, payment);
                cmd.Parameters.AddWithValue("@Id", payment.Id);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update payment | ادائیگی اپ ڈیٹ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Delete Payment | ادائیگی حذف کریں ─────────────────────
        /// <summary>
        /// Delete payment by ID | شناخت کے ذریعے ادائیگی حذف کریں
        /// </summary>
        public bool DeletePayment(int paymentId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    DELETE FROM Payments
                    WHERE Id = @Id;";
                cmd.Parameters.AddWithValue("@Id", paymentId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to delete payment | ادائیگی حذف کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Payment By ID | شناخت سے ادائیگی حاصل کریں ────────
        /// <summary>
        /// Get single payment by ID | ایک ادائیگی شناخت سے حاصل کریں
        /// </summary>
        public Payment? GetPaymentById(int paymentId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT * FROM Payments
                    WHERE Id = @Id;";
                cmd.Parameters.AddWithValue("@Id", paymentId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return MapPayment(reader);

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get payment | ادائیگی حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Payments By Loan | قرض کی ادائیگیاں حاصل کریں ─────
        /// <summary>
        /// Get all payments for a loan | ایک قرض کی تمام ادائیگیاں حاصل کریں
        /// </summary>
        public List<Payment> GetPaymentsByLoan(int loanId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT * FROM Payments
                    WHERE LoanId = @LoanId
                    ORDER BY PaymentDate DESC;";
                cmd.Parameters.AddWithValue("@LoanId", loanId);

                using var reader = cmd.ExecuteReader();
                var payments = new List<Payment>();

                while (reader.Read())
                    payments.Add(MapPayment(reader));

                return payments;
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
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT * FROM Payments
                    WHERE CustomerId = @CustomerId
                    ORDER BY PaymentDate DESC;";
                cmd.Parameters.AddWithValue("@CustomerId", customerId);

                using var reader = cmd.ExecuteReader();
                var payments = new List<Payment>();

                while (reader.Read())
                    payments.Add(MapPayment(reader));

                return payments;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get customer payments | قرض دار کی ادائیگیاں حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Payments By Date Range | تاریخ کے مطابق ادائیگیاں ──
        /// <summary>
        /// Get payments within a date range | تاریخ کی حد میں ادائیگیاں حاصل کریں
        /// </summary>
        public List<Payment> GetPaymentsByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT * FROM Payments
                    WHERE PaymentDate >= @StartDate
                      AND PaymentDate <= @EndDate
                    ORDER BY PaymentDate DESC;";

                cmd.Parameters.AddWithValue("@StartDate",
                    startDate.ToString("yyyy-MM-dd 00:00:00"));
                cmd.Parameters.AddWithValue("@EndDate",
                    endDate.ToString("yyyy-MM-dd 23:59:59"));

                using var reader = cmd.ExecuteReader();
                var payments = new List<Payment>();

                while (reader.Read())
                    payments.Add(MapPayment(reader));

                return payments;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get payments by date | تاریخ کے مطابق ادائیگیاں حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Today's Payments | آج کی ادائیگیاں ─────────────────
        /// <summary>
        /// Get all payments made today | آج کی تمام ادائیگیاں حاصل کریں
        /// </summary>
        public List<Payment> GetTodaysPayments()
        {
            return GetPaymentsByDateRange(
                DateTime.Today,
                DateTime.Today.AddDays(1).AddSeconds(-1));
        }

        // ─── Get Recent Payments | حالیہ ادائیگیاں ──────────────────
        /// <summary>
        /// Get recent payments with limit | حد کے ساتھ حالیہ ادائیگیاں
        /// </summary>
        public List<Payment> GetRecentPayments(int limit = 10)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT p.*, c.FullName AS CustomerName
                    FROM Payments p
                    LEFT JOIN Customers c ON p.CustomerId = c.Id
                    ORDER BY p.PaymentDate DESC
                    LIMIT @Limit;";
                cmd.Parameters.AddWithValue("@Limit", limit);

                using var reader = cmd.ExecuteReader();
                var payments = new List<Payment>();

                while (reader.Read())
                    payments.Add(MapPayment(reader));

                return payments;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get recent payments | حالیہ ادائیگیاں حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Payment Summary | ادائیگی کا خلاصہ ─────────────────
        /// <summary>
        /// Get payment summary for dashboard
        /// ڈیش بورڈ کے لیے ادائیگی کا خلاصہ حاصل کریں
        /// </summary>
        public (decimal TodayCollection, decimal MonthCollection,
                decimal TotalCollection, int TotalPayments) GetPaymentSummary()
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT
                        SUM(CASE WHEN date(PaymentDate) = date('now')
                            THEN PaidAmount ELSE 0 END)         AS TodayCollection,
                        SUM(CASE WHEN strftime('%Y-%m', PaymentDate)
                            = strftime('%Y-%m', 'now')
                            THEN PaidAmount ELSE 0 END)         AS MonthCollection,
                        SUM(PaidAmount)                         AS TotalCollection,
                        COUNT(*)                                AS TotalPayments
                    FROM Payments;";

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return (
                        TodayCollection: reader.IsDBNull(0) ? 0 : reader.GetDecimal(0),
                        MonthCollection: reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                        TotalCollection: reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                        TotalPayments: reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                    );
                }

                return (0, 0, 0, 0);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get payment summary | ادائیگی کا خلاصہ حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Monthly Collection Chart Data | ماہانہ چارٹ ڈیٹا ──
        /// <summary>
        /// Get monthly collection data for chart
        /// چارٹ کے لیے ماہانہ وصولی کا ڈیٹا حاصل کریں
        /// </summary>
        public List<(string Month, decimal Amount)> GetMonthlyCollectionData(int months = 6)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT
                        strftime('%Y-%m', PaymentDate) AS Month,
                        SUM(PaidAmount)                AS Amount
                    FROM Payments
                    WHERE PaymentDate >= date('now', @MonthsBack)
                    GROUP BY strftime('%Y-%m', PaymentDate)
                    ORDER BY Month ASC;";

                cmd.Parameters.AddWithValue("@MonthsBack", $"-{months} months");

                using var reader = cmd.ExecuteReader();
                var data = new List<(string Month, decimal Amount)>();

                while (reader.Read())
                {
                    data.Add((
                        Month: reader.GetString(0),
                        Amount: reader.GetDecimal(1)
                    ));
                }

                return data;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get monthly data | ماہانہ ڈیٹا حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Last Payment By Loan | قرض کی آخری ادائیگی ─────────
        /// <summary>
        /// Get last payment for a loan | ایک قرض کی آخری ادائیگی حاصل کریں
        /// </summary>
        public Payment? GetLastPaymentByLoan(int loanId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT * FROM Payments
                    WHERE LoanId = @LoanId
                    ORDER BY PaymentDate DESC
                    LIMIT 1;";
                cmd.Parameters.AddWithValue("@LoanId", loanId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return MapPayment(reader);

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get last payment | آخری ادائیگی حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Total Paid By Customer | قرض دار کی کل ادائیگی ─────
        /// <summary>
        /// Get total amount paid by a customer
        /// ایک قرض دار کی کل ادا شدہ رقم حاصل کریں
        /// </summary>
        public decimal GetTotalPaidByCustomer(int customerId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT COALESCE(SUM(PaidAmount), 0)
                    FROM Payments
                    WHERE CustomerId = @CustomerId;";
                cmd.Parameters.AddWithValue("@CustomerId", customerId);

                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get total paid | کل ادائیگی حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Private Helper Methods ──────────────────────────────────

        /// <summary>
        /// Generate unique voucher number | منفرد وصولی نمبر بنائیں
        /// Format: PMT-YYYYMMDD-XXXX
        /// </summary>
        private string GenerateVoucherNumber(
            SqliteConnection connection,
            SqliteCommand cmd,
            SqliteTransaction transaction)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT COUNT(*) FROM Payments;";
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return $"PMT-{DateTime.Now:yyyyMMdd}-{(count + 1):D4}";
        }

        /// <summary>
        /// Bind payment parameters to command | کمانڈ میں پیرامیٹر ڈالیں
        /// </summary>
        private void BindPaymentParameters(SqliteCommand cmd, Payment payment)
        {
            cmd.Parameters.AddWithValue("@LoanId", payment.LoanId);
            cmd.Parameters.AddWithValue("@CustomerId", payment.CustomerId);
            cmd.Parameters.AddWithValue("@PaidAmount", payment.PaidAmount);
            cmd.Parameters.AddWithValue("@RemainingBalanceAfterPayment",
                payment.RemainingBalanceAfterPayment);
            cmd.Parameters.AddWithValue("@BalanceBeforePayment",
                payment.BalanceBeforePayment);
            cmd.Parameters.AddWithValue("@InstallmentNumber",
                payment.InstallmentNumber);
            cmd.Parameters.AddWithValue("@RemainingInstallmentsAfterPayment",
                payment.RemainingInstallmentsAfterPayment);
            cmd.Parameters.AddWithValue("@TotalInstallments",
                payment.TotalInstallments);
            cmd.Parameters.AddWithValue("@PaymentDate",
                payment.PaymentDate.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@PaymentType", (int)payment.PaymentType);
            cmd.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);
            cmd.Parameters.AddWithValue("@VoucherNumber", payment.VoucherNumber);
            cmd.Parameters.AddWithValue("@ReceivedBy", payment.ReceivedBy);
            cmd.Parameters.AddWithValue("@Notes", payment.Notes);
            cmd.Parameters.AddWithValue("@IsVerified", payment.IsVerified ? 1 : 0);
            cmd.Parameters.AddWithValue("@CreatedAt",
                payment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        /// <summary>
        /// Map database reader to Payment object | ریڈر سے ادائیگی آبجیکٹ بنائیں
        /// </summary>
        private Payment MapPayment(SqliteDataReader reader)
        {
            return new Payment
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                LoanId = reader.GetInt32(reader.GetOrdinal("LoanId")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                PaidAmount = reader.GetDecimal(reader.GetOrdinal("PaidAmount")),
                RemainingBalanceAfterPayment = reader.GetDecimal(reader.GetOrdinal("RemainingBalanceAfterPayment")),
                BalanceBeforePayment = reader.GetDecimal(reader.GetOrdinal("BalanceBeforePayment")),
                InstallmentNumber = reader.GetInt32(reader.GetOrdinal("InstallmentNumber")),
                RemainingInstallmentsAfterPayment = reader.GetInt32(reader.GetOrdinal("RemainingInstallmentsAfterPayment")),
                TotalInstallments = reader.GetInt32(reader.GetOrdinal("TotalInstallments")),
                PaymentDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("PaymentDate"))),
                PaymentType = (PaymentType)reader.GetInt32(reader.GetOrdinal("PaymentType")),
                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                VoucherNumber = reader.GetString(reader.GetOrdinal("VoucherNumber")),
                ReceivedBy = reader.GetString(reader.GetOrdinal("ReceivedBy")),
                Notes = reader.GetString(reader.GetOrdinal("Notes")),
                IsVerified = reader.GetInt32(reader.GetOrdinal("IsVerified")) == 1,
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt")))
            };
        }

        /// <summary>
        /// Get daily payment totals for a specific month (day by day)
        /// کسی مخصوص مہینے کے لیے روزانہ ادائیگیوں کا کل حاصل کریں۔
        /// </summary>
        public List<(int Day, decimal Amount)> GetDailyCollectionForMonth(int year, int month)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT 
                        CAST(strftime('%d', PaymentDate) AS INTEGER) AS Day,
                        SUM(PaidAmount) AS Amount
                    FROM Payments
                    WHERE strftime('%Y', PaymentDate) = @Year
                      AND strftime('%m', PaymentDate) = @Month
                    GROUP BY Day
                    ORDER BY Day ASC;";
                cmd.Parameters.AddWithValue("@Year", year.ToString());
                cmd.Parameters.AddWithValue("@Month", month.ToString("D2"));
                using var reader = cmd.ExecuteReader();
                var result = new List<(int Day, decimal Amount)>();
                while (reader.Read())
                {
                    result.Add((reader.GetInt32(0), reader.GetDecimal(1)));
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get daily collection: {ex.Message}");
            }
        }
    }
}