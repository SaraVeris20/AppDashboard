using AppDashboard.ViewModels;
using AppDashboard.Services;

namespace AppDashboard.Views
{
    public partial class AreaAdministrativaPage : ContentPage
    {
        private readonly AreaAdministrativaViewModel _viewModel;

        public AreaAdministrativaPage(UsuarioService usuarioService)
        {
            InitializeComponent();

            _viewModel = new AreaAdministrativaViewModel(usuarioService);
            BindingContext = _viewModel;

            System.Diagnostics.Debug.WriteLine("✅ AreaAdministrativaPage criada");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            System.Diagnostics.Debug.WriteLine("📱 OnAppearing - Carregando dados...");

            try
            {
                await _viewModel.CarregarDadosAsync();
                System.Diagnostics.Debug.WriteLine($"✅ Dados carregados! Exibindo {_viewModel.UsuariosFiltrados?.Count ?? 0} usuários");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro: {ex.Message}");
                await DisplayAlert("Erro", $"Não foi possível carregar os dados.\n\n{ex.Message}", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnAdicionarUsuarioClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(AdicionarUsuarioPage));
        }

        private async void OnFiltroClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("🔍 Abrindo menu de filtro...");

            string action = await DisplayActionSheet(
                "Filtrar por Situação",
                "Cancelar",
                null,
                "📋 Todos",
                "✅ Trabalhando",
                "❌ Demitidos",
                "🏥 Aposentados por Invalidez",
                "🤕 Auxílio Doença");

            if (string.IsNullOrEmpty(action) || action == "Cancelar")
                return;

            string situacao = action switch
            {
                "📋 Todos" => "Todos",
                "✅ Trabalhando" => "Trabalhando",
                "❌ Demitidos" => "Demitidos",
                "🏥 Aposentados por Invalidez" => "Aposentadoria por Invalidez",
                "🤕 Auxílio Doença" => "Auxílio Doença",
                _ => "Todos"
            };

            System.Diagnostics.Debug.WriteLine($"🎯 Aplicando filtro: {situacao}");
            _viewModel.SituacaoSelecionada = situacao;

            await Task.Delay(300);
            System.Diagnostics.Debug.WriteLine($"📊 Resultado: {_viewModel.UsuariosFiltrados?.Count ?? 0} usuários");
        }

        private async void OnMenuClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet(
                "Opções",
                "Cancelar",
                null,
                "🔄 Atualizar Lista",
                "📊 Estatísticas",
                "🔍 Ver Situações do Banco",
                "ℹ️ Sobre");

            switch (action)
            {
                case "🔄 Atualizar Lista":
                    await _viewModel.CarregarDadosAsync();
                    break;

                case "📊 Estatísticas":
                    await MostrarEstatisticas();
                    break;

                case "🔍 Ver Situações do Banco":
                    await MostrarSituacoesDoBanco();
                    break;

                case "ℹ️ Sobre":
                    await DisplayAlert(
                        "RH Dashboard",
                        $"Sistema de Gestão de RH - v1.0\n\n" +
                        $"📊 Banco: rhsenior_heicomp\n" +
                        $"📋 Tabela: rhdataset\n" +
                        $"👥 Total: {_viewModel.TotalUsuarios} usuários\n\n" +
                        $"Recursos:\n" +
                        $"• Listagem de colaboradores\n" +
                        $"• Filtros por situação\n" +
                        $"• Busca em tempo real\n" +
                        $"• Estatísticas detalhadas",
                        "OK");
                    break;
            }
        }

        private async Task MostrarEstatisticas()
        {
            var mensagem = $"📊 ESTATÍSTICAS GERAIS\n\n" +
                          $"👥 Total: {_viewModel.TotalUsuarios}\n\n" +
                          $"POR SITUAÇÃO:\n" +
                          $"✅ Trabalhando: {_viewModel.TotalTrabalhando}\n" +
                          $"❌ Demitidos: {_viewModel.TotalDemitidos}\n" +
                          $"🏥 Aposentados: {_viewModel.TotalAposentados}\n" +
                          $"🤕 Auxílio Doença: {_viewModel.TotalAuxilioDoenca}";

            if (_viewModel.TotalUsuarios > 0)
            {
                var pTrab = (_viewModel.TotalTrabalhando * 100.0) / _viewModel.TotalUsuarios;
                var pDem = (_viewModel.TotalDemitidos * 100.0) / _viewModel.TotalUsuarios;
                var pApo = (_viewModel.TotalAposentados * 100.0) / _viewModel.TotalUsuarios;
                var pAux = (_viewModel.TotalAuxilioDoenca * 100.0) / _viewModel.TotalUsuarios;

                mensagem += $"\n\n📈 PERCENTUAIS:\n" +
                           $"• Trabalhando: {pTrab:F1}%\n" +
                           $"• Demitidos: {pDem:F1}%\n" +
                           $"• Aposentados: {pApo:F1}%\n" +
                           $"• Auxílio: {pAux:F1}%";
            }

            await DisplayAlert("Estatísticas", mensagem, "OK");
        }

        private async Task MostrarSituacoesDoBanco()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔍 Buscando situações únicas...");

                var service = new UsuarioService();
                var todos = await service.ObterTodosUsuariosAsync();

                // Agrupar por situação
                var situacoes = todos
                    .Where(u => !string.IsNullOrEmpty(u.DescricaoSituacao))
                    .GroupBy(u => u.DescricaoSituacao)
                    .OrderByDescending(g => g.Count())
                    .Take(15)
                    .ToList();

                var mensagem = $"📋 SITUAÇÕES NO BANCO\n" +
                              $"(Total: {todos.Count} usuários)\n\n";

                foreach (var grupo in situacoes)
                {
                    var emoji = grupo.Key?.ToUpper() switch
                    {
                        var s when s.Contains("TRABALH") => "✅",
                        var s when s.Contains("DEMIT") => "❌",
                        var s when s.Contains("APOSENT") => "🏥",
                        var s when s.Contains("AUXIL") || s.Contains("DOENÇA") || s.Contains("DOENCA") => "🤕",
                        _ => "❓"
                    };

                    mensagem += $"{emoji} '{grupo.Key}'\n   → {grupo.Count()} usuários\n\n";
                }

                mensagem += $"💡 Use essas informações para\n" +
                           $"entender os filtros disponíveis!";

                await DisplayAlert("Situações do Banco", mensagem, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
            }
        }
    }
}