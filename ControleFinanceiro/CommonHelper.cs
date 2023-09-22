namespace ControleFinanceiro
{
    public class CommonHelper
    {
        public static DateOnly ConverterDateTimeParaDateOnly(DateTime dateTime) => new(dateTime.Year, dateTime.Month, dateTime.Day);
           
        public static DateTime ConverterDateOnlyParaDateTime(DateOnly dateOnly) => dateOnly.ToDateTime(TimeOnly.Parse("10:00 PM"));
        
    }
}
