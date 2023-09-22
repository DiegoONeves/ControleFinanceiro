using ControleFinanceiro.Entities;
using ControleFinanceiro.Models;
using Dapper;
using System.Data.SqlClient;
using System.Text;
using System.Transactions;

namespace ControleFinanceiro.Services
{
    public class CategoriaService : BaseService
    {
        public CategoriaService(IConfiguration config)
           : base(config)
        {

        }

        public MovimentacaoCategoriaEdicaoViewModel ObterParaEditar(Guid codigo)
        {
            var categoriaDaBase = SelectSQL(codigo: codigo).First();
            return new MovimentacaoCategoriaEdicaoViewModel
            {
                Codigo = categoriaDaBase.Codigo,
                Descricao = categoriaDaBase.Descricao,
                Ativo = categoriaDaBase.Ativo
            };
        }
        public IEnumerable<MovimentacaoCategoriaViewModel> Obter()
        {
            var categoriasDaBase = SelectSQL().OrderBy(x => x.Descricao);

            foreach (var item in categoriasDaBase)
            {
                yield return new MovimentacaoCategoriaViewModel
                {
                    Codigo = item.Codigo,
                    Ativo = item.Ativo ? "Sim" : "Não",
                    Descricao = item.Descricao
                };
            }
        }

        public void AbrirTransacaoParaInserirCategoria(MovimentacaoCategoriaCadastroViewModel viewModel)
        {
            using (TransactionScope scope = new())
            {
                if (SelectSQL(descricao: viewModel.Descricao).Any())
                    throw new ValidationException($"Já existe uma categoria com o nome {viewModel.Descricao}");

                MovimentacaoCategoria c = new()
                {
                    Ativo = viewModel.Ativo,
                    Descricao = viewModel.Descricao,
                    Codigo = Guid.NewGuid()
                };
                InsertSQL(c);
                scope.Complete();
            }
        }

        public void AbrirTransacaoParaEditarCategoria(MovimentacaoCategoriaEdicaoViewModel viewModel)
        {
            using (TransactionScope scope = new())
            {
                var categoriaDaBase = SelectSQL(codigo: viewModel.Codigo).First();
                categoriaDaBase.Ativo = viewModel.Ativo;
                categoriaDaBase.Descricao = viewModel.Descricao;
                UpdateSQL(categoriaDaBase);
                scope.Complete();
            }
        }

        #region Crud

        public void InsertSQL(MovimentacaoCategoria categoria)
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Execute("insert into MovimentacaoCategoria (Codigo,Descricao,Ativo) values (@Codigo,@Descricao,@Ativo)", categoria);
        }
        public List<MovimentacaoCategoria> SelectSQL(Guid? codigo = null, string? descricao = null, bool? ativo = null)
        {
            using var conn = new SqlConnection(ConnectionString);
            return conn.Query<MovimentacaoCategoria>("SELECT A.* FROM MovimentacaoCategoria A (NOLOCK) WHERE A.Codigo = ISNULL(@Codigo,A.Codigo) AND A.Descricao = ISNULL(@Descricao,A.Descricao) AND A.Ativo = ISNULL(@Ativo,A.Ativo)", new { @Codigo = codigo, @Descricao = descricao, @Ativo = ativo }).ToList();
        }

        public void UpdateSQL(MovimentacaoCategoria categoria)
        {
            StringBuilder sql = new();
            sql.AppendLine(@"UPDATE MovimentacaoCategoria");
            sql.AppendLine("SET Descricao = @Descricao,");
            sql.AppendLine("Ativo = @Ativo");
            sql.AppendLine("WHERE Codigo = @Codigo");
            using var conn = new SqlConnection(ConnectionString);
            conn.Execute(sql.ToString(), categoria);
        }

        #endregion
    }
}
