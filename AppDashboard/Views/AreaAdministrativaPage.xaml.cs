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
                    "Não foi possível carregar os dados da tabela rhdataset.\n\nVerifique sua conexão.",
                    "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnMenuClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet(
                "Opções",
                "Cancelar",
                null,
                "🔄 Atualizar Lista",
                "🔍 Diagnóstico da Tabela",
                "📊 Estatísticas Detalhadas",
                "📋 Exportar Dados",
                "ℹ️ Sobre o Sistema");

            switch (action)
            {
                case "🔄 Atualizar Lista":
                    await _viewModel.CarregarDadosAsync();
                    break;

                case "🔍 Diagnóstico da Tabela":
                    if (_viewModel.DiagnosticoCommand.CanExecute(null))
                        _viewModel.DiagnosticoCommand.Execute(null);
                    break;

                case "📊 Estatísticas Detalhadas":
                    await MostrarEstatisticasDetalhadas();
                    break;

                case "📋 Exportar Dados":
                    await DisplayAlert(
                        "Exportar Dados",
                        "Funcionalidade de exportação será implementada em breve.\n\n" +
                        "Formatos suportados:\n" +
                        "• Excel (.xlsx)\n" +
                        "• CSV (.csv)\n" +
                        "• PDF (.pdf)",
                        "OK");
                    break;

                case "ℹ️ Sobre o Sistema":
                    await DisplayAlert(
                        "RH Dashboard",
                        "Sistema de Gestão de Recursos Humanos\n" +
                        "Versão 1.0\n\n" +
                        "📊 Fonte de Dados:\n" +
                        "• Banco: rhsenior_heicomp\n" +
                        "• Tabela: rhdataset\n" +
                        "• Coluna Status: Descrição (Situação)\n\n" +
                        "🏥 Situações Disponíveis:\n" +
                        "• ✅ Trabalhando\n" +
                        "• ❌ Demitido\n" +
                        "• 🏥 Aposentadoria por Invalidez\n" +
                        "• 🤕 Auxílio Doença\n\n" +
                        "🔧 Recursos:\n" +
                        "• Visualização em tempo real\n" +
                        "• Filtros avançados\n" +
                        "• Estatísticas detalhadas\n" +
                        "• Pull to refresh",
                        "Fechar");
                    break;
            }
        }

        private async Task MostrarEstatisticasDetalhadas()
        {
            var total = _viewModel.TotalUsuarios;
            var trabalhando = _viewModel.TotalTrabalhando;
            var demitidos = _viewModel.TotalDemitidos;
            var aposentados = _viewModel.TotalAposentados;
            var auxilioDoenca = _viewModel.TotalAuxilioDoenca;

            var mensagem = $"📊 ESTATÍSTICAS GERAIS\n\n" +
                          $"👥 Total de Colaboradores: {total}\n\n" +
                          $"DISTRIBUIÇÃO POR SITUAÇÃO:\n" +
                          $"✅ Trabalhando: {trabalhando}\n" +
                          $"❌ Demitidos: {demitidos}\n" +
                          $"🏥 Aposentados (Invalidez): {aposentados}\n" +
                          $"🤕 Auxílio Doença: {auxilioDoenca}\n\n";

            if (total > 0)
            {
                var percTrabalhando = (trabalhando * 100.0) / total;
                var percDemitidos = (demitidos * 100.0) / total;
                var percAposentados = (aposentados * 100.0) / total;
                var percAuxilio = (auxilioDoenca * 100.0) / total;

                mensagem += $"📈 PERCENTUAIS:\n" +
                           $"• Trabalhando: {percTrabalhando:F1}%\n" +
                           $"• Demitidos: {percDemitidos:F1}%\n" +
                           $"• Aposentados: {percAposentados:F1}%\n" +
                           $"• Auxílio Doença: {percAuxilio:F1}%";
            }

            await DisplayAlert("Estatísticas Detalhadas", mensagem, "Fechar");
        }
    }
}