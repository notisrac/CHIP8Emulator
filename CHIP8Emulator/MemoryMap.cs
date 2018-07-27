using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CHIP8Emulator
{
    public partial class MemoryMap : Form
    {
        public MemoryMap()
        {
            InitializeComponent();
            //Random rnd = new Random();
            //byte[] baTMP = new byte[4096];
            //for (int i = 0; i < baTMP.Length; i++)
            //{
            //    baTMP[i] = (byte)rnd.Next(255);
            //}
            //UpdateTable(baTMP);
        }

        public void UpdateTable(byte[] data)
        {
            DataTable dt = new DataTable("Memory");
            foreach (DataGridViewColumn dgColumn in dgMemoryGridView.Columns)
            {
                dt.Columns.Add(dgColumn.Name);
                dgColumn.DataPropertyName = dgColumn.Name;
            }

            int iCount = 0;
            while (iCount < data.Length)
            {
                List<object> olRow = new List<object>();
                int iRowNumber = (iCount / 16) * 16;
                olRow.Add(string.Format("0x{0:X4}", iRowNumber));
                for (int i = 0; i < 16; i++)
                {
                    olRow.Add(string.Format("{0:X2}", data[iCount + i]));
                }
                string sCombined = Encoding.ASCII.GetString(data, iCount, 16);
                olRow.Add(sCombined);
                
                iCount += 16;

                DataRow dr = dt.NewRow();
                dr.ItemArray = olRow.ToArray();
                dt.Rows.Add(dr);
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(dt);
            dgMemoryGridView.DataSource = ds;
            dgMemoryGridView.DataMember = "Memory";
            
        }
    }
}
