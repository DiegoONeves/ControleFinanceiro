using ControleFinanceiro.Entities;
using Dapper;
using System.Data.SqlClient;
using System.Text;

namespace ControleFinanceiro.Services
{
    public class CartaoDeCreditoService : BaseService
    {
        public CartaoDeCreditoService(IConfiguration config) : base(config)
        {
        }

        public List<CartaoDeCredito> Obter(Guid? codigo = null)
        {
            StringBuilder sb = new();
            sb.Append("SELECT A.*,B.* FROM CartaoDeCredito A (NOLOCK) INNER JOIN BandeiraCartao B (NOLOCK) ON A.CodigoBandeiraCartao = B.Codigo WHERE A.Codigo = ISNULL(@Codigo,A.Codigo)");
            using var conn = new SqlConnection(ConnectionString);
            return conn.Query<CartaoDeCredito, BandeiraCartao, CartaoDeCredito>(sb.ToString(),
                (cartaoDeCredito, bandeira) =>
                {
                    cartaoDeCredito.BandeiraCartao = bandeira;

                    return cartaoDeCredito;

                }, new { @Codigo = codigo }, splitOn: "Codigo").ToList();
        }


        public DateOnly ObterDataDaParcela(Guid codigoCartaoDeCredito, DateOnly dataDaCompra)
        {
            var cartaoEmUso = Obter(codigoCartaoDeCredito).First();
            int diaDeVencimento = int.Parse(cartaoEmUso.Vencimento);

            DateOnly dataDeCorte = new(dataDaCompra.Year, dataDaCompra.Month, int.Parse(cartaoEmUso.DataDeCorte));

            if (dataDaCompra < dataDeCorte)
                return new DateOnly(dataDaCompra.Year, dataDaCompra.Month, diaDeVencimento);
            else
                return new DateOnly(dataDaCompra.AddMonths(1).Year, dataDaCompra.AddMonths(1).Month, diaDeVencimento);
        }
    }
}
