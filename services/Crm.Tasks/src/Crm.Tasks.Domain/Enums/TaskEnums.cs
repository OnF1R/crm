namespace Crm.Tasks.Domain.Enums;

public enum TaskPriority
{
    Низкий = 1,
    Средний = 2,
    Высокий = 3,
    Критический = 4
}

public enum TaskStatus
{
    Новая = 1,
    ВРаботе = 2,
    НаПроверке = 3,
    Завершена = 4,
    Отменена = 5
}

public enum RelatedEntityType
{
    Client = 1,
    Deal = 2
}
