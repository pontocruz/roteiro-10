// FILE: RoteirosControllers.cs

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoAplicativo.Data;
using ProjetoAplicativo.Models;
using ProjetoAplicativo.Models.DTOs;
using static ProjetoAplicativo.Models.Enums.EnumInstrucao;

namespace ProjetoAplicativo.Controllers
{
    [Route("api/[controller]")]
    public class RoteirosController : Controller
    {
        private readonly ModeloEntidades _context;

        public RoteirosController(ModeloEntidades context)
        {
            _context = context;
        }

        [HttpGet("instrucoes/{id}")] // Add {id} to route
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
                .Select(i => new
                {
                    i.Id,
                    i.CenaId,
                    i.OrdemCronologica,
                    TipoDeInstrucao = i.TipoDeInstrucao.ToString(),
                    i.Texto,
                    InstrucoesPersonagens = i.InstrucoesPersonagens.Select(ip => new
                    {
                        ip.PersonagemId,
                        ip.ShowAll,
                        ip.ShowAllExcept,
                        PersonagemNome = ip.Personagem?.Nome
                    })
                }).ToList();

            return Json(result);
        }


        [HttpPost]
        public async Task<IActionResult> CreateInstrucao(
            [FromBody] CreateInstrucaoRequestDto request)
        {
            var ordem = await GetNextOrderNumber(request.CenaId);
            var (success, data, error) = await CreateInstructionCoreAsync(
                request.CenaId, request.TipoDeInstrucao, request.Texto, ordem, request.PersonagemIds);

            return Json(new { success, data, error });
        }


        #region SHARED PRIVATE METHODS

        private async Task<(bool Success, Instrucao? Data, string Error)>
            CreateInstructionCoreAsync(int cenaId, TipoDeInstrucao tipo, string? texto, int ordem,
                List<int>? executorIds)
        {
            try
            {
                var instrucao = new Instrucao
                {
                    CenaId = cenaId,
                    TipoDeInstrucao = tipo,
                    Texto = texto,
                    OrdemCronologica = ordem
                };
                _context.Add(instrucao);
                await _context.SaveChangesAsync();

                if (executorIds != null)
                {
                    await HandlePersonagens(instrucao.Id, executorIds, clearExisting: true);
                }

                var mentionedIds = ExtractMentionedPersonagemIds(texto);
                await HandleMentionedPersonagens(instrucao.Id, mentionedIds);

                var result = await LoadInstructionWithRelations(instrucao.Id);
                return (true, result, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        private async Task HandlePersonagens(int instrucaoId, List<int>? personagemIds, bool clearExisting = false)
        {
            if (clearExisting)
            {
                // Clear ONLY Executor/Exceto personagens (not Citado)
                await _context.InstrucaoPersonagem
                    .Where(x => x.InstrucaoId == instrucaoId &&
                                (x.TipoDeParticipacao == TipoDeParticipacao.Executor ||
                                 x.TipoDeParticipacao == TipoDeParticipacao.Exceto))
                    .ExecuteDeleteAsync();
            }

            if (personagemIds == null || !personagemIds.Any()) return;
            if (personagemIds.Contains(-1))
            {
                await _context.InstrucaoPersonagem.AddAsync(new InstrucaoPersonagem
                {
                    InstrucaoId = instrucaoId,
                    ShowAll = true,
                    TipoDeParticipacao = TipoDeParticipacao.Executor
                });
            }
            else if (personagemIds.Contains(-2))
            {
                foreach (var personagemId in personagemIds.Where(id => id > 0))
                {
                    await _context.InstrucaoPersonagem.AddAsync(new InstrucaoPersonagem
                    {
                        InstrucaoId = instrucaoId,
                        ShowAllExcept = true,
                        PersonagemId = personagemId,
                        TipoDeParticipacao = TipoDeParticipacao.Exceto
                    });
                }
            }
            else
            {
                foreach (var personagemId in personagemIds.Where(id => id > 0))
                {
                    await _context.InstrucaoPersonagem.AddAsync(new InstrucaoPersonagem
                    {
                        InstrucaoId = instrucaoId, PersonagemId = personagemId,
                        TipoDeParticipacao = TipoDeParticipacao.Executor
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task HandleMentionedPersonagens(int instrucaoId, List<int> mentionedIds)
        {
            if (mentionedIds == null || !mentionedIds.Any()) return;
            await _context.InstrucaoPersonagem
                .Where(x => x.InstrucaoId == instrucaoId && x.TipoDeParticipacao == TipoDeParticipacao.Citado)
                .ExecuteDeleteAsync();
            foreach (var personagemId in mentionedIds)
            {
                await _context.InstrucaoPersonagem.AddAsync(new InstrucaoPersonagem
                {
                    InstrucaoId = instrucaoId,
                    PersonagemId = personagemId,
                    TipoDeParticipacao = TipoDeParticipacao.Citado
                });
            }

            await _context.SaveChangesAsync();
        }

        private async Task<Instrucao> LoadInstructionWithRelations(int id)
        {
            return await _context.Instrucao
                .Include(i => i.InstrucoesPersonagens
                    .Where(ins => ins.TipoDeParticipacao == TipoDeParticipacao.Executor ||
                                  ins.TipoDeParticipacao == TipoDeParticipacao.Exceto))
                .ThenInclude(ins => ins.Personagem)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        private async Task<int> GetNextOrderNumber(int cenaId)
        {
            var lastOrder = await _context.Instrucao.Where(i => i.CenaId == cenaId)
                .OrderByDescending(i => i.OrdemCronologica).Select(i => i.OrdemCronologica).FirstOrDefaultAsync();
            return lastOrder + 1;
        }

        #endregion SHARED PRIVATE METHODS

        #region MENTION

        [HttpGet]
        public async Task<IActionResult> GetPersonagensForMentions(int cenaId) // Add cenaId parameter
        {
            // First get the PecaId for the current Cena
            var pecaId = await _context.Cena
                .Where(c => c.Id == cenaId)
                .Select(c => c.PecaId)
                .FirstOrDefaultAsync();

            if (pecaId == 0)
            {
                return Json(new List<object>()); // Return empty if no Peca found
            }

            // Get personagens filtered by PecaId
            var personagens = await _context.PecaPersonagem
                .Where(ps => ps.PecaId == pecaId)
                .Select(ps => new { ps.Personagem.Id, ps.Personagem.Nome })
                .ToListAsync();

            return Json(personagens);
        }

        private List<int> ExtractMentionedPersonagemIds(string text)
        {
            // Handle null/empty cases immediately
            if (string.IsNullOrEmpty(text))
            {
                return new List<int>();
            }

            var mentions = new List<int>();
            try
            {
                var matches = Regex.Matches(text, @"@\[(\d+)\|");
                foreach (Match match in matches)
                {
                    if (match.Success && int.TryParse(match.Groups[1].Value, out var personagemId))
                    {
                        mentions.Add(personagemId);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log regex errors if needed
                Console.WriteLine($"Mention extraction error: {ex.Message}");
            }

            return mentions.Distinct().ToList();
        }

        public static string ParseMentionsToButtons(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return Regex.Replace(text, @"@\[(\d+)\|([^\]]+)\]",
                match =>
                    $"<button class='mention-btn' data-personagem-id='{match.Groups[1].Value}'>{match.Groups[2].Value}</button>");
        }

        #endregion MENTION


        private InstrucaoResponseDto MapToDto(Instrucao instrucao)
        {
            var dto = new InstrucaoResponseDto
            {
                Id = instrucao.Id,
                CenaId = instrucao.CenaId,
                Tipo = instrucao.TipoDeInstrucao.ToString(),
                Texto = instrucao.Texto,
                Ordem = instrucao.OrdemCronologica
            };

            // Process Executors/Exclusions
            foreach (var ip in instrucao.InstrucoesPersonagens
                         .Where(ip => ip.TipoDeParticipacao == TipoDeParticipacao.Executor ||
                                      ip.TipoDeParticipacao == TipoDeParticipacao.Exceto))
            {
                dto.Executores.Add(new ExecutorDto
                {
                    PersonagemId = ip.PersonagemId,
                    Nome = ip.Personagem?.Nome,
                    IsAll = ip.ShowAll,
                    IsAllExcept = ip.ShowAllExcept
                });
            }

            // Process Mentions
            foreach (var ip in instrucao.InstrucoesPersonagens
                         .Where(ip => ip.TipoDeParticipacao == TipoDeParticipacao.Citado))
            {
                dto.Citados.Add(new CitadoDto
                {
                    PersonagemId = ip.PersonagemId,
                    Nome = ip.Personagem?.Nome
                });
            }

            return dto;
        }
    }
}