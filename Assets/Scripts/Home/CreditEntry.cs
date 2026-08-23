namespace Home
{
    /// <summary>
    /// クレジット1行分の「役割」と「担当者名」
    /// </summary>
    public readonly struct CreditEntry
    {
        public string Role { get; }
        public string Name { get; }

        public CreditEntry(string role, string name)
        {
            Role = role;
            Name = name;
        }
    }
}
