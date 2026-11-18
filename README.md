# Chat TCP em C#

Aplicação de chat em tempo real com **servidor** e **cliente** desktop, desenvolvida em **C# (Windows Forms)** usando **sockets TCP** e **threads**.

Projeto de estudo: partindo de uma implementação básica de chat TCP, o código foi revisado e refatorado para corrigir falhas de estabilidade, tratar erros de rede e deixar a estrutura mais clara. Cada melhoria está comentada no código com `// MELHORIA:`, explicando o que mudou e por quê.

![Servidor e clientes conversando](docs/chat-funcionando.png)

---

## Funcionalidades

- Servidor que aceita vários clientes ao mesmo tempo (uma thread por cliente)
- Mensagens enviadas para todos os usuários conectados
- Avisos automáticos de entrada e saída de usuários
- Bloqueio de nomes repetidos e do nome reservado "Administrador" (sem diferenciar maiúsculas e minúsculas)
- Log do servidor com horário de cada evento
- Status da conexão no cliente (Conectando, Conectado, Desconectado)
- Encerramento organizado: ao fechar o servidor, todos os clientes são avisados e nenhum programa trava

---

## Tecnologias

- C# / .NET Framework
- Windows Forms
- `System.Net.Sockets` (`TcpListener`, `TcpClient`)
- `System.Threading` (threads, `lock`, `volatile`)

---

## Estrutura

```
ServidorChat/
├── Servidor.cs                 → aceita conexões e distribui as mensagens
├── Conexao.cs                  → atende um cliente (uma instância por usuário)
├── StatusAlteradoEventArgs.cs  → dados do evento que atualiza a tela
└── FormServidorChat.cs         → tela do servidor

ChatCliente/
└── FormChatCliente.cs          → tela e lógica do cliente
```

### Protocolo de comunicação

1. O cliente se conecta e envia o **nome do usuário** (primeira linha).
2. O servidor responde:
   - `1` → conexão aceita
   - `0|motivo` → conexão recusada (ex.: `0|Este nome de usuário já existe.`)
3. A partir daí, cada linha enviada pelo cliente é repassada para todos os usuários.

---

## Principais melhorias

| Problema no código original | Solução aplicada |
|---|---|
| Loop infinito no servidor quando um cliente desconectava (CPU em 100%) | O laço de leitura para quando `ReadLine()` devolve `null` |
| Um erro em um cliente derrubava o servidor inteiro | Atendimento de cada cliente dentro de `try/catch/finally` |
| Dois usuários com o mesmo nome entrando juntos derrubavam o servidor | Verificação e inclusão feitas numa única operação com `lock` |
| Listas de usuários acessadas por várias threads sem proteção | `lock` e `Dictionary` tipado no lugar de `Hashtable` |
| Cliente fechava sozinho quando o servidor caía | Leitura em `try/finally` e tratamento de resposta nula |
| Não havia como parar o servidor | Novo método `PararAtendimento()` chamado ao fechar a janela |
| Erro ao iniciar (porta em uso) era escondido | Exceção exibida no log do servidor |
| Atualização da tela com `Invoke` podia travar no fechamento | `BeginInvoke` com verificação de janela fechada |
| Botão "Enviar" do cliente não funcionava | Evento implementado |
| Código de envio duplicado | Método único `EnviarParaTodos` (princípio DRY) |

---

## Como executar

1. Abra a solução no **Visual Studio**.
2. Execute o **ServidorChat**, informe o IP (ex.: `127.0.0.1`) e a porta (ex.: `1000`) e clique em **Start Server**.
3. Execute um ou mais **ChatCliente**, informe o mesmo IP e porta, escolha um nome de usuário e clique em **Conectar**.
4. Envie mensagens pelo botão **Enviar** ou pela tecla **Enter**.

Para testar com vários clientes na mesma máquina, abra o executável do cliente várias vezes (pasta `ChatCliente/bin/Debug`).

---

## Testes realizados

- Dois ou mais clientes conversando ao mesmo tempo
- Nome de usuário repetido → conexão recusada com mensagem
- Nome "administrador" → conexão recusada
- Cliente desconectando → demais usuários recebem o aviso de saída
- Servidor sendo fechado → clientes exibem "O servidor encerrou a conexão." sem travar
- Porta já em uso → erro exibido no log do servidor
- IP inválido → mensagem de validação

---

## Autor

**Darlan Rosendo** – [github.com/darlanrosendo](https://github.com/darlanrosendo)
