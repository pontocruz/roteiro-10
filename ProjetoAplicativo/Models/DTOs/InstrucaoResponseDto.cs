namespace ProjetoAplicativo.Models.DTOs;

public class InstrucaoResponseDto
{
    public int Id { get; set; }
    public int CenaId { get; set; }
    public string Tipo { get; set; } // Enum as string
    public string? Texto { get; set; }
    public int Ordem { get; set; }
    public List<ExecutorDto> Executores { get; set; } = new();
    public List<CitadoDto> Citados { get; set; } = new();
}

public class ExecutorDto
{
    public int? PersonagemId { get; set; }
    public string? Nome { get; set; }
    public bool ShowAll { get; set; } // For -1
    public bool ShowAllExcept { get; set; } // For -2
}

public class CitadoDto
{
    public int? PersonagemId { get; set; }
    public string Nome { get; set; }
}