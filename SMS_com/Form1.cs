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
using System.Threading;

namespace SMS_com
{
    public partial class Form1 : Form
    {
        SerialPort _serialPort;
        string[] myPort; //массив для COM-портов
        char[] textPort;
        

        public Form1()
        {
            InitializeComponent();
            _serialPort = new SerialPort("COM3");
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
            if (!_serialPort.IsOpen)//если порт открыт
            {
                _serialPort.Open();
               // MessageBox.Show("Открыто");
            }

            var text = textBox1.Text;//записываю текст сообщения в массив string
            Encoding encodingKoi8 = Encoding.GetEncoding("KOI8-R");
            //Encoding encodingUtf = Encoding.UTF8;
            byte[] bytesKoi8 = encodingKoi8.GetBytes(textBox1.Text);
            //byte[] convertBytes = Encoding.Convert(encodingKoi8,encodingKoi8,bytesKoi8);
            //string koiString = encodingUtf.GetString(convertBytes);
            //string koiString = encodingKoi8.GetString(bytesKoi8);

            //textPort = koiString.ToCharArray();


            for (int i = 0; i < bytesKoi8.Length; i++)
            {
                //результатом битовой операции является число значений типа int(int32),т.е. 4 байта
                //нужно преобразовать в байт
                bytesKoi8[i] = (byte) (bytesKoi8[i] & 0x7F);//обнуляем битовой операцией "И" старший бит в каждом байте

            }

            string stringUtf8result = encodingKoi8.GetString(bytesKoi8);
            _serialPort.WriteLine("AT \r\n");//переход в режим готовности
            Thread.Sleep(500);//обязательные паузы между командами
            _serialPort.Write("AT+CMGF=1 \r\n"); //устанавливается текстовый режим для отправки сообщений
            Thread.Sleep(500);
            _serialPort.Write("AT+CMGS=\"+79372611302\"" + "\r\n");//передаем команду с номером телефона получателя СМС
            Thread.Sleep(500);
            _serialPort.Write(bytesKoi8, 0, bytesKoi8.Length);
            //отправляем текст сообщения(26 = комбинация CTRL-Z, необходимо при передаче сообщения)
            _serialPort.Write(char.ConvertFromUtf32(26) + "\r\n");
            Thread.Sleep(500);
            _serialPort.Close();
        }
    }
}
