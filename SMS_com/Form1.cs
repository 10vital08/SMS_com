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
        string text;

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
            if (_serialPort.IsOpen)//если порт открыт
            {
                //MessageBox.Show("Открыто");
            }

            text = textBox1.Text;//записываю текст сообщения в массив string
            Encoding Koi8Fotmat = Encoding.GetEncoding("KOI8-R");
            Encoding UtfFormat = Encoding.UTF8;
            byte[] OriginagBytes = Koi8Fotmat.GetBytes(text);
            byte[] ConvertBytes = Encoding.Convert(Koi8Fotmat,Koi8Fotmat,OriginagBytes);
            string KoiString = UtfFormat.GetString(ConvertBytes);
            textPort = KoiString.ToCharArray();
            
            
            //for(int i = 0; i < text.Length; i++)
            //{
            //    bytes[i] = ;
            //}
            //foreach (char sym in textPort)
            //{
            //    bytes[i] = Convert.ToByte(sym);//запись в массив byte
            //    i++;
            //}
            
            _serialPort.WriteLine("AT \r\n");//переход в режим готовности
            Thread.Sleep(500);//обязательные паузы между командами
            _serialPort.Write("AT+CMGF=1 \r\n"); //устанавливается текстовый режим для отправки сообщений
            Thread.Sleep(500);
            _serialPort.Write("AT+CMGS=\"+79372611302\"" + "\r\n");//передаем команду с номером телефона получателя СМС
            Thread.Sleep(500);
            //отправляем текст сообщения(26 = комбинация CTRL-Z, необходимо при передаче сообщения)
            //_serialPort.Write(bytes, 0, text.Length);
            _serialPort.Write(char.ConvertFromUtf32(26) + "\r\n");
            Thread.Sleep(500);
            _serialPort.Close();//закрываю порт, чтобы по следующему клику проверить его открытие
        }
    }
}
