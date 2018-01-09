﻿using Sdl.Community.TMLifting.Helpers;
using Sdl.Community.TMLifting.TranslationMemory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using Sdl.Community.GroupShareKit;
using System.Threading.Tasks;
using Sdl.Community.GroupShareKit.Clients;
using System.Reflection;
using System.Net.Http;
using Newtonsoft.Json;
using Sdl.Community.GroupShareKit.Models.Response.TranslationMemory;
using System.Collections.ObjectModel;
using Sdl.LanguagePlatform.TranslationMemory;

namespace Sdl.Community.TMLifting
{
	public delegate void AddServerBasedTMsDetails(string userName, string password, string uri);
    public partial class TMLiftingForm : UserControl
    {
        private readonly TranslationMemoryHelper _tmHelper;
        private readonly BackgroundWorker _bw;
		private readonly BackgroundWorker _bwGS;
        private readonly Stopwatch _stopWatch;
        private readonly StringBuilder _elapsedTime;
		private TranslationMemory.ServerBasedTranslationMemoryGSKit _sbTMs;
		private UserCredentials _userCredentials;
		private AlertForm _alert;
		private LoginPage _currentInstance = null;
		private TranslationProviderServer _server;
		private ReadOnlyCollection<ServerBasedTranslationMemory> SBTMs;

		public TMLiftingForm()
        {
            InitializeComponent();
            _tmHelper = new TranslationMemoryHelper();
            _stopWatch = new Stopwatch();
            _elapsedTime = new StringBuilder();
            _bw = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
			_bwGS = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
			_sbTMs = new TranslationMemory.ServerBasedTranslationMemoryGSKit();
			_userCredentials = new UserCredentials();
		}

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _bw.DoWork += bw_DoWork;
            _bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            _bw.ProgressChanged += bw_ProgressChanged;
            reIndexCheckBox.Checked = true;
			tabControlTMLifting.SelectedIndexChanged += TabControlTMLifting_SelectedIndexChanged;
			comboBoxServerBasedTM.DataSource = await _sbTMs.GetServers();
		}

		private async void TabControlTMLifting_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(tabControlTMLifting.SelectedTab == tabControlTMLifting.TabPages["tabPageServerBasedTM"])
			{
				if (comboBoxServerBasedTM.SelectedItem != null)
				{
					_userCredentials = _sbTMs.GetUserCredentials(comboBoxServerBasedTM.SelectedItem as Uri);
					if (_userCredentials.UserName != "N/A" && _userCredentials.Password != "N/A")
					{
						_sbTMs = await TranslationMemory.ServerBasedTranslationMemoryGSKit.CreateAsync(_userCredentials.UserName, _userCredentials.Password, comboBoxServerBasedTM.SelectedItem.ToString());

						gridServerBasedTMs.DataSource = _sbTMs.ServerBasedTMDetails.Select(tm => new
						{ Name = tm.Name, Description = tm.Description, Location = tm.Location }).ToList(); ;
						gridServerBasedTMs.Visible = true;
					}
					else if (_currentInstance == null && !gridServerBasedTMs.Visible)
					{
						_currentInstance = new LoginPage(comboBoxServerBasedTM.Text);
						_currentInstance._addDetailsCallback = new AddServerBasedTMsDetails(this.AddDetailsCallbackFn);
						_currentInstance.FormClosed += instanceHasBeenClosed;
						_currentInstance.Show();
					}
					else
					{
						if (_currentInstance != null && !gridServerBasedTMs.Visible)
						{
							_currentInstance = new LoginPage(comboBoxServerBasedTM.Text);
							_currentInstance.FormClosed += instanceHasBeenClosed;
							_currentInstance.BringToFront();
						}
						
					}
				}
			}	
		}
		private void instanceHasBeenClosed(object sender, FormClosedEventArgs e)
		{
			_currentInstance = null;
		}
		private async void AddDetailsCallbackFn(string userName, string password, string uri)
		{
			_sbTMs = await ServerBasedTranslationMemoryGSKit.CreateAsync(userName, password, uri);
			gridServerBasedTMs.DataSource = _sbTMs.ServerBasedTMDetails;
			for (int i = 0; i < gridServerBasedTMs.Columns.Count; i++)
			{
				gridServerBasedTMs.Columns[i].Visible = false;
			}
			gridServerBasedTMs.Columns["Name"].Visible = true;
			gridServerBasedTMs.Columns["Description"].Visible = true;
			gridServerBasedTMs.Columns["CreatedOn"].Visible = true;
			gridServerBasedTMs.Columns["Location"].Visible = true;
			gridServerBasedTMs.Columns["ShouldRecomputeStatistics"].Visible = true;
			gridServerBasedTMs.Columns["LastReIndexDate"].Visible = true;
			gridServerBasedTMs.Columns["LastReIndexSize"].Visible = true;
			gridServerBasedTMs.Columns.Add("Status", "Status");
			//gridServerBasedTMs.DataSource = _sbTMs.ServerBasedTMDetails.Select(tm => new
			//{ Name = tm.Name, Description = tm.Description, Location = tm.Location, TranslationMemoryId = tm.TranslationMemoryId }).ToList();

			//_server = new TranslationProviderServer(new Uri(uri), false, userName, password);
			//SBTMs = _server.GetTranslationMemories(0);
			//gridServerBasedTMs.DataSource = SBTMs.Select(tm => new
			//{ Name = tm.Name, Description = tm.Description, TranslationMemoryId = tm.Id }).ToList();

			gridServerBasedTMs.ReadOnly = true;
			gridServerBasedTMs.Visible = true;
		}

		void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //rtbStatus.Text = e.UserState.ToString();
			_alert.Message = "In progress, please wait... "/* + e.ProgressPercentage.ToString() + "%"*/;
			_alert.ProgressValue = e.ProgressPercentage;
		}

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                btnReindex.Enabled = true;
                _stopWatch.Stop();
                string elapsedTime;
                var timeSpan = _stopWatch.Elapsed;
                if (timeSpan.Hours != 00)
                {
                    elapsedTime = " Process time " +
                                  $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}: {timeSpan.Seconds:00}.{timeSpan.Milliseconds / 10:00} hours.";
                }
                else
                {
                    elapsedTime = " Process time " +
                                  $"{timeSpan.Minutes:00}: {timeSpan.Seconds:00}.{timeSpan.Milliseconds / 10:00} minutes.";
                }

                _elapsedTime.Append("Process time:" + elapsedTime);
                rtbStatus.AppendText(elapsedTime);
            }
            else
            {
                _stopWatch.Stop();
                rtbStatus.AppendText("Process canceled.");
            }
			_alert.Close();
		}

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            _stopWatch.Start();
            var tms = e.Argument as List<TranslationMemoryInfo>;

            if (tms == null) return;

            _tmHelper.Process(tms, _bw, reIndexCheckBox.Checked, upLiftCheckBox.Checked);
            var bw = sender as BackgroundWorker;
            if (bw != null && bw.CancellationPending)
            {
                e.Cancel = true;
                btnReindex.Enabled = true;
            }
        }

        private async void btnReindex_Click(object sender, EventArgs e)
        {
			// create a new instance of the alert form	
			//_alert = new AlertForm();
			//_alert.Show();
			if (tabControlTMLifting.SelectedTab == tabControlTMLifting.TabPages["tabPageFileBasedTM"])
			{
				_alert = new AlertForm();
				_alert.Show();
				_alert.progressBar1.Style = ProgressBarStyle.Marquee;
				btnReindex.Enabled = false;

				var tms = lstTms.Items.OfType<TranslationMemoryInfo>().ToList();

				_bw.RunWorkerAsync(tms);
			}
			else
			{
				//var selectedRowIndex = gridServerBasedTMs.SelectedCells[0].RowIndex;
				//var selectedRow = gridServerBasedTMs.Rows[selectedRowIndex].Cells["TranslationMemoryId"].Value.ToString();
				//var selectedRowId = gridServerBasedTMs.Rows[selectedRowIndex].Cells["TranslationMemoryId"].Value.ToString();
				//var tm = _server.GetTranslationMemory(new Guid(selectedRow), 0);
				//var isReindexed = tm.ShouldRecomputeFuzzyIndexStatistics();
				//var a = tm.IsProjectTranslationMemory;
				//var c = tm.Uri;
				//var b = ServerBasedTranslationMemory.GetServerBasedTranslationMemoryPath(c);
				//var result = ServerBasedTranslationMemory.IsServerBasedTranslationMemory(c);
				//tm.RecomputeFuzzyIndexStatistics();

				var selectedRowIndex = gridServerBasedTMs.SelectedCells[0].RowIndex;
				var selectedRow = gridServerBasedTMs.Rows[selectedRowIndex].DataBoundItem as TranslationMemoryDetails;
				//alert.progressBar1.Style = ProgressBarStyle.Marquee;
				//btnReindex.Enabled = false;
				//_alert.progressBar1.Style = ProgressBarStyle.Marquee;
				var x = await _sbTMs.GroupShareClient.TranslationMemories.Reindex(selectedRow.TranslationMemoryId, new FuzzyRequest());
				gridServerBasedTMs.Rows[selectedRowIndex].Cells["Status"].Value = x.Status;
				var selectedTMId = selectedRow.TranslationMemoryId;
				//while (!_sbTMs.GroupShareClient.TranslationMemories.Reindex(selectedRow.TranslationMemoryId, new FuzzyRequest()).IsCompleted)
				//{

				//}
				//while (x.Statistics == null)
				//{

				//}
			}

        }

        private void chkLoadStudioTMs_CheckedChanged(object sender, EventArgs e)
        {
            if (chkLoadStudioTMs.Checked)
            {
                var tms = _tmHelper.LoadLocalUserTms();

                foreach (var tm in tms)
                {
                    lstTms.Items.Add(tm);
                }
            }
            else
            {
                var toRemoveItems = (from object item in lstTms.Items
                                     let tmInfo = item as TranslationMemoryInfo
                                     where tmInfo.IsStudioTm
                                     select item).ToList();

                foreach (var item in toRemoveItems)
                {
                    lstTms.Items.Remove(item);
                }
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var folderDialog = new FolderSelectDialog();

            if (folderDialog.ShowDialog())
            {
                List<TranslationMemoryInfo> tms = _tmHelper.LoadTmsFromPath(folderDialog.FileName);
                foreach (var tm in tms)
                {
                    lstTms.Items.Add(tm);
                }
            }
        }

        private void lstTms_DragOver(object sender, DragEventArgs e)
        {
            //// Determine whether string data exists in the drop data. If not, then 
            //// the drop effect reflects that the drop cannot occur. 
            if (!e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            object data = e.Data.GetData(DataFormats.FileDrop, false);

            if (data == null)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            var paths = data as string[];

            if (paths == null)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Copy;
        }

        private void lstTms_DragDrop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.FileDrop, false);

            if (data == null) return;

            var paths = data as string[];

            if (paths == null) return;

            List<TranslationMemoryInfo> tms = _tmHelper.LoadTmsFromPath(paths);
            foreach (var tm in tms)
            {
                lstTms.Items.Add(tm);
            }
        }

        private void cleanBtn_Click(object sender, EventArgs e)
        {
			if (tabControlTMLifting.SelectedTab == tabControlTMLifting.TabPages["tabPageServerBasedTM"])
			{
				//gridServerBasedTMs.Refresh();
				_currentInstance = new LoginPage(comboBoxServerBasedTM.Text);
				_currentInstance._addDetailsCallback = new AddServerBasedTMsDetails(this.AddDetailsCallbackFn);
			}
			else
			{
				lstTms.Items.Clear();
				rtbStatus.Text = string.Empty;
			}
			
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            _bw.CancelAsync();
            rtbStatus.AppendText(@"Process will be canceled");
        }

		//private async void btnOkServerBased_Click(object sender, EventArgs e)
		//{
		//	//var a = StudioPlatform.Studio.ActiveWindow.ServiceContext.GetService<IServerConnectionService>().GetUserCredentials(new Uri("http://gs2017dev.sdl.com"), false);
		//	var servers = await _sbTMs.GetServers();

		//	var serverString = servers.First().ToString();

		//	var userCredentials = _sbTMs.GetUserCredentials(servers.First());

		//	//_sbTMs = await TranslationMemory.ServerBasedTranslationMemory.CreateAsync(userNameTxtBox.Text, passwordTxtBox.Text, serverNameTxtBox.Text);

		//	_sbTMs = await TranslationMemory.ServerBasedTranslationMemory.CreateAsync(userCredentials.UserName, userCredentials.Password, serverString);
			
		//	gridServerBasedTMs.DataSource = _sbTMs.ServerBasedTMDetails;
		//	gridServerBasedTMs.Visible = true;
		//}

		private void connectToServer_Click(object sender, EventArgs e)
		{
			//var form = new LoginPage(comboBoxServerBasedTM.Text);
			//form._addDetailsCallback = new AddServerBasedTMsDetails(this.AddDetailsCallbackFn);
			//form.ShowDialog();
			if (_currentInstance == null)
			{
				_currentInstance = new LoginPage(comboBoxServerBasedTM.Text);
				_currentInstance._addDetailsCallback = new AddServerBasedTMsDetails(this.AddDetailsCallbackFn);
				_currentInstance.FormClosed += instanceHasBeenClosed;
				_currentInstance.Show();
			}
			else
			{
				if (_currentInstance != null)
				{
					_currentInstance = new LoginPage(comboBoxServerBasedTM.Text);
					_currentInstance.FormClosed += instanceHasBeenClosed;
					_currentInstance.BringToFront();
				}
				
			}
		}
	}
}