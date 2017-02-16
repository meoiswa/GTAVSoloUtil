using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GTAVSoloUtil
{
    public partial class Form1 : Form
    {
        KeyHandler ghk;

        string processName = "GTA5";

        Process process;
        Status status = Status.zero;

        enum Status
        {
            zero,
            search,
            ready,
            suspend,
            waiting,
            error
        }

        public Form1()
        {
            InitializeComponent();
            ghk = new GTAVSoloUtil.KeyHandler(Keys.Pause, this);
            ghk.Register();
        }

        private delegate bool StateChecker();

        private void Exit()
        {
            backgroundWorker.CancelAsync();
            while (backgroundWorker.IsBusy)
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }

            Application.Exit();
        }

        private void Maximize()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                notifyIcon1.ShowBalloonTip(0);
                Hide();
            }
        }

        private void HandleHotkey()
        {
            if (status == Status.ready)
            {
                status = Status.suspend;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Constants.WM_HOTKEY_MSG_ID)
                HandleHotkey();
            base.WndProc(ref m);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            BackgroundWorker worker = (BackgroundWorker) sender;

            int sleepTime = 0;

            while(!worker.CancellationPending)
            {
                if (process == null || process.HasExited)
                {
                    status = Status.search;
                    process = Process.GetProcessesByName(processName).FirstOrDefault();
                }
                else
                {
                    if (status == Status.search)
                    {
                        status = Status.ready;
                    }
                    else if (status == Status.suspend)
                    {
                        process.Suspend();
                        status = Status.waiting;
                        sleepTime = 0;
                    }
                    else if (status == Status.waiting)
                    {
                        if (sleepTime < 100)
                        {
                            sleepTime += 1;
                        }
                        else
                        {
                            process.Resume();
                            status = Status.ready;
                        }
                    }
                    
                }
                worker.ReportProgress(sleepTime, status);
                Thread.Sleep(100);
            }
            if (process != null && !process.HasExited)
            {
                process.Resume();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            backgroundWorker.RunWorkerAsync();
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string text = "Error";
            switch ((Status) e.UserState)
            {
                case Status.zero:
                    text = "Idle";
                    buttonSuspend.Enabled = false;
                    break;
                case Status.search:
                    text = "Searching";
                    buttonSuspend.Enabled = false;
                    break;
                case Status.ready:
                    text = "Ready";
                    buttonSuspend.Enabled = true;
                    break;
                case Status.suspend:
                    text = "Suspending";
                    buttonSuspend.Enabled = false;
                    break;
                case Status.waiting:
                    text = "Waiting ("+e.ProgressPercentage/10+")";
                    buttonSuspend.Enabled = false;
                    break;
            }
            labelStatus.Text = text;
        }

        private void buttonSuspend_Click(object sender, EventArgs e)
        {
            if (status == Status.ready)
            {
                status = Status.suspend;
            }
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Maximize();
        }

        private void maximizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Maximize();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Exit();
        }
    }
}
