using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace hhruParser
{
    public partial class MainWindow : Window
    {
        List<Job> jobs = new List<Job>();

        public MainWindow()
        {
            InitializeComponent();

            init();
        }

        private void init()
        {
            fillJobsListBox();
            safetyCheck();
            if (File.Exists(@".\output.xlsx")) { File.Delete(@".\output.xlsx"); }
        }

        private void fillJobsListBox()
        {
            string[] text = File.ReadAllLines("../../jobsData.txt");
            foreach (string s in text)
            {
                jobsListBox.Items.Add(new Job(s));
            }
        }

        private void callPythonCode(string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"C:\Users\Max\AppData\Local\Programs\Python\Python310\python.exe";
            start.Arguments = @"..\..\program.py" + " " + args;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = false;
            Process process = Process.Start(start);
        }

        private String getArgs()
        {
            string outMessage = "";
            foreach (Job job in jobs)
            {
                if (outMessage.Length != 0) outMessage += ";";
                outMessage += job.getId();
            }
            outMessage += " " + minWageTextBox.Text + ";" + maxWageTextBox.Text;
            if (dateTextBox.Text != "")
            {
                outMessage += " " + dateTextBox.Text;
            }
            else
            {
                outMessage += " 0";
            }
            
            return outMessage;
        }

        private void readExcel()
        {
            XSSFWorkbook xssfwb;
            parseDataGrid.Items.Clear();
            using (FileStream file = new FileStream(@".\output.xlsx", FileMode.Open, FileAccess.Read))
            {
                xssfwb = new XSSFWorkbook(file);
            }

            ISheet sheet = xssfwb.GetSheet("Sheet1");
            for (int row = 1; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    IRow rowContent = sheet.GetRow(row);
                    if ((maxWageTextBox.Text.Length == 0) || (int.Parse(rowContent.GetCell(1).StringCellValue) <= int.Parse(maxWageTextBox.Text)))
                    {
                        if ((minWageTextBox.Text.Length == 0) || (int.Parse(rowContent.GetCell(1).StringCellValue) >= int.Parse(minWageTextBox.Text)))
                        {
                            parseDataGrid.Items.Add(new Vacancy(rowContent.GetCell(0).StringCellValue, int.Parse(rowContent.GetCell(1).StringCellValue)));
                        }
                    }
                }
            }
            xssfwb.Close();
            File.Delete(@".\output.xlsx");
            safetyCheck();
        }

        private void writeExcel(string path)
        {
            XSSFWorkbook xssfwb = new XSSFWorkbook();
            ISheet sheet = xssfwb.CreateSheet("Sheet1");
            int rowCounter = 1;

            IRow headerRow = sheet.CreateRow(0);
            createCell(headerRow, 0, "Вакансия");
            createCell(headerRow, 1, "Зарплата");
            createCell(headerRow, 3, "Минимальная зп");
            createCell(headerRow, 4, "Максимальная зп");
            createCell(headerRow, 5, "Средняя зп");

            foreach(Vacancy vac in parseDataGrid.Items)
            {
                IRow currentRow = sheet.CreateRow(rowCounter);
                createCell(currentRow, 0, vac.name);
                createCell(currentRow, 1, vac.wage.ToString());

                if(rowCounter == 1)
                {
                    createCell(currentRow, 3, minWageLabel.Content.ToString());
                    createCell(currentRow, 4, maxWageLabel.Content.ToString());
                    createCell(currentRow, 5, avgWageLabel.Content.ToString());
                }

                rowCounter++;
            }

            int lastColumNum = sheet.GetRow(0).LastCellNum;
            for (int i = 0; i <= lastColumNum; i++)
            {
                sheet.AutoSizeColumn(i);
                GC.Collect();
            }

            using (var fileData = new FileStream(path + @"\ParseResult.xlsx", FileMode.Create))
            {
                xssfwb.Write(fileData);
            }
        }

        private void createCell(IRow currentRow, int index, string value)
        {
            ICell cell = currentRow.CreateCell(index);
            cell.SetCellValue(value);
        }

        private void findMinMaxAvgWage()
        {
            int minWage = 0;
            int maxWage = 0;
            int avgWage = 0;
            int counter = 0;

            foreach(Vacancy vac in parseDataGrid.Items)
            {
                if(counter == 0) { minWage = vac.wage; }
                counter++;
                avgWage += vac.wage;
                if(vac.wage > maxWage) { maxWage = vac.wage; }
                if (vac.wage < minWage) { minWage = vac.wage; }
            }
            try
            {
                avgWage = avgWage / counter;
            }
            catch (DivideByZeroException)
            {
                avgWage = 0;
            }

            minWageLabel.Content = "Минимальная зарплата = " + minWage;
            maxWageLabel.Content = "Максимальная зарплата = " + maxWage;
            avgWageLabel.Content = "Средняя зарплата = " + avgWage;

        }

        private void safetyCheck()
        {
            parseButton.IsEnabled = (jobs.Count != 0);
            excelButton.IsEnabled = (parseDataGrid.Items.Count != 0);
        }

        private void parseButton_Click(object sender, RoutedEventArgs e)
        {
            callPythonCode(getArgs());
            while (true)
            {
                if (File.Exists(@".\output.xlsx")) { break; }
            }
            System.Threading.Thread.Sleep(500);
            readExcel();
            findMinMaxAvgWage();
        }

        private void excelButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if(result == System.Windows.Forms.DialogResult.OK)
                {
                    writeExcel(dialog.SelectedPath);
                }
            }
        }

        private void jobsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (jobs.Contains(jobsListBox.SelectedItem))
            {
                jobs.Remove((Job)jobsListBox.SelectedItem);
                ((Job)jobsListBox.SelectedItem).delMark();
                jobsListBox.Items.Refresh();
            }
            else
            {
                jobs.Add((Job)jobsListBox.SelectedItem);
                ((Job)jobsListBox.SelectedItem).makeMark();
                jobsListBox.Items.Refresh();
            }
            safetyCheck();
        }
    }
}
