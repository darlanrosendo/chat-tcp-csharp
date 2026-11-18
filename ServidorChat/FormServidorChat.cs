using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace ServidorChat
{
    // MELHORIA: formulário renomeado de Form1 para FormServidorChat.
    // O nome descreve o que a tela é, facilitando encontrar o arquivo no projeto.
    public partial class FormServidorChat : Form
    {
        // MELHORIA: o servidor agora é guardado num campo da classe.
        // Antes era uma variável local dentro do clique do botão: depois de iniciar,
        // não havia como acessá-lo de novo para pará-lo.
        private Servidor servidor;

        // MELHORIA: nome mais claro (conectado -> servidorAtivo).
        // O formulário do servidor não "se conecta", ele fica ativo esperando conexões.
        private bool servidorAtivo = false;

        // MELHORIA: removido o delegate AtualizaStatusCallback.
        // O .NET já tem o tipo genérico Action, que faz o mesmo sem precisar declarar um novo.

        public FormServidorChat()
        {
            InitializeComponent();

            // MELHORIA: o evento de fechamento é ligado aqui no código.
            // Assim fica explícito que, ao fechar a janela, o servidor é encerrado
            // corretamente, sem depender de configuração escondida no Designer.
            this.FormClosing += FormServidorChat_FormClosing;
        }

        // Ligado pelo Designer. Mantido para não quebrar o vínculo do evento Load.
        private void FormServidorChat_Load(object sender, EventArgs e)
        {
        }

        private void buttonStartServer_Click(object sender, EventArgs e)
        {
            // Se já está ativo, o botão funciona como "Sair"
            if (servidorAtivo)
            {
                // MELHORIA: Close() em vez de Application.Exit().
                // Close() dispara o FormClosing, onde o servidor é parado de forma organizada.
                this.Close();
                return;
            }

            // MELHORIA: IPAddress.TryParse em vez de checar só se o texto está vazio.
            // Antes, um IP inválido (ex.: "abc") só era descoberto pela exceção do Parse.
            if (!IPAddress.TryParse(textBoxIP.Text.Trim(), out IPAddress enderecoIP))
            {
                MessageBox.Show("Informe um endereço de IP válido (ex.: 127.0.0.1).",
                    "IP inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxIP.Focus();
                return;
            }

            int portaHost = (int)numericUpDownNumPorta.Value;

            try
            {
                servidor = new Servidor(enderecoIP, portaHost);

                // MELHORIA: o evento é ligado ANTES de iniciar, para não perder nenhuma
                // mensagem que o servidor envie logo no início.
                Servidor.StatusChanged += Servidor_StatusChanged;

                // Se a porta estiver em uso, a exceção vem para o catch abaixo
                // (antes o Servidor escondia o erro com um catch vazio).
                servidor.IniciaAtendimento();

                AdicionarLog($"Servidor ativo em {enderecoIP}:{portaHost}, aguardando usuários...");
            }
            catch (Exception ex)
            {
                // MELHORIA: se falhar, desliga o evento e descarta o servidor,
                // deixando a tela pronta para uma nova tentativa.
                Servidor.StatusChanged -= Servidor_StatusChanged;
                servidor = null;

                AdicionarLog("Erro ao iniciar o servidor: " + ex.Message);
                return;
            }

            servidorAtivo = true;
            textBoxIP.Enabled = false;
            numericUpDownNumPorta.Enabled = false;
            buttonStartServer.ForeColor = Color.Red;
            buttonStartServer.Text = "Sair";
        }

        // Recebe os avisos do Servidor (conexões, saídas e mensagens).
        // Este método roda numa thread do servidor, não na thread da tela.
        private void Servidor_StatusChanged(object sender, StatusAlteradoEventArgs e)
        {
            // MELHORIA: verifica se a janela ainda existe antes de atualizar.
            // Se o formulário estiver fechando, chamar Invoke lançaria ObjectDisposedException.
            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            // MELHORIA: BeginInvoke em vez de Invoke.
            // Invoke faz a thread do servidor ESPERAR a tela atualizar; durante o fechamento
            // isso pode travar o programa (deadlock). BeginInvoke só agenda a atualização
            // e deixa a thread do servidor seguir.
            this.BeginInvoke(new Action(() => AdicionarLog(e.Mensagem)));
        }

        // MELHORIA: método único para escrever no log.
        // Antes o mesmo par de linhas (Items.Add + SetSelected) aparecia repetido em vários lugares.
        private void AdicionarLog(string mensagem)
        {
            // MELHORIA: horário em cada linha, facilitando acompanhar a ordem dos eventos.
            listBoxListaLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] {mensagem}");

            // MELHORIA: TopIndex rola até a última linha sem deixá-la selecionada (azul).
            listBoxListaLog.TopIndex = listBoxListaLog.Items.Count - 1;
        }

        // MELHORIA: novo tratamento ao fechar a janela.
        // Antes o programa fechava sem avisar os clientes nem liberar a porta.
        private void FormServidorChat_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Desliga o evento primeiro, para nenhuma thread tentar atualizar
            // a tela que está sendo fechada.
            Servidor.StatusChanged -= Servidor_StatusChanged;

            // Para o listener e fecha a conexão de todos os clientes
            servidor?.PararAtendimento();
        }
    }
}