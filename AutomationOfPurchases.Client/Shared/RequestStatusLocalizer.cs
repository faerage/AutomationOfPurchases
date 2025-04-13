using AutomationOfPurchases.Shared.Enums;

namespace AutomationOfPurchases.Shared.Localization
{
    /// Статичний клас для перекладу англомовного статусу (enum) на український рядок для відображення.
    public static class RequestStatusLocalizer
    {
        public static string Localize(RequestStatus status)
        {
            return status switch
            {
                RequestStatus.Draft => "Чернетка",
                RequestStatus.PendingDepartmentHead => "Очікується затвердження керівника",
                RequestStatus.PendingEconomist => "Очікується затвердження економіста",
                RequestStatus.Approved => "Затверджено",
                RequestStatus.Rejected => "Відхилено",
                _ => status.ToString()
            };
        }
    }
}
