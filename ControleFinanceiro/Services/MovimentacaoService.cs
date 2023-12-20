using ControleFinanceiro.Entities;
using ControleFinanceiro.Models;
using ControleFinanceiro.ValueObjects;
using Dapper;
using System.Data.SqlClient;
using System.Text;
using System.Transactions;

namespace ControleFinanceiro.Services
{
    public class MovimentacaoService : BaseService
    {
        private readonly TipoService _tipoService;
        private readonly CategoriaService _categoriaService;
        private readonly CartaoService _cartaoService;

        public MovimentacaoService(IConfiguration config,
            TipoService tipoService,
            CategoriaService categoriaService,
            CartaoService cartaoService) : base(config)
        {
            _tipoService = tipoService;
            _categoriaService = categoriaService;
            _cartaoService = cartaoService;
        }

        public void AbrirTransacaoParaInserirNovaMovimentacao(MovimentacaoNovaViewModel model)
        {
            using TransactionScope scope = new();
            var tipo = _tipoService.Obter(model.CodigoTipo).First();
            model.Valor = tipo.Descricao != TipoDeMovimentacao.Entrada ? model.Valor * -1 : model.Valor;
            model.DataMovimentacao = model.CodigoCartao is null ? model.DataDaCompra : _cartaoService.ObterDataDaParcela(model.CodigoCartao.Value, model.DataDaCompra);
            InserirMovimentacao(model);
            scope.Complete();
        }
        public void InserirMovimentacao(MovimentacaoNovaViewModel model)
        {
            Movimentacao movimentacao = new()
            {
                Codigo = Guid.NewGuid(),
                CodigoCartao = model.CodigoCartao,
                DataDaCompra = CommonHelper.ConverterDateOnlyParaDateTime(model.DataDaCompra),
                CodigoParcelamento = model.CodigoParcelamento,
                CodigoMovimentacaoRecorrente = model.CodigoMovimentacaoRecorrente,
                Descricao = model.Descricao,
                DespesaFixa = model.DespesaFixa,
                CodigoCategoria = model.CodigoCategoria,
                CodigoTipo = model.CodigoTipo,
                DataMovimentacao = CommonHelper.ConverterDateOnlyParaDateTime(model.DataMovimentacao),
                Valor = model.Valor,
                DataHora = DateTime.Now
            };
            InsertSQL(movimentacao);

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

            var movimentacoes = SelectSQL(ano: short.Parse(ano), mes: short.Parse(mes));
            var dataRef = new DateTime(int.Parse(ano), int.Parse(mes), 1);
            dataRef = dataRef.AddMonths(-1);
            var movimentacoesDoMesAnterior = SelectSQL(ano: dataRef.Year, mes: dataRef.Month);
            resultado.Movimentacoes = DePara(movimentacoes).OrderByDescending(x => x.DataDaCompra);
            resultado.Periodo = $"{mes}/{ano}";
            resultado.ValorTotalSaida = movimentacoes.Where(x => x.Valor < 0).Select(x => x.Valor).Sum();
            resultado.ValorTotalEntrada = movimentacoes.Where(x => x.Valor > 0).Select(x => x.Valor).Sum();
            resultado.ValorTotalDeParcelamento = movimentacoes.Where(x => x.Valor < 0 && x.CodigoParcelamento is not null).Select(x => x.Valor).Sum();

            resultado.Valor = movimentacoes.Select(x => x.Valor).Sum();

            var categorias = movimentacoes.Where(x => x.Tipo.Descricao == TipoDeMovimentacao.Saida).Select(x => x.Categoria.Descricao).Distinct();

            foreach (var item in categorias)
            {
                resultado.TotaisPorCategoria.Add(new DashboardDividaPorCategoriaViewModel
                {
                    Categoria = item,
                    Valor = CommonHelper.TransformarDecimalNegativoEmPositivo(movimentacoes.Where(x => x.Tipo.Descricao == TipoDeMovimentacao.Saida && x.Categoria.Descricao == item).Select(x => x.Valor).Sum()),
                    ValorMesAnterior = CommonHelper.TransformarDecimalNegativoEmPositivo(movimentacoesDoMesAnterior.Where(x => x.Categoria.Descricao == item).Select(x => x.Valor).Sum())
                });
            }

            var categoriasParcelamentos = movimentacoes.Where(x => x.CodigoParcelamento is not null && x.Tipo.Descricao == TipoDeMovimentacao.Saida)
                .Select(x => x.Categoria.Descricao).Distinct();

            foreach (var item in categoriasParcelamentos)
            {
                resultado.TotaisPorCategoriaParcelamentos.Add(new DashboardDividaPorCategoriaViewModel
                {
                    Categoria = item,
                    Valor = CommonHelper.TransformarDecimalNegativoEmPositivo(movimentacoes.Where(x => x.CodigoParcelamento is not null && x.Categoria.Descricao == item).Select(x => x.Valor).Sum())
                });
            }


            var categoriasPais = movimentacoes.Where(x => x.Categoria?.CategoriaPai is not null).Select(x => x.Categoria.CategoriaPai.Descricao).Distinct().ToList();
            foreach (var catPai in categoriasPais)
            {
                var somatoria = CommonHelper.TransformarDecimalNegativoEmPositivo(movimentacoes.Where(x => x.Categoria.CategoriaPai != null && x.Categoria.CategoriaPai.Descricao == catPai).Select(x => x.Valor).Sum());
                resultado.TotaisPorCategoriaPai.Add(new DashboardDividaPorCategoriaViewModel
                {
                    Categoria = catPai,
                    Valor = somatoria,
                    PercentualSobreRenda = resultado.ValorTotalEntrada > 0 ? somatoria / resultado.ValorTotalEntrada : 0
                });
            }

            resultado.TotaisPorCategoria = resultado.TotaisPorCategoria.OrderByDescending(x => x.Valor).ToList();
            resultado.TotaisPorCategoriaPai = resultado.TotaisPorCategoriaPai.OrderByDescending(x => x.Valor).ToList();
            resultado.TotaisPorCategoriaParcelamentos = resultado.TotaisPorCategoriaParcelamentos.OrderByDescending(x => x.Valor).ToList();
            resultado.ValorAmortizadoNoMes = resultado.Movimentacoes.Where(x => x.UltimaParcela).Select(x => x.Valor).Sum();
            resultado.ValorContasNaoEssenciais = resultado.Movimentacoes.Where(x => !x.DespesaFixa && x.Valor < 0).Select(x => x.Valor).Sum();
            resultado.ValorContasEssenciais = resultado.Movimentacoes.Where(x => x.DespesaFixa && x.Valor < 0).Select(x => x.Valor).Sum();

            return resultado;
        }


        private IEnumerable<MovimentacaoItemViewModel> DePara(List<Movimentacao> movimentacoes)
        {
            foreach (var item in movimentacoes)
            {

                decimal valorAmortizacao = 0;
                if (item.CodigoParcelamento is not null)
                    valorAmortizacao = GerarValorAmortizacao(item);

                yield return new MovimentacaoItemViewModel
                {
                    Codigo = item.Codigo,
                    CorDoTextDeValor = item.Valor < 0 ? "text-danger" : "text-success",
                    MeioDeParcelamento = CommonHelper.FormatarDescricaoCartao(item.Cartao),
                    Categoria = item.Categoria.Descricao,
                    Tipo = item.Tipo.Descricao,
                    DataDaCompra = item.DataDaCompra,
                    DataMovimentacao = item.DataMovimentacao,
                    Descricao = item.Descricao,
                    Valor = item.Valor,
                    Baixado = item.Baixado,
                    DespesaFixa = item.DespesaFixa,
                    UltimaParcela = valorAmortizacao < 0
                };
            }
        }




        public void AbrirTransacaoParaAtualizarMovimentacao(MovimentacaoEdicaoViewModel model)
        {
            if (model.Codigo == Guid.Empty)
                throw new Exception("O código da movimentação não foi informado");

            using TransactionScope scope = new();
            var movimentacaoParaEditar = SelectSQL(model.Codigo).First();
            var tipoCadastradoNaBase = _tipoService.Obter(model.CodigoTipo).First();
            if (movimentacaoParaEditar.CodigoParcelamento is null)
            {
                if (model.CodigoTipo == Guid.Empty)
                    throw new Exception("O código do tipo de movimentação não foi informado");

                if (model.CodigoCategoria == Guid.Empty)
                    throw new Exception("O código da categoria movimentação não foi informado");

                movimentacaoParaEditar.CodigoTipo = tipoCadastradoNaBase.Codigo;
                movimentacaoParaEditar.CodigoCategoria = model.CodigoCategoria;
                if (tipoCadastradoNaBase.Descricao == TipoDeMovimentacao.Saida)
                {
                    movimentacaoParaEditar.CodigoCartao = model.CodigoCartao;
                    movimentacaoParaEditar.DataMovimentacao = CommonHelper.ConverterDateOnlyParaDateTime(model.CodigoCartao is null ? model.DataDaCompra : _cartaoService.ObterDataDaParcela(model.CodigoCartao.Value, model.DataDaCompra));
                }
                movimentacaoParaEditar.Valor = tipoCadastradoNaBase.Descricao != TipoDeMovimentacao.Entrada ? model.Valor * -1 : model.Valor;
                movimentacaoParaEditar.DataDaCompra = CommonHelper.ConverterDateOnlyParaDateTime(model.DataDaCompra);
                movimentacaoParaEditar.DespesaFixa = model.DespesaFixa;
            }
            else
            {
                //se for parcelamento
                movimentacaoParaEditar.Valor = model.Valor * -1;
            }
            movimentacaoParaEditar.Descricao = model.Descricao;

            Update(movimentacaoParaEditar);
            scope.Complete();
        }

        public MovimentacaoEdicaoViewModel ObterParaEditar(Guid codigo)
        {
            var movimentacaoParaEditar = SelectSQL(codigo).First();
            return new MovimentacaoEdicaoViewModel
            {
                Codigo = movimentacaoParaEditar.Codigo,
                CodigoParcelamento = movimentacaoParaEditar.CodigoParcelamento,
                DataDaCompra = DateOnly.FromDateTime(movimentacaoParaEditar.DataDaCompra),
                CodigoCartao = movimentacaoParaEditar.CodigoCartao,
                CodigoCategoria = movimentacaoParaEditar.CodigoCategoria,
                CodigoTipo = movimentacaoParaEditar.CodigoTipo,
                DataMovimentacao = DateOnly.FromDateTime(movimentacaoParaEditar.DataMovimentacao),
                DespesaFixa = movimentacaoParaEditar.DespesaFixa,
                Descricao = movimentacaoParaEditar.Descricao,
                Valor = Math.Abs(movimentacaoParaEditar.Valor)
            };
        }

        public decimal GerarValorAmortizacao(Movimentacao parcela)
        {
            var valorRestanteDoParcelamento = SelectSQL(
                codigosParcelamentos: new List<Guid> { parcela.CodigoParcelamento.Value },
                dataMaiorQue: parcela.DataMovimentacao,
                baixado: false)
                .Select(x => x.Valor)
                .Sum();

            if (valorRestanteDoParcelamento == 0 && parcela.Valor < 0)
                return parcela.Valor;

            return 0;
        }

        public void BaixarOuReverter(Guid codigoMovimentacao)
        {
            var movimentacao = SelectSQL(codigo: codigoMovimentacao).First();

            if (movimentacao.Valor > 0)
                return;

            movimentacao.Baixado = !movimentacao.Baixado;
            Update(movimentacao);
        }

        #region Crud

        public void Update(Movimentacao movimentacao)
        {
            if (movimentacao.Codigo == Guid.Empty)
                throw new Exception("O código da movimentação não foi informado");

            if (movimentacao.CodigoTipo == Guid.Empty)
                throw new Exception("O código do tipo de movimentação não foi informado");

            if (movimentacao.CodigoCategoria == Guid.Empty)
                throw new Exception("O código da categoria movimentação não foi informado");

            using var conn = new SqlConnection(ConnectionString);
            conn.Execute("UPDATE Movimentacao SET Baixado = @Baixado, DespesaFixa = @DespesaFixa, DataDaCompra = @DataDaCompra, CodigoCartao = @CodigoCartao, DataMovimentacao = @DataMovimentacao,Valor = @Valor,CodigoCategoria = @CodigoCategoria,CodigoTipo = @CodigoTipo,Descricao = @Descricao WHERE Codigo = @Codigo", movimentacao);
        }

        public List<Movimentacao> SelectSQL(
           Guid? codigo = null,
           Guid? codigoTipo = null,
           Guid? codigoCategoria = null,
           DateTime? dataMaiorOuIgualA = null,
           DateTime? dataMaiorQue = null,
           DateTime? dataMenorQue = null,
           bool somenteParcelamentos = false,
           int? mes = null,
           int? ano = null,
           bool? baixado = null,
           List<Guid>? codigosParcelamentos = null)
        {
            StringBuilder sql = new();
            sql.AppendLine("SELECT A.*,B.*,C.*,D.*,E.*,F.*,G.* FROM Movimentacao A (NOLOCK)");
            sql.AppendLine("INNER JOIN Categoria B (NOLOCK) ON A.CodigoCategoria = B.Codigo");
            sql.AppendLine("INNER JOIN Tipo C (NOLOCK) ON A.CodigoTipo = C.Codigo");
            sql.AppendLine("LEFT JOIN Cartao D (NOLOCK) ON A.CodigoCartao = D.Codigo");
            sql.AppendLine("LEFT JOIN CartaoBandeira E (NOLOCK) ON D.CodigoCartaoBandeira = E.Codigo");
            sql.AppendLine("LEFT JOIN CartaoTipo F (NOLOCK) ON D.CodigoCartaoTipo = F.Codigo");
            sql.AppendLine("LEFT JOIN Categoria G (NOLOCK) ON B.CodigoCategoriaPai = G.Codigo");
            sql.AppendLine("WHERE A.Codigo = ISNULL(@Codigo,A.Codigo)");
            sql.AppendLine("AND A.CodigoTipo = ISNULL(@CodigoTipo,A.CodigoTipo)");
            sql.AppendLine("AND A.CodigoCategoria = ISNULL(@CodigoCategoria,A.CodigoCategoria)");

            if (dataMaiorOuIgualA is not null)
                sql.AppendLine("AND A.DataMovimentacao >= @DataMaiorOuIgualA");

            if (dataMaiorQue is not null)
                sql.AppendLine("AND A.DataMovimentacao > @dataMaiorQue");

            if (dataMenorQue is not null)
                sql.AppendLine("AND A.DataMovimentacao < @dataMenorQue");

            if (somenteParcelamentos)
                sql.AppendLine("AND A.CodigoParcelamento IS NOT NULL");

            if (mes is not null)
                sql.AppendLine("AND MONTH(A.DataMovimentacao) = @Mes");

            if (ano is not null)
                sql.AppendLine("AND YEAR(A.DataMovimentacao) = @Ano");

            if (baixado is not null)
                sql.AppendLine("AND A.Baixado = @Baixado");

            if (codigosParcelamentos is not null)
                sql.AppendLine("AND A.CodigoParcelamento IN @CodigosParcelamentos");

            sql.AppendLine("ORDER BY A.DataMovimentacao DESC");

            using var conn = new SqlConnection(ConnectionString);
            return conn.Query<Movimentacao, Categoria, Tipo, Cartao, CartaoBandeira, CartaoTipo, Categoria, Movimentacao>(sql.ToString(),
                (movimentacao, categoria, tipo, cartao, cartaoBandeira, cartaoTipo, categoriaPai) =>
                {
                    if (categoriaPai is not null)
                        categoria.CategoriaPai = categoriaPai;

                    movimentacao.Categoria = categoria;
                    movimentacao.Tipo = tipo;

                    if (cartao is not null)
                    {
                        cartao.CartaoTipo = cartaoTipo;
                        cartao.CartaoBandeira = cartaoBandeira;
                        movimentacao.Cartao = cartao;
                    }

                    return movimentacao;

                }, new
                {
                    @Codigo = codigo,
                    @CodigoTipo = codigoTipo,
                    @CodigoCategoria = codigoCategoria,
                    @DataMaiorOuIgualA = dataMaiorOuIgualA,
                    @Mes = mes,
                    @Ano = ano,
                    @Baixado = baixado,
                    @dataMaiorQue = dataMaiorQue,
                    @dataMenorQue = dataMenorQue,
                    @CodigosParcelamentos = codigosParcelamentos
                }, splitOn: "Codigo").ToList();

        }
        public void DeleteSQL(Guid? codigo = null, Guid? codigoParcelamento = null, Guid? codigoMovimentacaoRecorrente = null)
        {
            if (codigo is null && codigoParcelamento is null && codigoMovimentacaoRecorrente is null)
                throw new Exception("Nenhum parâmetro foi fornecido para peração de exclusão de movimentação");

            using var conn = new SqlConnection(ConnectionString);
            conn.Execute("delete from Movimentacao where Codigo = ISNULL(@Codigo,Codigo) and CodigoParcelamento = ISNULL(@CodigoParcelamento,CodigoParcelamento) and CodigoMovimentacaoRecorrente = ISNULL(@CodigoMovimentacaoRecorrente,CodigoMovimentacaoRecorrente)", new { @Codigo = codigo, @CodigoParcelamento = codigoParcelamento, @CodigoMovimentacaoRecorrente = codigoMovimentacaoRecorrente });
        }

        private void InsertSQL(Movimentacao movimentacao)
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Execute("insert into Movimentacao (CodigoMovimentacaoRecorrente,DespesaFixa,Codigo,DataDaCompra,CodigoCartao,CodigoParcelamento,DataMovimentacao,DataHora,Valor,CodigoCategoria,CodigoTipo,Descricao) values (@CodigoMovimentacaoRecorrente,@DespesaFixa,@Codigo,@DataDaCompra,@CodigoCartao,@CodigoParcelamento,@DataMovimentacao,@DataHora,@Valor,@CodigoCategoria,@CodigoTipo,@Descricao)", movimentacao);
        }

        #endregion

    }
}
