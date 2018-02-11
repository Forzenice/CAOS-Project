
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
//using Microsoft.Chart.Controls;

namespace Main_GUI
{
    public partial class Form1 : Form
    {
        //CY
        int CYi = 0;//counter for number of rows used.
        //KJ
        //Save Code//
        public static string fileNametosave;
        public static string fileName;
        //Save Code//
        public static List<String> resultProcess = new List<String>();
        public static List<int> resultBurst = new List<int>();
        public static double averageTAT = 0, averageWT = 0;
        //mm
        int dgx2JY = 0;
        int dgx3JY = 0;
        //mm graph
        int CYTotalCounter = 0;
        int CYTotalCounter2 = 0;
        int[] blocksarr = new int[10000];
        int CYTotalCounter3 = 0;
        int CYTotalCounter4 = 0;
        int[] blocksarr2 = new int[10000];
        //vm
        private static int dataFrame;
        private static string dataInString;
        private static int fault;
        private static int addcell = 0;

        public Form1()
        {
            InitializeComponent();

            lblResult.Text = "";
            lblResult2.Text = "";

            btnLocation.Enabled = false;
            btnSave.Enabled = false;
        }

        private void mainPanel_Paint_1(object sender, PaintEventArgs e)
        {
            
        }
        private void mainResetPanel()
        {
            //panel reset
            mainPanel.Visible = false;
            panelCPUFCFS.Visible = false;
            panelNPP.Visible = false;
            panelMainMemory.Visible = false;
            panelVM.Visible = false;
        }

        private void btnFCFS_Click(object sender, EventArgs e)
        {
            mainResetPanel();
            panelCPUFCFS.Visible = true;

            //datagridviewreset
            dgvCYFCFS.DataSource = null;
            dgvCYFCFS.Columns.Clear();
            dgvCYFCFS.Rows.Clear();
            dgvCYFCFS.Refresh();
            //end

            //setup default for datagridview
            dgvCYFCFS.Columns.Add("Process","Process");
            dgvCYFCFS.Columns.Add("Arrival Time", "Arrival Time");
            dgvCYFCFS.Columns.Add("Burst Time", "Burst Time");
            //end
        }

        private void lblCYRT_Click(object sender, EventArgs e)
        {

        }

        private void btnCYreturn_Click(object sender, EventArgs e)
        {
            mainResetPanel();
            mainPanel.Visible = true;
        }

        private void dgvCYFCFS_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnCYAdd_Click(object sender, EventArgs e)
        {
            try {
                //test for integer
                int o = Convert.ToInt32(txtCYPP.Text);
                int u = Convert.ToInt32(txtCYBT.Text);
                int f = Convert.ToInt32(txtCYAT.Text);
            }
           catch {
                MessageBox.Show("Error please enter numbers only");
                txtCYPP.Text = "";
                txtCYAT.Text = "";
                txtCYBT.Text = "";
                return;
            }
            if (txtCYPP.Text == "" || txtCYAT.Text == "" || txtCYBT.Text == "")
            {
                //check for empty textfield.
                MessageBox.Show("Error please fill in all fields");
                return;
            }    
            else
            {
                for(int t=0;t<dgvCYFCFS.Rows.Count-1;t++)
                {
                    if (txtCYPP.Text == dgvCYFCFS.Rows[t].Cells[0].Value.ToString())
                    {
                        //priority number duplicate
                        MessageBox.Show("Error duplicate priority detected.");
                        txtCYPP.Text = "";
                        txtCYAT.Text = "";
                        txtCYBT.Text = "";
                        return;
                    }
                }            
                dgvCYFCFS.Rows.Add(1);
                //int i = dgvCYFCFS.Rows.Count - 1;
                dgvCYFCFS.Rows[CYi].Cells[0].Value = Convert.ToInt32(txtCYPP.Text);
                dgvCYFCFS.Rows[CYi].Cells[1].Value = Convert.ToInt32(txtCYAT.Text);
                dgvCYFCFS.Rows[CYi].Cells[2].Value = Convert.ToInt32(txtCYBT.Text);
                CYi++;

                txtCYPP.Text = "";
                txtCYAT.Text = "";
                txtCYBT.Text = "";
            }
        }
        private void btnCYRemove_Click(object sender, EventArgs e)
        {
            if (dgvCYFCFS.Rows.Count != 1)
            {
                try { int del1 = dgvCYFCFS.CurrentCell.RowIndex; }
                catch
                {
                    MessageBox.Show("No Cells Selected");
                    return;
                }
                //get cellrow
                int del = dgvCYFCFS.CurrentCell.RowIndex;
                //delete row
                dgvCYFCFS.Rows.RemoveAt(del);
                CYi--;
            }
            else { MessageBox.Show("no data to remove"); }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //default load.
            mainResetPanel();
            mainPanel.Visible = true;
        }

        private void btnCYClr_Click(object sender, EventArgs e)
        {
            //setup for datagridview
            dgvCYFCFS.DataSource = null;
            dgvCYFCFS.Columns.Clear();
            dgvCYFCFS.Rows.Clear();
            dgvCYFCFS.Refresh();
            //end

            //setup default for datagridview
            dgvCYFCFS.Columns.Add("Process", "Process");
            dgvCYFCFS.Columns.Add("Arrival Time", "Arrival Time");
            dgvCYFCFS.Columns.Add("Burst Time", "Burst Time");

            CYi = 0;
        }

        private void btnCYStart_Click(object sender, EventArgs e) //start calculations.
        {
            FCFSgantt.Series.Clear(); // clear previous chart if any.
            FCFSgantt.ChartAreas[0].AxisX.LabelStyle.Enabled = false; //disable axis

            double bursttime=0;
            if(dgvCYFCFS.Rows.Count != 1)
            {
                int i = dgvCYFCFS.Rows.Count-1;//count number of data
                //sort by arrival time first.
                int[,] cystorageArray = new int[i,3];
                //int[] atArray = null;
                for (int t=0;t<i;t++) //pass into array
                {
                    //int ttt= Convert.ToInt32(dgvCYFCFS.Rows[t].Cells[0].Value);
                    //int[][] storageArray;
                   cystorageArray[t,0] = Convert.ToInt32(dgvCYFCFS.Rows[t].Cells[0].Value);
                   cystorageArray[t,1] = Convert.ToInt32(dgvCYFCFS.Rows[t].Cells[1].Value);
                   cystorageArray[t,2] = Convert.ToInt32(dgvCYFCFS.Rows[t].Cells[2].Value);
                   bursttime = bursttime+ Convert.ToInt32(dgvCYFCFS.Rows[t].Cells[2].Value);
                    //atArray[t] = Convert.ToInt32(dgvCYFCFS.Rows[t].Cells[1].Value);
                }
                for (int j = 0; j<i-1;j++) //sort by ascending arrival time.
                {

                        if (cystorageArray[j,1] > cystorageArray[j+1,1])
                        {
                        int one, two, three, four;
                        one = j;
                        two = cystorageArray[j,0];
                        three = cystorageArray[j,1];
                        four = cystorageArray[j,2];

                        cystorageArray[j,0] = cystorageArray[j + 1,0];
                        cystorageArray[j,1] = cystorageArray[j + 1,1];
                        cystorageArray[j,2] = cystorageArray[j + 1,2];

                        cystorageArray[j + 1,0] = two;
                        cystorageArray[j + 1,1] = three;
                        cystorageArray[j+1,2] = four;

                        j =-1;
                    }
                        else if(cystorageArray[j, 1] == cystorageArray[j + 1, 1]) //if same arrival time
                    {
                        if(cystorageArray[j,0]>cystorageArray[j+1,0]) //sort by Priority number
                        {
                            int one, two, three, four;
                            one = j;
                            two = cystorageArray[j, 0];
                            three = cystorageArray[j, 1];
                            four = cystorageArray[j, 2];

                            cystorageArray[j, 0] = cystorageArray[j + 1, 0];
                            cystorageArray[j, 1] = cystorageArray[j + 1, 1];
                            cystorageArray[j, 2] = cystorageArray[j + 1, 2];

                            cystorageArray[j + 1, 0] = two;
                            cystorageArray[j + 1, 1] = three;
                            cystorageArray[j + 1, 2] = four;
                            j = -1;//reset array counter
                        }
                    }
                }

                lblCYRT.Text = "Response Time : " + (Convert.ToInt32(cystorageArray[0, 2]));
                //Sort into order.
                int current=0; //current timer.
                int positioncounter = 0; //numbers of items transfered.
                int counter1 = i;
                int idleCount=0;
                int used=0; //numbers of new array used.
                string[,] cyFinalArray = new string[10000000, 3];
                
                for (int j=0;j<counter1; j++) //Sort into order.
                {
                    if(current < cystorageArray[positioncounter,1]) //if idle
                    {
                        cyFinalArray[j, 0] = "idle" + idleCount;
                        cyFinalArray[j, 1] = "0";
                        cyFinalArray[j, 2] =(cystorageArray[positioncounter,1]-current).ToString(); //set total idle time
                        current = current + (cystorageArray[positioncounter, 1] - current); //set current position to after adding idle time
                        counter1++; //each time an new item is added counter1++
                        used++; //total number of array used.
                        idleCount++;
                        //idleTime++;
                    }
                    else
                    { //if non idle.
                        cyFinalArray[j, 0] = Convert.ToString("P" + cystorageArray[positioncounter, 0]);
                        cyFinalArray[j, 1] = cystorageArray[positioncounter, 1].ToString();
                        cyFinalArray[j, 2]= cystorageArray[positioncounter, 2].ToString();
                        current = current+cystorageArray[positioncounter, 2];
                        positioncounter++;
                        used++;
                    }
                }
               double tttime = 0;
                //Calculate TT Time
                for (int o = 0; o<used; o++)
                {
                    tttime = tttime + Convert.ToInt32(cyFinalArray[o, 2]);
                }

                int totalexe = Convert.ToInt32(tttime);
                double bt = bursttime;
                bursttime = (bursttime / totalexe) * 100;
                lblCYCPUU.Text = "CPU Utilization : " + bursttime.ToString("0.##") + "%";

                FCFSgantt.ChartAreas[0].AxisY.Minimum = 0; //start from 0
                FCFSgantt.ChartAreas[0].AxisY.Maximum = tttime; //total turnaround time.
                FCFSgantt.ChartAreas[0].AxisY.Interval = 1; //per interval.
               
                for (int t=0;t<used/*total number of process + idle*/;t++)
                {
                    FCFSgantt.Series.Add(cyFinalArray[t, 0]); //add new series
                    FCFSgantt.Series[cyFinalArray[t, 0]].Label = cyFinalArray[t, 0]; //set legend name
                    FCFSgantt.Series[cyFinalArray[t, 0]].ChartType = SeriesChartType.StackedBar; //set into stacked bar.
                    FCFSgantt.Series[cyFinalArray[t, 0]].Points.AddY(cyFinalArray[t, 2]); //set value for ending 
                    FCFSgantt.Series[cyFinalArray[t, 0]].ChartArea = "ChartArea1"; 
                }
                tttime = tttime/positioncounter;
                //lblCYTAT.Text = "Turn Around Time : " + tttime.ToString("##.##");

                //calculate average waiting time.
                int totalexe2 =used;
                int[,] waitarr = new int[used, 1]; //wait array.
               
                for (int o = used; o-- > 0;) //reverse array
                {
                    if (Regex.IsMatch(cyFinalArray[o, 0], "idle")==true) //if is not idle.
                    {

                        totalexe = totalexe - Convert.ToInt32(cyFinalArray[o, 2]);
                    }
                    else { 
                        totalexe = totalexe - Convert.ToInt32(cyFinalArray[o, 2]);
                        waitarr[o, 0] = totalexe - Convert.ToInt32(cyFinalArray[o, 1]);
                    }//if is idle
                }
               double avgwaittime = 0;
                for (int k = 0; k <used;k++)
                {
                    avgwaittime = avgwaittime + waitarr[k, 0];//add all average waiting time.
                }

                bt = avgwaittime + bt;
                bt = bt / positioncounter;
                lblCYTAT.Text = "Average Turn"+"\n"+"Around Time : " + bt.ToString("0.##");
                avgwaittime = avgwaittime / positioncounter;
                lblCYWT.Text = "Average" + "\n" + "Waiting Time : " + avgwaittime.ToString("0.##"); //calculate waiting time.
                //lblCYRT.Text = "Response Time : " + (Convert.ToInt32(cystorageArray[0, 2])-Convert.ToInt32(cystorageArray[0,1]));
            }
        }

        private void btnNPP_Click(object sender, EventArgs e)
        {
            mainResetPanel();
            panelNPP.Visible = true;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            mainResetPanel();
            mainPanel.Visible = true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                Convert.ToInt16(txtProcess.Text);
                Convert.ToInt16(txtBurstTime.Text);
                Convert.ToInt16(txtArrivalTime.Text);
                Convert.ToInt16(txtPriority.Text);

            }

            catch
            {
                MessageBox.Show("Error Detected! Please type in NUMBER only and do NOT Leave Blank!");
                return;
            }


            if (txtProcess.Text == "" || txtBurstTime.Text == "" || txtArrivalTime.Text == "" || txtPriority.Text == "")
            {
                MessageBox.Show("Error Detected! Please type in NUMBER only and do NOT Leave Blank!");
                return;
            }

            else
            {
                DataGridViewRow row = (DataGridViewRow)dataGridView1.Rows[0].Clone();
                row.Cells[0].Value = Convert.ToInt16(txtProcess.Text);
                row.Cells[1].Value = Convert.ToInt16(txtArrivalTime.Text);
                row.Cells[2].Value = Convert.ToInt16(txtBurstTime.Text);
                row.Cells[3].Value = Convert.ToInt16(txtPriority.Text);

                for (int intCount = 0; intCount < dataGridView1.Rows.Count - 1; intCount++)
                {

                    //check if it already exists
                    if (txtProcess.Text == dataGridView1.Rows[intCount].Cells[0].Value.ToString())
                    {
                        MessageBox.Show("Error! Duplicate Process Found!");
                        return;
                    }

                }

                dataGridView1.Rows.Add(row);
                dataGridView1.Sort(dataGridView1.Columns["ArrivalTime"], ListSortDirection.Ascending);


                txtProcess.Text = "";
                txtBurstTime.Text = "";
                txtArrivalTime.Text = "";
                txtPriority.Text = "";
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            //Save Code//
            if ((dataGridView1.Rows.Count - 1) == 0)
            {
                btnLocation.Enabled = false;
                btnSave.Enabled = false;
            }

            else
            {
                int row = dataGridView1.CurrentCell.RowIndex;
                dataGridView1.Rows.RemoveAt(row);
            }
            //Save Code//
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            lblResult.Text = "";
            lblResult2.Text = "";
            //Save Code//
            btnLocation.Enabled = false;
            btnSave.Enabled = false;
            //Save Code//
            chtPriority.Series.Clear();
            chtPriority.ChartAreas[0].AxisX.LabelStyle.Enabled = false;

            btnAdd.Enabled = true;
            btnRemove.Enabled = true;
        }

        private void btnLocation_Click(object sender, EventArgs e)
        {
            string mystring;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Title = "Select File Location";

            if (string.IsNullOrWhiteSpace(txtSave.Text))
            {
                saveFileDialog1.FileName = "NP_Priority";
            }
            if (!string.IsNullOrWhiteSpace(txtSave.Text))
            {
                saveFileDialog1.FileName = txtSave.Text;

            }
            saveFileDialog1.Filter = "Excel file (*.csv)|*.csv|Text file (*.txt)|*.txt";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {

                mystring = saveFileDialog1.FileName;
                fileName = saveFileDialog1.FileName;
                fileNametosave = mystring.Remove(mystring.Length - 1, 1);
                txtSave.Text = mystring;

            }

            if (fileNametosave == null)
            {
                MessageBox.Show("Error! Please Select a Location");
                return;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnAdd.Enabled = false;
            btnRemove.Enabled = false;

            lblResult.Text = "";
            lblResult2.Text = "";

            chtPriority.Series.Clear();
            chtPriority.ChartAreas[0].AxisX.LabelStyle.Enabled = false;

            int bt = 0;
            String Process = null;

            List<String> process = new List<String>();
            List<int> burstTime = new List<int>();
            List<int> arrivalTime = new List<int>();
            List<int> priority = new List<int>();

            String[] removeFirst = new String[4];
            String[] removeSecond = new String[4];

            List<String> result = new List<String>();
            List<int> resultBT = new List<int>();
            List<int> queue = new List<int>(); // List value that is in bt
            List<int> comparePriority = new List<int>(); // List used to compare priority of queue
            List<int> finalBurst = new List<int>();
            List<int> finalArrival = new List<int>();
            resultProcess = new List<String>();
            resultBurst = new List<int>();
            averageTAT = 0;
            averageWT = 0;


            comparePriority.Clear();

            btnLocation.Enabled = true;
            btnSave.Enabled = true;

            if ((dataGridView1.Rows.Count - 1) == 1)
            {
                process.Add(Convert.ToString(dataGridView1.Rows[0].Cells[0].Value));
                arrivalTime.Add(Convert.ToInt32(dataGridView1.Rows[0].Cells[1].Value));
                burstTime.Add(Convert.ToInt32(dataGridView1.Rows[0].Cells[2].Value));
                priority.Add(Convert.ToInt32(dataGridView1.Rows[0].Cells[3].Value));

                if (arrivalTime[0] == 0) // check for first row
                {
                    bt = bt + burstTime[0];
                    result.Add("P" + process[0]);
                    resultBT.Add(burstTime[0]);
                    finalArrival.Add(arrivalTime[0]);
                    finalBurst.Add(bt);

                    process.RemoveAt(0);
                    arrivalTime.RemoveAt(0);
                    burstTime.RemoveAt(0);
                    priority.RemoveAt(0);
                }

                else
                {
                    bt = bt + arrivalTime[0];
                    result.Add("Idle");
                    resultBT.Add(arrivalTime[0]);
                    finalArrival.Add(arrivalTime[0]);
                    finalBurst.Add(bt);

                    bt = bt + burstTime[0];
                    result.Add("P" + process[0]);
                    resultBT.Add(burstTime[0]);
                    finalArrival.Add(arrivalTime[0]);
                    finalBurst.Add(bt);

                    process.RemoveAt(0);
                    arrivalTime.RemoveAt(0);
                    burstTime.RemoveAt(0);
                    priority.RemoveAt(0);
                }
            }

            else if (dataGridView1.Rows.Count > 1)
            {
                for (int j = 0; j < dataGridView1.Rows.Count - 2; j++)
                {
                    if (Convert.ToInt32(dataGridView1.Rows[j].Cells[1].Value) == Convert.ToInt32(dataGridView1.Rows[j + 1].Cells[1].Value)) //if have same arrival time
                    {
                        if (Convert.ToInt32(dataGridView1.Rows[j].Cells[3].Value) > Convert.ToInt32(dataGridView1.Rows[j + 1].Cells[3].Value)) //if 1st priority is > than 2nd priority
                        {
                            removeFirst[0] = Convert.ToString(dataGridView1.Rows[j].Cells[0].Value);
                            removeFirst[1] = Convert.ToString(dataGridView1.Rows[j].Cells[1].Value);
                            removeFirst[2] = Convert.ToString(dataGridView1.Rows[j].Cells[2].Value);
                            removeFirst[3] = Convert.ToString(dataGridView1.Rows[j].Cells[3].Value);

                            removeSecond[0] = Convert.ToString(dataGridView1.Rows[j + 1].Cells[0].Value);
                            removeSecond[1] = Convert.ToString(dataGridView1.Rows[j + 1].Cells[1].Value);
                            removeSecond[2] = Convert.ToString(dataGridView1.Rows[j + 1].Cells[2].Value);
                            removeSecond[3] = Convert.ToString(dataGridView1.Rows[j + 1].Cells[3].Value);

                            dataGridView1.Rows[j].Cells[0].Value = removeSecond[0];
                            dataGridView1.Rows[j].Cells[1].Value = removeSecond[1];
                            dataGridView1.Rows[j].Cells[2].Value = removeSecond[2];
                            dataGridView1.Rows[j].Cells[3].Value = removeSecond[3];

                            dataGridView1.Rows[j + 1].Cells[0].Value = removeFirst[0];
                            dataGridView1.Rows[j + 1].Cells[1].Value = removeFirst[1];
                            dataGridView1.Rows[j + 1].Cells[2].Value = removeFirst[2];
                            dataGridView1.Rows[j + 1].Cells[3].Value = removeFirst[3];

                            Array.Clear(removeFirst, 0, removeFirst.Length);
                            Array.Clear(removeSecond, 0, removeFirst.Length);
                        }
                    }
                }

                for (int i = 0; i < dataGridView1.Rows.Count - 1; i++) //Store all updated result from Data Table to List
                {
                    process.Add(Convert.ToString(dataGridView1.Rows[i].Cells[0].Value));
                    arrivalTime.Add(Convert.ToInt32(dataGridView1.Rows[i].Cells[1].Value));
                    burstTime.Add(Convert.ToInt32(dataGridView1.Rows[i].Cells[2].Value));
                    priority.Add(Convert.ToInt32(dataGridView1.Rows[i].Cells[3].Value));
                }

                //result.Add("Start");
                //resultBT.Add(0);

                if (arrivalTime[0] == 0) // check for first row
                {
                    bt = bt + burstTime[0];
                    result.Add("P" + process[0]);
                    resultBT.Add(burstTime[0]);
                    finalArrival.Add(arrivalTime[0]);
                    finalBurst.Add(bt);

                    process.RemoveAt(0);
                    arrivalTime.RemoveAt(0);
                    burstTime.RemoveAt(0);
                    priority.RemoveAt(0);
                }

                else
                {
                    bt = bt + arrivalTime[0];
                    result.Add("Idle");
                    resultBT.Add(arrivalTime[0]);
                    finalArrival.Add(arrivalTime[0]);
                    finalBurst.Add(bt);

                    bt = bt + burstTime[0];
                    result.Add("P" + process[0]);
                    resultBT.Add(burstTime[0]);
                    finalArrival.Add(arrivalTime[0]);
                    finalBurst.Add(bt);

                    process.RemoveAt(0);
                    arrivalTime.RemoveAt(0);
                    burstTime.RemoveAt(0);
                    priority.RemoveAt(0);
                }

                //lblResult.Text = process[0] + " " + arrivalTime[0] + " " + burstTime[0] + " " + priority[0];

                while (process.Count != 1)
                {
                    for (int i = 0; i < process.Count; i++) //check for second row
                    {
                        if (arrivalTime[i] <= bt)
                        {
                            comparePriority.Add(priority[i]);
                            comparePriority.Sort();
                        }
                    }
                    if (comparePriority.Count == 0)
                    {
                        resultBT.Add(arrivalTime[0] - bt);
                        bt = bt + (arrivalTime[0] - bt);
                        result.Add("Idle");
                        //resultBT.Add(arrivalTime[0]);
                        finalArrival.Add(arrivalTime[0]);
                        finalBurst.Add(bt);

                        bt = bt + burstTime[0];
                        result.Add("P" + process[0]);
                        resultBT.Add(burstTime[0]);
                        finalArrival.Add(arrivalTime[0]);
                        finalBurst.Add(bt);

                        process.RemoveAt(0);
                        arrivalTime.RemoveAt(0);
                        burstTime.RemoveAt(0);
                        priority.RemoveAt(0);

                    }
                    else if (comparePriority.Count > 0)
                    {
                        for (int k = (process.Count - 1); k >= 0; k--)
                        {
                            if (priority[k] == comparePriority.Min())
                            {
                                Process = process[k];
                            }
                        }

                        for (int l = 0; l < process.Count; l++)
                        {
                            if (process[l] == Process)
                            {
                                bt = bt + burstTime[l];
                                result.Add("P" + process[l]);
                                resultBT.Add(burstTime[l]);
                                finalArrival.Add(arrivalTime[l]);
                                finalBurst.Add(bt);

                                process.RemoveAt(l);
                                arrivalTime.RemoveAt(l);
                                burstTime.RemoveAt(l);
                                priority.RemoveAt(l);
                            }
                        }

                        comparePriority.Clear();
                    }
                }

                if (process.Count == 1)
                {
                    if (arrivalTime[0] <= bt)
                    {
                        bt = bt + burstTime[0];
                        result.Add("P" + process[0]);
                        resultBT.Add(burstTime[0]);
                        finalArrival.Add(arrivalTime[0]);
                        finalBurst.Add(bt);

                        process.RemoveAt(0);
                        arrivalTime.RemoveAt(0);
                        burstTime.RemoveAt(0);
                        priority.RemoveAt(0);
                    }

                    else if (arrivalTime[0] > bt)
                    {
                        resultBT.Add(arrivalTime[0] - bt);
                        bt = bt + (arrivalTime[0] - bt);
                        result.Add("Idle");
                        //resultBT.Add(arrivalTime[0]);
                        finalArrival.Add(arrivalTime[0]);
                        finalBurst.Add(bt);

                        bt = bt + burstTime[0];
                        result.Add("P" + process[0]);
                        resultBT.Add(burstTime[0]);
                        finalArrival.Add(arrivalTime[0]);
                        finalBurst.Add(bt);

                        process.RemoveAt(0);
                        arrivalTime.RemoveAt(0);
                        burstTime.RemoveAt(0);
                        priority.RemoveAt(0);
                    }
                }
            }
            /*
                        for (int n = 0; n < result.Count; n++)
                        {
                            lblResult.Text += result[n] + " ";
                            lblResult2.Text += resultBT[n] + " ";
                            lblResult3.Text += finalArrival[n] + " ";
                            lblResult4.Text += finalBurst[n] + " ";
                        }
            */

            for (int z = 0; z < result.Count; z++)
            {
                resultProcess.Add(result[z]);
                resultBurst.Add(finalBurst[z]);
            }

            chtPriority.ChartAreas[0].AxisY.Minimum = 0; // Set 
            chtPriority.ChartAreas[0].AxisY.Maximum = bt;
            chtPriority.ChartAreas[0].AxisY.Interval = 1;

            for (int x = 0; x < result.Count; x++)
            {
                chtPriority.Series.Add(result[x]);
                chtPriority.Series[result[x]].Label = result[x];
                chtPriority.Series[result[x]].ChartType = SeriesChartType.StackedBar;
                chtPriority.Series[result[x]].Points.AddY(resultBT[x]);
                chtPriority.Series[result[x]].ChartArea = "ChartArea1";
            }

            List<double> turnArroundTime = new List<double>();
            List<double> waitingTime = new List<double>();


            for (int p = 0; p < result.Count; p++)
            {
                if (result[p] == "Idle")
                {
                    result.RemoveAt(p);
                    resultBT.RemoveAt(p);
                    finalBurst.RemoveAt(p);
                    finalArrival.RemoveAt(p);
                }
            }

            for (int l = 0; l < result.Count; l++)
            {
                turnArroundTime.Add((finalBurst[l] - finalArrival[l]));
            }

            for (int q = 0; q < result.Count; q++)
            {
                waitingTime.Add((turnArroundTime[q] - resultBT[q]));
            }

            averageTAT = Convert.ToDouble(turnArroundTime.Sum() / (result.Count));     //-totalProcess));
            averageWT = Convert.ToDouble(waitingTime.Sum() / (result.Count));        //- totalProcess));
            lblResult.Text = "Avg Turnarround Time: " + string.Format("{0:0.00}", averageTAT) + "ms";
            lblResult2.Text = "Avg Waiting Time: " + string.Format("{0:0.00}", averageWT) + "ms";

        }

        private void panelNPP_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnMMBF_Click(object sender, EventArgs e)
        {
            mainResetPanel();
            panelMainMemory.Visible = true;

            processQueueJY1.DataSource = null;
            processQueueJY1.Rows.Clear();
            processQueueJY1.Columns.Clear();
            processQueueJY1.Refresh();
            GridViewMemJY2.DataSource = null;
            GridViewMemJY2.Rows.Clear();
            GridViewMemJY2.Columns.Clear();
            GridViewMemJY2.Refresh();
            processQueueJY2.DataSource = null;
            processQueueJY2.Rows.Clear();
            processQueueJY2.Columns.Clear();
            processQueueJY2.Refresh();
            //dataGridView1.Columns[0].Name = "OrderNo";
            // dataGridView1.Columns[1].Name = "ProcessSize";
            
            //CY codes
            GridViewMemJY2.Visible = false;
            processQueueJY2.Visible = false;
            //CY

            processQueueJY1.Columns.Add("OrderNo", "OrderNo");
            processQueueJY1.Columns.Add("ProcessSize", "ProcessSize");
            GridViewMemJY2.Columns.Add("", "");
            processQueueJY2.Columns.Add("", "");
            orderNoTxtBox.Text = "1";
            orderNoTxtBox.ReadOnly = true;
        }

        private void returnMP_Click(object sender, EventArgs e)
        {
            mainResetPanel();
            mainPanel.Visible = true;
        }

        private void addMemBlock_Click(object sender, EventArgs e)
        {

            if(newMemBlk.Text=="")
            { return; }
            string newMemBlkInputJY = newMemBlk.Text;
            CYTotalCounter += Convert.ToInt32(newMemBlk.Text);
            CYTotalCounter2++;
            blocksarr[CYTotalCounter2-1] = Convert.ToInt32(newMemBlk.Text);
       
            MMMBlockchart.Series.Clear(); // clear previous chart if any.
            MMMBlockchart.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
            MMMBlockchart.ChartAreas[0].AxisY.LabelStyle.Enabled = false;
            MMMBlockchart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            MMMBlockchart.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            MMMBlockchart.ChartAreas[0].AxisX.Minimum = 0; //start from 0
            MMMBlockchart.ChartAreas[0].AxisY.Maximum = CYTotalCounter; //total memory;
            //MMMBlockchart.ChartAreas[0].AxisX.Interval = 100;

            for (int t = 0; t < CYTotalCounter2; t++)
            {
                MMMBlockchart.Series.Add(t.ToString()); //add new series
                MMMBlockchart.Series[t.ToString()].Label = blocksarr[t].ToString(); //set legend name
                MMMBlockchart.Series[t.ToString()].ChartType = SeriesChartType.StackedColumn; //set into stacked bar.
                MMMBlockchart.Series[t.ToString()].Points.AddXY(blocksarr[t].ToString(), blocksarr[t].ToString()); //set value for ending 
                MMMBlockchart.Series[t.ToString()].ChartArea = "ChartArea1";    
            }
            
            GridViewMemJY2.Rows.Add(newMemBlkInputJY);
            if ((Int32.Parse(newMemBlkInputJY)) > 19)
            {
                GridViewMemJY2.Rows[dgx2JY].Height = Int32.Parse(newMemBlkInputJY) / 2 + Int32.Parse(newMemBlkInputJY);
            }
            else
                GridViewMemJY2.Rows[dgx2JY].Height = 20;
            dgx2JY++;

            newMemBlk.Text = "";
        }

        private void removeblk_Click(object sender, EventArgs e)
        {
            processQueueJY1.DataSource = null;
            processQueueJY1.Rows.Clear();
            processQueueJY1.Columns.Clear();
            processQueueJY1.Refresh();
            GridViewMemJY2.DataSource = null;
            GridViewMemJY2.Rows.Clear();
            GridViewMemJY2.Columns.Clear();
            GridViewMemJY2.Refresh();
            processQueueJY2.DataSource = null;
            processQueueJY2.Rows.Clear();
            processQueueJY2.Columns.Clear();
            processQueueJY2.Refresh();
            newMemBlk.Text = "";
            orderNoTxtBox.Text = "";
            processSizeTxtBox.Text = "";
            processQueueJY1.Columns.Add("OrderNo", "OrderNo");
            processQueueJY1.Columns.Add("ProcessSize", "ProcessSize");
            GridViewMemJY2.Columns.Add("", "");
            processQueueJY2.Columns.Add("", "");
            dgx2JY = 0;
            dgx3JY = 0;
            orderNoTxtBox.Text = "1";
            //
            internFrag.Text = "";
            extFrag.Text = "";
            unloadedBlk.Text = "";

            //CY
            CYTotalCounter = 0;
            CYTotalCounter2 = 0;
            CYTotalCounter3 = 0;
            CYTotalCounter4 = 0;
            MMDChart.Series.Clear();
            MMMBlockchart.Series.Clear();

        }

        private void addProcess_Click(object sender, EventArgs e)
        {
            if (processSizeTxtBox.Text=="")
            { return; }

            String orderNoInputJY = orderNoTxtBox.Text;
            String processSizeInputJY = processSizeTxtBox.Text;
            int noerrorJY = -1;
            int orderIncre = Int32.Parse(orderNoInputJY);


            CYTotalCounter3 += Convert.ToInt32(processSizeTxtBox.Text);
            CYTotalCounter4++;
            blocksarr2[CYTotalCounter4 - 1] = Convert.ToInt32(processSizeTxtBox.Text);

            MMDChart.Series.Clear(); // clear previous chart if any.
            MMDChart.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
            MMDChart.ChartAreas[0].AxisY.LabelStyle.Enabled = false;
            MMDChart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            MMDChart.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            MMDChart.ChartAreas[0].AxisX.Minimum = 0; //start from 0
            MMDChart.ChartAreas[0].AxisY.Maximum = CYTotalCounter3; //total memory;
            //MMMBlockchart.ChartAreas[0].AxisX.Interval = 100;

            for (int t = 0; t < CYTotalCounter4; t++)
            {
                MMDChart.Series.Add(t.ToString()); //add new series
                MMDChart.Series[t.ToString()].Label = blocksarr2[t].ToString(); //set legend name
                MMDChart.Series[t.ToString()].ChartType = SeriesChartType.StackedColumn; //set into stacked column
                MMDChart.Series[t.ToString()].Points.AddXY(blocksarr2[t].ToString(), blocksarr2[t].ToString()); //xy 
                MMDChart.Series[t.ToString()].ChartArea = "ChartArea1";
            }


            /*if (dataGridViewJY1.Rows.Count == 0)
            {
                dataGridViewJY1.Rows.Add(orderNoInputJY, processSizeInputJY);
                dataGridViewJY3.Rows.Add(processSizeInputJY);
            } 
            else
            { */
            for (int i = 0; i < processQueueJY1.RowCount; i++)
            {

                string cellTextJY = processQueueJY1.Rows[i].Cells[0].FormattedValue.ToString();
                if ((orderNoInputJY) == cellTextJY)
                {
                    errorJY.Visible = true;
                    noerrorJY = 1;
                    break;

                }
                else
                {
                    noerrorJY = 0;
                    errorJY.Visible = false;
                }

            }

            if (noerrorJY == 0)
            {
                processQueueJY1.Rows.Add(orderNoInputJY, processSizeInputJY);
                processQueueJY2.Rows.Add(processSizeInputJY);
                if ((Int32.Parse(processSizeInputJY)) > 19)
                {
                    processQueueJY2.Rows[dgx3JY].Height = Int32.Parse(processSizeInputJY) / 2 + Int32.Parse(processSizeInputJY);
                }
                else
                    processQueueJY2.Rows[dgx3JY].Height = 20;
                noerrorJY = -1;
                dgx3JY++;
                orderIncre++;
                orderNoTxtBox.Text = (orderIncre).ToString();
            }

            // }
            processSizeTxtBox.Clear();
        }

        private void startMM_Click(object sender, EventArgs e)
        {
            int totalFreeSpaceJY = 0;
            int totalSpaceUsedJY = 0;
            int internalFragmentationJY = 0;
            int externalFragmentationJY = 0;
            int iJY;
            int xJY = 0;
            int bJY = 0;
            int uneditIndex = 0;
            int bestFitComparisonJY = -1;
            int processIndexJY = 0;
            int unoccupiedMemoryJY;
            int extFragoccurJY = 0;
            int bestfitmemoryIndexJY = -1;
            int memoryValueJY = 0;
            int processValueJY = 0;
            string unprocessedJY = "";
            string unloadedProcessJY = "";
            string extFragDispJY = "";

            //  List<int> uneditedmemoryIndexJY = new List<int>();
            //List<string> originalprocessJY = new List<string>();
            List<string> internalmemoryJY = new List<string>();
            List<int> indexmemEditJY = new List<int>();

            for (int JY = 0; JY < GridViewMemJY2.RowCount; JY++)
            {
                internalmemoryJY.Add(GridViewMemJY2.Rows[JY].Cells[0].FormattedValue.ToString());
                //originalmemoryJY.Add(GridViewMemJY2.Rows[JY].Cells[0].FormattedValue.ToString());
            }
            for (xJY = 0; xJY < processQueueJY2.RowCount; xJY++)
            {
                for (int JY = 0; JY < GridViewMemJY2.RowCount; JY++)
                {
                    memoryValueJY = Int32.Parse(internalmemoryJY[JY]);
                    processValueJY = Int32.Parse(processQueueJY2.Rows[xJY].Cells[0].FormattedValue.ToString());
                    if (memoryValueJY >= processValueJY && bestFitComparisonJY == -1)
                    {
                        bestFitComparisonJY = memoryValueJY;
                        bestfitmemoryIndexJY = JY;
                    }
                    else if (memoryValueJY > processValueJY && bestFitComparisonJY > memoryValueJY)
                    {
                        bestFitComparisonJY = memoryValueJY;
                        bestfitmemoryIndexJY = JY;
                    }

                }
                if (bestFitComparisonJY != -1)
                {
                    memoryValueJY = Int32.Parse(internalmemoryJY[bestfitmemoryIndexJY]);
                    processValueJY = Int32.Parse(processQueueJY2.Rows[xJY].Cells[0].FormattedValue.ToString());
                    unoccupiedMemoryJY = memoryValueJY - processValueJY;
                    internalmemoryJY[bestfitmemoryIndexJY] = unoccupiedMemoryJY.ToString();
                    indexmemEditJY.Add(bestfitmemoryIndexJY);
                    bestfitmemoryIndexJY = -1;
                    bestFitComparisonJY = -1;
                }
                else
                {
                    unprocessedJY += "P" + (xJY + 1) + " ";
                    extFragoccurJY = 1;
                }

            }
            for (int aJY = 0; aJY < indexmemEditJY.Count; aJY++)
            {
                internalFragmentationJY += Int32.Parse(internalmemoryJY[indexmemEditJY[aJY]]);
            }
            internFrag.Text = internalFragmentationJY.ToString();
            internFrag.Visible = true;
            if (extFragoccurJY == 1)
            {
                /* for (int bJY = 0; bJY < indexmemEditJY.Count; bJY++)
                 {
                     externalFragmentationJY += Int32.Parse(internalmemoryJY[uneditedmemoryIndexJY[bJY]]);
                 } */
                while (bJY < internalmemoryJY.Count)
                {
                    for (int cJY = 0; cJY < indexmemEditJY.Count; cJY++)
                    {
                        if (bJY == indexmemEditJY[cJY])
                        {
                            bJY++;
                            uneditIndex = 1;
                            break;
                        }
                    }


                    if (uneditIndex == 0)
                    {
                        externalFragmentationJY += Int32.Parse(internalmemoryJY[bJY]);
                        bJY++;
                    }
                    uneditIndex = 0;
                }
                extFrag.Text = externalFragmentationJY.ToString();
                extFrag.Visible = true;
                unloadedBlk.Text = unprocessedJY;
                unloadedBlk.Visible = true;
            }
        }

        private void newMemBlk_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Clearance
            csdataGridView1.Rows.Clear();
            csdataGridView1.Columns.Clear();
            csdataGridView1.Refresh();
            txtDatabits.Text = "";
            txtFrame.Text = "";
            addcell = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int row = csdataGridView1.CurrentCell.RowIndex;
            csdataGridView1.Rows.RemoveAt(row);
            addcell--;
        }

        private void btnFIFO_Click(object sender, EventArgs e)
        {

        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    sw.WriteLine("Date:" + "," + DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year);
                    sw.WriteLine("Time" + "," + DateTime.Now.Hour + " Hrs :" + DateTime.Now.Minute + " Min :" + DateTime.Now.Second + " Sec");
                    sw.WriteLine("" + "" + "");

                    sw.WriteLine("Process" + "," + "Arrival Time" + "," + "Burst Time" + "," + "Priority");

                    for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                    {
                        sw.WriteLine(dataGridView1.Rows[i].Cells[0].Value + "," + dataGridView1.Rows[i].Cells[1].Value + "," +
                            dataGridView1.Rows[i].Cells[2].Value + "," + dataGridView1.Rows[i].Cells[3].Value);
                    }

                    sw.WriteLine("" + "" + "");
                    sw.WriteLine("Result");

                    for (int j = 0; j < resultProcess.Count; j++)
                    {
                        sw.WriteLine(resultProcess[j] + "," + resultBurst[j]);
                    }

                    sw.WriteLine("" + "" + "");
                    sw.WriteLine("Avg TAT:" + "," + averageTAT + "ms");
                    sw.WriteLine("Avg WT:" + "," + averageWT + "ms");

                }

                MessageBox.Show("Result has been Successfully Saved!");
                txtSave.Text = "";
            }
            catch
            {
                MessageBox.Show("Error Detected! Please select a File Location for File to Save." + "\n" +
                    "Do NOT Leave Blank!");
                return;
            }

        }


        private static void FIFO(int numFrames, int[] refString, int[] frame)
        {
            int i, j = 0, k, flag = 0;
            fault = 0;

            for (i = 0; i < dataInString.Length; i++)
            {
                for (k = 0; k < numFrames; k++)
                {
                    if (frame[k] == refString[i])
                    {
                        flag = 1;
                    }

                }

                if (flag == 0)
                {
                    frame[j] = refString[i];

                    if (j < numFrames)
                    {
                        frame[j++] = refString[i];
                        fault++;

                    }
                }

                else
                {
                    flag = 0;
                }

                if (j == numFrames)
                {
                    j = 0;
                }

            }
            Console.WriteLine("\nThe number of page faults with FIFO is: " + fault);
        }

        private static int LRU(int numframes, int[] refString, int[] frames)
        {

            int top = 0, fault = 0;
            int[] count = new int[numframes];

            for (int i = 0; i < refString.Length; i++)
            {

                int k = findmax(refString[i], frames, count, top, numframes);

                if (k < 0)
                {
                    count[top] = 0;
                    frames[top++] = refString[i];
                    fault++;
                }

                else if (frames[k] != refString[i])
                {

                    count[k] = 0;
                    frames[k] = refString[i];
                    fault++;

                }
                else count[k] = 0;

                for (int j = 0; j < top; j++)
                {
                    count[j]++;

                }

            }

            return (fault);

        }

        private void btnBacktomain_Click(object sender, EventArgs e)
        {
            //mainResetPanel();
           // mainPanel.Visible = true;
        }

        private void btnVM_Click(object sender, EventArgs e)
        {
            mainResetPanel();
            panelVM.Visible = true;
        }

        private void btnBackVM_Click(object sender, EventArgs e)
        {
            mainResetPanel();
            mainPanel.Visible = true;
        }

        private void btnFIFO_Click_1(object sender, EventArgs e)
        {

            List<int> databits = new List<int>();
            dataFrame = Convert.ToInt16(txtFrame.Text);
            dataInString = txtDatabits.Text;
            for (int csI = 0; csI < dataInString.Length; csI++)
            {
                databits.Add((int)(dataInString[csI]));

            }
            int[] frame = new int[7];
            FIFO(dataFrame, databits.ToArray(), frame);
            double prob = ((float)fault / (float)dataInString.Length) * 100;

            lblProbability.Text = "" + string.Format("{0:0.00}", prob);
            lblPageFault.Text = " " + fault;

            csdataGridView1.Rows.Add();
            csdataGridView1.Rows[addcell].Cells[0].Value = dataFrame;
            csdataGridView1.Rows[addcell].Cells[1].Value = dataInString;
            csdataGridView1.Rows[addcell].Cells[2].Value = fault;
            csdataGridView1.Rows[addcell].Cells[3].Value = string.Format("{0:0.00}", prob);
            csdataGridView1.Rows[addcell].Cells[4].Value = "FIFO";
            addcell++;


        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            csdataGridView1.Rows.Clear();

            txtDatabits.Text = "";
            txtFrame.Text = "";
            lblProbability.Text = "";
            lblPageFault.Text = "";
            addcell = 0;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            int row = csdataGridView1.CurrentCell.RowIndex;
            csdataGridView1.Rows.RemoveAt(row);
            addcell--;
        }

        private void btnSubmit_Click_1(object sender, EventArgs e)
        {
            try
            {
                List<int> databits = new List<int>();
                dataFrame = Convert.ToInt16(txtFrame.Text);
                dataInString = txtDatabits.Text;
                for (int csI = 0; csI < dataInString.Length; csI++)
                {
                    databits.Add((int)(dataInString[csI]));

                }

                int[] frame = new int[7];

                fault = LRU(dataFrame, databits.ToArray(), frame);

                lblPageFault.Text = " " + fault;

                double prob = ((float)fault / (float)dataInString.Length) * 100;
                lblProbability.Text = "" + string.Format("{0:0.00}", prob);

                csdataGridView1.Rows.Add();
                csdataGridView1.Rows[addcell].Cells[0].Value = dataFrame;
                csdataGridView1.Rows[addcell].Cells[1].Value = dataInString;
                csdataGridView1.Rows[addcell].Cells[2].Value = fault;
                csdataGridView1.Rows[addcell].Cells[3].Value = string.Format("{0:0.00}", prob);
                csdataGridView1.Rows[addcell].Cells[4].Value = "LRU";

                addcell++;

            }



            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void label23_Click(object sender, EventArgs e)
        {

        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void unloadedBlk_Click(object sender, EventArgs e)
        {

        }

        private void extFrag_Click(object sender, EventArgs e)
        {

        }

        private void internFrag_Click(object sender, EventArgs e)
        {

        }

        private void lblResult_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private static int findmax(int keyframe, int[] frames, int[] count, int top, int numframes)
        {

            int max = 0;

            for (int i = 0; i < top; i++)
            {

                if (frames[i] == keyframe)
                {

                    return (i);
                }
                if (count[max] < count[i])
                    max = i;

            }

            if (top < numframes)
                return (-1);
            return (max);
        }
        //Save Code//
    }
}

       

