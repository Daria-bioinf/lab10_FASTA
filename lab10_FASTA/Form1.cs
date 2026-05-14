using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using CsvHelper;
using System.Globalization;



namespace lab10_FASTA
{
    public partial class Form1 : Form
    {
        List<SequenceData> sequenceList = new List<SequenceData>();
        public Form1()
        {
            InitializeComponent();
        }

        public class SequenceData
        {
            public string Name { get; set; }
            public int Length { get; set; }
            public double GC_Content { get; set; }
            public int A_Count { get; set; }
            public int T_Count { get; set; }
            public int G_Count { get; set; }
            public int C_Count { get; set; }
            public int Codons { get; set; }
        }



        private void buttonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "FASTA files (*.fasta)|*.fasta|Text files (*.txt)|*.txt";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string[] lines = File.ReadAllLines(openFileDialog.FileName);

                if (!lines.Any(line => line.StartsWith(">")))
                {
                    MessageBox.Show("Invalid FASTA file!");
                    return;
                }

                sequenceList.Clear();

                string currentName = "";
                string currentSequence = "";

                foreach (string line in lines)
                {
                    if (line.StartsWith(">"))
                    {
                        if (currentSequence.Length > 0)
                        {
                            sequenceList.Add(AnalyzeSequence(currentName, currentSequence));
                        }

                        currentName = line.Substring(1);
                        currentSequence = "";
                    }
                    else
                    {
                        string sequencePart = line.Trim().ToUpper();

                        foreach (char ch in sequencePart)
                        {
                            if ("ATGCN".Contains(ch) == false)
                            {
                                MessageBox.Show("Invalid DNA sequence!");

                                return;
                            }
                        }

                        currentSequence += sequencePart;
                    }
                }

                if (currentSequence.Length > 0)
                {
                    sequenceList.Add(AnalyzeSequence(currentName, currentSequence));
                }

                listBox1.Items.Clear();

                foreach (var seq in sequenceList)
                {
                    listBox1.Items.Add(seq.Name);                
                }
                dataGridView1.DataSource = sequenceList;

                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            }
        }
        private SequenceData AnalyzeSequence(string name, string sequence)
        {
            int a = sequence.Count(x => x == 'A');
            int t = sequence.Count(x => x == 'T');
            int g = sequence.Count(x => x == 'G');
            int c = sequence.Count(x => x == 'C');

            int length = sequence.Length;
            int codons = length / 3;

            double gc = ((double)(g + c) / length) * 100;

            return new SequenceData
            {
                Name = name,
                Length = length,
                GC_Content = Math.Round(gc, 2),
                A_Count = a,
                T_Count = t,
                G_Count = g,
                C_Count = c,
                Codons = codons
            };

        }

        private void buttonJson_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "JSON files (*.json)|*.json";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string json =
                    JsonConvert.SerializeObject(sequenceList, Formatting.Indented);

                File.WriteAllText(saveFileDialog.FileName, json);

                MessageBox.Show("JSON exported!");
            }
        }

        private void buttonCsv_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "CSV files (*.csv)|*.csv";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (var writer = new StreamWriter(saveFileDialog.FileName))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(sequenceList);
                }

                MessageBox.Show("CSV exported!");
            }
        }
    }
}
