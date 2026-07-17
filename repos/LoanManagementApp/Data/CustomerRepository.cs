using System;
using System.Collections.Generic;
using LoanManagementApp.Models;
using Microsoft.Data.Sqlite;

namespace LoanManagementApp.Data
{
    /// <summary>
    /// CustomerRepository / قرض دار ریپوزٹری - All CRUD operations for Customers
    /// </summary>
    public class CustomerRepository
    {
        // ─── Add Customer | قرض دار شامل کریں ──────────────────────
        /// <summary>
        /// Add new customer to database | نیا قرض دار ڈیٹابیس میں شامل کریں
        /// </summary>
        public int AddCustomer(Customer customer)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                // Generate account number | اکاؤنٹ نمبر بنائیں
                if (string.IsNullOrEmpty(customer.AccountNumber))
                    customer.AccountNumber = GenerateAccountNumber(connection);

                customer.CreatedAt = DateTime.Now;
                customer.UpdatedAt = DateTime.Now;

                cmd.CommandText = @"
                    INSERT INTO Customers 
                    (
                        FullName, FatherName, EmiratesIdOrCNIC,
                        PhoneNumber1, PhoneNumber2, PhoneNumber3,
                        Address, City, AccountNumber, SonOf,
                        TotalLoanAmount, TotalPaidAmount, RemainingBalance,
                        LoanStartDate, LoanEndDate,
                        IsActive, Notes, CreatedAt, UpdatedAt
                    )
                    VALUES 
                    (
                        @FullName, @FatherName, @EmiratesIdOrCNIC,
                        @PhoneNumber1, @PhoneNumber2, @PhoneNumber3,
                        @Address, @City, @AccountNumber, @SonOf,
                        @TotalLoanAmount, @TotalPaidAmount, @RemainingBalance,
                        @LoanStartDate, @LoanEndDate,
                        @IsActive, @Notes, @CreatedAt, @UpdatedAt
                    );
                    SELECT last_insert_rowid();";

                BindCustomerParameters(cmd, customer);

                var newId = Convert.ToInt32(cmd.ExecuteScalar());
                return newId;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to add customer | قرض دار شامل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Update Customer | قرض دار اپ ڈیٹ کریں ────────────────
        /// <summary>
        /// Update existing customer | موجودہ قرض دار کو اپ ڈیٹ کریں
        /// </summary>
        public bool UpdateCustomer(Customer customer)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                customer.UpdatedAt = DateTime.Now;

                cmd.CommandText = @"
                    UPDATE Customers SET
                        FullName            = @FullName,
                        FatherName          = @FatherName,
                        EmiratesIdOrCNIC    = @EmiratesIdOrCNIC,
                        PhoneNumber1        = @PhoneNumber1,
                        PhoneNumber2        = @PhoneNumber2,
                        PhoneNumber3        = @PhoneNumber3,
                        Address             = @Address,
                        City                = @City,
                        AccountNumber       = @AccountNumber,
                        SonOf               = @SonOf,
                        TotalLoanAmount     = @TotalLoanAmount,
                        TotalPaidAmount     = @TotalPaidAmount,
                        RemainingBalance    = @RemainingBalance,
                        LoanStartDate       = @LoanStartDate,
                        LoanEndDate         = @LoanEndDate,
                        IsActive            = @IsActive,
                        Notes               = @Notes,
                        UpdatedAt           = @UpdatedAt
                    WHERE Id = @Id;";

                BindCustomerParameters(cmd, customer);
                cmd.Parameters.AddWithValue("@Id", customer.Id);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update customer | قرض دار اپ ڈیٹ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Delete Customer | قرض دار حذف کریں ────────────────────
        /// <summary>
        /// Delete customer by ID | شناخت کے ذریعے قرض دار حذف کریں
        /// </summary>
        public bool DeleteCustomer(int customerId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                // CASCADE will delete loans and payments too
                // CASCADE سے قرضے اور ادائیگیاں بھی حذف ہوں گی
                cmd.CommandText = @"
                    DELETE FROM Customers 
                    WHERE Id = @Id;";
                cmd.Parameters.AddWithValue("@Id", customerId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to delete customer | قرض دار حذف کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Soft Delete | نرم حذف ──────────────────────────────────
        /// <summary>
        /// Deactivate customer instead of deleting
        /// حذف کرنے کی بجائے غیر فعال کریں
        /// </summary>
        public bool DeactivateCustomer(int customerId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Customers SET
                        IsActive  = 0,
                        UpdatedAt = datetime('now')
                    WHERE Id = @Id;";
                cmd.Parameters.AddWithValue("@Id", customerId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to deactivate customer | قرض دار غیر فعال کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Customer By ID | شناخت سے قرض دار حاصل کریں ───────
        /// <summary>
        /// Get single customer by ID | ایک قرض دار شناخت سے حاصل کریں
        /// </summary>
        public Customer? GetCustomerById(int customerId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT * FROM Customers 
                    WHERE Id = @Id;";
                cmd.Parameters.AddWithValue("@Id", customerId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return MapCustomer(reader);

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get customer | قرض دار حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get All Customers | تمام قرض دار حاصل کریں ────────────
        /// <summary>
        /// Get all active customers | تمام فعال قرض دار حاصل کریں
        /// </summary>
        public List<Customer> GetAllCustomers(bool includeInactive = false)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = includeInactive
                    ? "SELECT * FROM Customers ORDER BY FullName ASC;"
                    : "SELECT * FROM Customers WHERE IsActive = 1 ORDER BY FullName ASC;";

                using var reader = cmd.ExecuteReader();
                var customers = new List<Customer>();

                while (reader.Read())
                    customers.Add(MapCustomer(reader));

                return customers;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get customers | قرض دار حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Search Customers | قرض دار تلاش کریں ──────────────────
        /// <summary>
        /// Search customers by name, CNIC, phone or account number
        /// نام، شناختی کارڈ، فون یا اکاؤنٹ نمبر سے تلاش کریں
        /// </summary>
        public List<Customer> SearchCustomers(string searchTerm)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                string term = $"%{searchTerm.Trim()}%";

                cmd.CommandText = @"
                    SELECT * FROM Customers
                    WHERE IsActive = 1 AND (
                        FullName            LIKE @term OR
                        FatherName          LIKE @term OR
                        EmiratesIdOrCNIC    LIKE @term OR
                        PhoneNumber1        LIKE @term OR
                        PhoneNumber2        LIKE @term OR
                        PhoneNumber3        LIKE @term OR
                        AccountNumber       LIKE @term OR
                        City                LIKE @term
                    )
                    ORDER BY FullName ASC;";
                cmd.Parameters.AddWithValue("@term", term);

                using var reader = cmd.ExecuteReader();
                var customers = new List<Customer>();

                while (reader.Read())
                    customers.Add(MapCustomer(reader));

                return customers;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to search customers | قرض دار تلاش کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Overdue Customers | میعاد ختم قرض دار ──────────────
        /// <summary>
        /// Get customers with overdue loans
        /// میعاد ختم قرضوں والے قرض دار حاصل کریں
        /// </summary>
        public List<Customer> GetOverdueCustomers()
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT * FROM Customers
                    WHERE IsActive = 1
                      AND RemainingBalance > 0
                      AND LoanEndDate < datetime('now')
                    ORDER BY LoanEndDate ASC;";

                using var reader = cmd.ExecuteReader();
                var customers = new List<Customer>();

                while (reader.Read())
                    customers.Add(MapCustomer(reader));

                return customers;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get overdue customers | میعاد ختم قرض دار حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Update Customer Balance | قرض دار کا بیلنس اپ ڈیٹ کریں
        /// <summary>
        /// Update customer loan summary after payment
        /// ادائیگی کے بعد قرض دار کا خلاصہ اپ ڈیٹ کریں
        /// </summary>
        public bool UpdateCustomerBalance(
            int customerId,
            decimal totalLoanAmount,
            decimal totalPaidAmount,
            decimal remainingBalance)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Customers SET
                        TotalLoanAmount  = @TotalLoanAmount,
                        TotalPaidAmount  = @TotalPaidAmount,
                        RemainingBalance = @RemainingBalance,
                        UpdatedAt        = datetime('now')
                    WHERE Id = @Id;";

                cmd.Parameters.AddWithValue("@TotalLoanAmount", totalLoanAmount);
                cmd.Parameters.AddWithValue("@TotalPaidAmount", totalPaidAmount);
                cmd.Parameters.AddWithValue("@RemainingBalance", remainingBalance);
                cmd.Parameters.AddWithValue("@Id", customerId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update balance | بیلنس اپ ڈیٹ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Update Customer Loan Dates | قرض دار کی قرض کی تاریخیں اپ ڈیٹ کریں ──
        /// <summary>
        /// Update only the loan start and end dates for a customer
        /// صرف قرض کی شروع اور ختم ہونے کی تاریخیں اپ ڈیٹ کریں
        /// </summary>
        public bool UpdateCustomerLoanDates(int customerId, DateTime startDate, DateTime endDate)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    UPDATE Customers SET
                        LoanStartDate = @StartDate,
                        LoanEndDate = @EndDate,
                        UpdatedAt = datetime('now')
                    WHERE Id = @Id;";
                cmd.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@Id", customerId);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update loan dates | قرض کی تاریخیں اپ ڈیٹ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Dashboard Stats | ڈیش بورڈ اعداد و شمار ───────────
        /// <summary>
        /// Get summary statistics for dashboard
        /// ڈیش بورڈ کے لیے خلاصہ اعداد و شمار حاصل کریں
        /// </summary>
        public (int TotalCustomers, int ActiveLoans, decimal TotalOutstanding,
                int OverdueCount) GetDashboardStats()
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT
                        COUNT(*)                            AS TotalCustomers,
                        SUM(CASE WHEN RemainingBalance > 0
                            THEN 1 ELSE 0 END)              AS ActiveLoans,
                        SUM(RemainingBalance)               AS TotalOutstanding,
                        SUM(CASE WHEN RemainingBalance > 0
                            AND LoanEndDate < datetime('now')
                            THEN 1 ELSE 0 END)              AS OverdueCount
                    FROM Customers
                    WHERE IsActive = 1;";

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return (
                        TotalCustomers: reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        ActiveLoans: reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                        TotalOutstanding: reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                        OverdueCount: reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                    );
                }

                return (0, 0, 0, 0);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get stats | اعداد و شمار حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Check CNIC Exists | شناختی کارڈ موجود ہے یا نہیں ──────
        /// <summary>
        /// Check if CNIC already exists | کیا شناختی کارڈ پہلے سے موجود ہے
        /// </summary>
        public bool CnicExists(string cnic, int excludeCustomerId = 0)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT COUNT(*) FROM Customers
                    WHERE EmiratesIdOrCNIC = @Cnic
                      AND Id != @ExcludeId;";
                cmd.Parameters.AddWithValue("@Cnic", cnic);
                cmd.Parameters.AddWithValue("@ExcludeId", excludeCustomerId);

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to check CNIC | شناختی کارڈ جانچنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Private Helper Methods ──────────────────────────────────

        /// <summary>
        /// Generate unique account number | منفرد اکاؤنٹ نمبر بنائیں
        /// </summary>
        private string GenerateAccountNumber(SqliteConnection connection)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Customers;";
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return (1000 + count + 1).ToString();
        }

        /// <summary>
        /// Bind customer parameters to command | کمانڈ میں پیرامیٹر ڈالیں
        /// </summary>
        private void BindCustomerParameters(SqliteCommand cmd, Customer customer)
        {
            cmd.Parameters.AddWithValue("@FullName", customer.FullName);
            cmd.Parameters.AddWithValue("@FatherName", customer.FatherName);
            cmd.Parameters.AddWithValue("@EmiratesIdOrCNIC", customer.EmiratesIdOrCNIC);
            cmd.Parameters.AddWithValue("@PhoneNumber1", customer.PhoneNumber1);
            cmd.Parameters.AddWithValue("@PhoneNumber2", customer.PhoneNumber2);
            cmd.Parameters.AddWithValue("@PhoneNumber3", customer.PhoneNumber3);
            cmd.Parameters.AddWithValue("@Address", customer.Address);
            cmd.Parameters.AddWithValue("@City", customer.City);
            cmd.Parameters.AddWithValue("@AccountNumber", customer.AccountNumber);
            cmd.Parameters.AddWithValue("@SonOf", customer.SonOf);
            cmd.Parameters.AddWithValue("@TotalLoanAmount", customer.TotalLoanAmount);
            cmd.Parameters.AddWithValue("@TotalPaidAmount", customer.TotalPaidAmount);
            cmd.Parameters.AddWithValue("@RemainingBalance", customer.RemainingBalance);
            cmd.Parameters.AddWithValue("@LoanStartDate", customer.LoanStartDate.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@LoanEndDate", customer.LoanEndDate.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@IsActive", customer.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@Notes", customer.Notes);
            cmd.Parameters.AddWithValue("@CreatedAt", customer.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@UpdatedAt", customer.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        /// <summary>
        /// Map database reader to Customer object | ریڈر سے قرض دار آبجیکٹ بنائیں
        /// </summary>
        private Customer MapCustomer(SqliteDataReader reader)
        {
            return new Customer
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
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
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
                UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt")))
            };
        }
    }
}