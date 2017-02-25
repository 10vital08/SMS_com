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
                byte b = ucs2[i + 1];
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
                bytesKoi8[i] = (byte)(bytesKoi8[i] & 0x7F);//обнуляем битовой операцией "И" старший бит в каждом байте
            }
            string stringUtf8result = encodingKoi8.GetString(bytesKoi8);//строка символов в KOI8-R

            string PhoneNumber = "+79272880849";// + maskedTextBox1.Text;//номер получателя
            PhoneNumber = "01" + "00" + PhoneNumber.Length.ToString("X2") + "91" + EncodePhoneNumber(PhoneNumber);
            stringUtf8result = StringToUCS2(stringUtf8result);//получение кода сообщения
            string leninByte = (stringUtf8result.Length / 2).ToString("X2");//получение длины в 16-чном формате
            //объединение кода номера, длины сообщения, кода сообщения и промежуточных кодовых символов
            stringUtf8result = PhoneNumber + "00" + "0" + "8" + leninByte + stringUtf8result;
            //ucs2Text = PhoneNumber + "00" + "0" + "8" + leninByte + ucs2Text;

            double lenMes = stringUtf8result.Length / 2;// кол-во октет в десятичной системе

            char[] charUnicode = stringUtf8result.ToCharArray();
            byte[] byteUnicode = new byte[stringUtf8result.Length];

            for (int i = 0; i < stringUtf8result.Length; i++)
            {
                byteUnicode[i] = (byte)charUnicode[i];
            }


            _serialPort.WriteLine("AT\r\n");//переход в режим готовности
            Thread.Sleep(500);//обязательные паузы между командами
            _serialPort.Write("AT+CMGF=0\r\n"); //устанавливается режим PDU для отправки сообщений на русском языке
            Thread.Sleep(500);
            _serialPort.Write("AT+CMGS=" + (Math.Ceiling(lenMes)).ToString() + "\r\n");//передаем команду с номером телефона получателя СМС
            Thread.Sleep(500);

            stringUtf8result = "00" + stringUtf8result;
            _serialPort.Write(byteUnicode, 0, byteUnicode.Length);
            _serialPort.Write(char.ConvertFromUtf32(26) + "\r\n");
            //отправляем текст сообщения(26 = комбинация CTRL-Z, необходимо при передаче сообщения)
            //_serialPort.Write(char.ConvertFromUtf32(26) + "\r\n");
            Thread.Sleep(500);
            _serialPort.Close();
        }
    }
}
