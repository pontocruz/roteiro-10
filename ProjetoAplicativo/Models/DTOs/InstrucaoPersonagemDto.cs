// FILE: ProjetoAplicativo/Models/DTOs/InstrucaoPersonagemDto.cs



using static ProjetoAplicativo.Models.Enums.EnumInstrucao;
using System.Text.Json.Serialization;

namespace ProjetoAplicativo.Models.DTOs
{
public class InstrucaoPersonagemDto
{
   public int? PersonagemId { get; set; }
   public bool ShowAll { get; set; }
   public bool ShowAllExcept { get; set; }
   public string? Nome { get; set; }

   // Parameterless constructor
   public InstrucaoPersonagemDto() {}

   // Constructor for entity→DTO
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