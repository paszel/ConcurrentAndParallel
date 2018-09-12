using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var jsonTask = GetJsonAsync(new Uri("https://postman-echo.com/get"));
            textBox1.Text = jsonTask.Result.ToString();
        }

        public static async Task<string> GetJsonAsync(Uri uri)
        {
            using (var client = new HttpClient())
            {
                /*
                 * ConfigureAwait(false) won't deadlock UI thread, because using default ThreadPool
                 * instead of getting back to UI thread which waits for .Result.ToString() (1st await)
                 */

                //2nd await
                var jsonString = await client.GetStringAsync(uri).ConfigureAwait(false);
                return jsonString;
            }
        }
    }
}
