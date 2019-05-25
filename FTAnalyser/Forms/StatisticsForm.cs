﻿using FTAnalyzer.Filters;
using FTAnalyzer.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace FTAnalyzer.Forms
{
    public partial class StatisticsForm : Form
    {
        public enum StatisticType { CousinCount = 1, HowManyDirects = 2, BirthdayEffect = 3 };

        StatisticType StatType { get; }

        public StatisticsForm(StatisticType type)
        {
            InitializeComponent();
            Top = Top + NativeMethods.TopTaskbarOffset;
            StatType = type;
            tsStatusLabel.Text = string.Empty;
            switch(type)
            {
                case StatisticType.CousinCount:
                    CousinsCountReport();
                    break;
                case StatisticType.HowManyDirects:
                    HowManyDirectsReport();
                    break;
                case StatisticType.BirthdayEffect:
                    BirthdayEffectReport();
                    break;
            }
        }

        void CousinsCountReport()
        {
            IEnumerable<Tuple<string,int>> relations = FamilyTree.Instance.AllIndividuals.Where(x => x.RelationToRoot.Length > 0).GroupBy(i => i.RelationToRoot)
                .Select(r => new Tuple<string, int>(r.Key, r.Count()));
            dgStatistics.DataSource = new SortableBindingList<Tuple<string, int>>(relations.ToList());
            dgStatistics.Columns[0].Width = 180;
            dgStatistics.Columns[0].SortMode = DataGridViewColumnSortMode.Automatic;
            dgStatistics.Columns[0].HeaderText = "Relation to Root";
            dgStatistics.Columns[1].Width = 60;
            dgStatistics.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgStatistics.Columns[1].SortMode = DataGridViewColumnSortMode.Automatic;
            dgStatistics.Columns[1].HeaderText = "Count";
            dgStatistics.Sort(dgStatistics.Columns[0], ListSortDirection.Ascending);
            tsStatusLabel.Text = "Double click to show all individuals with that relationship to root person.";
            tsStatusLabel.Visible = true;
        }

        void HowManyDirectsReport()
        {
            IEnumerable<DisplayGreatStats> relations = FamilyTree.Instance.AllIndividuals.Where(x => x.RelationToRoot.Length > 0 && (x.RelationType == Individual.DIRECT || x.RelationType == Individual.DESCENDANT))
                .GroupBy(i => (i.RelationToRoot, i.RelationSort))
                .Select(r => new DisplayGreatStats(r.Key.RelationToRoot, r.Key.RelationSort ,r.Count()));
            dgStatistics.DataSource = new SortableBindingList<DisplayGreatStats>(relations.ToList());
            dgStatistics.Columns[0].Width = 180;
            dgStatistics.Columns[0].SortMode = DataGridViewColumnSortMode.Automatic;
            dgStatistics.Columns[0].HeaderText = "Relation to Root";
            dgStatistics.Columns[1].Visible = false;
            dgStatistics.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgStatistics.Columns[1].SortMode = DataGridViewColumnSortMode.Automatic;
            dgStatistics.Columns[1].HeaderText = "Relation Sort";
            dgStatistics.Columns[2].Width = 60;
            dgStatistics.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgStatistics.Columns[2].SortMode = DataGridViewColumnSortMode.Automatic;
            dgStatistics.Columns[2].HeaderText = "Count";
            dgStatistics.Sort(dgStatistics.Columns[1], ListSortDirection.Descending);
            tsStatusLabel.Text = "Double click to show all individuals with that relationship to root person.";
            tsStatusLabel.Visible = true;
        }

        void BirthdayEffectReport()
        {
            List<Tuple<string, int>> birthdayEffect = FamilyTree.Instance.AllIndividuals.Where(x => x.BirthdayEffect).GroupBy(i => i.BirthMonth)
                .Select(r => new Tuple<string, int>(r.Key, r.Count())).ToList();
            List<Tuple<string, int>> exactDates = FamilyTree.Instance.AllIndividuals.Where(x => x.BirthDate.IsExact && x.DeathDate.IsExact).GroupBy(i => i.BirthMonth)
                .Select(r => new Tuple<string, int>(r.Key, r.Count())).ToList();
            birthdayEffect.Sort();
            exactDates.Sort();
            List<Tuple<string, string, string>> result = new List<Tuple<string, string, string>>();
            for(int i=0; i<12; i++)
            {
                var column2 = $"{birthdayEffect[i].Item2}/{exactDates[i].Item2}";
                float percent = (float)birthdayEffect[i].Item2 / exactDates[i].Item2;
                result.Add(new Tuple<string, string, string>(birthdayEffect[i].Item1, column2, string.Format("{0:P2}",percent)));
            }
            dgStatistics.DataSource = new SortableBindingList<Tuple<string, string, string>>(result);
            dgStatistics.Columns[0].Width = 150;
            dgStatistics.Columns[0].SortMode = DataGridViewColumnSortMode.Automatic;
            dgStatistics.Columns[0].HeaderText = "Birth Month";
            dgStatistics.Columns[1].Width = 80;
            dgStatistics.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgStatistics.Columns[1].SortMode = DataGridViewColumnSortMode.Automatic;
            dgStatistics.Columns[1].HeaderText = "Died Near Birthday";
            dgStatistics.Columns[2].Width = 80;
            dgStatistics.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgStatistics.Columns[2].SortMode = DataGridViewColumnSortMode.Automatic;
            dgStatistics.Columns[2].HeaderText = "Percentage";
            dgStatistics.Sort(dgStatistics.Columns[0], ListSortDirection.Ascending);
            tsStatusLabel.Text = "Double click shows those born who died within 15 days of birthday.";
            tsStatusLabel.Visible = true;
        }

        void DgStatistics_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (StatType == StatisticType.HowManyDirects)
                {
                    if (dgStatistics.Rows[e.RowIndex].DataBoundItem is DisplayGreatStats row)
                    {
                        People form = new People();
                        form.ListRelationToRoot(row.RelationToRoot);
                        form.Show();
                    }
                }
                else if (StatType == StatisticType.CousinCount)
                {
                    if (dgStatistics.Rows[e.RowIndex].DataBoundItem is Tuple<string, int> row)
                    {
                        People form = new People();
                        form.ListRelationToRoot(row.Item1);
                        form.Show();
                    }
                }
                else if(StatType == StatisticType.BirthdayEffect)
                {
                    if (dgStatistics.Rows[e.RowIndex].DataBoundItem is Tuple<string, string, string> row)
                    {
                        People form = new People();
                        bool filter(Individual x) => x.BirthdayEffect && x.BirthMonth == row.Item1;
                        List<Individual> individuals = FamilyTree.Instance.AllIndividuals.Filter(filter).ToList();
                        form.SetIndividuals(individuals, $"Indiviudals who died within 15 days of their birthday in {row.Item1.Substring(5)}");
                        form.Show();
                    }
                }
            }
        }

        void StatisticsForm_Load(object sender, EventArgs e)
        {
            SpecialMethods.SetFonts(this);
        }
    }
}
