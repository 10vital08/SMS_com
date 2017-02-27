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
        public static SerialPort _serialPort;
        string[] myPort; //массив для COM-портов

        //преобразование номера в чётный и переставление цифр местами
        public static string EncodePhoneNumber(string PhoneNumber)
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

        public static string StringToUCS2(string text)
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
            if(comboBox1.Text != "")
            {
                _serialPort = new SerialPort(comboBox1.Text);
                _serialPort.Open();//открытие порта
                if (!_serialPort.IsOpen)//если порт открыт
                {
                    _serialPort.Open();
                }

                var text = textBox1.Text;//записываю текст сообщения в массив string
                string PhoneNumber = maskedTextBox1.Text;//номер получателя
                bool result;
                result = RussianSMS(text, PhoneNumber);
                if (result == true)
                {
                    MessageBox.Show("Сообщение отправлено успешно.\r\nПолучателю придет сообщение: " + text);
                }
                else
                {
                    MessageBox.Show("Произошла ошибка при отправке");
                }

                _serialPort.Close();
            }
            else MessageBox.Show("Сообщение не отправлено.\r\nВыберите COM-порт из выпадающего списка");

        }

        

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text != "")
            {
                _serialPort = new SerialPort(comboBox1.Text);
                _serialPort.Open();//открытие порта
                if (!_serialPort.IsOpen)//если порт открыт
                {
                    _serialPort.Open();
                }

                var text = textBox1.Text;//записываю текст сообщения в массив string
                string PhoneNumber = maskedTextBox1.Text;//номер получателя
                bool result;
                result = TranslitSMS(text, PhoneNumber);
                if (result == true)
                {

                }
                else
                {
                    MessageBox.Show("Произошла ошибка при отправке");
                }

                _serialPort.Close();
            }
            else MessageBox.Show("Сообщение не отправлено.\r\nВыберите COM-порт из выпадающего списка");


        }

        private static bool RussianSMS(string text, string PhoneNumber)
        {

            if (!_serialPort.IsOpen) return false;

            try
            {
                _serialPort.WriteLine("AT\r\n");//переход в режим готовности
                Thread.Sleep(500);//обязательные паузы между командами
                _serialPort.Write("AT+CMGF=0\r\n"); //устанавливается режим PDU для отправки сообщений на русском языке
                Thread.Sleep(500);
            }
            catch
            {
                return false;
            }

            try
            {
                PhoneNumber = "01" + "00" + PhoneNumber.Length.ToString("X2") + "91" + EncodePhoneNumber(PhoneNumber);
                text = StringToUCS2(text);//получение кода сообщения
                string leninByte = (text.Length / 2).ToString("X2");//получение длины в 16-чном формате
                                                                    //объединение кода номера, длины сообщения, кода сообщения и промежуточных кодовых символов
                text = PhoneNumber + "00" + "0" + "8" + leninByte + text;

                double lenMes = text.Length / 2;// кол-во октет в десятичной системе


                _serialPort.Write("AT+CMGS=" + (Math.Ceiling(lenMes)).ToString() + "\r\n");//передаем команду с номером телефона получателя СМС
                Thread.Sleep(500);
                text = "00" + text;
                _serialPort.Write(text + char.ConvertFromUtf32(26) + "\r\n");
                Thread.Sleep(500);
                
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static bool TranslitSMS(string text, string PhoneNumber)
        {

            if (!_serialPort.IsOpen) return false;

            try
            {
                _serialPort.WriteLine("AT \r\n");//переход в режим готовности
                Thread.Sleep(500);//обязательные паузы между командами
                _serialPort.Write("AT+CMGF=1 \r\n"); //устанавливается текстовый режим для отправки сообщений
                Thread.Sleep(500);
            }
            catch
            {
                return false;
            }

            try
            {
                Encoding encodingKoi8 = Encoding.GetEncoding("KOI8-R");
                byte[] bytesKoi8 = encodingKoi8.GetBytes(text);

                for (int i = 0; i < bytesKoi8.Length; i++)
                {
                    //результатом битовой операции является число значений типа int(int32),т.е. 4 байта
                    //нужно преобразовать в байт
                    bytesKoi8[i] = (byte)(bytesKoi8[i] & 0x7F);//обнуляем битовой операцией "И" старший бит в каждом байте
                }
                string stringUtf8result = encodingKoi8.GetString(bytesKoi8);//строка символов в KOI8-R
                
                _serialPort.Write("AT+CMGS=\"" + PhoneNumber + "\"" + "\r\n");//передаем команду с номером телефона получателя СМС
                Thread.Sleep(500);
                _serialPort.Write(bytesKoi8, 0, bytesKoi8.Length);
                //отправляем текст сообщения(26 = комбинация CTRL-Z, необходимо при передаче сообщения)
                _serialPort.Write(char.ConvertFromUtf32(26) + "\r\n");
                Thread.Sleep(500);
                MessageBox.Show("Сообщение отправлено успешно.\r\nПолучателю придет сообщение: " + stringUtf8result);
            }
            catch
            {
                return false;
            }

            return true;
        }

    }
}
