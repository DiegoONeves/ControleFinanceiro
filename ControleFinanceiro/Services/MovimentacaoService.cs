using ControleFinanceiro.Models;

namespace ControleFinanceiro.Services
{
    public class MovimentacaoService
    {
        public MovimentacaoViewModel BuscarMovimentacoes(DateOnly? dataInicial = null, DateOnly? dataFinal = null)
        {
            if (dataInicial is null)
                dataInicial = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));

            if (dataFinal is null)
                dataFinal = ObterDiaFinal(DateOnly.FromDateTime(DateTime.Now));


            var resultado = new MovimentacaoViewModel
            {
                DataInicial = dataInicial.Value,
                DataFinal = dataFinal.Value
            };

            return resultado;
        }

        private DateOnly ObterDiaFinal(DateOnly data) => DateOnly.FromDateTime(new DateTime(data.Year, data.Month, DateTime.DaysInMonth(data.Year, data.Month)));
        
    }
}
