using ControleFinanceiro.Entities;

namespace ControleFinanceiro
{
    public class CommonHelper
    {
        public static DateOnly ConverterDateTimeParaDateOnly(DateTime dateTime) => new(dateTime.Year, dateTime.Month, dateTime.Day);

        public static DateTime ConverterDateOnlyParaDateTime(DateOnly dateOnly) => dateOnly.ToDateTime(TimeOnly.Parse("10:00 PM"));

        public static string FormatarDescricaoCartao(Cartao? cartao = null)
        {
            if (cartao == null)
                return "Outro";

            return $"{cartao.CartaoTipo.Descricao}/{cartao.CartaoBandeira.Descricao}{(!string.IsNullOrEmpty(cartao.NumeroCartao) ? $"/{cartao?.NumeroCartao.Substring(12, 4)}" : "")}/{((cartao?.Virtual ?? false) ? "Virtual" : "Físico")}";
        }

        public static decimal TransformarDecimalNegativoEmPositivo(decimal negativo)
        => negativo * -1;

    }
}
