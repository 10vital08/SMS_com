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
        SerialPort _serialPort;
        string[] myPort; //массив для COM-портов
        char[] textPort;
        string text;

        public Form1()
        {
            InitializeComponent();
            _serialPort = new SerialPort("COM3");
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
            _serialPort.Open();//открытие порта
            if (_serialPort.IsOpen)//если порт открыт
            {
                MessageBox.Show("Открыто");
            }

            text = textBox1.Text;//записываю текст сообщения в массив string
            textPort = text.ToCharArray();//запись сообщения в массив char
            byte[] bytes = new byte[text.Length];//массив байт для передачи информации порту
            int i = 0;

            foreach(char sym in textPort)
            {
                bytes[i] = Convert.ToByte(sym);//запись в массив byte
                i++;
            }
            
            _serialPort.Close();//закрываю порт, чтобы по следующему клику проверить его открытие
        }
    }
}
