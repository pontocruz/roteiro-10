// FILE: ProjetoAplicativo/Models/DTOs/InstrucaoPersonagemDto.cs
using static ProjetoAplicativo.Models.Enums.EnumInstrucao;
namespace ProjetoAplicativo.Models.DTOs
{

public class InstrucaoPersonagemDto
{
   public int? PersonagemId { get; set; }
   public bool ShowAll { get; set; }
   public bool ShowAllExcept { get; set; }
   public string? Nome { get; set; }

   public InstrucaoPersonagemDto(InstrucaoPersonagem ip)
   {
      PersonagemId = ip.PersonagemId;
      ShowAll = ip.ShowAll;
      ShowAllExcept = ip.ShowAllExcept;
      Nome = ip.Personagem?.Nome;
   }
   public InstrucaoPersonagem ToEntity() => new()
   {
      PersonagemId = PersonagemId,
      ShowAll = ShowAll,
      ShowAllExcept = ShowAllExcept,
      TipoDeParticipacao = ShowAll ? TipoDeParticipacao.Executor : 
         ShowAllExcept ? TipoDeParticipacao.Exceto : 
         TipoDeParticipacao.Executor
   };
}
}