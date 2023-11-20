using ControleFinanceiro.Entities;
using Dapper;
using System.Data.SqlClient;
using System.Text;

namespace ControleFinanceiro.Services
{
    public class CartaoService : BaseService
    {
        public CartaoService(IConfiguration config) : base(config)
        {
        }

        public List<Cartao> Obter(Guid? codigo = null, bool? ativo = null)
        {
            StringBuilder sb = new();
            sb.AppendLine("SELECT A.*,B.*,C.*");
            sb.AppendLine("FROM Cartao A (NOLOCK)");
            sb.AppendLine("INNER JOIN CartaoBandeira B (NOLOCK) ON A.CodigoCartaoBandeira = B.Codigo");
            sb.AppendLine("INNER JOIN CartaoTipo C (NOLOCK) ON A.CodigoCartaoTipo = C.Codigo");
            sb.AppendLine("WHERE A.Codigo = ISNULL(@Codigo,A.Codigo)");
            sb.AppendLine("AND A.Ativo = ISNULL(@Ativo,A.Ativo)");
            using var conn = new SqlConnection(ConnectionString);
            return conn.Query<Cartao, CartaoBandeira, CartaoTipo, Cartao>(sb.ToString(),
                (cartao, bandeira, cartaoTipo) =>
                {
                    cartao.CartaoBandeira = bandeira;
                    cartao.CartaoTipo = cartaoTipo;

                    return cartao;

                }, new { @Codigo = codigo, @Ativo = ativo }, splitOn: "Codigo").ToList();
        }


        public DateOnly ObterDataDaParcela(Guid codigoCartao, DateOnly dataDaCompra)
        {
            var cartaoEmUso = Obter(codigoCartao).First();
            if (cartaoEmUso.CartaoTipo.Descricao != "Crédito")
                return dataDaCompra;

            int diaDeVencimento = int.Parse(cartaoEmUso.Vencimento);

            DateOnly dataDeCorte = new(dataDaCompra.Year, dataDaCompra.Month, int.Parse(cartaoEmUso.DataDeCorte));

            if (dataDaCompra < dataDeCorte)
                return new DateOnly(dataDaCompra.Year, dataDaCompra.Month, diaDeVencimento);
            else
                return new DateOnly(dataDaCompra.AddMonths(1).Year, dataDaCompra.AddMonths(1).Month, diaDeVencimento);
        }
    }
}
