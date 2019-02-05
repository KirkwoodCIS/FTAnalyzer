﻿using FTAnalyzer.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static FTAnalyzer.ColourValues;

namespace FTAnalyzer.Forms
{
    public partial class ColourBMD : Form
    {
        ReportFormHelper reportFormHelper;

        Dictionary<BMDColour, DataGridViewCellStyle> styles;
        int birthColumnIndex;
        int burialColumnIndex;
        SortableBindingList<IDisplayColourBMD> reportList;
        Font boldFont;

        public ColourBMD(List<IDisplayColourBMD> reportList)
        {
            InitializeComponent();
            Top = Top + WindowHelper.TopTaskbarOffset;
            dgBMDReportSheet.AutoGenerateColumns = false;

            this.reportList = new SortableBindingList<IDisplayColourBMD>(reportList);
            reportFormHelper = new ReportFormHelper(this, "Colour BMD Report", dgBMDReportSheet, ResetTable, "Colour BMD");
            ExtensionMethods.DoubleBuffered(dgBMDReportSheet, true);
            
            boldFont = new Font(dgBMDReportSheet.DefaultCellStyle.Font, FontStyle.Bold);
            styles = new Dictionary<BMDColour, DataGridViewCellStyle>();
            DataGridViewCellStyle notRequired = new DataGridViewCellStyle();
            notRequired.BackColor = notRequired.ForeColor = Color.DarkGray;
            styles.Add(BMDColour.EMPTY, notRequired);
            DataGridViewCellStyle missingData = new DataGridViewCellStyle();
            missingData.BackColor = missingData.ForeColor = Color.Red;
            styles.Add(BMDColour.UNKNOWN_DATE, missingData);
            DataGridViewCellStyle openEndedDateRange = new DataGridViewCellStyle();
            openEndedDateRange.BackColor = openEndedDateRange.ForeColor = Color.OrangeRed;
            styles.Add(BMDColour.OPEN_ENDED_DATE, openEndedDateRange);
            DataGridViewCellStyle verywideDateRange = new DataGridViewCellStyle();
            verywideDateRange.BackColor = verywideDateRange.ForeColor = Color.Tomato;
            styles.Add(BMDColour.VERY_WIDE_DATE, verywideDateRange);
            DataGridViewCellStyle wideDateRange = new DataGridViewCellStyle();
            wideDateRange.BackColor = wideDateRange.ForeColor = Color.Orange;
            styles.Add(BMDColour.WIDE_DATE, wideDateRange);
            DataGridViewCellStyle narrowDateRange = new DataGridViewCellStyle();
            narrowDateRange.BackColor = narrowDateRange.ForeColor = Color.Yellow;
            styles.Add(BMDColour.NARROW_DATE, narrowDateRange);
            DataGridViewCellStyle justYearDateRange = new DataGridViewCellStyle();
            justYearDateRange.BackColor = justYearDateRange.ForeColor = Color.YellowGreen;
            styles.Add(BMDColour.JUST_YEAR_DATE, justYearDateRange);
            DataGridViewCellStyle approxDate = new DataGridViewCellStyle();
            approxDate.BackColor = approxDate.ForeColor = Color.PaleGreen;
            styles.Add(BMDColour.APPROX_DATE, approxDate);
            DataGridViewCellStyle exactDate = new DataGridViewCellStyle();
            exactDate.BackColor = exactDate.ForeColor = Color.LawnGreen;
            styles.Add(BMDColour.EXACT_DATE, exactDate);
            DataGridViewCellStyle noSpouse = new DataGridViewCellStyle();
            noSpouse.BackColor = noSpouse.ForeColor = Color.LightPink;
            styles.Add(BMDColour.NO_SPOUSE, noSpouse);
            DataGridViewCellStyle hasChildren = new DataGridViewCellStyle();
            hasChildren.BackColor = hasChildren.ForeColor = Color.LightCoral;
            styles.Add(BMDColour.NO_PARTNER, hasChildren);
            DataGridViewCellStyle noMarriage = new DataGridViewCellStyle();
            noMarriage.BackColor = noMarriage.ForeColor = Color.Firebrick;
            styles.Add(BMDColour.NO_MARRIAGE, noMarriage);
            DataGridViewCellStyle isLiving = new DataGridViewCellStyle();
            isLiving.BackColor = isLiving.ForeColor = Color.WhiteSmoke;
            styles.Add(BMDColour.ISLIVING, isLiving);
            DataGridViewCellStyle over90 = new DataGridViewCellStyle();
            over90.BackColor = over90.ForeColor = Color.DarkGray;
            styles.Add(BMDColour.OVER90, over90);

            birthColumnIndex = dgBMDReportSheet.Columns["Birth"].Index;
            burialColumnIndex = dgBMDReportSheet.Columns["CremBuri"].Index;
            dgBMDReportSheet.DataSource = this.reportList;
            reportFormHelper.LoadColumnLayout("ColourBMDColumns.xml");
            tsRecords.Text = Properties.Messages.Count + reportList.Count + " records listed.";
            string defaultProvider = (string)Application.UserAppDataRegistry.GetValue("Default Search Provider");
            if (defaultProvider == null)
                defaultProvider = "FamilySearch";
            if (defaultProvider.Equals("FreeCen"))
                defaultProvider = "FreeBMD";
            cbBMDSearchProvider.Text = defaultProvider;
            cbFilter.Text = "All Individuals";
        }

        void ResetTable()
        {
            dgBMDReportSheet.Sort(dgBMDReportSheet.Columns["BirthDate"], ListSortDirection.Ascending);
            dgBMDReportSheet.Sort(dgBMDReportSheet.Columns["Forenames"], ListSortDirection.Ascending);
            dgBMDReportSheet.Sort(dgBMDReportSheet.Columns["Surname"], ListSortDirection.Ascending);
            foreach (DataGridViewColumn column in dgBMDReportSheet.Columns)
                column.Width = column.MinimumWidth;
        }

        void DgReportSheet_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex == -1 || e.ColumnIndex == -1)
            {
                return;
            }
            if (e.ColumnIndex < birthColumnIndex || e.ColumnIndex > burialColumnIndex)
            {
                DataGridViewCell cell = dgBMDReportSheet.Rows[e.RowIndex].Cells["Relation"];

                // DataGridViewPrint is driven by the <cell>.Style, setting that property allows
                // colors/formatting to work in print/preview.
                DataGridViewCell thisCell = dgBMDReportSheet.Rows[e.RowIndex].Cells[e.ColumnIndex];

                string relation = (string)cell.Value;
                if (relation == "Direct Ancestor")
                {
                    e.CellStyle.Font = boldFont;
                    thisCell.Style.Font = boldFont;
                }
                if (relation == "Root Person")
                {
                    e.CellStyle.Font = boldFont;
                    e.CellStyle.ForeColor = Color.Red;
                    thisCell.Style.Font = boldFont;
                    thisCell.Style.ForeColor = Color.Red;
                }
            }
            else
            {
                DataGridViewCellStyle style = dgBMDReportSheet.DefaultCellStyle;
                DataGridViewCell cell = dgBMDReportSheet.Rows[e.RowIndex].Cells[e.ColumnIndex];
                BMDColour value = (BMDColour)cell.Value;
                styles.TryGetValue(value, out style);
                if (style != null)
                {
                    e.CellStyle.BackColor = style.BackColor;
                    e.CellStyle.ForeColor = style.ForeColor;
                    e.CellStyle.SelectionForeColor = e.CellStyle.SelectionBackColor;

                    cell.InheritedStyle.BackColor = style.BackColor;
                    cell.InheritedStyle.ForeColor = style.ForeColor;
                    cell.InheritedStyle.SelectionForeColor = cell.InheritedStyle.SelectionBackColor;

                    cell.Style = style; // For print/preview

                    switch (value)
                    {
                        case BMDColour.EMPTY: // Grey
                            if (e.ColumnIndex == burialColumnIndex - 1) // death column
                                cell.ToolTipText = "Individual is probably still alive"; // if OVER90 still grey cell but use different tooltip
                            else
                                cell.ToolTipText = string.Empty;
                            break;
                        case BMDColour.UNKNOWN_DATE: // Red
                            cell.ToolTipText = "Unknown date.";
                            break;
                        case BMDColour.OPEN_ENDED_DATE: // Orange Red
                            cell.ToolTipText = "Date is open ended, BEFore or AFTer a date.";
                            break;
                        case BMDColour.VERY_WIDE_DATE: // Tomato Red
                            cell.ToolTipText = "Date only accurate to more than ten year date range.";
                            break;
                        case BMDColour.WIDE_DATE: // Orange
                            cell.ToolTipText = "Date covers up to a ten year date range.";
                            break;
                        case BMDColour.NARROW_DATE: // Yellow
                            cell.ToolTipText = "Date accurate to within one to two year period.";
                            break;
                        case BMDColour.JUST_YEAR_DATE: // Yellow
                            cell.ToolTipText = "Date accurate to within one year period, but longer than 3 months.";
                            break;
                        case BMDColour.APPROX_DATE: // Pale Green 
                            cell.ToolTipText = "Date accurate to within 3 months (note may be date of registration not event date)";
                            break;
                        case BMDColour.EXACT_DATE: // Green
                            cell.ToolTipText = "Exact date.";
                            break;
                        case BMDColour.NO_SPOUSE: // pale grey
                            cell.ToolTipText = "Of marrying age but no spouse recorded";
                            break;
                        case BMDColour.NO_PARTNER: // light blue
                            cell.ToolTipText = "No partner but has shared fact or children";
                            break;
                        case BMDColour.NO_MARRIAGE: // dark blue
                            cell.ToolTipText = "Has partner but no marriage fact";
                            break;
                        case BMDColour.ISLIVING: // dark grey
                            cell.ToolTipText = "Is flagged as living";
                            break;
                        case BMDColour.OVER90:
                            cell.ToolTipText = "Individual may be still alive";
                            break;
                    }
                }
            }
        }

        void PrintToolStripButton_Click(object sender, EventArgs e)
        {
            reportFormHelper.PrintTitle = "Colour BMD Report";
            reportFormHelper.PrintReport("Colour BMD Report");
        }

        void PrintPreviewToolStripButton_Click(object sender, EventArgs e)
        {
            reportFormHelper.PrintPreviewReport();
        }

        void DgReportSheet_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                FamilyTree ft = FamilyTree.Instance;
                if (e.ColumnIndex >= birthColumnIndex && e.ColumnIndex <= burialColumnIndex)
                {
                    DataGridViewCell cell = dgBMDReportSheet.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    BMDColour value = (BMDColour)cell.Value;
                    if (value != BMDColour.EXACT_DATE)
                    {
                        IDisplayColourBMD person = (IDisplayColourBMD)dgBMDReportSheet.Rows[e.RowIndex].DataBoundItem;
                        Individual ind = ft.GetIndividual(person.IndividualID);
                        if (e.ColumnIndex == birthColumnIndex || e.ColumnIndex == birthColumnIndex + 1)
                        {
                            ft.SearchBMD(FamilyTree.SearchType.BIRTH, ind, ind.BirthDate, cbBMDSearchProvider.SelectedIndex, cbRegion.Text);
                        }
                        else if (e.ColumnIndex >= birthColumnIndex + 2 && e.ColumnIndex <= birthColumnIndex + 4)
                        {
                            FactDate marriageDate = FactDate.UNKNOWN_DATE;
                            if (e.ColumnIndex == birthColumnIndex + 2)
                                marriageDate = ind.FirstMarriageDate;
                            if (e.ColumnIndex == birthColumnIndex + 3)
                                marriageDate = ind.SecondMarriageDate;
                            if (e.ColumnIndex == birthColumnIndex + 4)
                                marriageDate = ind.ThirdMarriageDate;
                            ft.SearchBMD(FamilyTree.SearchType.MARRIAGE, ind, marriageDate, cbBMDSearchProvider.SelectedIndex, cbRegion.Text);
                        }
                        else if (e.ColumnIndex == burialColumnIndex || e.ColumnIndex == burialColumnIndex - 1)
                        {
                            ft.SearchBMD(FamilyTree.SearchType.DEATH, ind, ind.DeathDate, cbBMDSearchProvider.SelectedIndex, cbRegion.Text);
                        }
                    }
                }
                else if (e.ColumnIndex >= 0)
                {
                    string indID = (string)dgBMDReportSheet.CurrentRow.Cells["IndividualID"].Value;
                    Individual ind = ft.GetIndividual(indID);
                    Facts factForm = new Facts(ind);
                    factForm.Show();
                }
            }
        }

        void CbCensusSearchProvider_SelectedIndexChanged(object sender, EventArgs e)
        {
            string provider = cbBMDSearchProvider.SelectedItem.ToString();
            if (provider.Equals("FreeBMD"))
                provider = "FreeCen";
            Application.UserAppDataRegistry.SetValue("Default Search Provider", provider);
            dgBMDReportSheet.Refresh(); // forces refresh of tooltips
            dgBMDReportSheet.Focus();
        }

        private List<IDisplayColourBMD> BuildFilter(BMDColour toFind, bool all)
        {
            List<IDisplayColourBMD> result = new List<IDisplayColourBMD>();
            foreach (IDisplayColourBMD row in this.reportList)
            {
                if (all)
                {
                    if ((row.Birth == toFind || row.Birth == BMDColour.EMPTY) && (row.BaptChri == toFind || row.BaptChri == BMDColour.EMPTY) &&
                        (row.Marriage1 == toFind || row.Marriage1 == BMDColour.EMPTY) && (row.Marriage2 == toFind || row.Marriage2 == BMDColour.EMPTY) &&
                        (row.Marriage3 == toFind || row.Marriage3 == BMDColour.EMPTY) && (row.Death == toFind || row.Death == BMDColour.EMPTY) &&
                        (row.CremBuri == toFind || row.CremBuri == BMDColour.EMPTY))
                        result.Add(row);
                }
                else
                {
                    if (row.Birth == toFind || row.BaptChri == toFind || row.Marriage1 == toFind || row.Marriage2 == toFind ||
                        row.Marriage3 == toFind || row.Death == toFind || row.CremBuri == toFind)
                        result.Add(row);
                }
            }
            return result;
        }

        void CbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            switch (cbFilter.SelectedIndex)
            {
                case -1: // nothing selected
                case 0: // All Individuals
                    dgBMDReportSheet.DataSource = this.reportList;
                    break;
                case 1: // None Found (All Red)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.UNKNOWN_DATE, true));
                    break;
                case 2: // All Found (All Green)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.EXACT_DATE, true));
                    break;
                case 3: // All Open Ended ranges (Orange Red)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.OPEN_ENDED_DATE, true));
                    break;
                case 4: // All Wide date ranges (Tomato)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.VERY_WIDE_DATE, true));
                    break;
                case 5: // All Wide date ranges (Orange)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.WIDE_DATE, true));
                    break;
                case 6: // All Narrow date ranges (Yellow)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.NARROW_DATE, true));
                    break;
                case 7: // All Just year date ranges (Yellow)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.JUST_YEAR_DATE, true));
                    break;
                case 8: // All Approx date ranges (Light Green)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.APPROX_DATE, true));
                    break;
                case 9: // Some Missing (Some Red)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.UNKNOWN_DATE, false));
                    break;
                case 10: // Some found (Some Green)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.EXACT_DATE, false));
                    break;
                case 11: // Some Open Ended date ranges (Orange Red)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.OPEN_ENDED_DATE, false));
                    break;
                case 12: // Some Very Wide date ranges (Tomato)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.VERY_WIDE_DATE, false));
                    break;
                case 13: // Some Wide date ranges (Orange)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.WIDE_DATE, false));
                    break;
                case 14: // Some Narrow date ranges (Yellow)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.NARROW_DATE, false));
                    break;
                case 15: // Some Approx date ranges (Light Green)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.JUST_YEAR_DATE, false));
                    break;
                case 16: // Some Approx date ranges (Light Green)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.APPROX_DATE, false));
                    break;
                case 17: // Of Marrying age (Peach)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.NO_SPOUSE, false));
                    break;
                case 18: // No Partner shared fact/children (Light Blue)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.NO_PARTNER, false));
                    break;
                case 19: // Partner but no marriage (Dark Blue)
                    dgBMDReportSheet.DataSource = new SortableBindingList<IDisplayColourBMD>(BuildFilter(BMDColour.NO_MARRIAGE, false));
                    break;
            }
            dgBMDReportSheet.Focus();
            tsRecords.Text = $"{Properties.Messages.Count}{dgBMDReportSheet.RowCount} records listed.";
            Cursor = Cursors.Default;
        }

        void MnuExportToExcel_Click(object sender, EventArgs e) => reportFormHelper.DoExportToExcel<IDisplayColourBMD>();

        void MnuResetCensusColumns_Click(object sender, EventArgs e) => reportFormHelper.ResetColumnLayout("ColourBMDColumns.xml");

        void MnuSaveCensusColumnLayout_Click(object sender, EventArgs e)
        {
            reportFormHelper.SaveColumnLayout("ColourBMDColumns.xml");
            MessageBox.Show("Form Settings Saved", "BMD Colour");
        }

        void MnuViewFacts_Click(object sender, EventArgs e)
        {
            if (dgBMDReportSheet.CurrentRow != null)
            {
                IDisplayColourBMD ds = (IDisplayColourBMD)dgBMDReportSheet.CurrentRow.DataBoundItem;
                Individual ind = FamilyTree.Instance.GetIndividual(ds.IndividualID);
                Facts factForm = new Facts(ind);
                MainForm.DisposeDuplicateForms(factForm);
                factForm.Show();
            }
        }

        void DgBMDReportSheet_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                dgBMDReportSheet.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;
        }

        void ColourBMD_FormClosed(object sender, FormClosedEventArgs e) => Dispose();

        void ColourBMD_Load(object sender, EventArgs e) => SpecialMethods.SetFonts(this);
    }
}
