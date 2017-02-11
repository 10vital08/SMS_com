using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace SMS_com
{
    public partial class Form1 : Form
    {
        static SerialPort _serialPort;
        string[] myPort; //массив для COM-портов

        public Form1()
        {
            InitializeComponent();
            _serialPort = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
            //_serialPort = SerialPort();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            Form1_Load(sender, e);//вызов функции для обновления списка доступных COM-портов
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            myPort = SerialPort.GetPortNames();//в массив помещаются доступные порты
            comboBox1.Items.AddRange(myPort);//теперь этот массив заносится в список(comboBox)
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //foreach (string port in myPort)
            //{
            //    _serialPort = new SerialPort(port);
            //}

            _serialPort.Open();
            if (_serialPort.IsOpen)
            {
                MessageBox.Show("Открыто");
            }
            _serialPort.Close();//закрываю порт, чтобы по следующему клику проверить его открытие
        }
    }
}
