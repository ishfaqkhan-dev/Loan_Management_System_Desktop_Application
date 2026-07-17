// ╔══════════════════════════════════════════════════════════════════╗
// ║         PrintStatementWindow.xaml.cs                            ║
// ║         Loan Management System — Professional Statement         ║
// ╚══════════════════════════════════════════════════════════════════╝

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using LoanManagementApp.Models;

namespace LoanManagementApp.Views.Dialogs
{
    public partial class PrintStatementWindow : Window
    {
        // ──────────────────────────────────────────────
        // Fields
        // ──────────────────────────────────────────────
        private readonly FlowDocument _document;

        // ──────────────────────────────────────────────
        // Constructor
        // ──────────────────────────────────────────────
        public PrintStatementWindow(Customer customer, Loan loan, List<Payment> payments)
        {
            InitializeComponent();
            _document = GenerateStatement(customer, loan, payments);
            DocumentViewer.Document = _document;
        }

        // ══════════════════════════════════════════════
        // MAIN: Generate complete FlowDocument statement
        // ══════════════════════════════════════════════
        private FlowDocument GenerateStatement(Customer customer, Loan loan, List<Payment> payments)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(50, 40, 50, 40),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 11,
                Background = Brushes.White,
                Foreground = Brushes.Black,
                ColumnWidth = double.MaxValue    // Single column — no auto wrap
            };

            // ── 1. Company Logo / Header Banner ──────────────
            AddHeaderBanner(doc);

            // ── 2. Divider ────────────────────────────────────
            AddHorizontalLine(doc, topMargin: 0, bottomMargin: 20);

            // ── 3. Customer Details ───────────────────────────
            AddSectionHeader(doc, "CUSTOMER DETAILS", "قرض دار کی معلومات");
            AddInfoTable(doc, new[]
            {
                ("Full Name | پورا نام",               customer.FullName       ?? "—"),
                ("Father Name | والد کا نام",           customer.FatherName     ?? "—"),
                ("CNIC / Emirates ID | شناختی کارڈ",   customer.EmiratesIdOrCNIC ?? "—"),
                ("Phone | فون",                        customer.PhoneNumber1   ?? "—"),
                ("Address | پتہ",                      customer.Address        ?? "—"),
                ("City | شہر",                         customer.City           ?? "—"),
            });

            // ── 4. Loan Summary ───────────────────────────────
            AddSectionHeader(doc, "LOAN SUMMARY", "قرض کا خلاصہ");
            AddLoanSummaryBox(doc, loan);

            // ── 5. Payment History Table ──────────────────────
            AddSectionHeader(doc, "PAYMENT HISTORY", "ادائیگی تاریخ");
            AddPaymentTable(doc, payments);

            // ── 6. Generated timestamp ────────────────────────
            // NOTE: Totals footer removed — summary already shown in Loan Summary tiles
            AddFooterTimestamp(doc);

            return doc;
        }

        // ══════════════════════════════════════════════
        // SECTION: Header Banner
        // ══════════════════════════════════════════════
        private void AddHeaderBanner(FlowDocument doc)
        {
            // Company name — large bold
            var companyName = new Paragraph(new Run("LOAN MANAGEMENT SYSTEM"))
            {
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4),
                Foreground = new SolidColorBrush(Color.FromRgb(15, 52, 96))  // Deep navy
            };
            doc.Blocks.Add(companyName);

            // Sub-title — bilingual
            var subTitle = new Paragraph(new Run("Loan Statement  |  قرض اسٹیٹمنٹ"))
            {
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(90, 90, 110)),
                Margin = new Thickness(0, 0, 0, 6)
            };
            doc.Blocks.Add(subTitle);
        }

        // ══════════════════════════════════════════════
        // SECTION: Section Header (with colored left bar)
        // ══════════════════════════════════════════════
        private void AddSectionHeader(FlowDocument doc, string english, string urdu)
        {
            // Outer border — left accent bar + light background
            var headerBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(235, 242, 252)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(15, 52, 96)),
                BorderThickness = new Thickness(4, 0, 0, 0),
                Padding = new Thickness(12, 7, 12, 7),
                Margin = new Thickness(0, 18, 0, 10)
            };

            var sp = new StackPanel { Orientation = Orientation.Horizontal };

            // English title
            sp.Children.Add(new TextBlock
            {
                Text = english,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(15, 52, 96)),
            });

            // Separator
            sp.Children.Add(new TextBlock
            {
                Text = "  |  ",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 160)),
            });

            // Urdu title
            sp.Children.Add(new TextBlock
            {
                Text = urdu,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 80)),
            });

            headerBorder.Child = sp;
            doc.Blocks.Add(new BlockUIContainer(headerBorder));
        }

        // ══════════════════════════════════════════════
        // SECTION: Two-column Info Table (Label | Value)
        // ══════════════════════════════════════════════
        private void AddInfoTable(FlowDocument doc, (string label, string value)[] rows)
        {
            var grid = new Grid { Margin = new Thickness(4, 0, 0, 0) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(260) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            for (int i = 0; i < rows.Length; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Alternating row background
                if (i % 2 == 0)
                {
                    var rowBg = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(248, 249, 252))
                    };
                    Grid.SetRow(rowBg, i);
                    Grid.SetColumnSpan(rowBg, 2);
                    grid.Children.Add(rowBg);
                }

                // Label cell
                var labelBlock = new TextBlock
                {
                    Text = rows[i].label + ":",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(60, 80, 120)),
                    Margin = new Thickness(8, 5, 4, 5),
                    TextWrapping = TextWrapping.Wrap
                };

                // Value cell
                var valueBlock = new TextBlock
                {
                    Text = rows[i].value,
                    Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 40)),
                    Margin = new Thickness(8, 5, 4, 5),
                    FontWeight = FontWeights.Medium,
                    TextWrapping = TextWrapping.Wrap
                };

                Grid.SetRow(labelBlock, i); Grid.SetColumn(labelBlock, 0);
                Grid.SetRow(valueBlock, i); Grid.SetColumn(valueBlock, 1);
                grid.Children.Add(labelBlock);
                grid.Children.Add(valueBlock);
            }

            doc.Blocks.Add(new BlockUIContainer(grid));
        }

        // ══════════════════════════════════════════════
        // SECTION: Loan Summary — Styled 2-column box
        // ══════════════════════════════════════════════
        private void AddLoanSummaryBox(FlowDocument doc, Loan loan)
        {
            var outerBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(200, 215, 235)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(0, 0, 0, 0)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // ── Row 0: 3 summary tiles ──
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Tile 1 — Total Loan
            AddSummaryTile(grid, 0, 0,
                "Total Loan Amount",
                "کل قرض",
                $"{loan.TotalAmount:N0} PKR",
                Color.FromRgb(15, 52, 96),
                Color.FromRgb(235, 242, 255));

            // Tile 2 — Total Paid
            AddSummaryTile(grid, 0, 1,
                "Total Paid",
                "کل ادا شدہ",
                $"{loan.PaidAmount:N0} PKR",
                Color.FromRgb(22, 110, 60),
                Color.FromRgb(232, 248, 238));

            // Tile 3 — Remaining
            AddSummaryTile(grid, 0, 2,
                "Remaining Balance",
                "باقی رقم",
                $"{loan.RemainingAmount:N0} PKR",
                Color.FromRgb(160, 70, 10),
                Color.FromRgb(255, 244, 230));

            // ── Row 1: Installments ──
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddInstallmentRow(grid, 1, loan.TotalInstallments, loan.PaidInstallments);

            outerBorder.Child = grid;
            doc.Blocks.Add(new BlockUIContainer(outerBorder));
        }

        // Helper: Single summary tile
        private void AddSummaryTile(Grid grid, int row, int col,
            string englishLabel, string urduLabel, string value,
            Color accentColor, Color bgColor)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(bgColor),
                BorderBrush = new SolidColorBrush(accentColor) { Opacity = 0.3 },
                BorderThickness = new Thickness(0, 0, col < 2 ? 1 : 0, 1),
                Padding = new Thickness(16, 12, 16, 12)
            };

            var sp = new StackPanel();

            sp.Children.Add(new TextBlock
            {
                Text = englishLabel,
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(accentColor),
                Opacity = 0.8
            });
            sp.Children.Add(new TextBlock
            {
                Text = urduLabel,
                FontSize = 9,
                Foreground = new SolidColorBrush(accentColor),
                Opacity = 0.6,
                Margin = new Thickness(0, 1, 0, 4)
            });
            sp.Children.Add(new TextBlock
            {
                Text = value,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(accentColor)
            });

            border.Child = sp;
            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            grid.Children.Add(border);
        }

        // Helper: Installment row
        private void AddInstallmentRow(Grid grid, int row, int total, int paid)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(245, 246, 250)),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(16, 8, 16, 8)
            };

            var sp = new StackPanel { Orientation = Orientation.Horizontal };

            sp.Children.Add(new TextBlock
            {
                Text = $"Total Installments | کل اقساط:  ",
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(60, 80, 120)),
                FontSize = 11
            });
            sp.Children.Add(new TextBlock
            {
                Text = total.ToString(),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(15, 52, 96)),
                FontSize = 11,
                Margin = new Thickness(0, 0, 30, 0)
            });
            sp.Children.Add(new TextBlock
            {
                Text = $"Paid Installments | ادا شدہ اقساط:  ",
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(60, 80, 120)),
                FontSize = 11
            });
            sp.Children.Add(new TextBlock
            {
                Text = paid.ToString(),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(22, 110, 60)),
                FontSize = 11
            });

            border.Child = sp;
            Grid.SetRow(border, row);
            Grid.SetColumnSpan(border, 3);
            grid.Children.Add(border);
        }

        // ══════════════════════════════════════════════
        // SECTION: Payment History Table
        // Black & White — all fixed pixel widths
        // No Star columns (Star causes Urdu text to
        // wrap vertically in FlowDocument Table cells)
        // ══════════════════════════════════════════════
        private void AddPaymentTable(FlowDocument doc, List<Payment> payments)
        {
            var table = new Table
            {
                CellSpacing = 0,
                BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                BorderThickness = new Thickness(1)
            };

            // ── Fixed pixel widths only — NO Star/Auto ──
            // Total = 760px — fills full page (900px window - 50px padding x2 - 40px doc margin)
            table.Columns.Add(new TableColumn { Width = new GridLength(50) });  // # — serial number
            table.Columns.Add(new TableColumn { Width = new GridLength(130) });  // Date
            table.Columns.Add(new TableColumn { Width = new GridLength(180) });  // Paid Amount
            table.Columns.Add(new TableColumn { Width = new GridLength(230) });  // Balance After
            table.Columns.Add(new TableColumn { Width = new GridLength(145) });  // Type

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            // ── Header Row — dark grey background, white text ──
            var headerRow = new TableRow
            {
                Background = new SolidColorBrush(Color.FromRgb(50, 50, 50))
            };
            // English-only headers to prevent Urdu bidirectional wrap issue
            AddTableHeaderCell(headerRow, "#", TextAlignment.Center);
            AddTableHeaderCell(headerRow, "Date", TextAlignment.Left);
            AddTableHeaderCell(headerRow, "Paid Amount", TextAlignment.Right);
            AddTableHeaderCell(headerRow, "Balance After", TextAlignment.Right);
            AddTableHeaderCell(headerRow, "Type", TextAlignment.Center);
            rowGroup.Rows.Add(headerRow);

            // ── Sub-header Row — Urdu labels, light grey ──
            var urduRow = new TableRow
            {
                Background = new SolidColorBrush(Color.FromRgb(220, 220, 220))
            };
            AddTableSubHeaderCell(urduRow, "#", TextAlignment.Center);
            AddTableSubHeaderCell(urduRow, "تاریخ", TextAlignment.Left);
            AddTableSubHeaderCell(urduRow, "ادا شدہ", TextAlignment.Right);
            AddTableSubHeaderCell(urduRow, "بعد میں باقی", TextAlignment.Right);
            AddTableSubHeaderCell(urduRow, "قسم", TextAlignment.Center);
            rowGroup.Rows.Add(urduRow);

            // ── Data Rows ──
            if (payments == null || payments.Count == 0)
            {
                // Empty state row
                var emptyRow = new TableRow { Background = Brushes.White };
                var emptyCell = new TableCell(new Paragraph(new Run("No payments recorded.")))
                {
                    ColumnSpan = 5,
                    Padding = new Thickness(10, 10, 10, 10),
                    TextAlignment = TextAlignment.Center
                };
                emptyRow.Cells.Add(emptyCell);
                rowGroup.Rows.Add(emptyRow);
            }
            else
            {
                for (int i = 0; i < payments.Count; i++)
                {
                    var p = payments[i];
                    bool alt = i % 2 != 0;  // Alternating row shade

                    var row = new TableRow
                    {
                        Background = alt
                            ? new SolidColorBrush(Color.FromRgb(245, 245, 245))
                            : Brushes.White
                    };

                    // # — serial
                    AddTableDataCell(row, (i + 1).ToString(),
                        TextAlignment.Center, FontWeights.SemiBold,
                        Color.FromRgb(80, 80, 80));

                    // Date
                    AddTableDataCell(row,
                        p.PaymentDate.ToString("dd-MMM-yyyy"),
                        TextAlignment.Left, FontWeights.Normal,
                        Color.FromRgb(40, 40, 40));

                    // Paid Amount — darker green for B&W feel
                    AddTableDataCell(row,
                        $"{p.PaidAmount:N0} PKR",
                        TextAlignment.Right, FontWeights.SemiBold,
                        Color.FromRgb(30, 100, 50));

                    // Balance After — dark text
                    AddTableDataCell(row,
                        $"{p.RemainingBalanceAfterPayment:N0} PKR",
                        TextAlignment.Right, FontWeights.SemiBold,
                        Color.FromRgb(30, 30, 30));

                    // Type — Urdu in data cell (safe, no header wrapping)
                    AddTableDataCell(row,
                        p.PaymentTypeUrdu ?? "—",
                        TextAlignment.Center, FontWeights.Normal,
                        Color.FromRgb(60, 60, 60));

                    rowGroup.Rows.Add(row);
                }
            }

            doc.Blocks.Add(table);
        }

        // Helper: Table header cell — white bold text, right border divider
        private void AddTableHeaderCell(TableRow row, string text, TextAlignment align)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(text))
            {
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                Foreground = Brushes.White,
                TextAlignment = align
            })
            {
                Padding = new Thickness(10, 8, 10, 8),
                BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                BorderThickness = new Thickness(0, 0, 1, 0)
            });
        }

        // Helper: Urdu sub-header cell — dark text, lighter background
        private void AddTableSubHeaderCell(TableRow row, string text, TextAlignment align)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(text))
            {
                FontWeight = FontWeights.SemiBold,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                TextAlignment = align
            })
            {
                Padding = new Thickness(10, 4, 10, 4),
                BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                BorderThickness = new Thickness(0, 0, 1, 1)
            });
        }

        // Helper: Table data cell
        private void AddTableDataCell(TableRow row, string text,
            TextAlignment align, FontWeight weight, Color foreColor)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(text))
            {
                TextAlignment = align,
                FontWeight = weight,
                Foreground = new SolidColorBrush(foreColor)
            })
            {
                Padding = new Thickness(10, 6, 10, 6),
                BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                BorderThickness = new Thickness(0, 0, 1, 1)
            });
        }

        // ══════════════════════════════════════════════
        // SECTION: Totals Footer Box
        // ══════════════════════════════════════════════
        private void AddTotalsFooter(FlowDocument doc, Loan loan)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(15, 52, 96)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(20, 14, 20, 14),
                Margin = new Thickness(0, 20, 0, 0)
            };

            var sp = new StackPanel { Orientation = Orientation.Horizontal };

            sp.Children.Add(MakeTotalItem("Total Loaned", $"{loan.TotalAmount:N0} PKR", Colors.White, true));
            sp.Children.Add(MakeSeparator());
            sp.Children.Add(MakeTotalItem("Total Paid", $"{loan.PaidAmount:N0} PKR", Color.FromRgb(130, 230, 170), false));
            sp.Children.Add(MakeSeparator());
            sp.Children.Add(MakeTotalItem("Remaining", $"{loan.RemainingAmount:N0} PKR", Color.FromRgb(255, 195, 110), false));

            border.Child = sp;
            doc.Blocks.Add(new BlockUIContainer(border));
        }

        // Helper: one total item
        private StackPanel MakeTotalItem(string label, string value, Color valueColor, bool bold)
        {
            var sp = new StackPanel { Margin = new Thickness(0, 0, 0, 0) };
            sp.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 10,
                Foreground = new SolidColorBrush(Colors.White) { Opacity = 0.7 },
                FontWeight = FontWeights.Normal
            });
            sp.Children.Add(new TextBlock
            {
                Text = value,
                FontSize = 15,
                FontWeight = bold ? FontWeights.Bold : FontWeights.SemiBold,
                Foreground = new SolidColorBrush(valueColor)
            });
            return sp;
        }

        // Helper: vertical separator
        private Border MakeSeparator()
        {
            return new Border
            {
                Width = 1,
                Background = new SolidColorBrush(Colors.White) { Opacity = 0.25 },
                Margin = new Thickness(24, 2, 24, 2)
            };
        }

        // ══════════════════════════════════════════════
        // SECTION: Footer Timestamp
        // ══════════════════════════════════════════════
        private void AddFooterTimestamp(FlowDocument doc)
        {
            doc.Blocks.Add(new Paragraph(
                new Run($"Generated on {DateTime.Now:dddd, dd MMMM yyyy  hh:mm tt}"))
            {
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 155, 170)),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 16, 0, 0)
            });
        }

        // ══════════════════════════════════════════════
        // HELPER: Horizontal divider line
        // ══════════════════════════════════════════════
        private void AddHorizontalLine(FlowDocument doc, double topMargin = 10, double bottomMargin = 10)
        {
            doc.Blocks.Add(new BlockUIContainer(new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(200, 210, 228)),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Margin = new Thickness(0, topMargin, 0, bottomMargin)
            }));
        }

        // ══════════════════════════════════════════════
        // EVENT: Print Button Click
        // ══════════════════════════════════════════════
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // Adjust page size to match printer
                _document.PageHeight = printDialog.PrintableAreaHeight;
                _document.PageWidth = printDialog.PrintableAreaWidth;

                printDialog.PrintDocument(
                    ((IDocumentPaginatorSource)_document).DocumentPaginator,
                    "Loan Statement");
            }
        }

        // ══════════════════════════════════════════════
        // EVENT: Close Button Click
        // ══════════════════════════════════════════════
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}