namespace $DomainCommandsNamespace$.$EntityName$
{
    /// <summary>
    /// Comando de remoção da entidade $EntityName$
    /// </summary>
    public class Remove$EntityName$Command
    {
        public int Id { get; set; }
        public Remove$EntityName$Command(int id)
        {
            Id = id;
        }
    }
}
