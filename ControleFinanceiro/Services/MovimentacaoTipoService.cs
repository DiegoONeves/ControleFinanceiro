using ControleFinanceiro.Entities;
using ControleFinanceiro.ValueObjects;
using Dapper;
using System.Data.SqlClient;

namespace ControleFinanceiro.Services
{
    public class MovimentacaoTipoService : BaseService
    {
        public MovimentacaoTipoService(IConfiguration config)
         : base(config)
        {

        }

        public MovimentacaoTipo ObterEntrada() => Obter().First(x => x.Descricao == TipoDeMovimentacao.Entrada);
        public MovimentacaoTipo ObterSaida() => Obter().First(x => x.Descricao == TipoDeMovimentacao.Saida);

        public List<MovimentacaoTipo> Obter(Guid? codigo = null)
        {
            using var conn = new SqlConnection(ConnectionString);
            return conn.Query<MovimentacaoTipo>("SELECT * FROM MovimentacaoTipo (NOLOCK) WHERE Codigo = ISNULL(@Codigo,Codigo)", new { @Codigo = codigo }).ToList();
        }
    }
}
