using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServidorChat
{
    // Delegate do evento StatusChanged: define a assinatura dos métodos que tratam o evento.
    public delegate void StatusChangedEventHandler(object sender, StatusAlteradoEventArgs e);

    // Aceita conexões de clientes e distribui as mensagens entre os usuários conectados.
    class Servidor
    {
        // MELHORIA: Hashtable trocada por Dictionary<TKey, TValue>.
        // O Dictionary é tipado (string -> TcpClient): o compilador impede erros de tipo
        // e não é preciso converter (cast) os valores ao ler.
        // As tabelas também passaram a ser privadas: só os métodos desta classe (com lock)
        // podem alterá-las.
        // MELHORIA: StringComparer.OrdinalIgnoreCase faz "Joao" e "joao" serem o mesmo nome,
        // evitando usuários duplicados que só mudam maiúsculas/minúsculas.
        // OBS: o número 30 do código original era só a capacidade inicial, não um limite de conexões.
        private static readonly Dictionary<string, TcpClient> usuariosPorNome =
            new Dictionary<string, TcpClient>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<TcpClient, string> nomesPorConexao =
            new Dictionary<TcpClient, string>();

        // MELHORIA: objeto exclusivo para controlar o acesso às tabelas (lock).
        // Cada cliente roda numa thread própria e todas usam as tabelas acima.
        // O lock garante que só uma thread por vez lê ou altera as tabelas,
        // evitando listas corrompidas e exceções aleatórias.
        private static readonly object travaUsuarios = new object();

        // Notifica o formulário sobre conexões, desconexões e mensagens.
        public static event StatusChangedEventHandler StatusChanged;

        // MELHORIA: removido o campo estático "e" (StatusChangedEventsArgs).
        // Ele era compartilhado por todas as threads: duas mensagens ao mesmo tempo
        // podiam sobrescrever uma à outra. Agora cada envio cria seu próprio objeto local.

        // MELHORIA: campos marcados como readonly, pois só são definidos no construtor.
        private readonly IPAddress enderecoIP;
        private readonly int portaHost;

        private TcpListener tlsCliente;
        private Thread thrListener;

        // MELHORIA: nome corrigido (ServRodadno -> servidorRodando) e marcado como volatile.
        // "volatile" garante que a thread do listener veja na hora a mudança feita por outra
        // thread (por exemplo, quando PararAtendimento() coloca false).
        private volatile bool servidorRodando = false;

        public Servidor(IPAddress endereco, int porta)
        {
            enderecoIP = endereco;
            portaHost = porta;
        }

        // Inclui o usuário nas tabelas. Retorna false se o nome já estiver em uso.
        // MELHORIA: verificar e incluir agora acontecem dentro do mesmo lock.
        // Antes, a Conexao verificava (Contains) e só depois incluía (Add). Dois clientes com o
        // mesmo nome entrando juntos passavam na verificação, e o segundo Add lançava exceção,
        // derrubando o servidor.
        public static bool IncluirUsuario(TcpClient tcpUsuario, string nomeUsuario,bool avisarConexao)
        {
            lock (travaUsuarios)
            {
                if (usuariosPorNome.ContainsKey(nomeUsuario))
                    return false;

                usuariosPorNome.Add(nomeUsuario, tcpUsuario);
                nomesPorConexao.Add(tcpUsuario, nomeUsuario);
            }

            // MELHORIA: o aviso é enviado FORA do lock, para não travar as outras threads
            // enquanto a mensagem viaja pela rede.
            if(avisarConexao)
            EnviarMensagemAdmin(nomeUsuario + " se conectou ao servidor");
            return true;
        }

        // Remove o usuário das tabelas e avisa os demais.
        public static void RemoverUsuario(TcpClient tcpUsuario)
        {
            string nomeUsuario;

            lock (travaUsuarios)
            {
                // MELHORIA: TryGetValue busca e verifica numa operação só.
                // Se o usuário já foi removido (por outra thread), não faz nada.
                if (!nomesPorConexao.TryGetValue(tcpUsuario, out nomeUsuario))
                    return;

                nomesPorConexao.Remove(tcpUsuario);
                usuariosPorNome.Remove(nomeUsuario);
            }

            // MELHORIA: avisa DEPOIS de remover e fora do lock.
            // Antes avisava antes de remover, então tentava mandar "saiu..." também para o
            // próprio cliente que estava saindo (e que já podia estar desconectado).
            EnviarMensagemAdmin(nomeUsuario + " saiu...");
        }

        public static void OnStatusChanged(StatusAlteradoEventArgs e)
        {
            // MELHORIA: "?.Invoke" substitui copiar para variável e testar null.
            // Faz o mesmo, em uma linha, e é seguro entre threads.
            StatusChanged?.Invoke(null, e);
        }

        public static void EnviarMensagemAdmin(string mensagem)
        {
            EnviarParaTodos("Administrador: " + mensagem);
        }

        public static void EnviaMensagem(string origem, string mensagem)
        {
            EnviarParaTodos(origem + " disse: " + mensagem);
        }

        // MELHORIA: EnviarMensagemAdmin e EnviaMensagem tinham o mesmo laço de envio duplicado.
        // Agora os dois só montam o texto e chamam este método único (princípio DRY:
        // "Don't Repeat Yourself"). Uma correção aqui vale para os dois.
        private static void EnviarParaTodos(string texto)
        {
            // MELHORIA: mensagem vazia é descartada uma vez só, antes do laço.
            // Antes era testada de novo para cada cliente.
            if (string.IsNullOrWhiteSpace(texto))
                return;

            // Exibe primeiro no formulário do servidor
            OnStatusChanged(new StatusAlteradoEventArgs(texto));

            // MELHORIA: copia a lista DENTRO do lock e envia FORA dele.
            // Assim a lista não muda no meio do envio, e as outras threads não ficam
            // esperando enquanto as mensagens são enviadas.
            List<TcpClient> clientes;
            lock (travaUsuarios)
            {
                clientes = usuariosPorNome.Values.ToList();
            }

            // MELHORIA: clientes com erro são guardados e removidos só DEPOIS do laço.
            // Antes, RemoverUsuario era chamado no meio do envio e ele mesmo enviava outra
            // mensagem ("saiu..."), gerando chamadas encadeadas difíceis de acompanhar.
            List<TcpClient> clientesComErro = new List<TcpClient>();

            foreach (TcpClient cliente in clientes)
            {
                try
                {
                    // MELHORIA: lock por cliente. Se duas threads enviarem para o mesmo
                    // cliente ao mesmo tempo, as linhas não se misturam.
                    lock (cliente)
                    {
                        // O StreamWriter não é fechado de propósito: fechá-lo fecharia
                        // também a conexão de rede do cliente.
                        StreamWriter enviador = new StreamWriter(cliente.GetStream());
                        enviador.WriteLine(texto);
                        enviador.Flush();
                    }
                }
                catch (Exception)
                {
                    clientesComErro.Add(cliente);
                }
            }

            foreach (TcpClient cliente in clientesComErro)
            {
                RemoverUsuario(cliente);
            }
        }

        // Inicia o listener e a thread que aceita os clientes.
        // MELHORIA: o erro ao iniciar não é mais escondido.
        // Antes o catch estava vazio: se a porta estivesse em uso, o formulário mostrava
        // "Servidor ativo" mesmo sem estar. Agora a exceção sobe para o formulário,
        // que já tem try/catch e mostra o erro no log.
        public void IniciaAtendimento()
        {
            tlsCliente = new TcpListener(enderecoIP, portaHost);
            tlsCliente.Start();

            servidorRodando = true;

            thrListener = new Thread(MantemAtendimento);
            thrListener.IsBackground = true;
            thrListener.Start();
        }

        // MELHORIA: novo método para encerrar o servidor de forma organizada.
        // Antes não havia como parar: ao fechar o programa, as conexões caíam de repente
        // e os clientes recebiam erro.
        public void PararAtendimento()
        {
            servidorRodando = false;

            // Parar o listener faz o AcceptTcpClient() lançar exceção,
            // o que encerra o laço em MantemAtendimento().
            tlsCliente?.Stop();

            // Fecha todos os clientes. Cada Conexao percebe o fechamento,
            // sai do seu laço e faz a própria limpeza.
            List<TcpClient> clientes;
            lock (travaUsuarios)
            {
                clientes = usuariosPorNome.Values.ToList();
                usuariosPorNome.Clear();
                nomesPorConexao.Clear();
            }

            foreach (TcpClient cliente in clientes)
            {
                try { cliente.Close(); } catch { }
            }
        }

        // Laço que aceita novos clientes enquanto o servidor estiver rodando.
        private void MantemAtendimento()
        {
            while (servidorRodando)
            {
                try
                {
                    // MELHORIA: o cliente aceito agora é uma variável local.
                    // Antes era um campo da classe (tcpCliente), reaproveitado sem necessidade.
                    TcpClient novoCliente = tlsCliente.AcceptTcpClient();

                    // Cada cliente é atendido por uma Conexao própria, numa thread própria.
                    new Conexao(novoCliente);
                }
                // MELHORIA: filtro de exceção ("when"). Quando o listener é parado,
                // o AcceptTcpClient lança SocketException ou ObjectDisposedException.
                // Tratar as duas aqui encerra o laço sem derrubar a thread.
                catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                {
                    break;
                }
            }
        }
    }
}