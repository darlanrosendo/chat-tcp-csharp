using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace ServidorChat
{
    // Atende um cliente. Existe uma instância (e uma thread) para cada usuário conectado.
    internal class Conexao
    {
        // MELHORIA: campos marcados como readonly quando só são definidos no construtor.
        private readonly TcpClient tcpCliente;
        private readonly Thread thrAtendimento;

        private StreamReader srReceptor;
        private StreamWriter swEnviador;
        private string usuarioAtual;

        // MELHORIA: removido o campo strResposta. Ele só era usado dentro do laço,
        // então virou variável local (escopo menor = código mais fácil de entender).

        public Conexao(TcpClient tcpCon)
        {
            tcpCliente = tcpCon;

            // MELHORIA: thread renomeada (thrSender -> thrAtendimento).
            // Ela não só envia: ela atende o cliente inteiro (nome, mensagens, saída).
            thrAtendimento = new Thread(AceitaCliente);
            thrAtendimento.IsBackground = true;
            thrAtendimento.Start();
        }

        private void FechaConexao()
        {
            // MELHORIA: "?." evita NullReferenceException se o objeto ainda não foi criado,
            // e cada Close tem seu próprio try para que um erro não impeça os outros.
            try { srReceptor?.Close(); } catch { }
            try { swEnviador?.Close(); } catch { }
            try { tcpCliente?.Close(); } catch { }
        }

        private void AceitaCliente()
        {
            bool usuarioIncluido = false;

            // MELHORIA: todo o atendimento está dentro do try.
            // Antes, a leitura do nome e as verificações ficavam fora: qualquer erro ali
            // (cliente caindo, nome nulo) lançava exceção numa thread sem tratamento e
            // DERRUBAVA O SERVIDOR INTEIRO.
            try
            {
                srReceptor = new StreamReader(tcpCliente.GetStream());
                swEnviador = new StreamWriter(tcpCliente.GetStream());

                // 1) Lê o nome do usuário
                // MELHORIA: Trim() remove espaços antes/depois, evitando nomes como " Ana ".
                usuarioAtual = srReceptor.ReadLine()?.Trim();

                // MELHORIA: IsNullOrWhiteSpace trata null, vazio e só espaços.
                // Antes era testado só "" e, se o cliente caísse antes de mandar o nome,
                // o null seguia adiante e causava exceção.
                if (string.IsNullOrWhiteSpace(usuarioAtual))
                {
                    return; // o finally fecha a conexão
                }

                // MELHORIA: comparação sem diferenciar maiúsculas/minúsculas.
                // Antes, "administrador" ou "ADMINISTRADOR" passavam pelo bloqueio.
                if (usuarioAtual.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                {
                    swEnviador.WriteLine("0|Este nome de usuário é reservado.");
                    swEnviador.Flush();
                    return;
                }

                // MELHORIA: o nome é verificado e reservado numa operação só (com lock no Servidor).
                // Antes eram dois passos separados (Contains e depois Add), e dois clientes com o
                // mesmo nome entrando juntos derrubavam o servidor.
                // O aviso "se conectou" fica para depois do "1" (avisarConexao: false), porque o
                // cliente espera que a PRIMEIRA linha recebida seja a resposta ("1" ou "0|motivo").
                // MELHORIA: removido o espaço extra em "0| Este nome...", que aparecia no início
                // da mensagem de erro no cliente.
                if (!Servidor.IncluirUsuario(tcpCliente, usuarioAtual, avisarConexao: false))
                {
                    swEnviador.WriteLine("0|Este nome de usuário já existe.");
                    swEnviador.Flush();
                    return;
                }

                usuarioIncluido = true;

                // 1 = conectado com sucesso (protocolo esperado pelo cliente)
                swEnviador.WriteLine("1");
                swEnviador.Flush();

                // Só agora avisa todos que o usuário entrou
                Servidor.EnviarMensagemAdmin(usuarioAtual + " se conectou ao servidor");

                // 2) Lê as mensagens até o cliente desconectar
                string mensagem;

                // MELHORIA: o laço agora para quando ReadLine() devolve null.
                // null é o sinal real de "cliente desconectou". Antes o laço comparava com ""
                // e, quando o cliente saía, ficava recebendo null PARA SEMPRE: loop infinito,
                // CPU em 100% e uma thread presa a cada cliente que saía.
                while ((mensagem = srReceptor.ReadLine()) != null)
                {
                    // MELHORIA: linha vazia é ignorada (continue).
                    // Antes, uma linha vazia ENCERRAVA o laço, deixando o usuário na lista
                    // e a conexão aberta.
                    if (mensagem.Trim() == "")
                        continue;

                    Servidor.EnviaMensagem(usuarioAtual, mensagem);
                }
            }
            catch (Exception)
            {
                // Erro de rede com este cliente. A limpeza é feita no finally.
            }
            // MELHORIA: o finally SEMPRE executa, seja qual for o motivo da saída
            // (desconexão normal, erro de rede, nome recusado).
            // Antes, em vários caminhos o usuário ficava na lista e o TcpClient aberto.
            finally
            {
                if (usuarioIncluido)
                    Servidor.RemoverUsuario(tcpCliente);

                FechaConexao();
            }
        }
    }
}