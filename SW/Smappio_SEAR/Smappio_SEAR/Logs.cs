using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Smappio_SEAR
{
    public partial class Logs : Form
    {
        public Logs(List<Log> logs)
        {
            InitializeComponent();

            this._Logs = logs;
        }

        public List<Log> _Logs { get; }        

        private void Logs_Load(object sender, EventArgs e)
        {
            foreach (var log in _Logs)
            {
                lstLogs.Items.Add($"Id: {log.Id} -  Action: {log.LogAudit.ToString()}");
            }
        }
    }
}
