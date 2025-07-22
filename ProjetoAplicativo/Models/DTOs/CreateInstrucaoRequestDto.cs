using ProjetoAplicativo.Models.Enums;

namespace ProjetoAplicativo.Models.DTOs;

public class CreateInstrucaoRequestDto
{
    public int CenaId { get; set; }
    public EnumInstrucao.TipoDeInstrucao TipoDeInstrucao { get; set; }
    public string? Texto { get; set; }
    public List<int>? PersonagemIds { get; set; } // Handles -1 (ShowAll), -2 (ShowAllExcept)
}