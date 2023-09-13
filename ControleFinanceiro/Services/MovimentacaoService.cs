using ControleFinanceiro.Entities;
using ControleFinanceiro.Models;
using Dapper;
using System.Data.SqlClient;
using System.Text;
using System.Transactions;

namespace ControleFinanceiro.Services
{
    public class MovimentacaoService : BaseService
    {
        private readonly MovimentacaoTipoService _movimentacaoTipoService;
        private readonly MovimentacaoCategoriaService _movimentacaoCategoriaService;

        public MovimentacaoService(IConfiguration config, MovimentacaoTipoService movimentacaoTipoService, MovimentacaoCategoriaService movimentacaoCategoriaService) : base(config)
        {
            _movimentacaoTipoService = movimentacaoTipoService;
            _movimentacaoCategoriaService = movimentacaoCategoriaService;

        }

        public void AbrirTransacaoParaInserirNovaMovimentacao(MovimentacaoNovaViewModel model)
        {
            using TransactionScope scope = new();
            var tipo = _movimentacaoTipoService.Obter(model.CodigoMovimentacaoTipo).First();
            model.Valor = tipo.Descricao.ToLower() != "entrada" ? model.Valor * -1 : model.Valor;
            InserirMovimentacao(model);
            scope.Complete();
        }
        public void InserirMovimentacao(MovimentacaoNovaViewModel model)
        {
            Movimentacao movimentacao = new()
            {
                Codigo = Guid.NewGuid(),
                CodigoParcelamento = model.CodigoParcelamento,
                Descricao = model.Descricao,
                CodigoMovimentacaoCategoria = model.CodigoMovimentacaoCategoria,
                CodigoMovimentacaoTipo = model.CodigoMovimentacaoTipo,
                DataMovimentacao = model.DataMovimentacao.ToDateTime(TimeOnly.Parse("10:00 PM")),
                Valor = model.Valor,
                DataHora = DateTime.Now
            };
            using var conn = new SqlConnection(ConnectionString);
            conn.Execute("insert into Movimentacao (Codigo,CodigoParcelamento,DataMovimentacao,DataHora,Valor,CodigoMovimentacaoCategoria,CodigoMovimentacaoTipo,Descricao) values (@Codigo,@CodigoParcelamento,@DataMovimentacao,@DataHora,@Valor,@CodigoMovimentacaoCategoria,@CodigoMovimentacaoTipo,@Descricao)", movimentacao);

        }
        public MovimentacaoViewModel BuscarMovimentacoes(MovimentacaoPesquisaViewModel? model = null)
        {
            MovimentacaoViewModel resultado = new()
            {
                Movimentacoes = new List<MovimentacaoItemViewModel> { }
            };
            string mes = DateTime.Now.Month.ToString().PadLeft(2, '0');
            string ano = DateTime.Now.Year.ToString();

            if (model?.Periodo is not null)
            {
                if (model.Periodo.Length is 7)
                {
                    mes = model.Periodo.Split("/")[0].PadLeft(2, '0');
                    ano = model.Periodo.Split("/")[1];

                    if (model.Direcao is not null)
                    {
                        var d = new DateTime(int.Parse(ano), int.Parse(mes), 1);
                        int direcaoInt = 1;

                        if (model.Direcao == "Anterior")
                            direcaoInt = -1;

                        d = d.AddMonths(direcaoInt);

                        mes = d.Month.ToString().PadLeft(2, '0');
                        ano = d.Year.ToString();
                    }

                }
            }

            using (var conn = new SqlConnection(ConnectionString))
                resultado.Movimentacoes = conn.Query<MovimentacaoItemViewModel>("SELECT A.*,B.Descricao Tipo, C.Descricao Categoria FROM Movimentacao A INNER JOIN MovimentacaoTipo B ON A.CodigoMovimentacaoTipo = B.Codigo INNER JOIN MovimentacaoCategoria C ON A.CodigoMovimentacaoCategoria = C.Codigo  WHERE YEAR(A.DataMovimentacao) = @Ano AND MONTH(A.DataMovimentacao) = @Mes ORDER BY DataMovimentacao DESC", new { @Ano = ano, @Mes = int.Parse(mes) }).ToList();

            resultado.Periodo = $"{mes}/{ano}";

            foreach (var item in resultado.Movimentacoes)
                if (item.Valor < 0)
                    item.CorDoTextDeValor = "text-danger";
                else
                    item.CorDoTextDeValor = "text-success";

            resultado.Valor = resultado.Movimentacoes.Select(x => x.Valor).Sum();


            return resultado;
        }


        public IList<Movimentacao> Obter(
            Guid? codigo = null,
            Guid? codigoTipo = null,
            Guid? codigoCategoria = null,
            DateTime? dataMaiorOuIgualA = null,
            bool somenteParcelamentos = false,
            short? mes = null,
            short? ano = null)
        {
            StringBuilder sql = new();
            sql.AppendLine("SELECT A.*,B.*,C.* FROM Movimentacao A (NOLOCK) " +
                "INNER JOIN MovimentacaoCategoria B (NOLOCK) ON A.CodigoMovimentacaoCategoria = B.Codigo " +
                "INNER JOIN MovimentacaoTipo C (NOLOCK) ON A.CodigoMovimentacaoTipo = C.Codigo " +
                "WHERE A.Codigo = ISNULL(@Codigo,A.Codigo) " +
                "AND A.CodigoMovimentacaoTipo = ISNULL(@CodigoMovimentacaoTipo,A.CodigoMovimentacaoTipo) " +
                "AND A.CodigoMovimentacaoCategoria = ISNULL(@CodigoMovimentacaoCategoria,A.CodigoMovimentacaoCategoria)  " +
                "AND A.DataMovimentacao >= ISNULL(@DataMaiorOuIgualA,A.DataMovimentacao) ");

            if (somenteParcelamentos)
                sql.AppendLine("AND A.CodigoParcelamento IS NOT NULL");

            if (mes is not null)
                sql.AppendLine("AND MONTH(A.DataMovimentacao) = ISNULL(@Mes,MONTH(A.DataMovimentacao))");

            if (ano is not null)
                sql.AppendLine("AND YEAR(A.DataMovimentacao) = ISNULL(@Ano,YEAR(A.DataMovimentacao))");

            sql.AppendLine("ORDER BY A.DataMovimentacao DESC");

            using var conn = new SqlConnection(ConnectionString);
            return conn.Query<Movimentacao, MovimentacaoCategoria, MovimentacaoTipo, Movimentacao>(sql.ToString(), (mov, cat, tipo) =>
            {
                mov.MovimentacaoCategoria = cat;
                mov.MovimentacaoTipo = tipo;

                return mov;

            }, new
            {
                @Codigo = codigo,
                @CodigoMovimentacaoTipo = codigoTipo,
                @CodigoMovimentacaoCategoria = codigoCategoria,
                @DataMaiorOuIgualA = dataMaiorOuIgualA,
                @Mes = mes,
                @Ano = ano
            }, splitOn: "Codigo").ToList();

        }

        public void AbrirTransacaoParaAtualizarMovimentacao(MovimentacaoEdicaoViewModel model)
        {
            using TransactionScope scope = new();
            var movimentacaoParaEditar = Obter(model.Codigo).First();
            var tipo = _movimentacaoTipoService.Obter(model.CodigoMovimentacaoTipo).First();
            var categoria = _movimentacaoCategoriaService.Obter(model.CodigoMovimentacaoCategoria).First();
            movimentacaoParaEditar.CodigoMovimentacaoTipo = tipo.Codigo;
            movimentacaoParaEditar.CodigoMovimentacaoCategoria = categoria.Codigo;
            movimentacaoParaEditar.Descricao = model.Descricao;
            movimentacaoParaEditar.DataMovimentacao = model.DataMovimentacao.ToDateTime(TimeOnly.Parse("10:00 PM"));
            movimentacaoParaEditar.Valor = tipo.Descricao.ToLower() != "entrada" ? model.Valor * -1 : model.Valor;
            EditarMovimentacao(movimentacaoParaEditar);
            scope.Complete();
        }

        public void EditarMovimentacao(Movimentacao movimentacao)
        {
            if (movimentacao.Codigo == Guid.Empty)
                throw new Exception("O código da movimentação não foi informado");

            using var conn = new SqlConnection(ConnectionString);
            conn.Execute("update Movimentacao set DataMovimentacao = @DataMovimentacao,Valor = @Valor,CodigoMovimentacaoCategoria = @CodigoMovimentacaoCategoria,CodigoMovimentacaoTipo = @CodigoMovimentacaoTipo,Descricao = @Descricao WHERE Codigo = @Codigo", movimentacao);
        }

        public MovimentacaoEdicaoViewModel ObterParaEditar(Guid codigo)
        {
            var movimentacaoParaEditar = Obter(codigo).First();
            return new MovimentacaoEdicaoViewModel
            {
                Codigo = movimentacaoParaEditar.Codigo,
                CodigoMovimentacaoCategoria = movimentacaoParaEditar.CodigoMovimentacaoCategoria,
                CodigoMovimentacaoTipo = movimentacaoParaEditar.CodigoMovimentacaoTipo,
                DataMovimentacao = DateOnly.FromDateTime(movimentacaoParaEditar.DataMovimentacao),
                Descricao = movimentacaoParaEditar.Descricao,
                Valor = Math.Abs(movimentacaoParaEditar.Valor)
            };
        }

        //private DateOnly ObterDiaFinal(DateOnly data) => DateOnly.FromDateTime(new DateTime(data.Year, data.Month, DateTime.DaysInMonth(data.Year, data.Month)));

    }
}
