using System;

namespace ServidorChat
{
    // Dados do evento de status do servidor: carrega a mensagem que o formulário vai exibir
    // (ex.: "Administrador: Ana se conectou ao servidor").
    // MELHORIA: classe renomeada de StatusChangedEventsArgs para StatusAlteradoEventArgs.
    // Segue a convenção do .NET (nome terminando em "EventArgs", sem o "s" extra em "Events")
    // e mantém o padrão em português usado no restante do projeto.
    public class StatusAlteradoEventArgs : EventArgs
    {
        // MELHORIA: o campo privado (EventMsg) e a propriedade com get/set manuais foram
        // substituídos por uma propriedade automática, com menos código e o mesmo resultado.
        // MELHORIA: a propriedade é somente leitura (só "get"). A mensagem é definida no
        // construtor e não pode ser alterada depois, evitando que quem recebe o evento
        // mude o conteúdo sem querer.
        // MELHORIA: propriedade renomeada de EventMessage para Mensagem (padrão em português).
        public string Mensagem { get; }

        public StatusAlteradoEventArgs(string mensagem)
        {
            Mensagem = mensagem;
        }
    }
}