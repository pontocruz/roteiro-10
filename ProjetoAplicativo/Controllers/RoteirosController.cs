// FILE: RoteirosControllers.cs

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoAplicativo.Data;
using ProjetoAplicativo.Models;
using ProjetoAplicativo.Models.DTOs;
using static ProjetoAplicativo.Models.Enums.EnumInstrucao;


namespace ProjetoAplicativo.Controllers {
[Route("api/[controller]")]
public class RoteirosController : Controller {
   private readonly ModeloEntidades _context;
   public RoteirosController(ModeloEntidades context) { _context = context; }

   [HttpGet("instrucoes/{id}")]
   public async Task<IActionResult> GetInstrucoesJson(int id)
   {
      var cena = await _context.Cena
         .Include(c => c.Instrucoes)
         .ThenInclude(i => i.InstrucoesPersonagens
            .Where(ip => ip.TipoDeParticipacao == TipoDeParticipacao.Executor ||
                         ip.TipoDeParticipacao == TipoDeParticipacao.Exceto))
         .ThenInclude(ip => ip.Personagem)
         .FirstOrDefaultAsync(c => c.Id == id);

      var result = cena?.Instrucoes?
         .OrderBy(i => i.OrdemCronologica)
         .Select(i => new InstrucaoDto(i))
         .ToList();

      return Json(result);
   }
   
   
   [HttpPost]
   public async Task<IActionResult> CreateInstrucao([FromBody] InstrucaoDto dto)
   {
      var instrucao = dto.ToEntity();
      _context.Set<Instrucao>().Add(instrucao);
      await _context.SaveChangesAsync();
      return Json(new InstrucaoDto(instrucao));
   }


   [HttpPut("{id}")]
   public async Task<IActionResult> UpdateInstrucao(int id, [FromBody] InstrucaoDto dto)
   {
      var existing = await _context.Set<Instrucao>().Include(i => i.InstrucoesPersonagens).FirstOrDefaultAsync(i => i.Id == id);
      if (existing == null) return NotFound();

      var updated = dto.ToEntity();
      _context.Set<InstrucaoPersonagem>().RemoveRange(existing.InstrucoesPersonagens);
      _context.Entry(existing).CurrentValues.SetValues(updated);
      existing.InstrucoesPersonagens = updated.InstrucoesPersonagens;

      await _context.SaveChangesAsync();
      return Json(new InstrucaoDto(existing));
   }
}
}