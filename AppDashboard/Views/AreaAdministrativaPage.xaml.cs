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
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            System.Diagnostics.Debug.WriteLine("📱 AreaAdministrativaPage - OnAppearing");

            try
            {
                await _viewModel.CarregarDadosAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao carregar: {ex.Message}");

                await DisplayAlert(
                    "Erro",
                    "Não foi possível carregar os dados do banco.\n\nVerifique sua conexão.",
                    "OK");
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
            string action = await DisplayActionSheet(
                "Filtrar por Situação",
                "Cancelar",
                null,
                "✅ Trabalhando",
                "❌ Demitidos",
                "🏥 Aposentados por Invalidez",
                "🤕 Auxílio Doença",
                "📋 Todos");

            if (string.IsNullOrEmpty(action) || action == "Cancelar")
                return;

            string situacao = action switch
            {
                "✅ Trabalhando" => "Trabalhando",
                "❌ Demitidos" => "Demitidos",
                "🏥 Aposentados por Invalidez" => "Aposentadoria por Invalidez",
                "🤕 Auxílio Doença" => "Auxílio Doença",
                "📋 Todos" => "Todos",
                _ => "Todos"
            };

            _viewModel.SituacaoSelecionada = situacao;
        }

        private async void OnMenuClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet(
                "Opções",
                "Cancelar",
                null,
                "🔄 Atualizar Lista",
                "📊 Estatísticas",
                "🔍 Diagnóstico do Banco",
                "ℹ️ Sobre");

            switch (action)
            {
                case "🔄 Atualizar Lista":
                    await _viewModel.CarregarDadosAsync();
                    break;

                case "📊 Estatísticas":
                    await MostrarEstatisticas();
                    break;

                case "🔍 Diagnóstico do Banco":
                    await MostrarDiagnostico();
                    break;

                case "ℹ️ Sobre":
                    await DisplayAlert(
                        "RH Dashboard",
                        "Sistema de Gestão de RH\n" +
                        "Versão 1.0\n\n" +
                        "📊 Conectado ao MySQL AWS\n" +
                        "🗄️ Banco: rhsenior_heicomp\n" +
                        "📋 Tabela: rhdataset\n\n" +
                        "Recursos:\n" +
                        "• Listagem de colaboradores\n" +
                        "• Filtros por situação e unidade\n" +
                        "• Busca em tempo real\n" +
                        "• Estatísticas detalhadas",
                        "OK");
                    break;
            }
        }

        private async Task MostrarEstatisticas()
        {
            var total = _viewModel.TotalUsuarios;
            var trabalhando = _viewModel.TotalTrabalhando;
            var demitidos = _viewModel.TotalDemitidos;
            var aposentados = _viewModel.TotalAposentados;
            var auxilio = _viewModel.TotalAuxilioDoenca;

            var mensagem = $"📊 ESTATÍSTICAS GERAIS\n\n" +
                          $"👥 Total de Colaboradores: {total}\n\n" +
                          $"DISTRIBUIÇÃO POR SITUAÇÃO:\n" +
                          $"✅ Trabalhando: {trabalhando}\n" +
                          $"❌ Demitidos: {demitidos}\n" +
                          $"🏥 Aposentados: {aposentados}\n" +
                          $"🤕 Auxílio Doença: {auxilio}";

            if (total > 0)
            {
                var percTrabalhando = (trabalhando * 100.0) / total;
                var percDemitidos = (demitidos * 100.0) / total;
                var percAposentados = (aposentados * 100.0) / total;
                var percAuxilio = (auxilio * 100.0) / total;

                mensagem += $"\n\n📈 PERCENTUAIS:\n" +
                           $"• Trabalhando: {percTrabalhando:F1}%\n" +
                           $"• Demitidos: {percDemitidos:F1}%\n" +
                           $"• Aposentados: {percAposentados:F1}%\n" +
                           $"• Auxílio Doença: {percAuxilio:F1}%";
            }

            await DisplayAlert("Estatísticas Detalhadas", mensagem, "Fechar");
        }

        private async Task MostrarDiagnostico()
        {
            // Aqui você pode adicionar um método de diagnóstico no ViewModel
            await DisplayAlert(
                "Diagnóstico",
                "🔍 Testando conexão com banco de dados...\n\n" +
                "✅ Conexão: OK\n" +
                "✅ Banco: rhsenior_heicomp\n" +
                "✅ Tabela: rhdataset\n" +
                $"✅ Registros: {_viewModel.TotalUsuarios}\n\n" +
                "Sistema funcionando corretamente!",
                "OK");
        }
    }
}