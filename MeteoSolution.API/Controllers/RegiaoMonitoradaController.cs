using Microsoft.AspNetCore.Mvc;
using MeteoSolution.API.Models;
using MeteoSolution.API.Repositories;
using MeteoSolution.API.Controllers.DTOs;
using System.Text;
using System.Text.Json;

namespace MeteoSolution.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegiaoMonitoradaController : ControllerBase
{
    private readonly RegiaoMonitoradaRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;

    // URL da API de IA hospedada no Render
    private const string IA_API_URL = "https://meteo-solution-ai.onrender.com/predict";

    public RegiaoMonitoradaController(
        RegiaoMonitoradaRepository repository,
        IHttpClientFactory httpClientFactory)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
    }

    private RegiaoMonitoradaResponseDTO ToResponse(RegiaoMonitorada r) => new()
    {
        Id = r.Id,
        Nome = r.Nome,
        Latitude = r.Latitude,
        Longitude = r.Longitude,
        AltitudeMedia = r.AltitudeMedia,
        DeclividadePercentual = r.DeclividadePercentual,
        CoberturaVegetalPercentual = r.CoberturaVegetalPercentual,
        ImpermeabilizacaoPercentual = r.ImpermeabilizacaoPercentual,
        DistanciaRioMetros = r.DistanciaRioMetros,
        TipoSolo = r.TipoSolo,
        NivelUrbanizacao = r.NivelUrbanizacao,
        Ativa = r.Ativa,
        Cidade = r.Cidade is null ? null : new CidadeResponseDTO
        {
            Id = r.Cidade.Id,
            Nome = r.Cidade.Nome,
            Latitude = r.Cidade.Latitude,
            Longitude = r.Cidade.Longitude,
            Estado = r.Cidade.Estado is null ? null : new EstadoResponseDTO
            {
                Id = r.Cidade.Estado.Id,
                Nome = r.Cidade.Estado.Nome,
                Sigla = r.Cidade.Estado.Sigla,
                Pais = r.Cidade.Estado.Pais is null ? null : new PaisResponseDTO
                {
                    Id = r.Cidade.Estado.Pais.Id,
                    Nome = r.Cidade.Estado.Pais.Nome,
                    CodigoIso = r.Cidade.Estado.Pais.CodigoIso
                }
            }
        }
    };

    // Converte NivelUrbanizacao (string) para código numérico da IA
    private static int ConverterNivelUrbanizacao(string nivel) => nivel?.ToLower() switch
    {
        "baixo"      => 0,
        "medio"      => 1,
        "alto"       => 2,
        "muito_alto" => 3,
        _            => 1
    };

    // Converte TipoSolo (string) para código numérico da IA
    private static int ConverterTipoSolo(string tipo) => tipo?.ToLower() switch
    {
        "arenoso"  => 0,
        "argiloso" => 1,
        "rochoso"  => 2,
        _          => 1
    };

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var regioes = await _repository.GetAllAsync();
        return Ok(regioes.Select(ToResponse));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var regiao = await _repository.GetByIdAsync(id);
        if (regiao is null)
            return NotFound(new { message = $"Região com id {id} não encontrada." });

        return Ok(ToResponse(regiao));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RegiaoMonitoradaDTO dto)
    {
        var regiao = new RegiaoMonitorada
        {
            Nome = dto.Nome,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            AltitudeMedia = dto.AltitudeMedia,
            DeclividadePercentual = dto.DeclividadePercentual,
            CoberturaVegetalPercentual = dto.CoberturaVegetalPercentual,
            ImpermeabilizacaoPercentual = dto.ImpermeabilizacaoPercentual,
            DistanciaRioMetros = dto.DistanciaRioMetros,
            TipoSolo = dto.TipoSolo,
            NivelUrbanizacao = dto.NivelUrbanizacao,
            Ativa = dto.Ativa,
            CidadeId = dto.CidadeId
        };

        var criada = await _repository.CreateAsync(regiao);
        return CreatedAtAction(nameof(GetById), new { id = criada.Id }, ToResponse(criada));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] RegiaoMonitoradaDTO dto)
    {
        var regiaoAtualizada = new RegiaoMonitorada
        {
            Nome = dto.Nome,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            AltitudeMedia = dto.AltitudeMedia,
            DeclividadePercentual = dto.DeclividadePercentual,
            CoberturaVegetalPercentual = dto.CoberturaVegetalPercentual,
            ImpermeabilizacaoPercentual = dto.ImpermeabilizacaoPercentual,
            DistanciaRioMetros = dto.DistanciaRioMetros,
            TipoSolo = dto.TipoSolo,
            NivelUrbanizacao = dto.NivelUrbanizacao,
            Ativa = dto.Ativa,
            CidadeId = dto.CidadeId
        };

        var resultado = await _repository.UpdateAsync(id, regiaoAtualizada);
        if (resultado is null)
            return NotFound(new { message = $"Região com id {id} não encontrada." });

        return Ok(ToResponse(resultado));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deletado = await _repository.DeleteAsync(id);
        if (!deletado)
            return NotFound(new { message = $"Região com id {id} não encontrada." });

        return NoContent();
    }

   
    [HttpPost("{id}/predict")]
    public async Task<IActionResult> Predict(int id, [FromBody] PrevisaoClimaticaDTO dto)
    {
        
        var regiao = await _repository.GetByIdAsync(id);
        if (regiao is null)
            return NotFound(new { message = $"Região com id {id} não encontrada." });

        
        var payload = new
        {
            precipitacao_mm          = dto.PrecipitacaoMm,
            temperatura_c            = dto.TemperaturaC,
            umidade_percentual       = dto.UmidadePercentual,
            velocidade_vento_kmh     = dto.VelocidadeVentoKmh,
            pressao_hpa              = dto.PressaoHpa,
            declividade_percentual   = regiao.DeclividadePercentual,
            distancia_rio_metros     = regiao.DistanciaRioMetros,
            nivel_urbanizacao_cod    = ConverterNivelUrbanizacao(regiao.NivelUrbanizacao),
            tipo_solo_cod            = ConverterTipoSolo(regiao.TipoSolo),
            nivel_urbanizacao        = regiao.NivelUrbanizacao?.ToLower(),
            tipo_solo                = regiao.TipoSolo?.ToLower()
        };

        
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(60); 

            var json    = JsonSerializer.Serialize(payload);
            Console.WriteLine($"Payload enviado para IA: {json}");
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(IA_API_URL, content);

            if (!response.IsSuccessStatusCode)
                return StatusCode(502, new { message = "Erro ao chamar a API de IA.", status = (int)response.StatusCode });

            var resultadoJson = await response.Content.ReadAsStringAsync();
            var resultado     = JsonSerializer.Deserialize<JsonElement>(resultadoJson);

            
            return Ok(new
            {
                regiao = new
                {
                    id     = regiao.Id,
                    nome   = regiao.Nome,
                    cidade = regiao.Cidade?.Nome,
                    estado = regiao.Cidade?.Estado?.Nome
                },
                dadosClimaticos = dto,
                predicao        = resultado
            });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(504, new { message = "Timeout ao chamar a API de IA. Tente novamente em alguns instantes." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno ao processar a predição.", detalhe = ex.Message });
        }
    }
}
