using ConsoleAdmin.Features;

namespace ConsoleAdmin
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // TODO: Добавить в меню:
            // CLI (из Env переменной)
            // Генератор паролей
            // Список пользователей
            // Мониторинг сервисов
            // Запуск плейбуков на установку ПО
            // Запуск мониторинга системы: ЦПУ, ОЗУ, СХД, Диски, открытые порты
            // Запуск приложений-модулей, котоыре я написал (телеграм-бот, предпросмотр картинок и CSV-файлов)
            // Выход
            ConsoleAdmin.Features.PasswordGenerator.Run();
        }
    }
}
