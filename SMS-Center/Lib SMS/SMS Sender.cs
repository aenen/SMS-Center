using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Lib_SMS
{
    /// <summary>
    /// Класс реализует базовые возможности API ePochta
    /// </summary>
    public class SMSSender
    {
        /// <param name="login">Логин (email) пользователя в системе SMS Sender</param>
        /// <param name="password">Пароль пользователя в системе SMS Sender</param>
        /// <param name="sender">Отправитель смс. 14 цифровых символов или 11 цифробуквенных (английские буквы и цифры)</param>
        public SMSSender(string login, string password, string sender)
        {
            Login = login;
            Password = password;
            Sender = sender;
        }

        private string m_sender;

        /// <summary>
        /// Логин (email) пользователя в системе SMS Sender
        /// </summary>
        public string Login { private get; set; }

        /// <summary>
        /// Пароль пользователя в системе SMS Sender
        /// </summary>
        public string Password { private get; set; }

        /// <summary>
        /// Отправитель смс. 14 цифровых символов или 11 цифробуквенных (английские буквы и цифры)
        /// </summary>
        public string Sender
        {
            get { return m_sender; }
            set
            {
                if (value != "" && ((value.All(c => char.IsDigit(c)) && value.Length <= 14) || (Regex.IsMatch(value, @"[a-zA-Z0-9]") && value.Length <= 11)))
                    m_sender = value;
                else
                    m_sender = "SMS";
            }
        }

        /// <summary>
        /// Отправка SMS
        /// </summary>
        /// <param name="number">Номер получателя</param>
        /// <param name="message">Текст сообщения (до 70 Юникод символов)</param>
        /// <param name="messageId">(не обязательно) Уникальный ключ для возможности слежения за статусом SMS</param>
        /// <returns>Статус сообщения:
        /// >0 — Количество отправленных SMS
        /// -1 — Неправильный логин и/или пароль
        /// -2 — Неправильный формат XML
        /// -3 — Недостаточно кредитов на аккаунте пользователя
        /// -4 — Нет верных номеров получателей
        /// -5 — Ошибка подключения/исключение
        /// -6 — Текст сообщения больше 70 символов</returns>
        public int SendSms(string number, string message, string messageId = "")
        {
            if (message.Length > 70)
                return -6;

            try
            {
                string str_msgId = "";
                if (messageId != "")
                    str_msgId = $"messageID=\"{messageId}\"";

                string XML = "XML=<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                                "<SMS>\n" +
                                    "<operations>\n" +
                                        "<operation>SEND</operation>\n" +
                                    "</operations>\n" +
                                    "<authentification>\n" +
                                        $"<username>{Login}</username>\n" +
                                        $"<password>{Password}</password>\n" +
                                    "</authentification>\n" +
                                    "<message>\n" +
                                        $"<sender>{m_sender}</sender>\n" +
                                        $"<text>{message}</text>\n" +
                                    "</message>\n" +
                                    "<numbers>\n" +
                                        $"<number {str_msgId}>{number}</number>\n" +
                                    "</numbers>\n" +
                                "</SMS>\n";
                HttpWebRequest request = WebRequest.Create("http://api.myatompark.com/members/sms/xml.php") as HttpWebRequest;
                request.Method = "Post";
                request.ContentType = "application/x-www-form-urlencoded";
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] data = encoding.GetBytes(XML);
                request.ContentLength = data.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(data, 0, data.Length);
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Console.WriteLine($"Server error (HTTP {response.StatusCode}: {response.StatusDescription}).");
                        return -5;
                    }

                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string xmlResult = reader.ReadToEnd();

                    using (XmlReader xmlReader = XmlReader.Create(new StringReader(xmlResult)))
                    {
                        xmlReader.ReadToFollowing("status");
                        int status = xmlReader.ReadElementContentAsInt();
                        return status;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -5;
            }
        }

        /// <summary>
        /// Получение статуса SMS
        /// </summary>
        /// <param name="messageId">Уникальный ключ сообщения</param>
        /// <returns>Строка с датами отправки и доставки, а также статусом сообщения</returns>
        public string MessageState(string messageId)
        {
            try
            {
                string XML = "XML=<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                                "<SMS>\n" +
                                    "<operations>\n" +
                                        "<operation>GETSTATUS</operation>\n" +
                                    "</operations>\n" +
                                    "<authentification>\n" +
                                        $"<username>{Login}</username>\n" +
                                        $"<password>{Password}</password>\n" +
                                    "</authentification>\n" +
                                    "<statistics>\n" +
                                        $"<messageid>{messageId}</messageid>\n" +
                                    "</statistics>\n" +
                                "</SMS>\n";
                HttpWebRequest request = WebRequest.Create("http://api.myatompark.com/members/sms/xml.php") as HttpWebRequest;
                request.Method = "Post";
                request.ContentType = "application/x-www-form-urlencoded";
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] data = encoding.GetBytes(XML);
                request.ContentLength = data.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(data, 0, data.Length);
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Console.WriteLine($"Server error (HTTP {response.StatusCode}: {response.StatusDescription}).");
                        return "Server error";
                    }

                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string xmlResult = reader.ReadToEnd();

                    using (XmlReader xmlReader = XmlReader.Create(new StringReader(xmlResult)))
                    {
                        string status = "";
                        xmlReader.ReadToFollowing("message");
                        xmlReader.MoveToAttribute("id");
                        status += $"——— ID: {xmlReader.Value}\n";
                        xmlReader.MoveToAttribute("sentdate");
                        status += $"    Отослано: {xmlReader.Value}\n";
                        xmlReader.MoveToAttribute("donedate");
                        status += $"    Доставлено: {xmlReader.Value}\n";
                        xmlReader.MoveToAttribute("status");
                        status += $"    Статус: {xmlReader.Value}\n";
                        return status;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "exception";
            }
        }
    }
}
