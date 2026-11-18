using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace ChatCliente
{
    // MELHORIA: formulário renomeado de Form1 para FormChatCliente.
    // O nome descreve o que a tela é, facilitando encontrar o arquivo no projeto.
    public partial class FormChatCliente : Form
    {
        private TcpClient tcpServidor;
        private StreamWriter stwEnviador;
        private StreamReader strReceptor;
        private Thread threadRecebimento;

        // MELHORIA: "volatile" garante que a thread de recebimento veja na hora a mudança
        // feita pela tela (por exemplo, quando o usuário clica em Desconectar).
        private volatile bool conectado = false;

        // MELHORIA: removidos os delegates AtualizaLogCallBack e FechaConexaoCallBack.
        // O .NET já tem o tipo genérico Action, que faz o mesmo sem declarar tipos novos.

        // MELHORIA: removidos os campos NomeUsuario, enderecoIP e portaHost.
        // Eles só eram usados dentro da conexão, então viraram variáveis locais.

        public FormChatCliente()
        {
            InitializeComponent();

            // MELHORIA: os eventos de Enviar e de teclado são ligados aqui no código.
            // O botão Conectar está ligado pelo Designer (buttonConectar_Click_1).
            buttonEnviar.Click += buttonEnviar_Click;
            textBoxMensagem.KeyDown += textBoxMensagem_KeyDown;

            // MELHORIA: o fechamento da janela agora é tratado no FormClosing.
            // Antes era usado Application.ApplicationExit, que rodava depois da janela
            // já ter sido destruída e podia lançar exceção ao tentar atualizar o labelStatus.
            this.FormClosing += FormChatCliente_FormClosing;

            // MELHORIA: o estado inicial da tela é definido no código.
            // Antes dependia de configurar Enabled manualmente no Designer.
            HabilitarCampos(conexaoAberta: false, podeEnviar: false);
            AtualizarStatus("Desconectado", Color.Gray);
        }

        // ===================== CONEXÃO =====================

        // Clique do botão Conectar/Desconectar (ligado pelo Designer).
        // Se já estiver conectado, desconecta; senão, inicia a conexão.
        private void buttonConectar_Click_1(object sender, EventArgs e)
        {
            if (conectado)
                FechaConexao("Desconectado a pedido do usuário.");
            else
                IniciarConexao();
        }

        private void IniciarConexao()
        {
            // MELHORIA: o IP é validado com TryParse antes de tentar conectar.
            // Antes, um IP inválido só era descoberto pela exceção do Parse.
            if (!IPAddress.TryParse(textBoxServidorIP.Text.Trim(), out IPAddress enderecoIP))
            {
                MessageBox.Show("Informe um endereço de IP válido (ex.: 127.0.0.1).",
                    "IP inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxServidorIP.Focus();
                return;
            }

            // MELHORIA: o nome do usuário é validado antes de conectar.
            // Antes, um nome vazio era enviado e o servidor simplesmente fechava a conexão,
            // sem explicar o motivo para o usuário.
            string nomeUsuario = textBoxUsuario.Text.Trim();
            if (string.IsNullOrWhiteSpace(nomeUsuario))
            {
                MessageBox.Show("Informe o seu nome de usuário.",
                    "Usuário obrigatório", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxUsuario.Focus();
                return;
            }

            int porta = (int)numericUpDownPortaHost.Value;

            try
            {
                tcpServidor = new TcpClient();
                tcpServidor.Connect(enderecoIP, porta);

                NetworkStream stream = tcpServidor.GetStream();
                stwEnviador = new StreamWriter(stream);
                strReceptor = new StreamReader(stream);

                // Envia o nome do usuário (primeira linha do protocolo)
                stwEnviador.WriteLine(nomeUsuario);
                stwEnviador.Flush();

                conectado = true;

                // MELHORIA: enquanto o servidor não confirma, a tela mostra "Conectando...".
                // Antes mostrava "Conectado" logo após abrir o TCP, mesmo que o servidor
                // depois recusasse o nome (ex.: nome já em uso).
                // As mensagens só são liberadas quando o servidor responder "1".
                HabilitarCampos(conexaoAberta: true, podeEnviar: false);
                AtualizarStatus($"Conectando a {enderecoIP}:{porta}...", Color.DarkOrange);

                threadRecebimento = new Thread(ReceberMensagens);
                threadRecebimento.IsBackground = true;
                threadRecebimento.Start();
            }
            catch (Exception ex)
            {
                // MELHORIA: se a conexão falhar, os recursos já abertos são liberados.
                // Antes ficavam abertos (TcpClient sem Close).
                LiberarRecursos();
                AtualizarStatus("Erro na conexão: " + ex.Message, Color.Red);
            }
        }

        // Roda numa thread separada: lê tudo o que o servidor envia.
        private void ReceberMensagens()
        {
            // MELHORIA: guarda o leitor numa variável local.
            // Se a tela fechar a conexão e limpar o campo strReceptor, esta thread
            // continua usando a própria referência, sem NullReferenceException.
            StreamReader leitor = strReceptor;
            string motivo = "Conexão com o servidor perdida.";

            // MELHORIA: todo o recebimento está dentro de try/finally.
            // Antes, quando o servidor fechava, o ReadLine lançava exceção numa thread
            // sem tratamento e o PROGRAMA INTEIRO FECHAVA ("craxa quando eu fecho o servidor").
            try
            {
                // 1) Primeira linha: resposta da conexão ("1" ou "0|motivo")
                string resposta = leitor.ReadLine();

                // MELHORIA: trata resposta nula (servidor fechou antes de responder).
                // Antes, resposta[0] com resposta nula lançava NullReferenceException.
                if (resposta == null)
                {
                    motivo = "O servidor encerrou a conexão.";
                    return; // o finally fecha a conexão
                }

                if (!resposta.StartsWith("1"))
                {
                    // MELHORIA: protege o Substring. Antes, uma resposta curta como "0"
                    // lançava ArgumentOutOfRangeException.
                    motivo = "Não conectado: " +
                        (resposta.Length > 2 ? resposta.Substring(2) : "motivo não informado.");
                    return;
                }

                ExecutarNaTela(() =>
                {
                    AtualizarStatus("Conectado ao servidor de chat", Color.Green);
                    HabilitarCampos(conexaoAberta: true, podeEnviar: true);
                    AtualizarLog("Conectado com sucesso!");
                });

                // 2) Mensagens: lê até o servidor fechar a conexão
                string linha;

                // MELHORIA: o laço para quando ReadLine() devolve null (servidor fechou).
                // Antes o laço continuava e enviava linhas nulas para a tela sem parar.
                while ((linha = leitor.ReadLine()) != null)
                {
                    // MELHORIA: copia para uma variável dentro do laço.
                    // O BeginInvoke executa depois; se usasse "linha" direto, poderia
                    // mostrar o valor da PRÓXIMA leitura em vez desta.
                    string mensagem = linha;
                    ExecutarNaTela(() => AtualizarLog(mensagem));
                }

                motivo = "O servidor encerrou a conexão.";
            }
            catch (Exception)
            {
                // Erro de rede: o motivo padrão ("Conexão com o servidor perdida.") é usado.
            }
            finally
            {
                // MELHORIA: só fecha se ainda estiver conectado.
                // Se o usuário clicou em Desconectar, a conexão já foi fechada pela tela,
                // e esta thread apenas termina, sem mostrar erro.
                if (conectado)
                {
                    string motivoFinal = motivo;
                    ExecutarNaTela(() => FechaConexao(motivoFinal));
                }
            }
        }

        // Fecha a conexão e volta a tela ao estado inicial. Sempre roda na thread da tela.
        private void FechaConexao(string motivo)
        {
            // MELHORIA: se já está desconectado, não faz nada.
            // Evita fechar duas vezes (clique do usuário + queda do servidor ao mesmo tempo).
            if (!conectado)
                return;

            conectado = false;
            LiberarRecursos();

            AtualizarLog(motivo);
            HabilitarCampos(conexaoAberta: false, podeEnviar: false);
            AtualizarStatus("Desconectado", Color.Gray);
        }

        // MELHORIA: método único para fechar os objetos de rede.
        // Cada Close tem seu próprio try, então um erro não impede os outros.
        // Antes, se o stwEnviador fosse nulo, o Close lançava NullReferenceException.
        private void LiberarRecursos()
        {
            try { stwEnviador?.Close(); } catch { }
            try { strReceptor?.Close(); } catch { }
            try { tcpServidor?.Close(); } catch { }

            stwEnviador = null;
            strReceptor = null;
            tcpServidor = null;
        }

        // ===================== MENSAGENS =====================

        // MELHORIA: o botão Enviar agora funciona. Antes o método estava vazio,
        // e só era possível enviar com a tecla Enter.
        private void buttonEnviar_Click(object sender, EventArgs e)
        {
            EnviaMensagem();
        }

        // MELHORIA: KeyDown com SuppressKeyPress em vez de KeyPress.
        // Assim o Enter envia a mensagem sem fazer o "bip" do Windows.
        private void textBoxMensagem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                EnviaMensagem();
            }
        }

        private void EnviaMensagem()
        {
            string texto = textBoxMensagem.Text.Trim();

            // MELHORIA: não envia mensagem vazia nem quando está desconectado.
            if (!conectado || texto == "")
                return;

            // MELHORIA: o envio tem try/catch.
            // Antes, se o servidor tivesse caído, o WriteLine lançava exceção e fechava o programa.
            try
            {
                stwEnviador.WriteLine(texto);
                stwEnviador.Flush();
                textBoxMensagem.Clear();
            }
            catch (Exception)
            {
                FechaConexao("Não foi possível enviar: conexão com o servidor perdida.");
            }

            textBoxMensagem.Focus();
        }

        // ===================== TELA =====================

        // MELHORIA: um método único controla quais campos ficam habilitados.
        // Antes essa lógica estava repetida em DesabilitarCampos e em FechaConexao,
        // e o botão Enviar não era desabilitado ao desconectar.
        private void HabilitarCampos(bool conexaoAberta, bool podeEnviar)
        {
            textBoxServidorIP.Enabled = !conexaoAberta;
            numericUpDownPortaHost.Enabled = !conexaoAberta;
            textBoxUsuario.Enabled = !conexaoAberta;

            textBoxMensagem.Enabled = podeEnviar;
            buttonEnviar.Enabled = podeEnviar;

            buttonConectar.Text = conexaoAberta ? "Desconectar" : "Conectar";
            buttonConectar.ForeColor = conexaoAberta ? Color.Red : Color.Green;

            if (podeEnviar)
                textBoxMensagem.Focus();
        }

        // MELHORIA: método único para o status.
        // Antes o mesmo bloco (cor + texto + Invoke) aparecia repetido em vários lugares,
        // e usava Invoke mesmo já estando na thread da tela, sem necessidade.
        private void AtualizarStatus(string texto, Color cor)
        {
            labelStatus.ForeColor = cor;
            labelStatus.Text = texto;
        }

        private void AtualizarLog(string mensagem)
        {
            textBoxLog.AppendText(mensagem + Environment.NewLine);
        }

        // MELHORIA: método único para atualizar a tela a partir de outra thread.
        // Verifica se a janela ainda existe e usa BeginInvoke, que não trava a thread
        // de recebimento (Invoke podia travar o programa durante o fechamento).
        private void ExecutarNaTela(Action acao)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            try
            {
                BeginInvoke(acao);
            }
            catch (InvalidOperationException)
            {
                // A janela foi fechada entre a verificação e o BeginInvoke: ignora.
            }
        }

        private void FormChatCliente_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Ao fechar a janela, encerra a conexão sem tentar atualizar a tela.
            if (conectado)
            {
                conectado = false;
                LiberarRecursos();
            }
        }
    }
}