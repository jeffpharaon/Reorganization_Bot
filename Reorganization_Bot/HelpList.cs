using System;

namespace Reorganization_Bot
{
    internal class HelpList
    {
       public string showPeople = "📃СПИСОК КОМАНД📃" +
            "\r\n*Чтобы увидеть расширенный список необходимо авторизоваться*" +
            "\r\n" +
            "\r\n1. Просмотр правил /rules" +
            "\r\n2. Просмотр вакансий /jobs" +
            "\r\n3. Подача заявления на вступление /statement" +
            "\r\n4. Просмотр инфо-ресурсов /info" +
            "\r\n5. Авторизоваться /sign";

        public string showUser = "📃СПИСОК КОМАНД📃" +
            "\r\n" +
            "\r\n1. О аккаунте /account" +
            "\r\n2. Просмотр приказов /orders" +
            "\r\n3. Просмотр членов организации /structure" +
            "\r\n4. Просмотр инфо-ресурсов /info";

        public string showAdmin = "📃СПИСОК КОМАНД📃" +
            "\r\n" +
            "\r\n1. О аккаунте /account" +
            "\r\n2. Назначение приказов /add_order" +
            "\r\n3. Добавление участников /add_user" +
            "\r\n4. Удаление приказа /delete" +
            "\r\n5. Просмотр всех приказов /view" +
            "\r\n6. Просмотр личного состава /users" +
            "\r\n7. Просмотр инфо-ресурсов /info";
    }
}
