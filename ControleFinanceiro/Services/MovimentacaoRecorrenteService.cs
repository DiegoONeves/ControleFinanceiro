using ControleFinanceiro.Entities;
using ControleFinanceiro.Models;
using ControleFinanceiro.ValueObjects;
using Dapper;
using System.Data.SqlClient;
using System.Text;
using System.Transactions;

namespace ControleFinanceiro.Services
{
    public class MovimentacaoRecorrenteService : BaseService
    {
        private readonly TipoService _tipoService;
        private readonly CategoriaService _categoriaService;
        private readonly MovimentacaoService _movimentacaoService;

        public MovimentacaoRecorrenteService(IConfiguration config,
            TipoService tipoService,
            CategoriaService categoriaService,
            MovimentacaoService movimentacaoService) : base(config)
        {
            _tipoService = tipoService;
            _categoriaService = categoriaService;
            _movimentacaoService = movimentacaoService;
        }

        public void AbrirTransacaoParaInserirNovaMovimentacaoRecorrente(MovimentacaoRecorrenteNovaViewModel model)
        {
            using TransactionScope scope = new();
            var tipo = _tipoService.Obter(model.CodigoTipo).First();
            model.Valor = tipo.Descricao != TipoDeMovimentacao.Entrada ? model.Valor * -1 : model.Valor;
            var dataPrimeiraMovimentacao = CommonHelper.ConverterDateOnlyParaDateTime(model.DataDaPrimeiraMovimentacao);
            MovimentacaoRecorrente movimentacaoRecorrente = new()
            {
                Codigo = Guid.NewGuid(),
                DataDaPrimeiraMovimentacao = dataPrimeiraMovimentacao,
                DataDaUltimaMovimentacao = dataPrimeiraMovimentacao.AddMonths(model.QuantidadeMovimentacao - 1),
                Descricao = model.Descricao,
                DespesaFixa = model.DespesaFixa,
                QuantidadeMovimentacao = model.QuantidadeMovimentacao,
                CodigoCategoria = model.CodigoCategoria,
                CodigoTipo = model.CodigoTipo,
                Valor = model.Valor,
                DataHora = DateTime.Now
            };
            InsertSQL(movimentacaoRecorrente);
            InserirMovimentacoesDeRecorrencias(model, model.DataDaPrimeiraMovimentacao, movimentacaoRecorrente);
            scope.Complete();
        }
      
        private void InserirMovimentacoesDeRecorrencias(dynamic model, DateOnly dataDaMovimentacao, MovimentacaoRecorrente movimentacaoRecorrente)
        {
            Guid codigoTipo = _tipoService.Obter().First(x => x.Descricao == TipoDeMovimentacao.Saida).Codigo;
            for (int i = 0; i < model.QuantidadeMovimentacao; i++)
            {
                dataDaMovimentacao = dataDaMovimentacao.AddMonths(i == 0 ? 0 : 1);
                MovimentacaoNovaViewModel m = new()
                {
                    DataMovimentacao = dataDaMovimentacao,
                    DataDaCompra = model.DataDaPrimeiraMovimentacao,
                    CodigoTipo = codigoTipo,
                    CodigoMovimentacaoRecorrente = movimentacaoRecorrente.Codigo,
                    Valor = model.Valor,
                    Descricao = $"Recorrência {i + 1} de {model.QuantidadeMovimentacao} - {model.Descricao}",
                    CodigoCategoria = model.CodigoCategoria,
                    DespesaFixa = model.DespesaFixa
                };
                _movimentacaoService.InserirMovimentacao(m);
            }
        }
        public void AbrirTransacaoParaExcluirRecorrencias(Guid codigo)
        {
            //abro a trasação no banco de dados
            using TransactionScope scope = new();

            //exclusão de todas as movimentações desse parcelamento
            _movimentacaoService.DeleteSQL(codigoMovimentacaoRecorrente: codigo);

            //efetuando exclusão do parcelamento depois de excluir as movimentações
            DeleteSQL(codigo);

            scope.Complete();
        }
        public IEnumerable<MovimentacaoRecorrenteViewModel> BuscarMovimentacoesRecorrentes() => DePara(SelectSQL());       
        
        private IEnumerable<MovimentacaoRecorrenteViewModel> DePara(List<MovimentacaoRecorrente> movimentacoesRecorrentes)
        {
            foreach (var item in movimentacoesRecorrentes)
            {
                yield return new MovimentacaoRecorrenteViewModel
                {
                    Codigo = item.Codigo,
                    Categoria = item.Categoria.Descricao,
                    Tipo = item.Tipo.Descricao,
                    Descricao = item.Descricao,
                    Valor = item.Valor
                };
            }
        }




        public void AbrirTransacaoParaAtualizarMovimentacaoRecorrente(MovimentacaoRecorrenteEdicaoViewModel model)
        {
            if (model.Codigo == Guid.Empty)
                throw new Exception("O código da movimentação não foi informado");

            using TransactionScope scope = new();
            var movimentacaoRecorrenteParaEditar = SelectSQL(model.Codigo).First();
            var tipoCadastradoNaBase = _tipoService.Obter(model.CodigoTipo).First();
            if (model.CodigoTipo == Guid.Empty)
                throw new Exception("O código do tipo de movimentação não foi informado");

            if (model.CodigoCategoria == Guid.Empty)
                throw new Exception("O código da categoria movimentação não foi informado");

            movimentacaoRecorrenteParaEditar.CodigoTipo = tipoCadastradoNaBase.Codigo;
            movimentacaoRecorrenteParaEditar.CodigoCategoria = model.CodigoCategoria;
            movimentacaoRecorrenteParaEditar.Valor = tipoCadastradoNaBase.Descricao != TipoDeMovimentacao.Entrada ? model.Valor * -1 : model.Valor;
            movimentacaoRecorrenteParaEditar.DataDaPrimeiraMovimentacao = CommonHelper.ConverterDateOnlyParaDateTime(model.DataDaPrimeiraMovimentacao);
            movimentacaoRecorrenteParaEditar.DespesaFixa = model.DespesaFixa;
            movimentacaoRecorrenteParaEditar.Valor = tipoCadastradoNaBase.Descricao == TipoDeMovimentacao.Saida ? model.Valor * -1 : model.Valor;
            movimentacaoRecorrenteParaEditar.Descricao = model.Descricao;

            Update(movimentacaoRecorrenteParaEditar);
            scope.Complete();
        }

        public MovimentacaoRecorrenteEdicaoViewModel ObterParaEditar(Guid codigo)
        {
            var movimentacaoParaEditar = SelectSQL(codigo).First();
            return new MovimentacaoRecorrenteEdicaoViewModel
            {
                Codigo = movimentacaoParaEditar.Codigo,
                DataDaPrimeiraMovimentacao = DateOnly.FromDateTime(movimentacaoParaEditar.DataDaPrimeiraMovimentacao),
                QuantidadeMovimentacao = movimentacaoParaEditar.QuantidadeMovimentacao,
                CodigoCategoria = movimentacaoParaEditar.CodigoCategoria,
                CodigoTipo = movimentacaoParaEditar.CodigoTipo,
                DespesaFixa = movimentacaoParaEditar.DespesaFixa,
                Descricao = movimentacaoParaEditar.Descricao,
                Valor = Math.Abs(movimentacaoParaEditar.Valor)
            };
        }


        #region Crud

        public void Update(MovimentacaoRecorrente movimentacaoRecorrente)
        {
            if (movimentacaoRecorrente.Codigo == Guid.Empty)
                throw new Exception("O código da movimentação não foi informado");

            if (movimentacaoRecorrente.CodigoTipo == Guid.Empty)
                throw new Exception("O código do tipo de movimentação não foi informado");

            if (movimentacaoRecorrente.CodigoCategoria == Guid.Empty)
                throw new Exception("O código da categoria movimentação não foi informado");

            using var conn = new SqlConnection(ConnectionString);
            conn.Execute("UPDATE MovimentacaoRecorrente SET DespesaFixa = @DespesaFixa,QuantidadeMovimentacao = @QuantidadeMovimentacao, DataDaPrimeiraMovimentacao = @DataDaPrimeiraMovimentacao, DataDaUltimaMovimentacao = @DataDaUltimaMovimentacao, Valor = @Valor,CodigoCategoria = @CodigoCategoria,CodigoTipo = @CodigoTipo,Descricao = @Descricao WHERE Codigo = @Codigo", movimentacaoRecorrente);
        }

        public List<MovimentacaoRecorrente> SelectSQL(
           Guid? codigo = null,
           Guid? codigoTipo = null,
           Guid? codigoCategoria = null)
        {
            StringBuilder sql = new();
            sql.AppendLine("SELECT A.*,B.*,C.* FROM MovimentacaoRecorrente A (NOLOCK)");
            sql.AppendLine("INNER JOIN Categoria B (NOLOCK) ON A.CodigoCategoria = B.Codigo");
            sql.AppendLine("INNER JOIN Tipo C (NOLOCK) ON A.CodigoTipo = C.Codigo");
            sql.AppendLine("WHERE A.Codigo = ISNULL(@Codigo,A.Codigo)");
            sql.AppendLine("AND A.CodigoTipo = ISNULL(@CodigoTipo,A.CodigoTipo)");
            sql.AppendLine("AND A.CodigoCategoria = ISNULL(@CodigoCategoria,A.CodigoCategoria)");
            sql.AppendLine("ORDER BY A.DataDaPrimeiraMovimentacao DESC");

            using var conn = new SqlConnection(ConnectionString);
            return conn.Query<MovimentacaoRecorrente, Categoria, Tipo, MovimentacaoRecorrente>(sql.ToString(),
                (movimentacaoRecorrente, categoria, tipo) =>
                {
                    movimentacaoRecorrente.Categoria = categoria;
                    movimentacaoRecorrente.Tipo = tipo;


                    return movimentacaoRecorrente;

                }, new
                {
                    @Codigo = codigo,
                    @CodigoTipo = codigoTipo,
                    @CodigoCategoria = codigoCategoria
                }, splitOn: "Codigo").ToList();

        }
        public void DeleteSQL(Guid? codigo = null)
        {
            if (codigo is null)
                throw new Exception("Nenhum parâmetro foi fornecido para peração de exclusão de movimentação");

            using var conn = new SqlConnection(ConnectionString);
            conn.Execute("delete from MovimentacaoRecorrente where Codigo = ISNULL(@Codigo,Codigo)", new { @Codigo = codigo });
        }

        private void InsertSQL(MovimentacaoRecorrente movimentacaoRecorrente)
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Execute("insert into MovimentacaoRecorrente (QuantidadeMovimentacao,DespesaFixa,Codigo,DataDaPrimeiraMovimentacao,DataDaUltimaMovimentacao,DataHora,Valor,CodigoCategoria,CodigoTipo,Descricao) values (@QuantidadeMovimentacao,@DespesaFixa,@Codigo,@DataDaPrimeiraMovimentacao,@DataDaUltimaMovimentacao,@DataHora,@Valor,@CodigoCategoria,@CodigoTipo,@Descricao)", movimentacaoRecorrente);
        }

        #endregion

    }
}
