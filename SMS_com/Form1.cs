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
        string smsCenterPhoneNumber = "+79232909090"; //номер смс центра Мегафона
        
        //преобразование номера в чётный и переставление цифр местами
        public string EncodePhoneNumber(string PhoneNumber)
        {
            string result = "";
            PhoneNumber = PhoneNumber.Replace("+", "");//вырезание + из номера
            //если номер нечетный, то в конец добавляется символ F
            if ((PhoneNumber.Length % 2) > 0) PhoneNumber += "F";
            int i = 0;
            while (i < PhoneNumber.Length)
            {
                result += PhoneNumber[i + 1].ToString() + PhoneNumber[i].ToString();
                i += 2;
            }
            return result.Trim();
        }

        public string StringToUCS2(string text)
        {
            UnicodeEncoding ue = new UnicodeEncoding();
            byte[] ucs2 = ue.GetBytes(text);
            int i = 0;
            while (i < ucs2.Length)
            {
                byte b = ucs2[i + 2];
                ucs2[i + 1] = ucs2[i];
                ucs2[i] = b;
                i += 2;
            }
            return BitConverter.ToString(ucs2).Replace("-", "");
        }

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
            }
            
            var text = textBox1.Text;//записываю текст сообщения в массив string
            Encoding encodingKoi8 = Encoding.GetEncoding("KOI8-R");
            byte[] bytesKoi8 = encodingKoi8.GetBytes(textBox1.Text);
            
            for (int i = 0; i < bytesKoi8.Length; i++)
            {
                //результатом битовой операции является число значений типа int(int32),т.е. 4 байта
                //нужно преобразовать в байт
                bytesKoi8[i] = (byte) (bytesKoi8[i] & 0x7F);//обнуляем битовой операцией "И" старший бит в каждом байте
            }

            string stringUtf8result = encodingKoi8.GetString(bytesKoi8);
            string PhoneNumber = "+7" + maskedTextBox1.Text;//номер получателя

            string smsnum = EncodePhoneNumber(smsCenterPhoneNumber);//подготовка перевернутого номера

            //подготовка номера получателя: длина номера, формат номера и сам перевернутый номер
            string recipient = "01" + "00" + PhoneNumber.Length.ToString("X2") + "92" + EncodePhoneNumber(PhoneNumber);
            string ucs2Text = StringToUCS2(text);//преобразование в формат UCS2(Unicode)
            string leninByte = (smsnum.Length / 2).ToString("X2");
            ucs2Text = PhoneNumber + "00" + "0" + "8" + leninByte + ucs2Text;

            double lenMes = ucs2Text.Length / 2;// кол-во октет в десятичной системе


            

            _serialPort.WriteLine("AT\r\n");//переход в режим готовности
            Thread.Sleep(500);//обязательные паузы между командами
            _serialPort.Write("AT+CMGF=0\r\n"); //устанавливается режим PDU для отправки сообщений на русском языке
            Thread.Sleep(500);
            _serialPort.Write("AT+CMGS=" + (Math.Ceiling(lenMes)).ToString() + "\r\n");//передаем команду с номером телефона получателя СМС
            Thread.Sleep(500);
            ucs2Text = "00" + ucs2Text;
            _serialPort.Write(ucs2Text + char.ConvertFromUtf32(26) + "\r\n");
            //отправляем текст сообщения(26 = комбинация CTRL-Z, необходимо при передаче сообщения)
            _serialPort.Write(char.ConvertFromUtf32(26) + "\r\n");
            Thread.Sleep(500);
            
            //_serialPort.WriteLine("AT \r\n");//переход в режим готовности
            //Thread.Sleep(500);//обязательные паузы между командами
            //_serialPort.Write("AT+CMGF=1 \r\n"); //устанавливается текстовый режим для отправки сообщений
            //Thread.Sleep(500);
            //_serialPort.Write("AT+CMGS=\"+79272880849\"" + "\r\n");//передаем команду с номером телефона получателя СМС
            //Thread.Sleep(500);
            //_serialPort.Write(bytesKoi8, 0, bytesKoi8.Length);
            ////отправляем текст сообщения(26 = комбинация CTRL-Z, необходимо при передаче сообщения)
            //_serialPort.Write(char.ConvertFromUtf32(26) + "\r\n");
            //Thread.Sleep(500);
            _serialPort.Close();
        }
    }
}
