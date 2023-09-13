using ControleFinanceiro.Entities;
using Dapper;
using System.Data.SqlClient;

namespace ControleFinanceiro.Services
{
    public class MovimentacaoCategoriaService: BaseService
    {
        public MovimentacaoCategoriaService(IConfiguration config)
           : base(config)
        {

        }
        public List<MovimentacaoCategoria> Obter(Guid? codigo = null)
        {
            using var conn = new SqlConnection(ConnectionString);
            return conn.Query<MovimentacaoCategoria>("SELECT A.* FROM MovimentacaoCategoria A (NOLOCK) WHERE A.Codigo = ISNULL(@Codigo,A.Codigo)", new { @Codigo = codigo}).ToList();
        }
    }
}
