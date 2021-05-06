namespace Cambios
{
    using Cambios.Modelos;
    using Cambios.Servicos;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    public partial class Form1 : Form
    {
        #region Atributos

        private List<Rate> Rates;

        private NetWorkService netWorkService;

        private ApiService apiService;

        private DialogService dialogService;

        private DataService dataService;

        #endregion

        public Form1()
        {
            InitializeComponent();
            netWorkService  = new NetWorkService();
            apiService = new ApiService();
            dialogService = new DialogService();
            dataService = new DataService();
            LoadRates();
        }

        private async void LoadRates()
        {
            bool load;

            labelResultado.Text = "A atualizar taxas...";

            var connection = netWorkService.CheckConnection();

            if (!connection.IsSuccess)
            {
                LoadLocalRates();
                load = false;
            }
            else
            {
                await LoadApiRates();
                load = true;
            }

            if (Rates.Count == 0)
            {
                labelResultado.Text = "Não há ligação a internet" + Environment.NewLine + "e não foram previmente carregadas as taxas." + Environment.NewLine + "tente mais tarde!";

                labelStatus.Text = "Primeira inicialização devera ter ligação a internet";

                return;
            }

            comboBoxOrigem.DataSource = Rates;
            comboBoxOrigem.DisplayMember = "Name";

            comboBoxDestino.BindingContext = new BindingContext();

            comboBoxDestino.DataSource = Rates;
            comboBoxDestino.DisplayMember = "Name";


            

            labelResultado.Text = "Taxas Carregadas...";

            if (load)
            {
                labelStatus.Text = string.Format("Taxas carregadas da internet em {0:F}", DateTime.Now);
            }
            else
            {
                labelStatus.Text = string.Format("Taxas carregadas na base de dados.");
            }

            progressBar1.Value = 100;
            ButtonConverter.Enabled = true;
            buttonTroca.Enabled = true;

        }

        private void LoadLocalRates()
        {
            Rates = dataService.GetData();
        }

        private async Task LoadApiRates()
        {
            progressBar1.Value = 0;
            var response = await apiService.GetRates("http://cambios.somee.com", "/api/rates");

            Rates = (List<Rate>)response.Result;

            dataService.DeleteData();

            dataService.SaveData(Rates);
        }

        private void ButtonConverter_Click(object sender, EventArgs e)
        {
            Converter();
        }

        private void Converter()
        {
            if (string.IsNullOrEmpty(textBoxValor.Text))
            {
                dialogService.ShowMessage("Erro","Insira um valor a converter");
                return;
            }
            decimal valor;
            if (!decimal.TryParse(textBoxValor.Text, out valor))
            {
                dialogService.ShowMessage("Erro de converção", "Valor terá de ser numérico");
                return;
            }
            if (comboBoxOrigem.SelectedItem== null)
            {
                dialogService.ShowMessage("Erro", "Tem de escolher uma moeda a converter");
            }
            if (comboBoxDestino.SelectedItem == null)
            {
                dialogService.ShowMessage("Erro", "Tem de escolher uma moeda de destino para converter");
            }

            var taxaOrigem = (Rate) comboBoxOrigem.SelectedItem;

            var taxaDestino = (Rate)comboBoxDestino.SelectedItem;

            var valorConvertido = valor / (decimal)taxaOrigem.TaxRate * (decimal)taxaDestino.TaxRate;

            labelResultado.Text = string.Format("{0} {1:C2} = {2} {3:C2}", taxaOrigem.Code, valor, taxaDestino.Code, valorConvertido);
        }

        private void buttonTroca_Click(object sender, EventArgs e)
        {
            Troca();
        }

        private void Troca()
        {
            var aux = comboBoxOrigem.SelectedItem;            
            comboBoxOrigem.SelectedItem = comboBoxDestino.SelectedItem;
            comboBoxDestino.SelectedItem = aux;
            Converter();
        }
    }
}
