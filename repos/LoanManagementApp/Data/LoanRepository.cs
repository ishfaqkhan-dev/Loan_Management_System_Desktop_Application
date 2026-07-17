using System;
using System.Collections.Generic;
using LoanManagementApp.Models;
using Microsoft.Data.Sqlite;

namespace LoanManagementApp.Data
{
    /// <summary>
    /// LoanRepository / قرض ریپوزٹری - All CRUD operations for Loans
    /// </summary>
    public class LoanRepository
    {
        // ─── Add Loan | قرض شامل کریں ───────────────────────────────
        /// <summary>
        /// Add new loan to database | نیا قرض ڈیٹابیس میں شامل کریں
        /// </summary>
        public int AddLoan(Loan loan)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                loan.CreatedAt = DateTime.Now;
                loan.UpdatedAt = DateTime.Now;

                // Get loan number for this customer
                // اس قرض دار کے لیے قرض نمبر حاصل کریں
                loan.LoanNumber = GetNextLoanNumber(connection, loan.CustomerId);

                cmd.CommandText = @"
                    INSERT INTO Loans
                    (
                        CustomerId, TotalAmount, RemainingAmount, PaidAmount,
                        TotalInstallments, RemainingInstallments, PaidInstallments,
                        InstallmentAmount, StartDate, EndDate,
                        Status, LoanNumber, IsMerged, Notes,
                        CreatedAt, UpdatedAt
                    )
                    VALUES
                    (
                        @CustomerId, @TotalAmount, @RemainingAmount, @PaidAmount,
                        @TotalInstallments, @RemainingInstallments, @PaidInstallments,
                        @InstallmentAmount, @StartDate, @EndDate,
                        @Status, @LoanNumber, @IsMerged, @Notes,
                        @CreatedAt, @UpdatedAt
                    );
                    SELECT last_insert_rowid();";

                BindLoanParameters(cmd, loan);

                var newId = Convert.ToInt32(cmd.ExecuteScalar());
                return newId;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to add loan | قرض شامل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Update Loan | قرض اپ ڈیٹ کریں ─────────────────────────
        /// <summary>
        /// Update existing loan | موجودہ قرض اپ ڈیٹ کریں
        /// </summary>
        public bool UpdateLoan(Loan loan)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                loan.UpdatedAt = DateTime.Now;

                cmd.CommandText = @"
                    UPDATE Loans SET
                        TotalAmount             = @TotalAmount,
                        RemainingAmount         = @RemainingAmount,
                        PaidAmount              = @PaidAmount,
                        TotalInstallments       = @TotalInstallments,
                        RemainingInstallments   = @RemainingInstallments,
                        PaidInstallments        = @PaidInstallments,
                        InstallmentAmount       = @InstallmentAmount,
                        StartDate               = @StartDate,
                        EndDate                 = @EndDate,
                        Status                  = @Status,
                        LoanNumber              = @LoanNumber,
                        IsMerged                = @IsMerged,
                        Notes                   = @Notes,
                        UpdatedAt               = @UpdatedAt
                    WHERE Id = @Id;";

                BindLoanParameters(cmd, loan);
                cmd.Parameters.AddWithValue("@Id", loan.Id);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update loan | قرض اپ ڈیٹ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Delete Loan | قرض حذف کریں ─────────────────────────────
        /// <summary>
        /// Delete loan by ID | شناخت کے ذریعے قرض حذف کریں
        /// </summary>
        public bool DeleteLoan(int loanId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                // CASCADE will delete payments too
                // CASCADE سے ادائیگیاں بھی حذف ہوں گی
                cmd.CommandText = @"
                    DELETE FROM Loans 
                    WHERE Id = @Id;";
                cmd.Parameters.AddWithValue("@Id", loanId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to delete loan | قرض حذف کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Loan By ID | شناخت سے قرض حاصل کریں ───────────────
        /// <summary>
        /// Get single loan by ID | ایک قرض شناخت سے حاصل کریں
        /// </summary>
        public Loan? GetLoanById(int loanId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT * FROM Loans 
                    WHERE Id = @Id;";
                cmd.Parameters.AddWithValue("@Id", loanId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return MapLoan(reader);

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get loan | قرض حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Loans By Customer | قرض دار کے قرضے حاصل کریں ─────
        /// <summary>
        /// Get all loans for a customer | ایک قرض دار کے تمام قرضے حاصل کریں
        /// </summary>
        public List<Loan> GetLoansByCustomer(int customerId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT * FROM Loans
                    WHERE CustomerId = @CustomerId
                    ORDER BY CreatedAt DESC;";
                cmd.Parameters.AddWithValue("@CustomerId", customerId);

                using var reader = cmd.ExecuteReader();
                var loans = new List<Loan>();

                while (reader.Read())
                    loans.Add(MapLoan(reader));

                return loans;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get loans | قرضے حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Active Loan By Customer | فعال قرض حاصل کریں ───────
        /// <summary>
        /// Get active loan for a customer | قرض دار کا فعال قرض حاصل کریں
        /// </summary>
        public Loan? GetActiveLoanByCustomer(int customerId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT * FROM Loans
                    WHERE CustomerId = @CustomerId
                      AND Status = 1
                    ORDER BY CreatedAt DESC
                    LIMIT 1;";
                cmd.Parameters.AddWithValue("@CustomerId", customerId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return MapLoan(reader);

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get active loan | فعال قرض حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get All Active Loans | تمام فعال قرضے ──────────────────
        /// <summary>
        /// Get all active loans | تمام فعال قرضے حاصل کریں
        /// </summary>
        public List<Loan> GetAllActiveLoans()
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT * FROM Loans
                    WHERE Status = 1
                    ORDER BY CreatedAt DESC;";

                using var reader = cmd.ExecuteReader();
                var loans = new List<Loan>();

                while (reader.Read())
                    loans.Add(MapLoan(reader));

                return loans;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get active loans | فعال قرضے حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Merge Loans | قرضے ضم کریں ─────────────────────────────
        /// <summary>
        /// Merge new loan amount into existing active loan
        /// نئی رقم موجودہ فعال قرض میں شامل کریں
        /// </summary>
        public bool MergeLoan(int existingLoanId, decimal additionalAmount,
            int additionalInstallments, string notes = "")
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var transaction = connection.BeginTransaction();

                try
                {
                    using var cmd = connection.CreateCommand();
                    cmd.Transaction = transaction;

                    // Get existing loan | موجودہ قرض حاصل کریں
                    cmd.CommandText = "SELECT * FROM Loans WHERE Id = @Id;";
                    cmd.Parameters.AddWithValue("@Id", existingLoanId);

                    Loan? existingLoan = null;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            existingLoan = MapLoan(reader);
                    }

                    if (existingLoan == null)
                        throw new Exception("Loan not found | قرض نہیں ملا");

                    // Calculate new values | نئی اقدار حساب کریں
                    decimal newTotalAmount = existingLoan.TotalAmount + additionalAmount;
                    decimal newRemainingAmount = existingLoan.RemainingAmount + additionalAmount;
                    int newTotalInstallments = existingLoan.TotalInstallments + additionalInstallments;
                    int newRemainingInstallments = existingLoan.RemainingInstallments + additionalInstallments;
                    decimal newInstallmentAmount = newRemainingInstallments > 0
                        ? newRemainingAmount / newRemainingInstallments
                        : 0;

                    // Update existing loan | موجودہ قرض اپ ڈیٹ کریں
                    cmd.Parameters.Clear();
                    cmd.CommandText = @"
                        UPDATE Loans SET
                            TotalAmount             = @TotalAmount,
                            RemainingAmount         = @RemainingAmount,
                            TotalInstallments       = @TotalInstallments,
                            RemainingInstallments   = @RemainingInstallments,
                            InstallmentAmount       = @InstallmentAmount,
                            IsMerged                = 1,
                            Notes                   = @Notes,
                            UpdatedAt               = datetime('now')
                        WHERE Id = @Id;";

                    cmd.Parameters.AddWithValue("@TotalAmount", newTotalAmount);
                    cmd.Parameters.AddWithValue("@RemainingAmount", newRemainingAmount);
                    cmd.Parameters.AddWithValue("@TotalInstallments", newTotalInstallments);
                    cmd.Parameters.AddWithValue("@RemainingInstallments", newRemainingInstallments);
                    cmd.Parameters.AddWithValue("@InstallmentAmount", newInstallmentAmount);
                    cmd.Parameters.AddWithValue("@Notes",
                        string.IsNullOrEmpty(notes)
                            ? $"Merged: +{additionalAmount:N0} PKR | ضم شدہ: +{additionalAmount:N0}"
                            : notes);
                    cmd.Parameters.AddWithValue("@Id", existingLoanId);
                    cmd.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
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
                    $"Failed to merge loan | قرض ضم کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Close Loan | قرض بند کریں ──────────────────────────────
        /// <summary>
        /// Mark loan as closed when fully paid
        /// مکمل ادائیگی پر قرض بند کریں
        /// </summary>
        public bool CloseLoan(int loanId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Loans SET
                        Status          = 2,
                        RemainingAmount = 0,
                        UpdatedAt       = datetime('now')
                    WHERE Id = @Id;";
                cmd.Parameters.AddWithValue("@Id", loanId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to close loan | قرض بند کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Update Loan After Payment | ادائیگی کے بعد قرض اپ ڈیٹ ─
        /// <summary>
        /// Update loan balances after a payment is recorded
        /// ادائیگی کے بعد قرض کا بیلنس اپ ڈیٹ کریں
        /// </summary>
        public bool UpdateLoanAfterPayment(int loanId, decimal paidAmount)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Loans SET
                        PaidAmount              = PaidAmount + @PaidAmount,
                        RemainingAmount         = RemainingAmount - @PaidAmount,
                        PaidInstallments        = PaidInstallments + 1,
                        RemainingInstallments   = RemainingInstallments - 1,
                        Status                  = CASE 
                            WHEN (RemainingAmount - @PaidAmount) <= 0 THEN 2
                            ELSE Status 
                        END,
                        UpdatedAt               = datetime('now')
                    WHERE Id = @Id;";

                cmd.Parameters.AddWithValue("@PaidAmount", paidAmount);
                cmd.Parameters.AddWithValue("@Id", loanId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update loan after payment | ادائیگی کے بعد قرض اپ ڈیٹ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Overdue Loans | میعاد ختم قرضے ─────────────────────
        /// <summary>
        /// Get all overdue loans | تمام میعاد ختم قرضے حاصل کریں
        /// </summary>
        // ─── Get Overdue Loans | میعاد ختم قرضے ─────────────────────
        /// <summary>
        /// Get all overdue loans with customer information
        /// تمام میعاد ختم قرضے کسٹمر کی معلومات کے ساتھ حاصل کریں
        /// </summary>
        public List<Loan> GetOverdueLoans()
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
            SELECT l.*, 
                   c.Id AS CustomerId, 
                   c.FullName, 
                   c.FatherName, 
                   c.EmiratesIdOrCNIC,
                   c.PhoneNumber1, 
                   c.PhoneNumber2, 
                   c.PhoneNumber3,
                   c.Address, 
                   c.City, 
                   c.AccountNumber,
                   c.SonOf,
                   c.TotalLoanAmount,
                   c.TotalPaidAmount,
                   c.RemainingBalance,
                   c.LoanStartDate,
                   c.LoanEndDate,
                   c.IsActive,
                   c.Notes,
                   c.CreatedAt AS CustomerCreatedAt,
                   c.UpdatedAt AS CustomerUpdatedAt
            FROM Loans l
            INNER JOIN Customers c ON l.CustomerId = c.Id
            WHERE l.Status = 1
              AND l.RemainingAmount > 0
              AND l.EndDate < datetime('now')
            ORDER BY l.EndDate ASC;";

                using var reader = cmd.ExecuteReader();
                var loans = new List<Loan>();

                while (reader.Read())
                {
                    var loan = MapLoan(reader);

                    // Map customer from the same reader
                    loan.Customer = new Customer
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                        FullName = reader.GetString(reader.GetOrdinal("FullName")),
                        FatherName = reader.GetString(reader.GetOrdinal("FatherName")),
                        EmiratesIdOrCNIC = reader.GetString(reader.GetOrdinal("EmiratesIdOrCNIC")),
                        PhoneNumber1 = reader.GetString(reader.GetOrdinal("PhoneNumber1")),
                        PhoneNumber2 = reader.GetString(reader.GetOrdinal("PhoneNumber2")),
                        PhoneNumber3 = reader.GetString(reader.GetOrdinal("PhoneNumber3")),
                        Address = reader.GetString(reader.GetOrdinal("Address")),
                        City = reader.GetString(reader.GetOrdinal("City")),
                        AccountNumber = reader.GetString(reader.GetOrdinal("AccountNumber")),
                        SonOf = reader.GetString(reader.GetOrdinal("SonOf")),
                        TotalLoanAmount = reader.GetDecimal(reader.GetOrdinal("TotalLoanAmount")),
                        TotalPaidAmount = reader.GetDecimal(reader.GetOrdinal("TotalPaidAmount")),
                        RemainingBalance = reader.GetDecimal(reader.GetOrdinal("RemainingBalance")),
                        LoanStartDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("LoanStartDate"))),
                        LoanEndDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("LoanEndDate"))),
                        IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1,
                        Notes = reader.GetString(reader.GetOrdinal("Notes")),
                        CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CustomerCreatedAt"))),
                        UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CustomerUpdatedAt")))
                    };

                    loans.Add(loan);
                }

                return loans;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get overdue loans | میعاد ختم قرضے حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Loan Summary | قرض کا خلاصہ ───────────────────────
        /// <summary>
        /// Get loan summary stats for dashboard
        /// ڈیش بورڈ کے لیے قرض کا خلاصہ حاصل کریں
        /// </summary>
        public (decimal TotalLoaned, decimal TotalRemaining,
                decimal TotalCollected, int TotalLoans) GetLoanSummary()
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT
                        SUM(CASE WHEN Status != 4 THEN TotalAmount ELSE 0 END) AS TotalLoaned,
                        SUM(CASE WHEN Status != 4 THEN RemainingAmount ELSE 0 END) AS TotalRemaining,
                        SUM(CASE WHEN Status != 4 THEN PaidAmount ELSE 0 END) AS TotalCollected,
                        COUNT(CASE WHEN Status != 4 THEN 1 END) AS TotalLoans
                    FROM Loans;";

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return (
                        TotalLoaned: reader.IsDBNull(0) ? 0 : reader.GetDecimal(0),
                        TotalRemaining: reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                        TotalCollected: reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                        TotalLoans: reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                    );
                }

                return (0, 0, 0, 0);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get loan summary | قرض کا خلاصہ حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Private Helper Methods ──────────────────────────────────

        /// <summary>
        /// Get next loan number for customer | قرض دار کا اگلا قرض نمبر
        /// </summary>
        private int GetNextLoanNumber(SqliteConnection connection, int customerId)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT COUNT(*) FROM Loans 
                WHERE CustomerId = @CustomerId;";
            cmd.Parameters.AddWithValue("@CustomerId", customerId);
            return Convert.ToInt32(cmd.ExecuteScalar()) + 1;
        }

        /// <summary>
        /// Bind loan parameters to command | کمانڈ میں پیرامیٹر ڈالیں
        /// </summary>
        private void BindLoanParameters(SqliteCommand cmd, Loan loan)
        {
            cmd.Parameters.AddWithValue("@CustomerId", loan.CustomerId);
            cmd.Parameters.AddWithValue("@TotalAmount", loan.TotalAmount);
            cmd.Parameters.AddWithValue("@RemainingAmount", loan.RemainingAmount);
            cmd.Parameters.AddWithValue("@PaidAmount", loan.PaidAmount);
            cmd.Parameters.AddWithValue("@TotalInstallments", loan.TotalInstallments);
            cmd.Parameters.AddWithValue("@RemainingInstallments", loan.RemainingInstallments);
            cmd.Parameters.AddWithValue("@PaidInstallments", loan.PaidInstallments);
            cmd.Parameters.AddWithValue("@InstallmentAmount", loan.InstallmentAmount);
            cmd.Parameters.AddWithValue("@StartDate",
                loan.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@EndDate",
                loan.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@Status", (int)loan.Status);
            cmd.Parameters.AddWithValue("@LoanNumber", loan.LoanNumber);
            cmd.Parameters.AddWithValue("@IsMerged", loan.IsMerged ? 1 : 0);
            cmd.Parameters.AddWithValue("@Notes", loan.Notes);
            cmd.Parameters.AddWithValue("@CreatedAt",
                loan.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@UpdatedAt",
                loan.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        /// <summary>
        /// Map database reader to Loan object | ریڈر سے قرض آبجیکٹ بنائیں
        /// </summary>
        private Loan MapLoan(SqliteDataReader reader)
        {
            return new Loan
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                RemainingAmount = reader.GetDecimal(reader.GetOrdinal("RemainingAmount")),
                PaidAmount = reader.GetDecimal(reader.GetOrdinal("PaidAmount")),
                TotalInstallments = reader.GetInt32(reader.GetOrdinal("TotalInstallments")),
                RemainingInstallments = reader.GetInt32(reader.GetOrdinal("RemainingInstallments")),
                PaidInstallments = reader.GetInt32(reader.GetOrdinal("PaidInstallments")),
                InstallmentAmount = reader.GetDecimal(reader.GetOrdinal("InstallmentAmount")),
                StartDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("StartDate"))),
                EndDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("EndDate"))),
                Status = (LoanStatus)reader.GetInt32(reader.GetOrdinal("Status")),
                LoanNumber = reader.GetInt32(reader.GetOrdinal("LoanNumber")),
                IsMerged = reader.GetInt32(reader.GetOrdinal("IsMerged")) == 1,
                Notes = reader.GetString(reader.GetOrdinal("Notes")),
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
                UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt")))
            };
        }
    }
}