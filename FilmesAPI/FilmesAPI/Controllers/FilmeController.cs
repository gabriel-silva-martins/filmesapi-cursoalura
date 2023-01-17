using AutoMapper;
using FilmesAPI.Data;
using FilmesAPI.Data.DTO;
using FilmesAPI.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace FilmesAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class FilmeController : ControllerBase
{

    private FilmeContext _context;
    private IMapper _mapper;

    public FilmeController(FilmeContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Adiciona um filme ao banco de dados
    /// </summary>
    /// <param name="filmeDto">Objeto com os campos necessários para criação de um filme</param>
    /// <returns>IActionResult</returns>
    /// <response code="201">Caso inserção seja feita com sucesso</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult AdicionaFilme([FromBody] CreateFilmeDTO filmeDTO)
    {
        Filme filme = _mapper.Map<Filme>(filmeDTO);
        _context.Filmes.Add(filme);
        _context.SaveChanges();
        return CreatedAtAction(nameof(RecuperaFilmePorId),
            new { id = filme.Id }, filme);
    }

    /// <summary>
    /// Informa os filmes que constam no banco de dados
    /// </summary>
    /// <param name="skip">Para pular a quantidade de filmes</param>
    /// <param name="take">Para compor a paginação pela quantidade informada</param>
    /// <returns>IActionResult</returns>
    /// <response code="200">Caso os dados cheguem com sucesso</response>
    [HttpGet]
    public IEnumerable<ReadFilmeDTO> RecuperaFilmes([FromQuery] int skip = 0,
        [FromQuery] int take = 10)
    {
        return _mapper.Map<List<ReadFilmeDTO>>(_context.Filmes.Skip(skip).Take(take));
    }

    /// <summary>
    /// Traz um filme do banco de dados
    /// </summary>
    /// <param name="id">ID para trazer o filme por esta chave</param>
    /// <returns>IActionResult</returns>
    /// <response code="200">Caso localização seja realizada com sucesso</response>
    [HttpGet("{id}")]
    public IActionResult RecuperaFilmePorId(int id)
    {
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);
        if (filme == null) return NotFound();
        var filmeDTO = _mapper.Map<ReadFilmeDTO>(filme);
        return Ok(filmeDTO);
    }

    /// <summary>
    /// Atualiza um filme do banco de dados
    /// </summary>
    /// <param name="id">ID para informar o filme a ser atualizado</param>
    /// <returns>IActionResult</returns>
    /// <response code="204">Caso atualização seja realizada com sucesso</response>
    [HttpPut("{id}")]
    public IActionResult AtualizaFilme(int id, [FromBody] UpdateFilmeDTO filmeDTO)
    {
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);
        if (filme == null) return NotFound();
        _mapper.Map(filmeDTO, filme);
        _context.SaveChanges();
        return NoContent();
    }

    /// <summary>
    /// Atualiza de forma parcial um filme do banco de dados
    /// </summary>
    /// <param name="id">ID para informar o filme a ser atualizado</param>
    /// <param name="patch">Caminho e operação para atualizar parcialmente</param>
    /// <returns>IActionResult</returns>
    /// <response code="204">Caso atualização seja realizada com sucesso</response>
    [HttpPatch("{id}")]
    public IActionResult AtualizaFilmeParcial(int id, JsonPatchDocument<UpdateFilmeDTO> patch)
    {
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);
        if (filme == null) return NotFound();

        var filmeParaAtualizar = _mapper.Map<UpdateFilmeDTO>(filme);
        patch.ApplyTo(filmeParaAtualizar, ModelState);

        if (!TryValidateModel(filmeParaAtualizar))
        {
            return ValidationProblem(ModelState);
        }

        _mapper.Map(filmeParaAtualizar, filme);
        _context.SaveChanges();
        return NoContent();
    }

    /// <summary>
    /// Deleta um filme do banco de dados
    /// </summary>
    /// <param name="id">ID para informar o filme a ser deletado</param>
    /// <returns>IActionResult</returns>
    /// <response code="204">Caso remoção seja realizada com sucesso</response>
    [HttpDelete("{id}")]
    public IActionResult DeletaFilme(int id)
    {
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);
        if (filme == null) return NotFound();

        _context.Remove(filme);
        _context.SaveChanges();
        return NoContent();
    }
}
