using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
using System;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Forms
{
    public partial class DebugForm : Form
    {
        private readonly Client[] _clients;

        public DebugForm(Client[] clients)
        {
            _clients = clients;

            InitializeComponent();
        }

        private void btnSendEmptyMessage_Click(object sender, EventArgs e)
        {
            foreach (Client c in _clients)
            {
                c.Send(new TestEmptyMessage());
            }
        }

        private void btnTestMessage_Click(object sender, EventArgs e)
        {
            foreach (Client c in _clients)
            {
                c.Send(new TestMessage 
                {
                    String1 = "Server Ping",
                    String2 = ""
                });
            }
        }
    }
}
