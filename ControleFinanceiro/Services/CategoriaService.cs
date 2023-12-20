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

        public CategoriaEdicaoViewModel ObterParaEditar(Guid codigo)
        {
            var categoriaDaBase = SelectSQL(codigo: codigo).First();
            return new CategoriaEdicaoViewModel
            {
                Codigo = categoriaDaBase.Codigo,
                Descricao = categoriaDaBase.Descricao,
                Ativo = categoriaDaBase.Ativo
            };
        }
        public IEnumerable<CategoriaViewModel> ObterParaListar(Guid? codigo = null, bool? somentePais = null)
        {
            var categoriasDaBase = SelectSQL(
                codigo: codigo,
                somentePais: somentePais).OrderBy(x => x.Descricao);

            foreach (var item in categoriasDaBase)
            {
                yield return new CategoriaViewModel
                {
                    Codigo = item.Codigo,
                    Ativo = item.Ativo ? "Sim" : "Não",
                    Descricao = item.Descricao,
                    DescricaoCategoriaPai = item.CategoriaPai != null ? item.CategoriaPai.Descricao : string.Empty
                };
            }
        }

        public void AbrirTransacaoParaInserirCategoria(CategoriaCadastroViewModel viewModel)
        {
            using (TransactionScope scope = new())
            {
                if (SelectSQL(descricao: viewModel.Descricao).Any())
                    throw new ValidationException($"Já existe uma categoria com o nome {viewModel.Descricao}");

                Categoria c = new()
                {
                    Ativo = viewModel.Ativo,
                    Descricao = viewModel.Descricao,
                    CodigoCategoriaPai = viewModel.CodigoCategoriaPai,
                    Codigo = Guid.NewGuid()
                };
                InsertSQL(c);
                scope.Complete();
            }
        }

        public void AbrirTransacaoParaEditarCategoria(CategoriaEdicaoViewModel viewModel)
        {
            using (TransactionScope scope = new())
            {
                var categoriaDaBase = SelectSQL(codigo: viewModel.Codigo).First();
                categoriaDaBase.Ativo = viewModel.Ativo;
                categoriaDaBase.CodigoCategoriaPai = viewModel.CodigoCategoriaPai;
                categoriaDaBase.Descricao = viewModel.Descricao;
                UpdateSQL(categoriaDaBase);
                scope.Complete();
            }
        }

        #region Crud

        public void InsertSQL(Categoria categoria)
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Execute("insert into Categoria (Codigo,Descricao,CodigoCategoriaPai,Ativo) values (@Codigo,@Descricao,@CodigoCategoriaPai,@Ativo)", categoria);
        }
        public List<Categoria> SelectSQL(Guid? codigo = null, string? descricao = null, bool? ativo = null, bool? somentePais = null)
        {
            StringBuilder sb = new();
            sb.AppendLine("SELECT A.*,B.*");
            sb.AppendLine("FROM Categoria A (NOLOCK)");
            sb.AppendLine("LEFT JOIN Categoria B (NOLOCK) ON A.CodigoCategoriaPai = B.Codigo");
            sb.AppendLine("WHERE A.Codigo = ISNULL(@Codigo, A.Codigo)");
            sb.AppendLine("AND A.Descricao = ISNULL(@Descricao,A.Descricao)");
            sb.AppendLine("AND A.Ativo = ISNULL(@Ativo,A.Ativo)");
            if (somentePais != null && somentePais.Value)
                sb.AppendLine("AND A.CodigoCategoriaPai IS NULL");

            using var conn = new SqlConnection(ConnectionString);
            return conn.Query<Categoria, Categoria, Categoria>(sb.ToString(),
                (categoria, categoriaPai) =>
                {
                    if (categoriaPai is not null)
                        categoria.CategoriaPai = categoriaPai;

                    return categoria;
                },
                new { @Codigo = codigo, @Descricao = descricao, @Ativo = ativo }, splitOn: "Codigo").ToList();
        }

        public void UpdateSQL(Categoria categoria)
        {
            StringBuilder sql = new();
            sql.AppendLine(@"UPDATE Categoria");
            sql.AppendLine("SET Descricao = @Descricao,");
            sql.AppendLine("CodigoCategoriaPai = @CodigoCategoriaPai,");
            sql.AppendLine("Ativo = @Ativo");
            sql.AppendLine("WHERE Codigo = @Codigo");
            using var conn = new SqlConnection(ConnectionString);
            conn.Execute(sql.ToString(), categoria);
        }

        #endregion
    }
}
