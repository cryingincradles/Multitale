namespace Multitale.Sources.Localisation;

public class Russian : Localisation.Base
{
    public Russian()
    {
        Alias = "Русский";
        HomeMenu = new Localisation.HomeMenu
        {
            Tint = "Используйте стрелки для выбора, Enter или пробел для подтверждения",
            Home = "Домашняя",
            About = "О проекте",
            Launcher = "Лаунчер",
            Settings = "Настройки",
            Donate = "Помочь проекту"
        };
        AboutMenu = new Localisation.AboutMenu
        {
            Tint = "Используйте стрелки для выбора, Enter или пробел для подтверждения",
            About = "О проекте",
            Developers = "Разработчики",
            AboutText =
                "Я решил сделать проект, который мог бы заинтересовать как меня, так и людей, знающих, для чего он нужен. " +
                "Захотелось попробовать сделать что-то, что мог бы переделывать для себя и дополнять любой разработчик, который хотел бы себя попробовать в этой сфере",
            DevelopersText = "cradles, saykowa",
            Links = "Ссылки",
            LinksText = "https://github.com/cryingincradles/Multitale" + "\n" +
                        "https://t.me/multitale" + "\n" +
                        "https://zelenka.guru/threads/5167800",
            GoBack = "Вернуться назад"
        };
        DonateMenu = new Localisation.DonateMenu
        {
            Tint = "Используйте стрелки для выбора, Enter или пробел для подтверждения",
            Donate = "Помочь проекту",
            DonateText =
                "Multitale - это моя самая большая практика развития, которой я уделяю огромное количество своего времени. " +
                "Итак, если вы хотите, чтобы проект продолжал поддерживаться, обновляться и дополняться, вы можете оставить свое пожертвование, воспользовавшись реквизитами, указанными ниже",
            Requisites = "Реквизиты",
            RequisitesText = "ETH/BSC -> 0x375c1A4CcC41FcB2d35122aDDA008A8ecD384333" + "\n" +
                             "BTC -> bc1ql3thytsud4x9nulym3xkpmppv5eh8cj84tqsnp" + "\n" +
                             "LTC -> LaX2ZRgDwAWqsYhHLvZDc3xYz1sA2LEFuM" + "\n" +
                             "USDT -> TH2n3dQ8Jd2mzvoGatkpBPz5CHRhNTmnan" + "\n" +
                             "SOL -> EJWQbh4T43uV8mxkcbyLXCthG6CafqeyQBrvLMpHfSm2"
        };
        LauncherMenu = new Localisation.LauncherMenu
        {
            Tint = "Используйте стрелки для выбора, Enter или пробел для подтверждения",
            StartTint = "Пожалуйста, будьте терпеливы и ожидайте завершения процесса",
            Start = "Запустить",
            Settings = "Настройки",
            GoBack = "Вернуться назад",
            Launcher = "Лаунчер",
            Decoder = "Дешифровщик",
            Fetcher = "Фетчер",
            ProxyScrapper = "Прокси Скраппер",
            ProxyChecker = "Прокси Чекер",
            ThreadsNotSet = "Количество потоков не установлено в настройках",
            DataFileNotSet = "Проверяемый файл не установлен в настройках",
            DataFileEmpty = "Проверяемый файл пуст",
            DataFileNothingParsed = "Не было спаршено ни одной строки из проверяемого файла",
            ParsingDataFile = "Собираю строки из файла",
            CollectingProxy = "Собираю прокси",
            FromFile = "Из файла",
            ValidatingProxy = "Проверяю прокси",
            Fetching = "Проверяю строки"
        };
        SettingsMenu = new Localisation.SettingsMenu
        {
            Tint = "Используйте стрелки для выбора, Enter или пробел для подтверждения",
            InputTint = "Оставьте поле ввода пустым и нажмите Enter чтобы вернуться назад",
            Settings = "Настройки",
            ViewMode = "Режим отображения",
            Language = "Язык",
            Theme = "Тема",
            ProxyPath = "Путь к прокси",
            ProxyTimeout = "Прокси таймаут",
            DataFilePath = "Проверяемый файл",
            SaveDetails = "Сохранять детали",
            GoBack = "Вернуться назад",
            GoBackToInput = "Вернуться к вводу",
            Change = "Изменить",
            Enable = "Включить",
            Disable = "Выключить",
            Enabled = "Включено",
            Disabled = "Выключено",
            InputValue = "Введите новое значение",
            InputNumber = "Введите число",
            InputDirectoryPath = "Введите путь к папке",
            InputFilePath = "Введите путь к файлу",
            DirectoryNotExists = "Указанная папка не существует или не была найдена",
            FileNotExists = "Указанный файл не существует или не был найден",
            NotNumber = "Указанное значение не является числовым",
            NotSet = "Не установлено",
            CurrentValue = "Текущее значение",
            Threads = "Потоки"
        };
    }
}