using ControleFinanceiro.Entities;
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

        public MovimentacaoTipo ObterEntrada() => Obter().First(x => x.Descricao == "Entrada");
        public MovimentacaoTipo ObterSaida() => Obter().First(x => x.Descricao == "Saída");

        public List<MovimentacaoTipo> Obter(Guid? codigo = null)
        {
            using var conn = new SqlConnection(ConnectionString);
            return conn.Query<MovimentacaoTipo>("SELECT * FROM MovimentacaoTipo (NOLOCK) WHERE Codigo = ISNULL(@Codigo,Codigo)", new { @Codigo = codigo }).ToList();
        }
    }
}
