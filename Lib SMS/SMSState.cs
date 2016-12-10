using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib_SMS
{
    public enum SMSState
    {
        SENT,                           // Отослано
        NOT_DELIVERED,                  // Не доставлено
        DELIVERED,                      // Доставлено
        NOT_ALLOWED,                    // Оператор не обслуживается
        INVALID_DESTINATION_ADDRESS,    // Неверный адрес для доставки
        INVALID_SOURCE_ADDRESS,         // Неправильное имя «От кого»
        NOT_ENOUGH_CREDITS,             // Недостаточно кредитов

        ERROR                           // Произошла ошибка подключения
    }
}
