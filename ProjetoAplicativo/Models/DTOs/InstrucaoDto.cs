// FILE: ProjetoAplicativo/Models/DTOs/InstrucaoDto.cs

using System.Text.Json.Serialization;
using static ProjetoAplicativo.Models.Enums.EnumInstrucao;

namespace ProjetoAplicativo.Models.DTOs
{
public class InstrucaoDto
{
   // Properties must have public setters
   public int Id { get; set; }
   public int CenaId { get; set; }
   public int OrdemCronologica { get; set; }
   public string TipoDeInstrucao { get; set; }
   public string Texto { get; set; }
   public List<InstrucaoPersonagemDto> InstrucoesPersonagens { get; set; }

   // Parameterless constructor for JSON deserialization
   public InstrucaoDto()
   {
      InstrucoesPersonagens = new List<InstrucaoPersonagemDto>();
   }

   // Constructor for entity→DTO mapping (used in GET)
   public InstrucaoDto(Instrucao instrucao) : this() // Calls parameterless constructor
   {
      Id = instrucao.Id;
      CenaId = instrucao.CenaId;
      OrdemCronologica = instrucao.OrdemCronologica;
      TipoDeInstrucao = instrucao.TipoDeInstrucao.ToString();
      Texto = instrucao.Texto;
      InstrucoesPersonagens = instrucao.InstrucoesPersonagens?
         .Select(ip => new InstrucaoPersonagemDto(ip))
         .ToList() ?? new List<InstrucaoPersonagemDto>();
   }
        
   public Instrucao ToEntity()
   {
      return new Instrucao
      {
         Id = Id,
         CenaId = CenaId,
         OrdemCronologica = OrdemCronologica,
         TipoDeInstrucao = Enum.Parse<TipoDeInstrucao>(TipoDeInstrucao),
         Texto = Texto,
         InstrucoesPersonagens = InstrucoesPersonagens.Select(ip => ip.ToEntity()).ToList()
      };
   }
}
}